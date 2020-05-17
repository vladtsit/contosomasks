using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ContosoMasks.ServiceHost.Models;

namespace ContosoMasks.ServiceHost.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index(bool frontdoor = false, bool cdn = false)
        {
            if ( frontdoor )
            {
                string url = string.IsNullOrEmpty(SiteConfiguration.FrontDoorURL) ? "/" : SiteConfiguration.FrontDoorURL;
                if ( cdn )
                { 
                    return Redirect(url + "?cdn=true");
                }
                else
                {
                    return Redirect(url);
                }
            }

            string cdnEndPoint = SiteConfiguration.StaticAssetRoot;

            if ( string.IsNullOrEmpty(cdnEndPoint))
            {
                cdnEndPoint = "/";
            }
            else
            {
                cdnEndPoint = cdnEndPoint.TrimEnd('/') + "/";
            }

            this.Response.Headers.Add("X-ContosoMasks-StaticEndpoint", cdnEndPoint);

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Message()
        {
            string cdnEndPoint = SiteConfiguration.StaticAssetRoot;

            if (string.IsNullOrEmpty(cdnEndPoint))
            {
                cdnEndPoint = "/";
            }
            else
            {
                cdnEndPoint = cdnEndPoint.TrimEnd('/') + "/";
            }

            this.Response.Headers.Add("X-ContosoMasks-StaticEndpoint", cdnEndPoint);


            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
