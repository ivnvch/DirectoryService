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

    public async Task<Result<Guid, Errors>> Handle(CreatePositionCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _positionValidator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToError().ToErrors();

        List<Guid> departmentIds = command.DepartmentIds;

        var allDepartmentsExists = await _departmentRepository.AllDepartmentsExistAsync(departmentIds, cancellationToken);
        if (allDepartmentsExists.IsFailure)
            return allDepartmentsExists.Error;
        
        if (allDepartmentsExists.Value == false)
            return Error.NotFound(
                "department.not.found", "One or more departments are not found.").ToErrors();
        
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
            return position.Error.ToErrors();
        
       var result = await _positionRepository.Add(position.Value, cancellationToken);

       if (result.IsFailure)
           return Error.Failure(result.Error.Messages).ToErrors();

        return positionId;
    }
}