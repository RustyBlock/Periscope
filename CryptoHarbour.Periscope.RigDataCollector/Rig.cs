using CryptoHarbour.Periscope.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoHarbour.Periscope.RigDataCollector
{
    internal static class Rig
    {
        internal static Dictionary<string, IWorker> Collectors = new Dictionary<string, IWorker>();
    }
}
