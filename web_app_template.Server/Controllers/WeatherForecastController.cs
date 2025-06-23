using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using web_app_template.Data.Repositories;
using web_app_template.Domain.Enums;
using web_app_template.Domain.Helpers;
using web_app_template.Domain.Interfaces;
using web_app_template.Domain.Models;
using web_app_template.Domain.Models.Entities;

namespace web_app_template.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IStringLocalizer<WeatherForecastController> _localizer;
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IGenericInterface _genericService;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public WeatherForecastController(IStringLocalizer<WeatherForecastController> localizer, ILogger<WeatherForecastController> logger, GenericRepository repository, IGenericInterface genericService)
        {
            _localizer = localizer;
            _logger = logger;
            _genericService = genericService;
        }

        [HttpGet("GetWeatherForecast")]
        public ActionResult<ApiResponse<List<WeatherForecast>>> Get()
        {
            string helloworldMessage = _localizer["HelloWorld"];
            var weatherForecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToList();

            ApiResponse<List<WeatherForecast>> response = new ApiResponse<List<WeatherForecast>>
            {
                Message = ResponseStatusCodes.Ok,
                Data = weatherForecasts
            };

            return response;
        }

        [HttpGet("GetPokemons")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<Pokemon>>>> GetPokemons()
        {
            ApiResponse<PaginatedResult<Pokemon>> response = new ApiResponse<PaginatedResult<Pokemon>>();

            var result = await _genericService.FilterPaginatedAsync<Pokemon, Pokemon>("Owner", "Ash Ketchum", 1, 2);

            response.Message = ResponseStatusCodes.Ok;
            response.Data = new PaginatedResult<Pokemon>
            {
                Items = result.Items,
                TotalCount = result.TotalCount
            };

            return response;
        }

        [HttpPost("UploadFile")]
        public async Task<ActionResult<ApiResponse<string>>> UploadFile(IFormFile file)
        {
            string directory = "testfiles";
            var stream = file.OpenReadStream();
            var url = await FirebaseHelper.UploadFile(stream, file.FileName, directory);
            return new ApiResponse<string>
            {
                Message = ResponseStatusCodes.Ok,
                Data = url
            };
        }
    }
}
