using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using InclusiON.DTOs.Responses;

namespace InclusiON.Api.Filters
{
    /// <summary>
    /// Filtro que centraliza la validacion de ModelState para todos los controllers.
    /// Retorna 400 BadRequest con ApiResponse estandarizado si el modelo es invalido.
    /// </summary>
    public class ValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid)
                return;

            var fieldErrors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            context.Result = new BadRequestObjectResult(
                ApiResponse<object>.ValidationError(fieldErrors));
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
