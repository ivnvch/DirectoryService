using System.ComponentModel.Design.Serialization;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Repositories;
using DirectoryService.Application.Extensions.Validation;
using DirectoryService.Application.Positions.Repositories;
using DirectoryService.Shared.Errors;
using FluentValidation;

namespace DirectoryService.Application.Positions.Commands.CreatePositions;

public class CreatePositionValidator : AbstractValidator<CreatePositionCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IPositionRepository _positionRepository;
    
    public CreatePositionValidator(IDepartmentRepository departmentRepository, IPositionRepository positionRepository)
    {
        _departmentRepository = departmentRepository;
        _positionRepository = positionRepository;

        RuleFor(x => x.Name)
            .NotEmpty()
            .NotNull()
            .MaximumLength(1000)
            .MinimumLength(3)
            .WithError(GeneralErrors.ValueIsRequired("Name"));

        RuleFor(x => x.Name)
            .MustAsync(async(name, cancellationToken) =>
                {
                    UnitResult<Error> result = await _positionRepository.ExistsActiveWithName(name, cancellationToken);

                    return result is { IsSuccess: true};
                })
            .WithError(Error.Validation(new ErrorMessage(
                "name.already.exists", "Position name already exists")));
        
        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithError(GeneralErrors.ValueIsInvalid("Description"));
        
        RuleFor(x => x.DepartmentIds)
            .NotNull()
            .Must(list => list.Count > 0)
            .WithError(GeneralErrors.ValueIsRequired("DepartmentIds"));

        RuleFor(x => x.DepartmentIds)
            .MustAsync(async (ids, cancellation) =>
            {
                if (ids is null)
                    return false;

                var result = await _departmentRepository.AllDepartmentsExistAsync(ids, cancellation);

                return result is {IsSuccess: true, Value: true};
            })
            .WithError(Error.Validation(
                new ErrorMessage("CreatePositionCommand.DepartmentIds.is.not.unique", 
                    "Some of departments are not exists")));
    }
}