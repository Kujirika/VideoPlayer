using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalPlayback.Controllers
{
    public class TimelineState
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
    }
}