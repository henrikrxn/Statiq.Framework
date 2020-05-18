﻿using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Testing
{
    public static class IBootstrapperExtensions
    {
        /// <summary>
        /// Runs tests on the bootstrapper with the specified file system and reports the results.
        /// It's advisable not to run bootstrapper tests in parallel (it'll work, but lock contention
        /// will result in very slow tests).
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <param name="fileProvider">The file provider to use.</param>
        /// <returns>Results from running the bootstrapper such as phase outputs and exit code.</returns>
        public static async Task<BootstrapperTestResult> RunTestAsync(this IBootstrapper bootstrapper, IFileProvider fileProvider = null) =>
            await bootstrapper.RunTestAsync(LogLevel.Warning, fileProvider);

        /// <summary>
        /// Runs tests on the bootstrapper with the specified file system and reports the results.
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <param name="throwLogLevel">The log level at which to throw.</param>
        /// <param name="fileProvider">The file provider to use.</param>
        /// <returns>Results from running the bootstrapper such as phase outputs and exit code.</returns>
        public static async Task<BootstrapperTestResult> RunTestAsync(this IBootstrapper bootstrapper, LogLevel throwLogLevel, IFileProvider fileProvider = null)
        {
            BootstrapperTestResult results = new BootstrapperTestResult();

            // Prevent disposal by the console log provider and instrument with a test logger
            TestLoggerProvider loggerProvider = new TestLoggerProvider
            {
                ThrowLogLevel = throwLogLevel
            };
            bootstrapper.ConfigureServices(services =>
            {
                services.RemoveAll<ILoggerProvider>();  // The console logger isn't friendly to tests
                services.AddSingleton<ILoggerProvider>(loggerProvider);
            });

            // Instrument the Bootstrapper
            GatherDocuments phaseInputs = new GatherDocuments();
            GatherDocuments phaseOutputs = new GatherDocuments();
            bootstrapper.ConfigureEngine(engine =>
            {
                results.Engine = engine;
                foreach (IPipeline pipeline in engine.Pipelines.Values)
                {
                    pipeline.InputModules?.Insert(0, phaseInputs);
                    pipeline.InputModules?.Add(phaseOutputs);
                    pipeline.ProcessModules?.Insert(0, phaseInputs);
                    pipeline.ProcessModules?.Add(phaseOutputs);
                    pipeline.PostProcessModules?.Insert(0, phaseInputs);
                    pipeline.PostProcessModules?.Add(phaseOutputs);
                    pipeline.OutputModules?.Insert(0, phaseInputs);
                    pipeline.OutputModules?.Add(phaseOutputs);
                }
                if (fileProvider != null)
                {
                    engine.FileSystem.RootPath = "/";
                    engine.FileSystem.FileProvider = fileProvider;
                }
            });

            // Return the results
            results.LogMessages = loggerProvider.Messages;
            results.ExitCode = await bootstrapper.RunAsync();
            results.Inputs = phaseInputs.Documents;
            results.Outputs = phaseOutputs.Documents;
            return results;
        }
    }
}
