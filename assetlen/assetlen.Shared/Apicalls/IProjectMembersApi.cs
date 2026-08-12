using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using Refit;

namespace assetlen.Shared.Apicalls;

public interface IProjectMembersApi
{
    [Post("/api/ProjectMembers/AddMember")]
    Task<IApiResponse<ProjectMemberDto>> AddMember([Body] ProjectMemberCreateDto dto);

    [Get("/api/ProjectMembers/GetMembersByProject")]
    Task<IApiResponse<List<ProjectMemberDto>>> GetMembersByProject([Query] string projectId);

    /// <summary>Move a member across sides, or appoint / stand down a mediator.</summary>
    [Put("/api/ProjectMembers/UpdateMember")]
    Task<IApiResponse<ProjectMemberDto>> UpdateMember([Body] ProjectMemberUpdateDto dto);

    /// <summary>The caller's own standing — which surface to render.</summary>
    [Get("/api/ProjectMembers/GetMyStanding")]
    Task<IApiResponse<ProjectAccessDto>> GetMyStanding([Query] string projectId);

    [Delete("/api/ProjectMembers/DeactivateMember")]
    Task<IApiResponse<bool>> DeactivateMember([Query] string memberId);
}
