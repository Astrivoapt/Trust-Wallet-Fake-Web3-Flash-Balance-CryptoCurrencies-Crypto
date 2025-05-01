using Asp.Versioning;
using Dfe.Complete.Application.Projects.Queries.Csv;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Text;

namespace Dfe.Complete.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class CsvExportController(ISender sender) : ControllerBase
    {
        [HttpPost]
        [SwaggerResponse(200, "File", typeof(FileContentResult))]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        public async Task<IActionResult> GetConversionCsvByMonthAsync([FromQuery] GetConversionCsvByMonthQuery request, CancellationToken cancellationToken)
        {
            var fileContents = await sender.Send(request, cancellationToken);
            if (fileContents.IsSuccess == false)
            {
                return BadRequest(fileContents.Error);
            }
            byte[] bytes = Encoding.ASCII.GetBytes(fileContents.Value);

            return new FileContentResult(bytes, "application/octet-stream")
            {
                FileDownloadName = "filename.csv"
            };
         
        }

        [HttpPost]
        [Route("Contents")]
        [SwaggerResponse(200, "File contents", typeof(string))]
        [SwaggerResponse(400, "Invalid request data.")]
        public async Task<IActionResult> GetConversionCsvByMonthContentsAsync([FromQuery] GetConversionCsvByMonthQuery request, CancellationToken cancellationToken)
        {
            var fileContents = await sender.Send(request, cancellationToken);
            if (fileContents.IsSuccess == false)
            {
                return BadRequest(fileContents.Error);
            }

            return Ok(fileContents.Value);
        }
    }
}
