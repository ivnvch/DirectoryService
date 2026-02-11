using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Departments.Repositories;
using DirectoryService.Application.Extensions.Validation;
using DirectoryService.Application.Locations.Repositories;
using DirectoryService.Application.Positions.Repositories;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObject;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using DirectoryService.Shared.Errors;
using FluentValidation;
using FluentValidation.Results;

namespace DirectoryService.Application.Departments.Commands.SoftDeleteDepartment;

public class SoftDeleteDepartmentHandler : ICommandHandler<Guid, SoftDeleteDepartmentCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IPositionRepository _positionRepository;
    private readonly IValidator<SoftDeleteDepartmentCommand> _validator;
    private readonly ITransactionManager  _transactionManager;

    public SoftDeleteDepartmentHandler(
        IDepartmentRepository departmentRepository,
        ILocationRepository locationRepository,
        IPositionRepository positionRepository,
        IValidator<SoftDeleteDepartmentCommand> validator, 
        ITransactionManager transactionManager)
    {
        _departmentRepository = departmentRepository;
        _locationRepository = locationRepository;
        _positionRepository = positionRepository;
        _validator = validator;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Errors>> Handle(SoftDeleteDepartmentCommand command, CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToError().ToErrors();
        
        Result<ITransactionScope, Error> transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();
        
        using ITransactionScope transactionScope =  transactionScopeResult.Value;
        
        Result<Department, Error> departmentResult = await _departmentRepository.GetByAsync(x => x.Id == command.DepartmentId, cancellationToken);
        if (departmentResult.IsFailure)
        {
            transactionScope.Rollback();
            return Error.NotFound("department.not.found", $"{departmentResult.Error}").ToErrors();
        }
        
        departmentResult.Value.SoftDelete();
        
       var oldPath = DepartmentPath.Create(departmentResult.Value.DepartmentPath.Value);
       var markAsDeleted = departmentResult.Value.DepartmentPath.MarkAsDeleted();

       var updatePath = await _departmentRepository.UpdatePathAfterSoftDeleted(
           oldPath,
           markAsDeleted,
           cancellationToken);
       if (updatePath.IsFailure)
       {
           transactionScope.Rollback();
           return updatePath.Error.ToErrors();
       }

        UnitResult<Error> locationsAsDeleted = await _locationRepository.GetLocationsExclusiveToDepartment(command.DepartmentId, cancellationToken);
        if (locationsAsDeleted.IsFailure)
        {
            transactionScope.Rollback();
            return locationsAsDeleted.Error.ToErrors();
        }
        
        UnitResult<Error> positionAsDeleted = await _positionRepository.GetPositionsExclusiveToDepartment(command.DepartmentId, cancellationToken);
        if (positionAsDeleted.IsFailure)
        {
            transactionScope.Rollback();
            return positionAsDeleted.Error.ToErrors();
        }

        var saveChanges = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChanges.IsFailure)
        {
            transactionScope.Rollback();
            return saveChanges.Error.ToErrors();
        }

        var commitedResult = transactionScope.Commit();
        if (commitedResult.IsFailure)
            return commitedResult.Error.ToErrors();

        return departmentResult.Value.Id;
    }
}