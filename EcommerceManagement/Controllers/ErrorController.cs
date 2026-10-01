using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceManagement.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode:int}")]
        public IActionResult Index(int statusCode)
        {
            Response.StatusCode = statusCode;

            return statusCode switch
            {
                403 => View("403"),
                404 => View("404"),
                500 => View("500"),
                _ => View("500")
            };
        }
    }
}