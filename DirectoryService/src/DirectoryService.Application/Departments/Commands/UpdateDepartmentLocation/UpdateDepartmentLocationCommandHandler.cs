using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Departments.Repositories;
using DirectoryService.Application.Extensions.Validation;
using DirectoryService.Application.Locations.Repositories;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Shared.Errors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Commands.UpdateDepartmentLocation;

public class UpdateDepartmentLocationCommandHandler : ICommandHandler<Guid,  UpdateDepartmentLocationCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IValidator<UpdateDepartmentLocationCommand> _validator;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<UpdateDepartmentLocationCommandHandler> _logger;

    public UpdateDepartmentLocationCommandHandler(
        IDepartmentRepository departmentRepository,
        ILocationRepository locationRepository,
        IValidator<UpdateDepartmentLocationCommand> validator,
        ITransactionManager  transactionManager,
        ILogger<UpdateDepartmentLocationCommandHandler> logger)
    {
        _departmentRepository = departmentRepository;
        _locationRepository = locationRepository;
        _validator = validator;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(UpdateDepartmentLocationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToError().ToErrors();

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();
        
        using var transactionScope = transactionScopeResult.Value;
        
        var department = await _departmentRepository.GetByAsync(x => x.Id == command.DepartmentId, cancellationToken);
        if (department.IsFailure)
        {
            transactionScope.Rollback();
            return department.Error.ToErrors();
        }
        
        var locationIds = await _locationRepository.AllExistAsync(command.LocationIds.ToArray(), cancellationToken);
        if (locationIds.IsFailure)
        {
            transactionScope.Rollback();
            return locationIds.Error;
        }
        var newLocationsIds = command.LocationIds.Select(x => DepartmentLocation.Create(
            department.Value.Id,
            x)).ToList();
        
       department.Value.UpdateLocations(newLocationsIds);
        
       var deleteLocations = await _departmentRepository.DeleteLocationsAsync(command.DepartmentId, cancellationToken);
       if (deleteLocations.IsFailure)
       {
           transactionScope.Rollback();
           return deleteLocations.Error.ToErrors();
       }
       
       var saveChanges = await _transactionManager.SaveChangesAsync(cancellationToken);
       if (saveChanges.IsFailure)
       {
           transactionScope.Rollback();
           return saveChanges.Error.ToErrors();
       }
       
       var commitedResult =  transactionScope.Commit();
       if (commitedResult.IsFailure)
           return commitedResult.Error.ToErrors();

       return department.Value.Id;
    }
}