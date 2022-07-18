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
 *
*/

using System;
using System.Linq;
using NUnit.Framework;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.Results;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Tests.Engine.DataFeeds;
using QuantConnect.Lean.Engine.TransactionHandlers;
using QuantConnect.Tests.Common.Data.UniverseSelection;

namespace QuantConnect.Tests.Engine.Results
{
    [TestFixture]
    public class LiveTradingResultHandlerTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void GetHoldingsPositions(bool invested)
        {
            var algorithm = new AlgorithmStub();
            algorithm.AddFuture(Futures.Indices.SP500EMini);
            var equity = algorithm.AddEquity("SPY");
            equity.Holdings.SetHoldings(1, 10);
            var result = LiveTradingResultHandler.GetHoldings(algorithm.Securities.Values, algorithm.SubscriptionManager.SubscriptionDataConfigService, invested);

            if (invested)
            {
                Assert.AreEqual(1, result.Count);
            }
            else
            {
                Assert.AreEqual(2, result.Count);
                Assert.IsTrue(result.TryGetValue("/ES", out var holding));
                Assert.AreEqual(0, holding.Quantity);
            }

            Assert.IsTrue(result.TryGetValue("SPY", out var holding2));
            Assert.AreEqual(10, holding2.Quantity);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetHoldingsNoPosition(bool invested)
        {
            var algorithm = new AlgorithmStub();
            algorithm.AddFuture(Futures.Indices.SP500EMini);
            algorithm.AddEquity("SPY");
            var result = LiveTradingResultHandler.GetHoldings(algorithm.Securities.Values, algorithm.SubscriptionManager.SubscriptionDataConfigService, invested);

            if (invested)
            {
                Assert.AreEqual(0, result.Count);
            }
            else
            {
                Assert.AreEqual(2, result.Count);
                Assert.IsTrue(result.TryGetValue("/ES", out var holding));
                Assert.AreEqual(0, holding.Quantity);
                Assert.IsTrue(result.TryGetValue("SPY", out var holding2));
                Assert.AreEqual(0, holding2.Quantity);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetHoldingsSkipCanonicalOption(bool invested)
        {
            var algorithm = new AlgorithmStub();
            algorithm.AddEquity("SPY");
            algorithm.AddOption("SPY");
            var result = LiveTradingResultHandler.GetHoldings(algorithm.Securities.Values, algorithm.SubscriptionManager.SubscriptionDataConfigService, invested);

            if (invested)
            {
                Assert.AreEqual(0, result.Count);
            }
            else
            {
                Assert.AreEqual(1, result.Count);
                Assert.IsTrue(result.TryGetValue("SPY", out var holding));
                Assert.AreEqual(0, holding.Quantity);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void DailySampleValueBasedOnMarketHour(bool extendedMarketHoursEnabled)
        {
            var referenceDate = new DateTime(2020, 11, 25);
            var resultHandler = new LiveTradingResultHandler();
            resultHandler.Initialize(new LiveNodePacket(),
                new QuantConnect.Messaging.Messaging(),
                new Api.Api(), 
                new BacktestingTransactionHandler());

            var algo = new AlgorithmStub(createDataManager:false);
            algo.SetFinishedWarmingUp();
            var dataManager = new DataManagerStub(new TestDataFeed(), algo);
            algo.SubscriptionManager.SetDataManager(dataManager);
            var aapl = algo.AddEquity("AAPL", extendedMarketHours: extendedMarketHoursEnabled);
            algo.PostInitialize();
            resultHandler.SetAlgorithm(algo, 100000);
            resultHandler.OnSecuritiesChanged(SecurityChangesTests.AddedNonInternal(aapl));

            // Add values during market hours, should always update
            algo.Portfolio.CashBook["USD"].AddAmount(1000);
            algo.Portfolio.InvalidateTotalPortfolioValue();

            resultHandler.Sample(referenceDate.AddHours(15));
            Assert.IsTrue(resultHandler.Charts.ContainsKey("Strategy Equity"));
            Assert.AreEqual(1, resultHandler.Charts["Strategy Equity"].Series["Equity"].Values.Count);

            var currentEquityValue = resultHandler.Charts["Strategy Equity"].Series["Equity"].Values.Last().y;
            Assert.AreEqual(101000, currentEquityValue);

            // Add value to portfolio, see if portfolio updates with new sample
            // will be changed to 'extendedMarketHoursEnabled' = true
            algo.Portfolio.CashBook["USD"].AddAmount(10000);
            algo.Portfolio.InvalidateTotalPortfolioValue();

            resultHandler.Sample(referenceDate.AddHours(22));
            Assert.AreEqual(2, resultHandler.Charts["Strategy Equity"].Series["Equity"].Values.Count);

            currentEquityValue = resultHandler.Charts["Strategy Equity"].Series["Equity"].Values.Last().y;
            Assert.AreEqual(extendedMarketHoursEnabled ? 111000 : 101000, currentEquityValue);

            resultHandler.Exit();
        }

        private class TestDataFeed : IDataFeed
        {
            public bool IsActive { get; }

            public void Initialize(
                IAlgorithm algorithm,
                AlgorithmNodePacket job,
                IResultHandler resultHandler,
                IMapFileProvider mapFileProvider,
                IFactorFileProvider factorFileProvider,
                IDataProvider dataProvider,
                IDataFeedSubscriptionManager subscriptionManager,
                IDataFeedTimeProvider dataFeedTimeProvider,
                IDataChannelProvider dataChannelProvider
                )
            {
            }
            public Subscription CreateSubscription(SubscriptionRequest request)
            {
                return null;
            }
            public void RemoveSubscription(Subscription subscription)
            {
            }
            public void Exit()
            {
            }
        }
    }
}
