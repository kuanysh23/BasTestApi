using Microsoft.AspNetCore.Mvc;

namespace BasTestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{
    private readonly TaskQueueService _taskQueueService;

    public TaskController(TaskQueueService taskQueueService)
    {
        _taskQueueService = taskQueueService;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] TaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message) || request.Message.Length > 100)
        {
            return BadRequest("Data field is required and should be less than or equal to 100 characters.");
        }

        var response = await _taskQueueService.EnqueueTaskAsync(request);
        return Ok(response);
    }
}
