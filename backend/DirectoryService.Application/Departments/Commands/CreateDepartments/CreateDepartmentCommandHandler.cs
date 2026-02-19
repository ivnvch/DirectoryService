using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Departments.Repositories;
using DirectoryService.Application.Extensions.Validation;
using DirectoryService.Application.Locations.Repositories;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObject;
using DirectoryService.Shared.Errors;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Commands.CreateDepartments;

public class CreateDepartmentCommandHandler : ICommandHandler<Guid, CreateDepartmentCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<CreateDepartmentCommand> _departmentValidator;
    private readonly ILogger<CreateDepartmentCommandHandler> _logger;

    public CreateDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        ILocationRepository locationRepository,
        ITransactionManager transactionManager,
        IValidator<CreateDepartmentCommand> departmentValidator,
        ILogger<CreateDepartmentCommandHandler> logger)
    {
        _departmentRepository = departmentRepository;
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _departmentValidator = departmentValidator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _departmentValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            return validationResult.ToError().ToErrors();

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();
        
        using var transactionScope = transactionScopeResult.Value;

        DepartmentName name = DepartmentName.Create(command.Name).Value;
        DepartmentIdentifier identifier = DepartmentIdentifier.Create(command.Identifier).Value;
        
        Department? parent = null;

        if (command.ParentId.HasValue)
        {
            var parentId = await _departmentRepository.GetByAsync(x => x.ParentId == command.ParentId.Value, cancellationToken);
            if (parentId.IsFailure)
            {
                transactionScope.Rollback();
                return parentId.Error.ToErrors();
            }
            
            parent = parentId.Value;
        }

        Guid[] locationIds = command.LocationIds;
        
        var allLocationExists = await _locationRepository.AllExistAsync(locationIds, cancellationToken);

        if (allLocationExists.IsFailure)
        {
            transactionScope.Rollback();
            return allLocationExists.Error;
        }

        if (allLocationExists.Value == false)
            return Error.NotFound("location.not.found", "One or more locations were not found.").ToErrors();
        
        Guid departmentId = Guid.NewGuid();

        var departmentLocations = locationIds.Select(dl => DepartmentLocation.Create(
            departmentId,
            dl));

        var department = parent is null
            ? Department.CreateParent(name, identifier, departmentLocations, departmentId)
            : Department.CreateChild(name, identifier, parent, departmentLocations, departmentId);
        
        if(department.IsFailure)
            return department.Error.ToErrors();
        
        var result = await _departmentRepository.Add(department.Value, cancellationToken);

        if (result.IsFailure)
        {
            transactionScope.Rollback();
            Error.Failure(result.Error.Messages);
        }

        await _transactionManager.SaveChangesAsync(cancellationToken);
        
        var commitedResult =  transactionScope.Commit();
        if (commitedResult.IsFailure)
            return commitedResult.Error.ToErrors();

        return departmentId;
    }
}