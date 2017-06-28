using CryptoHarbour.Periscope.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace CryptoHarbour.Periscope.EthermineCollector.Tests
{
    [TestClass]
    public class SystemTests
    {
        [TestMethod]
        public void TestStatistics()
        {
            var stor = new Mock<IStorage>();
            var result = string.Empty;
            stor.Setup(s => s.Save(It.IsAny<string>())).Callback<string>((s) => {
                result = s;
            });
            var ctx = new Context
            {
                Storage = stor.Object,
                Error = Console.Error,
                Output = Console.Out
            };

            var wrkr = new Worker();
            wrkr.Start(ctx);

            wrkr.Ctx_Pulse(DateTime.Now);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
        }
    }
}
