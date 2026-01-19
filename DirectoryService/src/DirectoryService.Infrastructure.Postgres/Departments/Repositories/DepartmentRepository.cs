using System.Data.Common;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Repositories;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.Errors;
using DirectoryService.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Npgsql.PostgresTypes;

namespace DirectoryService.Infrastructure.Departments.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly DirectoryDbContext _context;
    private readonly ILogger<DepartmentRepository> _logger;

    public DepartmentRepository(DirectoryDbContext context, ILogger<DepartmentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Add(Department department, CancellationToken cancellationToken = default)
    {
       _context.Departments.Add(department);

       try
       {
           await _context.SaveChangesAsync(cancellationToken);

           _logger.LogInformation($"Department: {department.Name} with Department ID: {department.Id} has been added");

           return department.Id;
       }
       catch (OperationCanceledException ex)
       {
           _logger.LogError(ex, "Operation cancelled while creating department: {department}", department);

           return DepartmentError.OperationCancelled();
       }
       catch (Exception ex)
       {
          _logger.LogError(ex, "Unexpected error while creating department: {department}", department);

          return DepartmentError.DatabaseError();
       }
    }

    public async Task<Result<Department, Error>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (department is null)
            return GeneralErrors.NotFound(id);
        
        return department;
    }

    public async Task<Result<bool, Errors>> AllDepartmentsExistAsync(IReadOnlyCollection<Guid> departmentIds, CancellationToken cancellationToken = default)
    {
        if (departmentIds.Count == 0)
            return false;
        
        var existingCount = await _context.Departments
            .Where(d => departmentIds.Contains(d.Id))
            .CountAsync(cancellationToken);
        
        return existingCount == departmentIds.Count;
    }
}