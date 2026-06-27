using Microsoft.AspNetCore.Mvc;

namespace IoTRecommendation.Web.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index() => View();
}
