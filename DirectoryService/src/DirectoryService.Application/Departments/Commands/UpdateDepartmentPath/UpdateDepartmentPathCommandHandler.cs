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
        ITransactionManager transactionManager, ILogger<UpdateDepartmentPathCommandHandler> logger)
    {
        _departmentRepository = departmentRepository;
        _validator = validator;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(UpdateDepartmentPathCommand command, CancellationToken token)
    {
        var validationResult = await _validator.ValidateAsync(command, token);
        if (!validationResult.IsValid)
            validationResult.ToError().ToErrors();

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(token);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();
        
        using var transactionScope = transactionScopeResult.Value;
        
        var departmentResult = await _departmentRepository.GetByIdWithLockAsync(command.DepartmentId, token);
        if (departmentResult.IsFailure)
        {
            transactionScope.Rollback();
            return departmentResult.Error.ToErrors();
        }
        
        var department = departmentResult.Value;
        DepartmentPath oldPath = department.DepartmentPath;

        if (command.ParentId.HasValue)
        {
            var parentDepartmentResult = await _departmentRepository.GetByIdWithLockAsync(command.ParentId.Value, token);
            if (parentDepartmentResult.IsFailure)
            {
                transactionScope.Rollback();
                return parentDepartmentResult.Error.ToErrors();
            }
            Department? parentDepartment = parentDepartmentResult.Value;
            
            var isDescendants = await _departmentRepository.IsDescendantsAsync(department.Id, parentDepartment.Id, token);
            if (isDescendants.IsFailure)
            {
                transactionScope.Rollback();
                return isDescendants.Error.ToErrors();
            }

            var setPathWithNewParent = department.UpdatePathWithParent(parentDepartment.Id, (short)parentDepartment.Depth, parentDepartment.DepartmentPath);

            var result = await _departmentRepository
                .UpdateDepartmentsHierarchyAsync(department, oldPath, token);
            
            _logger.LogInformation($"В Department: {department.Name} установлен новый родитель и обновлены глубина и путь подразделения");
        }
        else
        {
            var setNewPath = department.UpdatePathWithoutParent();
            if (setNewPath.IsFailure)
            {
                transactionScope.Rollback();
                return setNewPath.Error.ToErrors();
            }
            
            await _departmentRepository.UpdateDepartmentsHierarchyAsync(department, oldPath, token);
            
            _logger.LogError("Путь департамента изменён на корневой. Обновлены все пути дочерних подразделений");
        }

        await _transactionManager.SaveChangesAsync(token);
        transactionScope.Commit();
        
        return department.Id;
    }
}