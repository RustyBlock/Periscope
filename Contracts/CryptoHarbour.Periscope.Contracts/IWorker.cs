using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoHarbour.Periscope.Contracts
{
    public interface IWorker
    {
        void Start(Context ctx);
    }
}
