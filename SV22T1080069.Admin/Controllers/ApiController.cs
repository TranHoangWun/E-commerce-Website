using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080069.DomainModels;

namespace SV22T1080069.Admin.Controllers
{
    [Authorize]
    public class ApiController : Controller
    {
        public async Task<IActionResult> Customer(int id)
        {
            var data = await BusinessLayers.CommonDataService.CustomerDB.GetAsync(id);
            if (data == null)
                return Json(new Customer());
            return Json(data);
        }
    }    
}
