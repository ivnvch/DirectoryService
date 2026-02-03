using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Departments.Repositories;
using DirectoryService.Application.Extensions.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObject;
using DirectoryService.Shared.Errors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.Commands.UpdateDepartmentPath;

public class UpdateDepartmentPathCommandHandler : ICommandHandler<Guid, UpdateDepartmentPathCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IValidator<UpdateDepartmentPathCommand> _validator;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<UpdateDepartmentPathCommandHandler> _logger;

    public UpdateDepartmentPathCommandHandler(
        IValidator<UpdateDepartmentPathCommand> validator,
        IDepartmentRepository departmentRepository, 
        ITransactionManager transactionManager, 
        ILogger<UpdateDepartmentPathCommandHandler> logger)
    {
        _departmentRepository = departmentRepository;
        _validator = validator;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(UpdateDepartmentPathCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            validationResult.ToError().ToErrors();

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();
        
        using var transactionScope = transactionScopeResult.Value;
        
        var departmentResult = await _departmentRepository.GetByIdWithLockAsync(command.DepartmentId, cancellationToken);
        if (departmentResult.IsFailure)
        {
            transactionScope.Rollback();
            return departmentResult.Error.ToErrors();
        }
        
        var department = departmentResult.Value;
        DepartmentPath oldPath = department.DepartmentPath;

        if (command.ParentId.HasValue)
        {
            var parentDepartmentResult = await _departmentRepository.GetByIdWithLockAsync(command.ParentId.Value, cancellationToken);
            if (parentDepartmentResult.IsFailure)
            {
                transactionScope.Rollback();
                return parentDepartmentResult.Error.ToErrors();
            }
            Department? parentDepartment = parentDepartmentResult.Value;
            
            var isDescendants = await _departmentRepository.IsDescendantsAsync(department.Id, parentDepartment.Id, cancellationToken);
            if (isDescendants)
            {
                transactionScope.Rollback();
                return Error.Failure("department.not.descendant", $"Cannot set a descendant department with Id {parentDepartment.Id} as parent.").ToErrors();
            }

            var setPathWithNewParent = department
                .UpdatePathWithParent(parentDepartment.Id, (short)parentDepartment.Depth, parentDepartment.DepartmentPath);

            var result = await _departmentRepository
                .UpdateDepartmentsHierarchyAsync(department, oldPath, cancellationToken);
            
            _logger.LogInformation($"New parent Department: {department.Name} has been installed and the depth and path of the subdivision have been updated ");
        }
        else
        {
            var setNewPath = department.UpdatePathWithoutParent();
            if (setNewPath.IsFailure)
            {
                transactionScope.Rollback();
                return setNewPath.Error.ToErrors();
            }
            
            await _departmentRepository.UpdateDepartmentsHierarchyAsync(department, oldPath, cancellationToken);
            
            _logger.LogError("The department's path has been changed to the root path. All paths of the child divisions have been updated");
        }

        await _transactionManager.SaveChangesAsync(cancellationToken);
        transactionScope.Commit();
        
        return department.Id;
    }
}