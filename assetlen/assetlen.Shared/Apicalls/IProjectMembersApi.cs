using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Refit;

namespace assetlen.Shared.Apicalls;

public interface IProjectMembersApi
{
    [Post("/api/ProjectMembers/AddMember")]
    Task<IApiResponse<ProjectMemberDto>> AddMember([Body] ProjectMemberCreateDto dto);

    [Get("/api/ProjectMembers/GetMembersByProject")]
    Task<IApiResponse<List<ProjectMemberDto>>> GetMembersByProject([Query] string projectId);

    [Delete("/api/ProjectMembers/DeactivateMember")]
    Task<IApiResponse<bool>> DeactivateMember([Query] string memberId);
}
