using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using assetlen.Shared.Models.statics;
using System.ComponentModel.DataAnnotations;

namespace assetlen.API.Controllers;

/// <summary>
/// Markup as a layer on the original (assetlen.md Law 2). Every role reaches
/// the controller; the file's visibility and the reader's standing decide the rest.
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Crew},{UserRoles.Client},{UserRoles.Guest}",
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AnnotationsController : ControllerBase
{
    private readonly IAnnotationDAL _dal;
    private readonly ITenantProvider _tenant;

    public AnnotationsController(IAnnotationDAL dal, ITenantProvider tenant)
    {
        _dal = dal;
        _tenant = tenant;
    }

    private ActionResult Answer<T>(ServiceResult<T> result) =>
        result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error.Message);

    [HttpGet]
    [ProducesResponseType(typeof(AnnotatedArtifactDto), 200)]
    public async Task<ActionResult> GetForArtifact([FromQuery][Required] string artifactId, CancellationToken ct)
        => Answer(await _dal.GetForArtifact(artifactId, _tenant.GetUserId(), ct));

    [HttpPost]
    [ProducesResponseType(typeof(AnnotationDto), 200)]
    public async Task<ActionResult> Save([FromBody] AnnotationSaveDto dto, CancellationToken ct)
        => Answer(await _dal.Save(dto, _tenant.GetUserId(), ct));

    /// <summary>Circle the thing, ask why. The query lands on the commitment.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AskResultDto), 200)]
    public async Task<ActionResult> Ask([FromBody] AnnotationAskDto dto, CancellationToken ct)
        => Answer(await _dal.Ask(dto, _tenant.GetUserId(), ct));

    [HttpPut]
    [ProducesResponseType(typeof(AnnotationDto), 200)]
    public async Task<ActionResult> Expose([FromBody] AnnotationExposeDto dto, CancellationToken ct)
        => Answer(await _dal.Expose(dto, _tenant.GetUserId(), ct));
}
