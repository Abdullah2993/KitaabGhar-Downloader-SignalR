using System;

namespace Kitaabghar
{
    public class StatusChangedEventHandler:EventArgs
    {
        public string Status { get; private set; }

        public StatusChangedEventHandler(string status)
        {
            Status = status;
        }
    }
}