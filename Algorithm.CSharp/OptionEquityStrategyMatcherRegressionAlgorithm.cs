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

using System.Linq;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using System.Collections.Generic;
using QuantConnect.Securities.Option.StrategyMatcher;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Regression algorithm to assert that the option strategy matcher works as expected
    /// </summary>
    public class OptionEquityStrategyMatcherRegressionAlgorithm : OptionEquityBaseStrategyRegressionAlgorithm
    {
        public override void Initialize()
        {
            base.Initialize();
            AddEquity("SPY", Resolution.Hour);
        }

        /// <summary>
        /// OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        /// </summary>
        /// <param name="slice">Slice object keyed by symbol containing the stock data</param>
        public override void OnData(Slice slice)
        {
            if (!Portfolio.Invested)
            {
                OptionChain chain;
                if (IsMarketOpen(_optionSymbol) && slice.OptionChains.TryGetValue(_optionSymbol, out chain) && Securities["SPY"].HasData)
                {
                    var contracts = chain
                        .Where(contract => contract.Right == OptionRight.Call)
                        .GroupBy(x => x.Expiry)
                        .First()
                        .OrderBy(x => x.Strike)
                        .ToList();

                    // let's setup and trade a butterfly call
                    var distanceBetweenStrikes = 2.5m;
                    var lowerCall = contracts.First();
                    var middleCall = contracts.First(contract => contract.Expiry == lowerCall.Expiry && contract.Strike == lowerCall.Strike + distanceBetweenStrikes);
                    var highestCall = contracts.First(contract => contract.Expiry == lowerCall.Expiry && contract.Strike == middleCall.Strike + distanceBetweenStrikes);

                    var initialMargin = Portfolio.MarginRemaining;
                    MarketOrder(lowerCall.Symbol, 10);
                    MarketOrder(middleCall.Symbol, -20);
                    MarketOrder(highestCall.Symbol, 10);
                    var freeMarginPostTrade = Portfolio.MarginRemaining;

                    AssertOptionStrategyIsPresent(OptionStrategyDefinitions.ButterflyCall.Name, 10);

                    // let's make some trades to add some noise
                    MarketOrder(_optionSymbol.Underlying, 490);
                    freeMarginPostTrade = Portfolio.MarginRemaining;

                    AssertOptionStrategyIsPresent(OptionStrategyDefinitions.ButterflyCall.Name, 10);
                    AssertDefaultGroup(_optionSymbol.Underlying, 490);

                    LimitOrder(_optionSymbol.Underlying, 100, Securities[_optionSymbol.Underlying].AskPrice);

                    AssertOptionStrategyIsPresent(OptionStrategyDefinitions.ButterflyCall.Name, 10);
                    AssertDefaultGroup(_optionSymbol.Underlying, 490);

                    MarketOrder(lowerCall.Symbol, 5);
                    freeMarginPostTrade = Portfolio.MarginRemaining;

                    AssertOptionStrategyIsPresent(OptionStrategyDefinitions.ButterflyCall.Name, 10);
                    AssertDefaultGroup(_optionSymbol.Underlying, 490);
                    AssertDefaultGroup(lowerCall.Symbol, 5);

                    MarketOrder(middleCall.Symbol, -5);
                    freeMarginPostTrade = Portfolio.MarginRemaining;

                    AssertOptionStrategyIsPresent(OptionStrategyDefinitions.ButterflyCall.Name, 10);
                    AssertOptionStrategyIsPresent(OptionStrategyDefinitions.CoveredCall.Name, 4);
                    AssertOptionStrategyIsPresent(OptionStrategyDefinitions.BullCallSpread.Name, 1);
                    AssertDefaultGroup(_optionSymbol.Underlying, 90);
                    AssertDefaultGroup(lowerCall.Symbol, 4);

                    // trade some other asset
                    MarketOrder("SPY", 200);
                    freeMarginPostTrade = Portfolio.MarginRemaining;

                    AssertOptionStrategyIsPresent(OptionStrategyDefinitions.ButterflyCall.Name, 10);
                    AssertOptionStrategyIsPresent(OptionStrategyDefinitions.CoveredCall.Name, 4);
                    AssertOptionStrategyIsPresent(OptionStrategyDefinitions.BullCallSpread.Name, 1);
                    AssertDefaultGroup(_optionSymbol.Underlying, 90);
                    AssertDefaultGroup(lowerCall.Symbol, 4);
                }
            }
        }

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public override Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "8"},
            {"Average Win", "0%"},
            {"Average Loss", "0%"},
            {"Compounding Annual Return", "0%"},
            {"Drawdown", "0%"},
            {"Expectancy", "0"},
            {"Net Profit", "0%"},
            {"Sharpe Ratio", "0"},
            {"Probabilistic Sharpe Ratio", "0%"},
            {"Loss Rate", "0%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "0"},
            {"Beta", "0"},
            {"Annual Standard Deviation", "0"},
            {"Annual Variance", "0"},
            {"Information Ratio", "0"},
            {"Tracking Error", "0"},
            {"Treynor Ratio", "0"},
            {"Total Fees", "$16.95"},
            {"Estimated Strategy Capacity", "$1000.00"},
            {"Lowest Capacity Asset", "GOOCV W78ZFM61MKLI|GOOCV VP83T1ZUHROL"},
            {"Fitness Score", "0"},
            {"Kelly Criterion Estimate", "0"},
            {"Kelly Criterion Probability Value", "0"},
            {"Sortino Ratio", "0"},
            {"Return Over Maximum Drawdown", "0"},
            {"Portfolio Turnover", "0"},
            {"Total Insights Generated", "0"},
            {"Total Insights Closed", "0"},
            {"Total Insights Analysis Completed", "0"},
            {"Long Insight Count", "0"},
            {"Short Insight Count", "0"},
            {"Long/Short Ratio", "100%"},
            {"Estimated Monthly Alpha Value", "$0"},
            {"Total Accumulated Estimated Alpha Value", "$0"},
            {"Mean Population Estimated Insight Value", "$0"},
            {"Mean Population Direction", "0%"},
            {"Mean Population Magnitude", "0%"},
            {"Rolling Averaged Population Direction", "0%"},
            {"Rolling Averaged Population Magnitude", "0%"},
            {"OrderListHash", "63d0336c6fd05ac54aaec6a4af623f05"}
        };
    }
}
