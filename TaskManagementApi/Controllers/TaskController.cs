using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Models;
using TaskManagementApi.Services;

namespace TaskManagementApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaskController(TaskSenderService taskService) : ControllerBase
    {
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateTaskArgs args)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await taskService.CreateAsync(args);

            return Ok();
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] UpdateTaskArgs args)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await taskService.UpdateAsync(args);

            return Ok();
        }


        [HttpGet("ShowAll")]
        public async Task<IActionResult> GetAll()
        {
            List<TaskView> result = await taskService.GetAllAsync();

            return Ok(result);
        }
    }
}
