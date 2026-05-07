using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Repositories;
using DirectoryService.Domain.Departments;
using FileService.Contracts;
using FileService.Contracts.MediaAssets.Queries.Responses;
using Microsoft.Extensions.Logging;
using Shared.Abstractions;
using Shared.CommonErrors;
using Shared.Database;

namespace DirectoryService.Application.Departments.Commands.AttachVideos;

public sealed class AttachVideoToDepartmentHandler : ICommandHandler<Guid, AttachVideoToDepartmentCommand>
{
    private readonly ITransactionManager _transactionManager;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IFileCommunicationService _fileCommunicationService;
    private readonly ILogger<AttachVideoToDepartmentHandler> _logger;
        
    public AttachVideoToDepartmentHandler(
        ITransactionManager transactionManager, 
        IFileCommunicationService fileCommunicationService, 
        IDepartmentRepository departmentRepository,
        ILogger<AttachVideoToDepartmentHandler> logger) 
    {
        _logger = logger;
        _transactionManager = transactionManager;
        _fileCommunicationService = fileCommunicationService;
        _departmentRepository = departmentRepository;
    }
    
    public async Task<Result<Guid, Errors>> Handle(AttachVideoToDepartmentCommand toDepartmentCommand, CancellationToken cancellationToken)
    {
        if (toDepartmentCommand.Request!.VideoId.HasValue)
        {
            Result<CheckMediaAssetExistsResponse, Errors> existsResult = 
                await _fileCommunicationService.CheckMediaAssetExists(toDepartmentCommand.Request.VideoId.Value, cancellationToken);

            if (existsResult.IsFailure)
                return existsResult.Error;
            
            if (!existsResult.Value.Exists)
                return Error.NotFound("file.not_found", "Video not found").ToErrors();
        }

        Result<Department, Error> departmentResult =
            await _departmentRepository.GetByAsync(d => d.Id == toDepartmentCommand.DepartmentId, cancellationToken);
        
        if (departmentResult.IsFailure)
            return departmentResult.Error.ToErrors();

        Department department = departmentResult.Value;
        department.AttachVideo(toDepartmentCommand.Request.VideoId);

        await _transactionManager.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Video attached to department {DepartmentId}", department.Id);
        
        return department.Id;
    }
}