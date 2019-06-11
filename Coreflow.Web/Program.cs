﻿using Coreflow.Storage;
using Coreflow.Storage.ArgumentInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Coreflow.Web
{
    public class Program
    {
        public static Coreflow CoreflowInstance;


        static Program()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            //This ignores reference versions and other stuff. Only the filename will be compared!!
            string toFindName = new AssemblyName(args.Name).Name;

            if (toFindName.Contains(".resources"))
                return null;

            foreach (Assembly an in AppDomain.CurrentDomain.GetAssemblies())
                if (an.GetName().Name == toFindName)
                    return an;

            string path = Path.GetFullPath(Path.Combine("Libraries", toFindName + ".dll"));

            Console.WriteLine(path);

            if (File.Exists(path))
                return Assembly.LoadFile(path);

            return null;
        }



        public static void Main(string[] args)
        {

            Thread.Sleep(500);


            var configureNamedOptions = new ConfigureNamedOptions<ConsoleLoggerOptions>("", null);
            var optionsFactory = new OptionsFactory<ConsoleLoggerOptions>(new[] { configureNamedOptions }, Enumerable.Empty<IPostConfigureOptions<ConsoleLoggerOptions>>());
            var optionsMonitor = new OptionsMonitor<ConsoleLoggerOptions>(optionsFactory, Enumerable.Empty<IOptionsChangeTokenSource<ConsoleLoggerOptions>>(), new OptionsCache<ConsoleLoggerOptions>());
            var loggerFactory = new LoggerFactory(new[] { new ConsoleLoggerProvider(optionsMonitor) }, new LoggerFilterOptions { MinLevel = LogLevel.Trace });


            CoreflowInstance = new Coreflow(
                  new SimpleFlowDefinitionFileStorage(@"Flows"),
                //  new RepositoryFlowDefinitionStorage("http://localhost:5701/"),
                new SimpleFlowInstanceFileStorage("FlowInstances"),
                new JsonFileArgumentInjectionStore("Arguments.json"),
                "Plugins",
                loggerFactory
               );


            /*

            Thread flowThread = new Thread(() =>
            {
                Guid? identifier = CoreflowInstance.GetFlowIdentifier("init");

                if (identifier == null)
                {
                    Console.WriteLine("init flow not found!");
                    return;
                }

                try
                {
                    CoreflowInstance.CompileFlows();
                    CoreflowInstance.RunFlow(identifier.Value);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("Thread crashed!!!");
                }
            });

            flowThread.IsBackground = true;
            flowThread.Start();
            */

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://0.0.0.0:5700")
                .ConfigureKestrel((context, options) =>
                {
                    //       options.AllowSynchronousIO = true;
                });
    }
}
