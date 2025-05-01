using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Dfe.Complete.Application.Users.Models;
using Dfe.Complete.Application.Users.Queries.GetUser;
using Dfe.Complete.Application.Users.Queries.ListAllUsers;

namespace Dfe.Complete.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class UsersController(ISender sender) : ControllerBase
    {
        
        /// <summary>
        /// Gets a User with their assigned projects
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        //[Authorize(Policy = "API.Read")]
        [HttpGet]
        [SwaggerResponse(200, "Project", typeof(UserWithProjectsDto))]
        [SwaggerResponse(400, "Invalid request data.")]
        public async Task<IActionResult> GetUserWithProjectsAsync([FromQuery] GetUserWithProjectsQuery request, CancellationToken cancellationToken)
        {
            var project = await sender.Send(request, cancellationToken);
            return Ok(project.Value);
        }
        
        /// <summary>
        /// Returns a list of Users with their assigned projects
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        //[Authorize(Policy = "API.Read")]
        [HttpGet]
        [Route("List/All")]
        [SwaggerResponse(200, "Project", typeof(List<UserWithProjectsDto>))]
        [SwaggerResponse(400, "Invalid request data.")]
        public async Task<IActionResult> ListAllUsersWithProjectsAsync([FromQuery] ListAllUsersWithProjectsQuery request, CancellationToken cancellationToken)
        {
            var project = await sender.Send(request, cancellationToken);
            return Ok(project.Value);
        }
    }
}
