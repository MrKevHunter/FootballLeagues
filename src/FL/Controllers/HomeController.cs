using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FL.Commands;
using MediatR;
using Microsoft.AspNet.Mvc;

namespace FL.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMediator _mediator;

        public HomeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            var enumerable = _mediator.Send(new AreaCommand());
            enumerable.ToList();
            return View();
        }

        public IActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
