﻿using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Execution;
using Wyam.Core.Execution;
using Wyam.Core.Modules.Control;
using Wyam.Testing;
using Wyam.Testing.Execution;
using Wyam.Testing.Modules;

namespace Wyam.Core.Tests.Modules.Control
{
    [TestFixture]
    [NonParallelizable]
    public class IfFixture : BaseFixture
    {
        public class ExecuteTests : IfFixture
        {
            [Test]
            public async Task IfResultsInCorrectCounts()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2
                };
                CountModule b = new CountModule("B")
                {
                    AdditionalOutputs = 2
                };
                CountModule c = new CountModule("C")
                {
                    AdditionalOutputs = 3
                };
                engine.Pipelines.Add(a, new If((x, y) => x.Content == "1", b), c);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(1, b.InputCount);
                Assert.AreEqual(5, c.InputCount);
                Assert.AreEqual(3, a.OutputCount);
                Assert.AreEqual(3, b.OutputCount);
                Assert.AreEqual(20, c.OutputCount);
            }

            [Test]
            public async Task ElseIfResultsInCorrectCounts()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2
                };
                CountModule b = new CountModule("B")
                {
                    AdditionalOutputs = 2
                };
                CountModule c = new CountModule("C")
                {
                    AdditionalOutputs = 3
                };
                CountModule d = new CountModule("B")
                {
                    AdditionalOutputs = 2
                };
                engine.Pipelines.Add(
                    a,
                    new If((x, y) => x.Content == "1", b)
                        .ElseIf((x, y) => x.Content == "2", c),
                    d);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, d.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(1, b.InputCount);
                Assert.AreEqual(1, c.InputCount);
                Assert.AreEqual(8, d.InputCount);
                Assert.AreEqual(3, a.OutputCount);
                Assert.AreEqual(3, b.OutputCount);
                Assert.AreEqual(4, c.OutputCount);
                Assert.AreEqual(24, d.OutputCount);
            }

            [Test]
            public async Task ElseResultsInCorrectCounts()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2
                };
                CountModule b = new CountModule("B")
                {
                    AdditionalOutputs = 2
                };
                CountModule c = new CountModule("C")
                {
                    AdditionalOutputs = 3
                };
                CountModule d = new CountModule("B")
                {
                    AdditionalOutputs = 2
                };
                engine.Pipelines.Add(
                    a,
                    new If((x, y) => x.Content == "1", b)
                        .Else(c),
                    d);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, d.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(1, b.InputCount);
                Assert.AreEqual(2, c.InputCount);
                Assert.AreEqual(11, d.InputCount);
                Assert.AreEqual(3, a.OutputCount);
                Assert.AreEqual(3, b.OutputCount);
                Assert.AreEqual(8, c.OutputCount);
                Assert.AreEqual(33, d.OutputCount);
            }

            [Test]
            public async Task IfElseAndElseResultsInCorrectCounts()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 3
                };
                CountModule b = new CountModule("B")
                {
                    AdditionalOutputs = 2
                };
                CountModule c = new CountModule("C")
                {
                    AdditionalOutputs = 3
                };
                CountModule d = new CountModule("B")
                {
                    AdditionalOutputs = 2
                };
                CountModule e = new CountModule("B")
                {
                    AdditionalOutputs = 3
                };
                engine.Pipelines.Add(
                    a,
                    new If((x, y) => x.Content == "1", b)
                        .ElseIf((x, y) => x.Content == "3", c)
                        .Else(d),
                    e);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, d.ExecuteCount);
                Assert.AreEqual(1, e.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(1, b.InputCount);
                Assert.AreEqual(1, c.InputCount);
                Assert.AreEqual(2, d.InputCount);
                Assert.AreEqual(13, e.InputCount);
                Assert.AreEqual(4, a.OutputCount);
                Assert.AreEqual(3, b.OutputCount);
                Assert.AreEqual(4, c.OutputCount);
                Assert.AreEqual(6, d.OutputCount);
                Assert.AreEqual(52, e.OutputCount);
            }

            [Test]
            public async Task IfWithContextResultsInCorrectCounts()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2
                };
                CountModule b = new CountModule("B")
                {
                    AdditionalOutputs = 2
                };
                CountModule c = new CountModule("C")
                {
                    AdditionalOutputs = 3
                };
                engine.Pipelines.Add(a, new If(x => true, b), c);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(3, b.InputCount);
                Assert.AreEqual(9, c.InputCount);
                Assert.AreEqual(3, a.OutputCount);
                Assert.AreEqual(9, b.OutputCount);
                Assert.AreEqual(36, c.OutputCount);
            }

            [Test]
            public async Task FalseIfWithContextResultsInCorrectCounts()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2
                };
                CountModule b = new CountModule("B")
                {
                    AdditionalOutputs = 2
                };
                CountModule c = new CountModule("C")
                {
                    AdditionalOutputs = 3
                };
                engine.Pipelines.Add(a, new If(x => false, b), c);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(0, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(0, b.InputCount);
                Assert.AreEqual(3, c.InputCount);
                Assert.AreEqual(3, a.OutputCount);
                Assert.AreEqual(0, b.OutputCount);
                Assert.AreEqual(12, c.OutputCount);
            }

            [Test]
            public async Task UnmatchedDocumentsAreAddedToResults()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2
                };
                CountModule b = new CountModule("B")
                {
                    AdditionalOutputs = 2
                };
                CountModule c = new CountModule("C")
                {
                    AdditionalOutputs = 3
                };
                engine.Pipelines.Add(a, new If((doc, ctx) => false, b), c);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(0, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(0, b.InputCount);
                Assert.AreEqual(3, c.InputCount);
                Assert.AreEqual(3, a.OutputCount);
                Assert.AreEqual(0, b.OutputCount);
                Assert.AreEqual(12, c.OutputCount);
            }

            [Test]
            public async Task UnmatchedDocumentsAreNotAddedToResults()
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2
                };
                CountModule b = new CountModule("B")
                {
                    AdditionalOutputs = 2
                };
                CountModule c = new CountModule("C")
                {
                    AdditionalOutputs = 3
                };
                engine.Pipelines.Add(a, new If((doc, ctx) => false, b).WithoutUnmatchedDocuments(), c);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(0, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(0, b.InputCount);
                Assert.AreEqual(0, c.InputCount);
                Assert.AreEqual(3, a.OutputCount);
                Assert.AreEqual(0, b.OutputCount);
                Assert.AreEqual(0, c.OutputCount);
            }
        }
    }
}
