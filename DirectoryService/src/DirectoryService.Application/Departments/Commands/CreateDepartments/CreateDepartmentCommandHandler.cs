using CSharpFunctionalExtensions;
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
    private readonly IValidator<CreateDepartmentCommand> _departmentValidator;
    private readonly ILogger<CreateDepartmentCommandHandler> _logger;

    public CreateDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        ILocationRepository locationRepository,
        IValidator<CreateDepartmentCommand> departmentValidator,
        ILogger<CreateDepartmentCommandHandler> logger)
    {
        _departmentRepository = departmentRepository;
        _locationRepository = locationRepository;
        _departmentValidator = departmentValidator;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Handle(CreateDepartmentCommand command, CancellationToken token)
    {
        ValidationResult validationResult = await _departmentValidator.ValidateAsync(command, token);

        if (!validationResult.IsValid)
            return validationResult.ToError();

        DepartmentName name = DepartmentName.Create(command.Name).Value;
        DepartmentIdentifier identifier = DepartmentIdentifier.Create(command.Identifier).Value;
        
        Department? parent = null;

        if (command.ParentId.HasValue)
        {
            var parentId = await _departmentRepository.GetById(command.ParentId.Value, token);
            if (parentId.IsFailure)
                return parentId.Error;
            
            parent = parentId.Value;
        }

        Guid[] locationIds = command.LocationIds;
        
        var allLocationExists = await _locationRepository.AllExistAsync(locationIds, token);
        
        if(allLocationExists.IsFailure)
            return Error.Failure("locationIds.is.", allLocationExists.Error.ToString());

        if (allLocationExists.Value == false)
            return Error.NotFound("location.not.found", "Одна или несколько локаций были не найдены");
        
        Guid departmentId = Guid.NewGuid();

        var departmentLocations = locationIds.Select(dl => new DepartmentLocation(
            Guid.NewGuid(),
            departmentId,
            dl));

        var department = parent is null
            ? Department.CreateParent(name, identifier, departmentLocations, departmentId)
            : Department.CreateChild(name, identifier, parent, departmentLocations, departmentId);
        
        if(department.IsFailure)
            return department.Error;
        
        await _departmentRepository.Add(department.Value, token);

        return departmentId;
    }
}