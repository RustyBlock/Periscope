using System;
using System.IO;

namespace CryptoHarbour.Periscope.Contracts
{
    public delegate void PulseEvent(DateTime timestamp);

    public class Context
    {
        public virtual TextWriter Output { get; set; }
        public virtual TextWriter Error { get; set; }
        public virtual IStorage Storage { get; set; }

        public virtual event PulseEvent Pulse;

        public virtual void TriggerPulse()
        {
            var dt = DateTime.Now; // get common timestamp for all event handlers
            Pulse?.Invoke(dt);
        }
    }
}
