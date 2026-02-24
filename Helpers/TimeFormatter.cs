namespace MinimalPlayback.Helpers
{
    public static class TimeFormatter
    {
        public static string Format(long ms)
        {
            var time = TimeSpan.FromMilliseconds(ms);
            return time.Hours > 0
                ? time.ToString(@"hh\:mm\:ss")
                : time.ToString(@"mm\:ss");
        }
    }
}