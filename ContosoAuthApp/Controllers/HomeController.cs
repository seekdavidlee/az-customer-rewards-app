using ContosoAuthApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ContosoAuthApp.Controllers
{
	[Authorize]
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly ITokenAcquisition _tokenAcquisition;
		private readonly IConfiguration _configuration;

		public HomeController(ILogger<HomeController> logger,
			ITokenAcquisition tokenAcquisition,
			IConfiguration configuration)
		{
			_logger = logger;
			_tokenAcquisition = tokenAcquisition;
			_configuration = configuration;
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[AuthorizeForScopes(Scopes = new[] { "api://contoso-cs-rewards-api/Points.List" })]
		public async Task<IActionResult> Test()
		{
			// Acquire the access token.
			string[] scopes = new string[] { "api://contoso-cs-rewards-api/Points.List" };
			var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);

			var baseUrl = _configuration["BaseUrl"];
			var url = $"{baseUrl}/WeatherForecast";

			using (var httpClientHandler = new HttpClientHandler())
			{
				// If you have SSL enabled, and you are using a self-signed SSL cert, you may need 
				// the following line to get pass any SSL validation errors.

				//httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
				//{
				//	return true;
				//};

				using (var client = new HttpClient(httpClientHandler))
				{
					client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
					var json = await client.GetStringAsync(url);

					ViewData["backendData"] = json;
					return View();
				}
			}
		}

		[AllowAnonymous]
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
