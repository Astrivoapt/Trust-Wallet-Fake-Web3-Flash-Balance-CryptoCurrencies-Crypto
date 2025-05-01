using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Dfe.Complete.Application.Projects.Models;
using Dfe.Complete.Application.Projects.Queries.ListAllProjects;

namespace Dfe.Complete.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class TrustController(ISender sender) : ControllerBase
    {
        /// <summary>
        /// Returns a list of trusts with projects
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        //[Authorize(Policy = "API.Read")]
        [HttpGet]
        [Route("List/All")]
        [SwaggerResponse(200, "Trusts", typeof(List<ListTrustsWithProjectsResultModel>))]
        [SwaggerResponse(400, "Invalid request data.")]
        public async Task<IActionResult> ListAllTrustsWithProjectsAsync([FromQuery] ListAllTrustsWithProjectsQuery request, CancellationToken cancellationToken)
        {
            var trusts = await sender.Send(request, cancellationToken);
            return Ok(trusts.Value);
        }


    }
}
