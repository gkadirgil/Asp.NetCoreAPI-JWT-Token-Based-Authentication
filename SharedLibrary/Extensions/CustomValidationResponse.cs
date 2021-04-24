using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Extensions
{
    public static class CustomValidationResponse
    {
        //Extention Methods is wrtitten as static methods.

        public static void UseCustomValidationResponse(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState.Values.Where(x => x.Errors.Count > 0).SelectMany(s => s.Errors).Select(s => s.ErrorMessage);

                    ErrorDTO errorDTO = new ErrorDTO(errors.ToList(), true);

                    var response = Response<NoContentResult>.Fail(errorDTO, 400);

                    return new BadRequestObjectResult(response);
                };
            });
        }
    }
}
