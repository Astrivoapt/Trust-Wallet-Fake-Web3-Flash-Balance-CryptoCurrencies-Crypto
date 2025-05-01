using Asp.Versioning;
using Dfe.Complete.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Dfe.Complete.Application.Projects.Commands.CreateProject;
using Dfe.Complete.Application.Projects.Queries.CountAllProjects;
using Dfe.Complete.Application.Projects.Queries.GetProject;
using Dfe.Complete.Application.Projects.Queries.ListAllProjects;
using Dfe.Complete.Application.Projects.Models;
using Microsoft.AspNetCore.Authorization;
using Dfe.Complete.Application.Projects.Commands.RemoveProject;

namespace Dfe.Complete.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class ProjectsController(ISender sender) : ControllerBase
    {
        /// <summary>
        /// Creates a new Project
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        [Authorize(Policy = "CanReadWrite")]
        [HttpPost]
        [SwaggerResponse(201, "Project created successfully.", typeof(ProjectId))]
        [SwaggerResponse(400, "Invalid request data.")]
        public async Task<IActionResult> CreateProjectAsync([FromBody] CreateConversionProjectCommand request, CancellationToken cancellationToken)
        {
            var projectId = await sender.Send(request, cancellationToken);
            return Created("", projectId);
        }
        
        /// <summary>
        /// Gets a Project
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        [Authorize(Policy = "CanRead")]
        [HttpGet]
        [SwaggerResponse(200, "Project", typeof(ProjectDto))]
        [SwaggerResponse(400, "Invalid request data.")]
        public async Task<IActionResult> GetProjectAsync([FromQuery] GetProjectByUrnQuery request, CancellationToken cancellationToken)
        {
            var project = await sender.Send(request, cancellationToken);
            return Ok(project.Value);
        }
        
        /// <summary>
        /// Returns a list of Projects
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        [Authorize(Policy = "CanRead")]
        [HttpGet]
        [Route("List/All")]
        [SwaggerResponse(200, "Project", typeof(List<ListAllProjectsResultModel>))]
        [SwaggerResponse(400, "Invalid request data.")]
        public async Task<IActionResult> ListAllProjectsAsync([FromQuery] ListAllProjectsQuery request, CancellationToken cancellationToken)
        {
            var project = await sender.Send(request, cancellationToken);
            return Ok(project.Value);
        }
        
        /// <summary>
        /// Returns the number of Projects
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        [Authorize(Policy = "CanRead")]
        [HttpGet]
        [Route("Count/All")]
        [SwaggerResponse(200, "Project", typeof(int))]
        [SwaggerResponse(400, "Invalid request data.")]
        public async Task<IActionResult> CountAllProjectsAsync([FromQuery] CountAllProjectsQuery request, CancellationToken cancellationToken)
        {
            var project = await sender.Send(request, cancellationToken);
            return Ok(project.Value);
        }

        /// <summary>
        /// Gets the UKPRN for a group reference number.
        /// </summary>
        /// <param name="groupReferenceNumber">The group reference number.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        [Authorize(Policy = "CanRead")]
        [HttpGet("{groupReferenceNumber}/project_group")]
        [SwaggerResponse(200, "Project Group returned successfully.", typeof(ProjectGroupDto))]
        [SwaggerResponse(400, "Invalid group reference number.")]
        [SwaggerResponse(404, "Project Group not found for the given group reference number.")]
        public async Task<IActionResult> GetProjectGroupByGroupReferenceNumber_Async(string groupReferenceNumber, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(groupReferenceNumber))
            {
                return BadRequest("Group reference number is required.");
            }

            var request = new GetProjectGroupByGroupReferenceNumberQuery(groupReferenceNumber);
            var ukprn = await sender.Send(request, cancellationToken);

            return Ok(ukprn);
        }


        /// <summary>
        /// Removes project based on URN for test purposes.
        /// </summary>
        /// <param name="urn">Urn to remove.</param>
        [HttpDelete]
        [Authorize(Policy = "CanReadWriteUpdateDelete")]
        [SwaggerResponse(204, "Project Group returned successfully.")]
        public async Task<IActionResult> RemoveProject(Urn urn, CancellationToken cancellationToken)
        {
            if (urn == null)
            {
                return BadRequest("Urn is required.");
            }

            var request = new RemoveProjectCommand(urn);
            await sender.Send(request, cancellationToken);

            return NoContent();
        }
    }
}
