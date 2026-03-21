using DotnetEnterpriseApi.Application.Features.Tasks.Commands.CreateTask;
using DotnetEnterpriseApi.Application.Features.Tasks.Commands.DeleteTask;
using DotnetEnterpriseApi.Application.Features.Tasks.Commands.UpdateTask;
using DotnetEnterpriseApi.Application.Features.Tasks.Queries.GetAllTasks;
using DotnetEnterpriseApi.Application.Features.Tasks.Queries.GetTaskById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DotnetEnterpriseApi.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IOutputCacheStore _cacheStore;

        public TasksController(IMediator mediator, IOutputCacheStore cacheStore)
        {
            _mediator = mediator;
            _cacheStore = cacheStore;
        }

        [HttpGet]
        [OutputCache(PolicyName = "tasks")]
        public async Task<IActionResult> GetAllTasks([FromQuery] int? cursor = null, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllTasksQuery { Cursor = cursor, PageSize = pageSize };
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        [OutputCache(PolicyName = "tasks")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var query = new GetTaskByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return NotFound(new { message = result.Message, errors = result.Errors });
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            await _cacheStore.EvictByTagAsync("tasks", default);

            return CreatedAtAction(nameof(GetTaskById), new { id = result.Data?.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            await _cacheStore.EvictByTagAsync("tasks", default);

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var command = new DeleteTaskCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return NotFound(new { message = result.Message, errors = result.Errors });
            }

            await _cacheStore.EvictByTagAsync("tasks", default);

            return Ok(result);
        }
    }
}
