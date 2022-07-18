/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using QuantConnect.Api;
using QuantConnect.Optimizer.Parameters;
using QuantConnect.Statistics;

namespace QuantConnect.Tests.API
{
    [TestFixture]
    public class OptimizationBacktestJsonConverterTests
    {
        private string _validSerialization = "{\"name\":\"ImABacktestName\",\"id\":\"backtestId\",\"progress\":0.0,\"exitCode\":0,"+
            "\"statistics\":[0.374,0.217,0.047,-4.51,2.86,-0.664,52.602,17.800,6300000.00,0.196,1.571,27.0,123.888,77.188,0.63,1.707,1390.49,180.0,0.233,-0.558,73.0]," +
            "\"parameterSet\":{\"pinocho\":\"19\",\"pepe\":\"-1\"},\"equity\":[[1,1.0],[2,2.0],[3,3.0]]}";

        [Test]
        public void SerializationNulls()
        {
            var optimizationBacktest = new OptimizationBacktest(null, null, null);

            var serialized = JsonConvert.SerializeObject(optimizationBacktest);
            Assert.AreEqual("{}", serialized);
        }

        [Test]
        public void Serialization()
        {
            var optimizationBacktest = new OptimizationBacktest(new ParameterSet(18,
                new Dictionary<string, string>
                {
                    { "pinocho", "19" },
                    { "pepe", "-1" }
                }), "backtestId", "ImABacktestName");

            optimizationBacktest.Statistics = new Dictionary<string, string>
            {
                { "Total Trades", "180" },
                { "Average Win", "2.86%" },
                { "Average Loss", "-4.51%" },
                { "Compounding Annual Return", "52.602%" },
                { "Drawdown", "17.800%" },
                { "Expectancy", "0.196" },
                { "Net Profit", "123.888%" },
                { "Sharpe Ratio", "1.707" },
                { "Probabilistic Sharpe Ratio", "77.188%" },
                { "Loss Rate", "27%" },
                { "Win Rate", "73%" },
                { "Profit-Loss Ratio", "0.63" },
                { "Alpha", "0.374" },
                { "Beta", "-0.664" },
                { "Annual Standard Deviation", "0.217" },
                { "Annual Variance", "0.047" },
                { "Information Ratio", "1.571" },
                { "Tracking Error", "0.233" },
                { "Treynor Ratio", "-0.558" },
                { "Total Fees", "$1390.49" },
                { "Estimated Strategy Capacity", "$6300000.00" },
            };

            optimizationBacktest.Equity = new Series
            {
                Values = new List<ChartPoint> { new ChartPoint(1, 1), new ChartPoint(2, 2), new ChartPoint(3, 3) }
            };

            var serialized = JsonConvert.SerializeObject(optimizationBacktest);

            Assert.AreEqual(_validSerialization, serialized);
        }

        [Test]
        public void Deserialization()
        {
            var deserialized = JsonConvert.DeserializeObject<OptimizationBacktest>(_validSerialization);
            Assert.IsNotNull(deserialized);
            Assert.AreEqual("ImABacktestName", deserialized.Name);
            Assert.AreEqual("backtestId", deserialized.BacktestId);
            Assert.AreEqual(0.0m, deserialized.Progress);
            Assert.AreEqual(0, deserialized.ExitCode);
            Assert.AreEqual(-1, deserialized.ParameterSet.Id);
            Assert.IsTrue(deserialized.ParameterSet.Value.Count == 2);
            Assert.IsTrue(deserialized.ParameterSet.Value["pinocho"] == "19");
            Assert.IsTrue(deserialized.ParameterSet.Value["pepe"] == "-1");
            Assert.IsTrue(deserialized.Equity.Values.Count == 3);
            Assert.IsTrue(deserialized.Equity.Values[0].x == 1);
            Assert.IsTrue(deserialized.Equity.Values[0].y == 1m);
            Assert.IsTrue(deserialized.Equity.Values[1].x == 2);
            Assert.IsTrue(deserialized.Equity.Values[1].y == 2m);
            Assert.IsTrue(deserialized.Equity.Values[2].x == 3);
            Assert.IsTrue(deserialized.Equity.Values[2].y == 3m);
            Assert.AreEqual("77.188", deserialized.Statistics[PerformanceMetrics.ProbabilisticSharpeRatio]);
        }
    }
}
