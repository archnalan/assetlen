using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using assetlen.Shared.Models.statics;

namespace assetlen.API.Controllers;

/// <summary>
/// Peter's search (plan.md P6): one question across every project he stands on,
/// answered grouped by the thing each result is. Every role reaches the
/// controller; standing on each project decides what comes back.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew},{UserRoles.Client},{UserRoles.Guest}",
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class SearchController : ControllerBase
{
    private readonly ISearchDAL _dal;
    private readonly ITenantProvider _tenant;

    public SearchController(ISearchDAL dal, ITenantProvider tenant)
    {
        _dal = dal;
        _tenant = tenant;
    }

    [HttpGet]
    [ProducesResponseType(typeof(SearchResultDto), 200)]
    public async Task<ActionResult> Query([FromQuery] string? q, [FromQuery] string? projectId, [FromQuery] int take = 20, CancellationToken ct = default)
    {
        var result = await _dal.SearchAsync(q, projectId, take, _tenant.GetUserId(), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error.Message);
    }
}
