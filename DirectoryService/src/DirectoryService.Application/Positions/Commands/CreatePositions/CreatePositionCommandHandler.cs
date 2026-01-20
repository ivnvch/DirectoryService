using CSharpFunctionalExtensions;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Departments.Repositories;
using DirectoryService.Application.Extensions.Validation;
using DirectoryService.Application.Positions.Repositories;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Positions;
using DirectoryService.Shared.Errors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Positions.Commands.CreatePositions;

public class CreatePositionCommandHandler : ICommandHandler<Guid, CreatePositionCommand>
{
    private readonly IValidator<CreatePositionCommand> _positionValidator;
    private readonly IPositionRepository _positionRepository;
    private readonly IDepartmentRepository  _departmentRepository;
    private readonly ILogger<CreatePositionCommandHandler> _logger;

    public CreatePositionCommandHandler(IValidator<CreatePositionCommand> positionValidator, 
        IPositionRepository positionRepository,
        IDepartmentRepository departmentRepository,
        ILogger<CreatePositionCommandHandler> logger) 
    {
        _positionValidator = positionValidator;
        _positionRepository = positionRepository;
        _logger = logger;
        _departmentRepository = departmentRepository;
    }

    public async Task<Result<Guid, Error>> Handle(CreatePositionCommand command, CancellationToken token)
    {
        var validationResult = await _positionValidator.ValidateAsync(command, token);
        if (validationResult.IsValid == false)
            return validationResult.ToError();

        List<Guid> departmentIds = command.DepartmentIds;

        var allDepartmentsExists = await _departmentRepository.AllDepartmentsExistAsync(departmentIds, token);
        if (allDepartmentsExists.IsFailure)
            return Error.Failure(new ErrorMessage(
                "allDepartmentsExists.is.failure", "An error occurred during the search departments."));

        if (allDepartmentsExists.Value == false)
            return Error.NotFound(
                "department.not.found", "One or more departments are not found.");
        
        Guid positionId = Guid.NewGuid();

        var departmentPositions = departmentIds.Select(d => new DepartmentPosition(
            Guid.NewGuid(),
            d,
            positionId
        ));

        var position = Position.Create(
            command.Name,
            command.Description,
            departmentPositions);

        if (position.IsFailure)
            return position.Error;
        
       var result = await _positionRepository.Add(position.Value, token);

       if (result.IsFailure)
           return Error.Failure(result.Error.Messages);

        return positionId;
    }
}