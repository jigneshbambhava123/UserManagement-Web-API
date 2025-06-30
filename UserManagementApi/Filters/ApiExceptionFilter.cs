using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UserManagementApi.Exceptions;

namespace UserManagementApi.Filters
{
    public class ApiExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ApiExceptionFilter> _logger;
        private readonly IWebHostEnvironment _env;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }   

        public void OnException(ExceptionContext context)
        {
            var ex = context.Exception;
            _logger.LogError(ex, "Unhandled exception occurred.");

            ObjectResult result;
            switch (ex)
            {
                case ValidationException ve:
                    result = new ObjectResult(new { Message = ve.Message })
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                    break;

                case NotFoundException nfe:
                    result = new ObjectResult(new { Message = nfe.Message })
                    {
                        StatusCode = StatusCodes.Status404NotFound
                    };
                    break;

                default:
                    var details = _env.IsDevelopment() ? ex.ToString() : null;
                    result = new ObjectResult(new {
                        Message = "An unexpected error occurred.",
                        Details = details
                    })
                    { 
                        StatusCode = StatusCodes.Status500InternalServerError 
                    };
                    break;
            }

            context.Result = result;
            context.ExceptionHandled = true;
        }
    }
}
