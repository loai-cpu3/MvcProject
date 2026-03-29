namespace MvcProject.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToRelativeTime(this DateTime dateTime)
        {
            var timespan = DateTime.UtcNow - dateTime.ToUniversalTime();

            if (timespan.TotalSeconds < 60) return "just now";
            if (timespan.TotalMinutes < 2) return "1 minute ago";
            if (timespan.TotalMinutes < 60) return $"{Math.Floor(timespan.TotalMinutes)} minutes ago";
            if (timespan.TotalHours < 2) return "1 hour ago";
            if (timespan.TotalHours < 24) return $"{Math.Floor(timespan.TotalHours)} hours ago";
            if (timespan.TotalDays < 2) return "yesterday";
            if (timespan.TotalDays < 30) return $"{Math.Floor(timespan.TotalDays)} days ago";
            if (timespan.TotalDays < 60) return "1 month ago";
            if (timespan.TotalDays < 365) return $"{Math.Floor(timespan.TotalDays / 30)} months ago";
            
            return $"{Math.Floor(timespan.TotalDays / 365)} years ago";
        }
    }
}
