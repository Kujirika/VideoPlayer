namespace MinimalPlayback.Controllers
{
    public class TimelineController
    {
        public bool IsDragging { get; private set; }

        public void StartDrag()
        {
            IsDragging = true;
        }

        public void EndDrag()
        {
            IsDragging = false;
        }

        public long CalculateTime(double mouseX, double width, double max)
        {
            if (width <= 0 || max <= 0)
                return 0;

            var ratio = mouseX / width;
            ratio = Math.Clamp(ratio, 0, 1);

            return (long)(ratio * max);
        }

        public long SnapToSeconds(double value)
        {
            return (long)(Math.Round(value / 1000.0) * 1000);
        }
    }
}