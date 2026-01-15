using System.ComponentModel.Design.Serialization;
using DirectoryService.Application.Departments.Repositories;
using DirectoryService.Application.Extensions.Validation;
using DirectoryService.Application.Locations.Repositories;
using DirectoryService.Domain.Departments.ValueObject;
using DirectoryService.Shared.Errors;
using FluentValidation;

namespace DirectoryService.Application.Departments.Commands.CreateDepartments;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
{
    private readonly ILocationRepository _locationRepository;
    
    public CreateDepartmentValidator(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
        
        RuleFor(x => x.Name)
            .MustBeValueObject(DepartmentName.Create);
        
        RuleFor(x => x.Identifier)
            .MustBeValueObject(DepartmentIdentifier.Create);
        
        RuleFor(x => x.ParentId)
            .NotEqual(Guid.Empty)
            .When(x => x.ParentId.HasValue);
        

        RuleFor(x => x.LocationIds)
            .MustAsync(async (ids, cancellation) =>
            {
                if (ids is null)
                    return true;

               var result = await _locationRepository.AllExistAsync(ids, cancellation);
               
               return result is { IsSuccess: true, Value: true };
            })
            .WithError(Error.Validation(
                new ErrorMessage("CreateDepartmentCommand.LocationIds.is.invalid", 
                              "Some LocationIds are not exists")));// нужно ли здесь пробрасывать ошибку

        /*RuleFor(x => x.LocationIds)
            .Must(ids => ids.Length == ids.ToHashSet().Count)
            .WithError(Error.Validation(new ErrorMessage("CreateDepartmentCommand.LocationIds.is.invalid", "Duplicate LocationIds are not allowed")));
            */
    }
}