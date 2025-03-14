using Microsoft.SemanticKernel;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace BlazorAI.Plugins
{
    public class WeatherPlugin
    {
        private readonly IConfiguration _configuration;  // Not being used in this plugin, but here for examples
        private readonly IHttpClientFactory _httpClientFactory;
        public WeatherPlugin(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;

        }

        [KernelFunction("get_weather_lat_long_16_days")]
        [Description("Get the forecast weather at lat / long location for up to 16 days in the future")]
        [return: Description("String containing the weather forecast for up to 16 days in future")]
        public async Task<string> GetWeatherForLatLong16Days(string latitude, string longitude, string daysInFuture)
        {
            var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current=temperature_2m,relative_humidity_2m,apparent_temperature,precipitation,rain,showers,snowfall,weather_code,wind_speed_10m,wind_direction_10m,wind_gusts_10m&hourly=temperature_2m,relative_humidity_2m,apparent_temperature,precipitation_probability,precipitation,rain,showers,snowfall,weather_code,cloud_cover,wind_speed_10m,uv_index&temperature_unit=fahrenheit&wind_speed_unit=mph&precipitation_unit=inch&forecast_days={daysInFuture}";
            using HttpClient httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetStringAsync(url);
            return response;
        }

        [KernelFunction("get_weather_lat_long_duration")]
        [Description("Get the forecast weather at lat / long location for for a number of days in past")]
        [return: Description("String containing the weather forecast for a number of days in past")]
        public async Task<string> GetWeatherForLatLongNumberOfPastDays(string latitude, string longitude, string numberOfDaysInPast)
        {
            var url = @$"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&daily=weather_code,temperature_2m_max,temperature_2m_min,apparent_temperature_max,apparent_temperature_min,sunrise,sunset,daylight_duration,uv_index_max,precipitation_sum,rain_sum,showers_sum,snowfall_sum,precipitation_hours,wind_speed_10m_max,wind_gusts_10m_max&temperature_unit=fahrenheit&wind_speed_unit=mph&precipitation_unit=inch&past_days={numberOfDaysInPast}";
            using HttpClient httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetStringAsync(url);
            return response;
        }
    }
}

