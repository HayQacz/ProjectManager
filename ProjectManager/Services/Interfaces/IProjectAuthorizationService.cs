using System;
using System.Threading.Tasks;

namespace ProjectManager.Services.Interfaces
{
    public interface IProjectAuthorizationService
    {
        Task<bool> CanViewProject(Guid userId, Guid projectId);
        Task<bool> CanEditProject(Guid userId, Guid projectId);
        Task<bool> CanDeleteProject(Guid userId, Guid projectId);
        Task<bool> CanManageMembers(Guid userId, Guid projectId);
    }
}
