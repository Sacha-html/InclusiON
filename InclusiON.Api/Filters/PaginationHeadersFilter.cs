using InclusiON.DTOs.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace InclusiON.Api.Filters
{
    /// <summary>
    /// Result filter global: si la respuesta es ApiResponse&lt;PagedResponse&lt;T&gt;&gt;,
    /// escribe X-Total-Count y X-Total-Pages en los headers.
    /// Permite peticiones HEAD livianas para obtener KPIs sin cargar datos.
    /// </summary>
    public class PaginationHeadersFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(
            ResultExecutingContext context,
            ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult { Value: not null } objResult)
            {
                // ApiResponse<PagedResponse<T>>.Data es PagedResponse<T> que implementa IHasTotalCount.
                // Dos niveles: primero sacar .Data de ApiResponse, luego castear a IHasTotalCount.
                var inner = objResult.Value
                    .GetType()
                    .GetProperty("Data")
                    ?.GetValue(objResult.Value);

                if (inner is IHasTotalCount paged)
                {
                    var headers = context.HttpContext.Response.Headers;
                    headers["X-Total-Count"]       = paged.TotalRecords.ToString();
                    headers["X-Total-Pages"]       = paged.TotalPages.ToString();
                    headers["X-Current-Page"]      = paged.CurrentPage.ToString();
                }
            }

            await next();
        }
    }
}
