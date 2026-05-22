using System.ComponentModel.DataAnnotations;
using assetlen.Service.DbServices;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Models.Models.ViewModels.ExportDtos;
using assetlen.Shared.Models.statics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace assetlen.API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize(Roles = $"{UserRoles.Contractor}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class SegmentsController : ControllerBase
	{
		private readonly ISegmentsDAL _segmentsDAL;

		public SegmentsController(ISegmentsDAL segmentsDAL)
		{
			_segmentsDAL = segmentsDAL;
		}


		[HttpPost]
		[ProducesResponseType(typeof(SegmentsDto), 200)]
		public async Task<ActionResult> AddSegment([FromBody] SegmentsDto s)
		{
			var result = await _segmentsDAL.AddSegment(s);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpDelete]
		public async Task<ActionResult> DeleteSegment(string segmentId)
		{
			var result = await _segmentsDAL.DeleteSegment(segmentId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return NoContent();
		}

		[HttpGet]
		[ProducesResponseType(typeof(PaginationDetails<ComboBoxDto>), 200)]
		public async Task<ActionResult> SearchSegmentsForComboBoxes([FromQuery] string? keywords, [FromQuery] int? offSet, [FromQuery] int? limit, [FromQuery] string? sortByColumn = null, [FromQuery] bool sortAscending = false, [FromQuery] CancellationToken cancellationToken = default)
		{
			int offset1 = offSet ?? 0;
			int limit1 = limit ?? 30;
			string keywords1 = keywords ?? string.Empty;

			var result = await _segmentsDAL.SearchSegmentsForComboBoxes(keywords1, offset1, limit1, sortByColumn, sortAscending, cancellationToken);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(int), 200)]
		[Authorize(Roles = $"{UserRoles.Contractor}, {UserRoles.Crew}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> GetSegmentIDBasedOnSegmentName(string segmentName)
		{
			var result = await _segmentsDAL.GetSegmentIDBasedOnSegmentName(segmentName);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(SegmentsDto), 200)]
		[Authorize(Roles = $"{UserRoles.Contractor}, {UserRoles.Crew}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> GetSegmentsBasedOnSegmentId(string segmentId)
		{
			var result = await _segmentsDAL.GetSegmentsBasedOnSegmentId(segmentId);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(PaginationDetails<SegmentsDto>), 200)]
		[Authorize(Roles = $"{UserRoles.Contractor}, {UserRoles.Crew}",
		AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public async Task<ActionResult> GetSegmentsFromDB([FromQuery] int? offSet, [FromQuery] int? limit, [FromQuery] string? sortByColumn = null, [FromQuery] bool sortAscending = false, [FromQuery] CancellationToken cancellationToken = default)
		{
			int offset1 = offSet ?? 0;
			int limit1 = limit ?? 30;

			var result = await _segmentsDAL.GetSegmentsFromDB(offset1, limit1, sortByColumn, sortAscending, cancellationToken);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpPut]
		[ProducesResponseType(typeof(SegmentsDto), 200)]
		public async Task<ActionResult> UpdateSegment(SegmentsDto s)
		{
			var result = await _segmentsDAL.UpdateSegment(s);

			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return Ok(result.Data);
		}

		[HttpGet]
		[ProducesResponseType(typeof(PaginationDetails<SegmentsDto>), 200)]
		public async Task<ActionResult> SearchSegmentsFromDB([FromQuery] string? keywords, [FromQuery] int? offSet, [FromQuery] int? limit, [FromQuery] string? sortByColumn = null, [FromQuery] bool sortAscending = false, [FromQuery] CancellationToken cancellationToken = default)
		{
			int offset1 = offSet ?? 0;
			int limit1 = limit ?? 30;
			string keywords1 = keywords ?? string.Empty;
			var result = await _segmentsDAL.SearchSegmentsFromDB(keywords1, offset1, limit1, sortByColumn, sortAscending, cancellationToken);
			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);
			return Ok(result.Data);
		}


		[HttpPost]
		[ProducesResponseType(typeof(FileResult), 200)]
		public async Task<ActionResult> GetSegmentsForCSVExportBySelectedFields([Required][FromBody] List<string> selectedColumnNames)
		{
			var result = await _segmentsDAL.GetSegmentsForCSVExportBySelectedFields(selectedColumnNames);
			if (!result.IsSuccess)
				return StatusCode(result.StatusCode, result.Error);

			return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CategoriesExport.xlsx");
		}

		[HttpPost]
		[ProducesResponseType(typeof(ImportResultSummary), 200)]
		public async Task<ActionResult> ImportSegmentsFromExcel([FromBody] ImportDataDto p, CancellationToken token)
		{
			// Extend the timeout to 300 seconds for this action
			using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
			cts.CancelAfter(TimeSpan.FromSeconds(300));

			try
			{
				var result = await _segmentsDAL.ImportSegmentsFromExcel(p);

				if (!result.IsSuccess)
					return StatusCode(result.StatusCode, result.Error);

				return Ok(result.Data);
			}
			catch (OperationCanceledException)
			{
				return StatusCode(408, "Request timed out.");
			}
		}

	}
}
