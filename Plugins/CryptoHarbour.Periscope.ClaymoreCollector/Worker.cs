using System;
using CryptoHarbour.Periscope.Contracts;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("CryptoHarbour.Periscope.ClaymoreCollector.Tests")]

namespace CryptoHarbour.Periscope.ClaymoreCollector
{
    public class Worker : IWorker
    {
        private Context _ctx;
        private IConfigurationRoot _cfg;

        public void Start(Context ctx)
        {
            _cfg = new ConfigurationBuilder()
                .AddJsonFile("Plugins\\CryptoHarbour.Periscope.ClaymoreCollector.json", optional: false, reloadOnChange: true)
               .Build();

            _ctx = ctx;
            ctx.Pulse += Ctx_Pulse;
        }

        internal virtual void Ctx_Pulse(DateTime dt)
        {
            var i = 0;
            var name = String.Empty;
            while (!string.IsNullOrEmpty(name = _cfg[$"Rigs:{i}:Name"]))
            {
                var ip = _cfg[$"Rigs:{i}:Ip"];
                var port = int.Parse(_cfg[$"Rigs:{i++}:Port"]);

                try
                {
                    var result = Collect(ip, port);
                    var log = string.Format(Constants.LogTemplate, "Claymore", dt.Ticks, 
                        $"\"Rig\":{{\"Name\": \"{name}\",\"Ip\": \"{ip}\",\"Port\": {port}}},\"Data\":{result}");
                    _ctx.Storage.Save(log);
                } catch (Exception ex)
                {
                    _ctx.Error.WriteLine($"Failed to read status from Claymore: {ex.ToString()}");
                }
            }
        }

        internal virtual string Collect(string ip, int port)
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), port);

            var result = string.Empty;
            using (Socket s =
                new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                s.Connect(ipe);
                var buf = Encoding.ASCII.GetBytes(@"{""id"":0,""jsonrpc"":""2.0"",""method"":""miner_getstat1""}");
                s.Send(buf, buf.Length, 0);

                var bytes = 0;
                byte[] bytesReceived = new Byte[1024];
                do
                {
                    bytes = s.Receive(bytesReceived, bytesReceived.Length, 0);
                    result = result + Encoding.ASCII.GetString(bytesReceived, 0, bytes);
                }
                while (bytes > 0);
            }
            return result;
        }
    }
}
