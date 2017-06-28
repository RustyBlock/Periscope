using Microsoft.VisualStudio.TestTools.UnitTesting;
using CryptoHarbour.Periscope.Contracts;
using Moq;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;
using System;

namespace CryptoHarbour.Periscope.ClaymoreCollector.Tests
{
    [TestClass]
    public class ClaymoreCollectorTests
    {
        private const string _testIp = "127.0.0.1";
        private const int _testPort = 3333;

        class Ctx : Context
        {
            public override event PulseEvent Pulse;

            public int GetEventCount()
            {
                return Pulse.GetInvocationList().Length;
            }
        }

        [TestMethod]
        public void TestStart()
        {
            var wrkr = new Worker();
            var ctx = new Ctx();
            wrkr.Start(ctx);
            Assert.AreEqual(ctx.GetEventCount(), 1);
        }

        [TestMethod]
        public void TestCollect()
        {
            var srvr = new TcpListener(IPAddress.Parse(_testIp), _testPort);
            srvr.Start();
            srvr.AcceptTcpClientAsync().ContinueWith((t) =>
            {
                var client = t.Result;
                byte[] bytes = new byte[256];
                string data = null;

                using (var stream = client.GetStream())
                {
                    var i = stream.Read(bytes, 0, bytes.Length);
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    Assert.IsTrue(data.Contains("miner_getstat"));

                    byte[] msg = System.Text.Encoding.ASCII.GetBytes("test");
                    stream.Write(msg, 0, msg.Length);
                }
            });

            var wrkr = new Worker();
            var result = wrkr.Collect(_testIp, _testPort);
            Assert.AreEqual(result, "test");
        }

        public class Wrkr : Worker
        {
            internal override string Collect(string ip, int port)
            {
                return "{ \"test\": \"data\"}";
            }
        }

        [TestMethod]
        public void TestPulse()
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

            var wrkr = new Wrkr();
            wrkr.Start(ctx);
            ctx.TriggerPulse();

            var json = JsonConvert.DeserializeObject<dynamic>(result);

            Assert.AreEqual(json.Data.test.ToString(), "data");
        }
    }
}
