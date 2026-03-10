namespace MinimalPlayback.Controllers
{
    public static class TimelineMath
    {
        public static long CalculateTime(double mouseX, double width, double max)
        {
            if (width <= 0 || max <= 0)
                return 0;

            var ratio = mouseX / width;
            ratio = Math.Clamp(ratio, 0, 1);

            return (long)(ratio * max);
        }

        public static long SnapToSeconds(double value)
        {
            return (long)(Math.Round(value / 1000.0) * 1000);
        }
    }
}