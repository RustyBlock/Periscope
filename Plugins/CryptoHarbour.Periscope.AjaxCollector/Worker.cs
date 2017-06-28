using CryptoHarbour.Periscope.Contracts;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.PhantomJS;
using System;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("CryptoHarbour.Periscope.AjaxCollector.Tests")]

namespace CryptoHarbour.Periscope.AjaxCollector
{
    public class Worker : IWorker
    {
        private IWebDriver _web;
        private Context _ctx;
        private IConfigurationRoot _cfg;

        public void Start(Context ctx)
        {
            _cfg = new ConfigurationBuilder()
                .AddJsonFile("Plugins\\CryptoHarbour.Periscope.AjaxCollector.json", optional: false, reloadOnChange: true)
               .Build();

            switch(_cfg["Browser"].ToLower())
            {
                case "chrome":
                    _web = new ChromeDriver();
                    break;
                case "ie":
                    _web = new InternetExplorerDriver();
                    break;
                case "firefox":
                    _web = new FirefoxDriver();
                    break;
                case "phantomjs":
                    _web = new PhantomJSDriver();
                    break;
                default:
                    throw new Exception($"{_cfg["Browser"]} not supported");
            }

            _web.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
            Login();

            _ctx = ctx;
            ctx.Pulse += Ctx_Pulse;
        }

        internal void Ctx_Pulse(DateTime timestamp)
        {
            // find block with list of devices and select the target one
            var dev = (from d in _web.FindElements(By.TagName("h5"))
                where string.Equals(d.Text, _cfg["DeviceName"], StringComparison.CurrentCultureIgnoreCase)
                select d).FirstOrDefault();
            if(dev == null)
            {
                return;
            }
            dev.Click();

            // read device information
            var parms = _web.FindElements(By.CssSelector("device-params>article"));
            var res = new StringBuilder("\"Device\":{");
            var first = true;
            foreach(var p in parms)
            {
                if(!first)
                {
                    res.Append(",");
                } else
                {
                    first = false;
                }
                var name = p.FindElement(By.CssSelector("div.indicators-name")).Text;
                var value = p.FindElement(By.CssSelector("div.indicators-value")).Text;
                res.Append($"\"{name}\":\"{value}\"");
            }
            res.Append("}");

            // store json snapshot
            var log = string.Format(Constants.LogTemplate, "Ajax", timestamp.Ticks, res);
            _ctx.Storage.Save(log);
        }

        internal void Login()
        {
            _web.Navigate().GoToUrl(_cfg["DeviceWebAddress"]);
            _web.FindElement(By.CssSelector("input[placeholder='Email']"))
                .SendKeys(_cfg["UserEmail"]);
            _web.FindElement(By.CssSelector("input[placeholder='Password']"))
                .SendKeys(_cfg["UserPassword"]);
            _web.FindElement(By.CssSelector("button.default-blue"))
                .Click();
            _web.FindElement(By.CssSelector("section.device-list-col>article.device-item"));
        }
    }
}
