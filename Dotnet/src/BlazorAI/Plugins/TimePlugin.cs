using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace BlazorAI.Plugins
{
    public class TimePlugin
    {
        private readonly IConfiguration _configuration;  // Not being used in this plugin, but here for examples
        public TimePlugin(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [KernelFunction("date_time_now")]
        [Description("Return the current Date and Time")]
        [return: Description("String containing the Date and Time with context")]
        public string CurrentDateTimeAsync()
        {
            return $"The current date and time is: {DateTime.Now.ToString()}";
        }

        [KernelFunction("get_year")]
        [Description("Return the Year for a date passed in as a parameter")]
        [return: Description("String containing the year with context")]
        public string GetYear(
            [Description("Date in a standard format (e.g., yyyy-MM-dd)")] string dateString)
        {
            if (DateTime.TryParse(dateString, out DateTime date))
            {
                return $"The year for date {dateString} is: {date.Year}";
            }
            throw new ArgumentException("Invalid date format provided");
        }

        [KernelFunction("get_month")]
        [Description("Return the Month for a date passed in as a parameter")]
        [return: Description("String containing the month with context")]
        public string GetMonth(
            [Description("Date in a standard format (e.g., yyyy-MM-dd)")] string dateString)
        {
            if (DateTime.TryParse(dateString, out DateTime date))
            {
                return $"The month for date {dateString} is: {date.Month} ({date.ToString("MMMM")})";
            }
            throw new ArgumentException("Invalid date format provided");
        }

        [KernelFunction("get_day_of_week")]
        [Description("Return the Day of Week for a date passed in as a parameter")]
        [return: Description("String representing the day of week with context")]
        public string GetDayOfWeek(
            [Description("Date in a standard format (e.g., yyyy-MM-dd)")] string dateString)
        {
            if (DateTime.TryParse(dateString, out DateTime date))
            {
                return $"The day of week for date {dateString} is: {date.DayOfWeek}";
            }
            throw new ArgumentException("Invalid date format provided");
        }
    }
}

