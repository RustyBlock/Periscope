using CryptoHarbour.Periscope.Contracts;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;

namespace CryptoHarbour.Periscope.RigDataCollector
{
    class Program
    {
        private static readonly string _pluginsPath = $"{Path.GetDirectoryName(typeof(Program).GetTypeInfo().Assembly.Location)}\\Plugins";
        private static Timer _timer;

        static void Main(string[] args)
        {
            var cfg = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .Build();

            var ctx = new Context
            {
                Output = Console.Out,
                Error = Console.Error,
                Storage = new MongoDbStorage("farm", "collection", cfg["MongoDbConnection"])
            };

            AssemblyLoadContext.Default.Resolving += Default_Resolving;

            var i = 0;
            var name = String.Empty;
            while (!string.IsNullOrEmpty(name = cfg[$"Plugins:{i}:name"]))
            {
                var typeName = cfg[$"plugins:{i++}:type"];
                AddWorker(name, typeName, ctx, (wrkr) => {
                    wrkr.Start(ctx);
                    Rig.Collectors.Add(name, wrkr);
                });
            }

            StartPulse(ctx, int.Parse(cfg["PulseIntervalSeconds"]));

            while(true)
            {
                Console.ReadLine();
            }
        }

        private static Assembly Default_Resolving(AssemblyLoadContext arg1, AssemblyName arg2)
        {
            Console.WriteLine($"Trying to resolve: {arg2.FullName}");
            return arg1.LoadFromAssemblyPath($"{_pluginsPath}\\{arg2.Name}.dll");
        }

        internal static void AddWorker(string workerName, string workerType, Context ctx, Action<IWorker> registerWorker)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workerType))
                {
                    ctx.Error.WriteLine($"No type specified for {workerName} plugin");
                    return;
                }
                var typeAndAsm = workerType.Split(",".ToCharArray());
                var asmPath = $"{_pluginsPath}\\{typeAndAsm[1].Trim()}.dll";

                if (!File.Exists(asmPath))
                {
                    ctx.Error.WriteLine($"Assembly not found for {workerName} plugin");
                    return;
                }

                ctx.Output.WriteLine($"Loading worker assembly from {typeAndAsm[1]}");
                //var loader = new AssemblyLoader(path);
                var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(asmPath);

                ctx.Output.WriteLine($"Getting worker type information for {typeAndAsm[0]}");
                var type = asm.GetType(typeAndAsm[0].Trim());
                var wrkr = (IWorker)Activator.CreateInstance(type);

                registerWorker(wrkr);
            } catch(Exception ex)
            {
                ctx.Error.WriteLine($"Failed to load worker {workerName}: {ex.ToString()}");
            }
        }

        internal static void StartPulse(Context ctx, int interval)
        {
            _timer = new Timer((obj) => {
                try
                {
                    ctx.TriggerPulse();
                } catch(Exception ex)
                {
                    ctx.Error.WriteLine($"Exception in pulse event: {ex.ToString()}");
                }
            }, null, 0, interval * 1000);
        }
    }
}