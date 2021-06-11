using Irc.Infrastructure.Controller;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Horth.Service.Email.Controllers
{
    public class HomeController : IrcController
    {

        public HomeController(IConfiguration configuration):base(configuration)
        {
        }

        public IActionResult Index()
        {
            return new RedirectResult("~/swagger");
        }

    }
}
