using System;
using CryptoHarbour.Periscope.Contracts;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("CryptoHarbour.Periscope.EthermineCollector.Tests")]

namespace CryptoHarbour.Periscope.EthermineCollector
{
    public class Worker : IWorker
    {
        private Context _ctx;
        private IConfigurationRoot _cfg;

        public void Start(Context ctx)
        {
            _cfg = new ConfigurationBuilder()
                .AddJsonFile("Plugins\\CryptoHarbour.Periscope.EthermineCollector.json", optional: false, reloadOnChange: true)
               .Build();

            _ctx = ctx;
            ctx.Pulse += Ctx_Pulse;
        }

        internal virtual void Ctx_Pulse(DateTime dt)
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", "RustyLock Periscope");

                var fullStats = client.GetStringAsync($"https://ethermine.org/api/miner_new/{_cfg["PayoutAddress"]}").Result;

                var log = string.Format(Constants.LogTemplate, "Ethermine.org", dt.Ticks,
                    $"\"Data\":{fullStats}");

                _ctx.Storage.Save(log);
            }
            catch (Exception ex)
            {
                _ctx.Error.WriteLine($"Failed to read status from Claymore: {ex.ToString()}");
            }
        }
    }
}
