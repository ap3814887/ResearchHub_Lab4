using ResearchHub.Api.DTO.Projects;
using ResearchHub.Api.DTO.Tasks;
using ResearchHub.Api.DTO.Users;
using ResearchHub.Api.Models;

namespace ResearchHub.Api.Mapping;

public static class MappingExtensions
{
    public static UserResponse ToResponse(this UserRecord x) => new(x.Id, x.Name, x.Email, x.CreatedAt);
    public static ProjectResponse ToResponse(this ProjectRecord x) => new(x.Id, x.Name, x.Description, x.OwnerId, x.CreatedAt);
    public static ProjectMemberResponse ToResponse(this ProjectMemberRecord x) => new(x.ProjectId, x.UserId, x.UserName, x.Email, x.Role, x.JoinedAt);
    public static TaskResponse ToResponse(this TaskRecord x) => new(x.Id, x.ProjectId, x.Title, x.Description, x.AssigneeId, x.Status, x.Priority, x.DueDate, x.CreatedAt, x.UpdatedAt);
    public static TaskStatusChangeResponse ToResponse(this TaskStatusChangeRecord x) => new(x.Id, x.OldStatus, x.NewStatus, x.UpdatedAt);
    public static UserTaskResponse ToResponse(this UserTaskRecord x) => new(x.Id, x.ProjectId, x.ProjectName, x.Title, x.Status, x.Priority, x.DueDate);
    public static TaskStatusCountResponse ToResponse(this TaskStatusCountRecord x) => new(x.Status, x.TaskCount);
}
