﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Execution
{
    [TestFixture]
    public class EngineFixture : BaseFixture
    {
        public class GetTriggeredPipelines : EngineFixture
        {
            [Test]
            public void GetsAllForNull()
            {
                // Given
                Engine engine = GetEngine();

                // When
                HashSet<string> triggeredPipelines = engine.GetTriggeredPipelines(null);

                // Then
                triggeredPipelines.ShouldBe(new[] { "A", "D", "E", "F" }, true);
            }

            [Test]
            public void GetsAlwaysForZeroLength()
            {
                // Given
                Engine engine = GetEngine();

                // When
                HashSet<string> triggeredPipelines = engine.GetTriggeredPipelines(Array.Empty<string>());

                // Then
                triggeredPipelines.ShouldBe(new[] { "F" }, true);
            }

            [Test]
            public void GetsSpecifiedPipelines()
            {
                // Given
                Engine engine = GetEngine();

                // When
                HashSet<string> triggeredPipelines = engine.GetTriggeredPipelines(new[] { "A", "B" });

                // Then
                triggeredPipelines.ShouldBe(new[] { "A", "B", "F" }, true);
            }

            [Test]
            public void GetsTransativeDependency()
            {
                // Given
                Engine engine = GetEngine();

                // When
                HashSet<string> triggeredPipelines = engine.GetTriggeredPipelines(new[] { "E" });

                // Then
                triggeredPipelines.ShouldBe(new[] { "A", "D", "E", "F" }, true);
            }

            [Test]
            public void ThrowsForUndefinedPipeline()
            {
                // Given
                Engine engine = GetEngine();

                // When, Then
                Should.Throw<ArgumentException>(() => engine.GetTriggeredPipelines(new[] { "Z" }));
            }

            private Engine GetEngine()
            {
                Engine engine = new Engine();
                engine.Pipelines.Add("A", new TestPipeline
                {
                    Trigger = PipelineTrigger.Default
                });
                engine.Pipelines.Add("B", new TestPipeline
                {
                    Trigger = PipelineTrigger.Manual
                });
                engine.Pipelines.Add("C", new TestPipeline
                {
                    Trigger = PipelineTrigger.Dependency
                });
                engine.Pipelines.Add("D", new TestPipeline
                {
                    Trigger = PipelineTrigger.Dependency,
                    Dependencies = new HashSet<string>(new[] { "A" })
                });
                engine.Pipelines.Add("E", new TestPipeline
                {
                    Trigger = PipelineTrigger.Default,
                    Dependencies = new HashSet<string>(new[] { "D" })
                });
                engine.Pipelines.Add("F", new TestPipeline
                {
                    Trigger = PipelineTrigger.Always
                });
                return engine;
            }
        }

        public class GetPipelinePhasesTests : EngineFixture
        {
            [Test]
            public void ThrowsForIsolatedPipelineWithDependencies()
            {
                // Given
                IPipelineCollection pipelines = new TestPipelineCollection();
                pipelines.Add("Bar");
                pipelines.Add("Foo", new TestPipeline
                {
                    Dependencies = new HashSet<string>(new[] { "Bar" }),
                    Isolated = true
                });
                TestLogger logger = new TestLogger();

                // When, Then
                Should.Throw<PipelineException>(() => Engine.GetPipelinePhases(pipelines, logger));
            }

            [Test]
            public void ThrowsForMissingDependency()
            {
                // Given
                IPipelineCollection pipelines = new TestPipelineCollection();
                pipelines.Add("Foo", new TestPipeline
                {
                    Dependencies = new HashSet<string>(new[] { "Bar" })
                });
                TestLogger logger = new TestLogger();

                // When, Then
                Should.Throw<PipelineException>(() => Engine.GetPipelinePhases(pipelines, logger));
            }

            [Test]
            public void ThrowsForSelfDependency()
            {
                // Given
                IPipelineCollection pipelines = new TestPipelineCollection();
                pipelines.Add("Foo", new TestPipeline
                {
                    Dependencies = new HashSet<string>(new[] { "Foo" })
                });
                TestLogger logger = new TestLogger();

                // When, Then
                Should.Throw<PipelineException>(() => Engine.GetPipelinePhases(pipelines, logger));
            }

            [Test]
            public void ThrowsForCyclicDependency()
            {
                // Given
                IPipelineCollection pipelines = new TestPipelineCollection();
                pipelines.Add("Baz", new TestPipeline
                {
                    Dependencies = new HashSet<string>(new[] { "Foo" })
                });
                pipelines.Add("Bar", new TestPipeline
                {
                    Dependencies = new HashSet<string>(new[] { "Baz" })
                });
                pipelines.Add("Foo", new TestPipeline
                {
                    Dependencies = new HashSet<string>(new[] { "Bar" })
                });
                TestLogger logger = new TestLogger();

                // When, Then
                Should.Throw<PipelineException>(() => Engine.GetPipelinePhases(pipelines, logger));
            }

            [Test]
            public void ThrowsForIsolatedDependency()
            {
                // Given
                IPipelineCollection pipelines = new TestPipelineCollection();
                pipelines.Add("Bar", new TestPipeline
                {
                    Isolated = true
                });
                pipelines.Add("Foo", new TestPipeline
                {
                    Dependencies = new HashSet<string>(new[] { "Bar" })
                });
                TestLogger logger = new TestLogger();

                // When, Then
                Should.Throw<PipelineException>(() => Engine.GetPipelinePhases(pipelines, logger));
            }

            [Test]
            public void ThrowsForManualDependency()
            {
                // Given
                IPipelineCollection pipelines = new TestPipelineCollection();
                pipelines.Add("Bar", new TestPipeline
                {
                    Trigger = PipelineTrigger.Manual
                });
                pipelines.Add("Foo", new TestPipeline
                {
                    Dependencies = new HashSet<string>(new[] { "Bar" })
                });
                TestLogger logger = new TestLogger();

                // When, Then
                Should.Throw<PipelineException>(() => Engine.GetPipelinePhases(pipelines, logger));
            }

            [Test]
            public void DependenciesAreCaseInsensitive()
            {
                // Given
                IPipelineCollection pipelines = new TestPipelineCollection();
                pipelines.Add("Bar");
                pipelines.Add("Foo", new TestPipeline
                {
                    Dependencies = new HashSet<string>(new[] { "bar" })
                });
                TestLogger logger = new TestLogger();

                // When
                PipelinePhase[] phases = Engine.GetPipelinePhases(pipelines, logger);

                // Then
                phases.Select(x => (x.PipelineName, x.Phase)).ShouldBe(new (string, Phase)[]
                {
                    ("Bar", Phase.Input),
                    ("Foo", Phase.Input),
                    ("Bar", Phase.Process),
                    ("Foo", Phase.Process),
                    ("Bar", Phase.Transform),
                    ("Foo", Phase.Transform),
                    ("Bar", Phase.Output),
                    ("Foo", Phase.Output)
                });
            }
        }

        public class GetServiceTests : EngineFixture
        {
            [Test]
            public void GetsEngineService()
            {
                // Given
                Engine engine = new Engine();

                // When
                IReadOnlyFileSystem fileSystem = engine.Services.GetRequiredService<IReadOnlyFileSystem>();

                // Then
                fileSystem.ShouldBe(engine.FileSystem);
            }

            [Test]
            public void GetsExternalService()
            {
                // Given
                TestFileProvider testFileProvider = new TestFileProvider();
                ServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection.AddSingleton<IFileProvider>(testFileProvider);
                Engine engine = new Engine(serviceCollection);

                // When
                IFileProvider fileProvider = engine.Services.GetRequiredService<IFileProvider>();

                // Then
                fileProvider.ShouldBe(testFileProvider);
            }

            [Test]
            public void GetsEngineServiceInNestedScope()
            {
                // Given
                Engine engine = new Engine();
                IServiceScopeFactory serviceScopeFactory = engine.Services.GetRequiredService<IServiceScopeFactory>();
                IServiceScope serviceScope = serviceScopeFactory.CreateScope();

                // When
                IReadOnlyFileSystem fileSystem = serviceScope.ServiceProvider.GetRequiredService<IReadOnlyFileSystem>();

                // Then
                fileSystem.ShouldBe(engine.FileSystem);
            }
        }

        public class ExecuteTests : EngineFixture
        {
            [Test]
            public async Task ExecutesModule()
            {
                // Given
                Engine engine = new Engine();
                IPipeline pipeline = engine.Pipelines.Add("TestPipeline");
                CountModule module = new CountModule("Foo")
                {
                    EnsureInputDocument = true
                };
                pipeline.ProcessModules.Add(module);
                CancellationTokenSource cts = new CancellationTokenSource();

                // When
                IPipelineOutputs outputs = await engine.ExecuteAsync(cts);

                // Then
                module.ExecuteCount.ShouldBe(1);
                outputs["TestPipeline"].Select(x => x.GetInt("Foo")).ShouldBe(new int[] { 1 });
            }

            [Test]
            public async Task BeforeModuleEventOverriddesOutputs()
            {
                // Given
                Engine engine = new Engine();
                IPipeline pipeline = engine.Pipelines.Add("TestPipeline");
                CountModule module = new CountModule("Foo")
                {
                    EnsureInputDocument = true
                };
                pipeline.ProcessModules.Add(module);
                CancellationTokenSource cts = new CancellationTokenSource();
                engine.Events.Subscribe<BeforeModuleExecution>(x => x.OverrideOutputs(new TestDocument()
                {
                    { "Foo", 123 }
                }.Yield()));

                // When
                IPipelineOutputs outputs = await engine.ExecuteAsync(cts);

                // Then
                module.ExecuteCount.ShouldBe(0);
                outputs["TestPipeline"].Select(x => x.GetInt("Foo")).ShouldBe(new int[] { 123 });
            }

            [Test]
            public async Task AfterModuleEventOverriddesOutputs()
            {
                // Given
                Engine engine = new Engine();
                IPipeline pipeline = engine.Pipelines.Add("TestPipeline");
                CountModule module = new CountModule("Foo")
                {
                    EnsureInputDocument = true
                };
                pipeline.ProcessModules.Add(module);
                CancellationTokenSource cts = new CancellationTokenSource();
                engine.Events.Subscribe<AfterModuleExecution>(x => x.OverrideOutputs(new TestDocument()
                {
                    { "Foo", x.Outputs[0].GetInt("Foo") + 123 }
                }.Yield()));

                // When
                IPipelineOutputs outputs = await engine.ExecuteAsync(cts);

                // Then
                module.ExecuteCount.ShouldBe(1);
                outputs["TestPipeline"].Select(x => x.GetInt("Foo")).ShouldBe(new int[] { 124 });
            }
        }
    }
}