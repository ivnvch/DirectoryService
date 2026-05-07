using DirectoryService.Shared.Departments;
using Shared.Abstractions;

namespace DirectoryService.Application.Departments.Commands.AttachVideos;

public record AttachVideoToDepartmentCommand(Guid DepartmentId, AttachVideoToDepartmentRequest? Request) : ICommand;