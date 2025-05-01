using System.Web;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;

namespace Dfe.Complete.Api.Middleware
{
    public class UrlDecoderMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var queryString = context.Request.QueryString.ToString();
            var decodedQueryString = HttpUtility.UrlDecode(queryString);
            var newQuery = QueryHelpers.ParseQuery(decodedQueryString);
            var items = newQuery
                .SelectMany(x => x.Value, (col, value) => new KeyValuePair<string, string>(col.Key, value!)).ToList();
            var qb = new QueryBuilder(items);
            context.Request.QueryString = qb.ToQueryString();
            
            await next(context);
        }
    }
}
