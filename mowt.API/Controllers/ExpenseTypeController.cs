using mowt.Service.DbServices.ServiceInterfaces;
using mowt.Shared.Models.Models.ViewModels;
using mowt.Shared.Models.statics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace mowt.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = $"{UserRoles.LibraryModuleLogin}",
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ExpenseTypeController : ControllerBase
    {
        private readonly IExpenseTypeDAL _expenseTypeDAL;
        public ExpenseTypeController(IExpenseTypeDAL expenseTypeDAL)
        {
            _expenseTypeDAL = expenseTypeDAL;
        }
        [HttpGet]
        [ProducesResponseType(typeof(List<ExpenseTypeDto>), 200)]
        public async Task<IActionResult> GetExpenseTypes()
        {
            var result = await _expenseTypeDAL.GetExpenseTypes();
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ComboBoxDto>), 200)]
        public async Task<IActionResult> GetExpensesForComboBox()
        {
            var result = await _expenseTypeDAL.GetExpensesForComboBox();
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);
            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ExpenseTypeDto), 200)]
        public async Task<IActionResult> GetExpenseType(string typeId)
        {
            var result = await _expenseTypeDAL.GetExpenseType(typeId);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);
            return Ok(result.Data);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ExpenseTypeDto), 200)]
        public async Task<IActionResult> AddExpenseType([FromBody] ExpenseTypeDto expenseTypeDto)
        {
            var result = await _expenseTypeDAL.AddExpenseType(expenseTypeDto);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);
            return Ok(result.Data);
        }
        [HttpPost]
        [ProducesResponseType(typeof(List<ExpenseTypeDto>), 200)]
        public async Task<IActionResult> AddMultipleExpenseTypes([FromBody] List<ExpenseTypeDto> expenseTypeDtoList)
        {
            var result = await _expenseTypeDAL.AddMultipleExpenseTypes(expenseTypeDtoList);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);
            return Ok(result.Data);
        }

        [HttpPut]
        [ProducesResponseType(typeof(ExpenseTypeDto), 200)]
        public async Task<IActionResult> UpdateExpenseType([FromBody] ExpenseTypeDto expenseTypeDto)
        {
            var result = await _expenseTypeDAL.UpdateExpenseType(expenseTypeDto);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);
            return Ok(result.Data);
        }

        [HttpDelete]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<IActionResult> DeleteExpenseType(string typeId)
        {
            var result = await _expenseTypeDAL.DeleteExpenseType(typeId);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);
            return Ok(result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ComboBoxDto>), 200)]
        public async Task<IActionResult> SearchExpenseTypesForComboBoxes(string? searchText = "")
        {
            var result = await _expenseTypeDAL.SearchExpenseTypesForComboBoxes(searchText);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }
    }
}
