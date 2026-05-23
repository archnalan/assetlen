using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;
using assetlen.Shared.Models.statics;
using System.ComponentModel.DataAnnotations;

namespace assetlen.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
// Finance access matrix: Contractor=Admin, Manager=Read, Client=Read,
// Guest+Crew=None. Crew authors Journal entries but never sees money.
[Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager},{UserRoles.Client}",
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class BudgetController : ControllerBase
{
    private readonly IBudgetDAL _dal;
    private readonly ITenantProvider _tenantProvider;

    public BudgetController(IBudgetDAL dal, ITenantProvider tenantProvider)
    {
        _dal = dal;
        _tenantProvider = tenantProvider;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ProjectBudgetSummaryDto), 200)]
    public async Task<ActionResult> GetSummary([FromQuery][Required] string projectId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _dal.GetSummary(projectId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BudgetLineItemDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> AddLineItem([FromBody] BudgetLineItemCreateDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _dal.AddLineItem(dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpPut]
    [ProducesResponseType(typeof(BudgetLineItemDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> UpdateLineItem([FromBody] BudgetLineItemUpdateDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _dal.UpdateLineItem(dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpDelete]
    [ProducesResponseType(typeof(bool), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> DeleteLineItem([FromQuery][Required] string lineItemId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _dal.DeleteLineItem(lineItemId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReceiptDto), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> AddReceipt([FromBody] ReceiptCreateDto dto)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _dal.AddReceipt(dto, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ReceiptDto>), 200)]
    public async Task<ActionResult> GetReceiptsByLineItem([FromQuery][Required] string lineItemId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _dal.GetReceiptsByLineItem(lineItemId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }

    [HttpDelete]
    [ProducesResponseType(typeof(bool), 200)]
    [Authorize(Roles = $"{UserRoles.Contractor},{UserRoles.Manager}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> DeleteReceipt([FromQuery][Required] string receiptId)
    {
        var userId = _tenantProvider.GetUserId();
        var result = await _dal.DeleteReceipt(receiptId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Error.Message);
        return Ok(result.Data);
    }
}
