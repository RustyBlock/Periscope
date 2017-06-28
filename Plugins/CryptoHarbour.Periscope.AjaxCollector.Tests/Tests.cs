using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CryptoHarbour.Periscope.Contracts;

namespace CryptoHarbour.Periscope.AjaxCollector.Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void TestLogin()
        {
            var wrkr = new Worker();
            wrkr.Start(new Context
            {
                
            });
            wrkr.Ctx_Pulse(DateTime.Now);
        }
    }
}
