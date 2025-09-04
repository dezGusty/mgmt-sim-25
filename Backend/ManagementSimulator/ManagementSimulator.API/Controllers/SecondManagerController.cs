using ManagementSimulator.Core.Dtos.Requests.SecondManager;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagementSimulator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SecondManagerController : ControllerBase
    {
        private readonly ISecondManagerService _secondManagerService;

        public SecondManagerController(ISecondManagerService secondManagerService)
        {
            _secondManagerService = secondManagerService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<SecondManagerResponseDto>>> GetAllSecondManagers()
        {
            var secondManagers = await _secondManagerService.GetAllSecondManagersAsync();
            return Ok(secondManagers);
        }

        [HttpGet("debug-status/{userId}")]
        [Authorize]
        public async Task<ActionResult> GetDebugStatus(int userId)
        {
            var allSecondManagers = await _secondManagerService.GetAllSecondManagersAsync();
            var activeSecondManagers = await _secondManagerService.GetActiveSecondManagersAsync();
            var isActing = await _secondManagerService.IsUserActingAsSecondManagerAsync(userId);
            
            return Ok(new {
                AllSecondManagers = allSecondManagers.Count,
                ActiveSecondManagers = activeSecondManagers.Count,
                IsUserActingAsSecondManager = isActing,
                ActiveSecondManagersList = activeSecondManagers,
                UserId = userId,
                CurrentTime = DateTime.Now
            });
        }

        [HttpGet("active")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<SecondManagerResponseDto>>> GetActiveSecondManagers()
        {
            var activeSecondManagers = await _secondManagerService.GetActiveSecondManagersAsync();
            return Ok(activeSecondManagers);
        }

        [HttpGet("by-replaced-manager/{replacedManagerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<SecondManagerResponseDto>>> GetSecondManagersByReplacedManagerId(int replacedManagerId)
        {
            var secondManagers = await _secondManagerService.GetSecondManagersByReplacedManagerIdAsync(replacedManagerId);
            return Ok(secondManagers);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SecondManagerResponseDto>> CreateSecondManager([FromBody] CreateSecondManagerRequestDto request)
        {
            var secondManager = await _secondManagerService.CreateSecondManagerAsync(request);
            return Ok(new
            {
                Message = "Second manager created successfully",
                Data = secondManager,
                Success = true,
                Timestamp = DateTime.Now
            });
        }

        [HttpPut("{secondManagerEmployeeId}/{replacedManagerId}/{startDate:datetime}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SecondManagerResponseDto>> UpdateSecondManager(
            int secondManagerEmployeeId, 
            int replacedManagerId, 
            DateTime startDate, 
            [FromBody] UpdateSecondManagerRequestDto request)
        {
            var updatedSecondManager = await _secondManagerService.UpdateSecondManagerAsync(
                secondManagerEmployeeId, replacedManagerId, startDate, request);
            return Ok(new
            {
                Message = "Second manager updated successfully",
                Data = updatedSecondManager,
                Success = true,
                Timestamp = DateTime.Now
            });
        }

        [HttpDelete("{secondManagerEmployeeId}/{replacedManagerId}/{startDate:datetime}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteSecondManager(
            int secondManagerEmployeeId, 
            int replacedManagerId, 
            DateTime startDate)
        {
            await _secondManagerService.DeleteSecondManagerAsync(secondManagerEmployeeId, replacedManagerId, startDate);
            return Ok(new
            {
                Message = "Second manager deleted successfully",
                Success = true,
                Timestamp = DateTime.Now
            });
        }

        [HttpGet("is-acting-as-second-manager/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<bool>> IsUserActingAsSecondManager(int userId)
        {
            var isActing = await _secondManagerService.IsUserActingAsSecondManagerAsync(userId);
            return Ok(isActing);
        }

        [HttpGet("managers-replaced-by/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<int>>> GetManagersBeingReplacedByUser(int userId)
        {
            var managerIds = await _secondManagerService.GetManagersBeingReplacedByUserAsync(userId);
            return Ok(managerIds);
        }
    }
} 