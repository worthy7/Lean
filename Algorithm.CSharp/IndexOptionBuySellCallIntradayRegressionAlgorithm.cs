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
using System.Linq;
using System.Reflection;
using QuantConnect.Interfaces;
using QuantConnect.Securities;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// This regression algorithm tests In The Money (ITM) index option calls across different strike prices.
    /// We expect 4* orders from the algorithm, which are:
    ///
    ///   * (1) Initial entry, buy SPX Call Option (SPXF21 expiring ITM)
    ///   * (2) Initial entry, sell SPX Call Option at different strike (SPXF21 expiring ITM)
    ///   * [2] Option assignment, settle into cash
    ///   * [1] Option exercise, settle into cash
    ///
    /// Additionally, we test delistings for index options and assert that our
    /// portfolio holdings reflect the orders the algorithm has submitted.
    ///
    /// * Assignments are counted as orders
    /// </summary>
    public class IndexOptionBuySellCallIntradayRegressionAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        public override void Initialize()
        {
            SetStartDate(2021, 1, 4);
            SetEndDate(2021, 1, 31);

            var spx = AddIndex("SPX", Resolution.Minute).Symbol;

            // Select a index option expiring ITM, and adds it to the algorithm.
            var spxOptions = OptionChainProvider.GetOptionContractList(spx, Time)
                .Where(x => (x.ID.StrikePrice == 3700m || x.ID.StrikePrice == 3800m) && x.ID.OptionRight == OptionRight.Call && x.ID.Date.Year == 2021 && x.ID.Date.Month == 1)
                .Select(x => AddIndexOptionContract(x, Resolution.Minute).Symbol)
                .OrderBy(x => x.ID.StrikePrice)
                .ToList();

            var expectedContract3700 = QuantConnect.Symbol.CreateOption(
                spx,
                Market.USA,
                OptionStyle.European,
                OptionRight.Call,
                3700m,
                new DateTime(2021, 1, 15));

            var expectedContract3800 = QuantConnect.Symbol.CreateOption(
                spx,
                Market.USA,
                OptionStyle.European,
                OptionRight.Call,
                3800m,
                new DateTime(2021, 1, 15));

            if (spxOptions.Count != 2)
            {
                throw new Exception($"Expected 2 index options symbols from chain provider, found {spxOptions.Count}");
            }

            if (spxOptions[0] != expectedContract3700)
            {
                throw new Exception($"Contract {expectedContract3700} was not found in the chain, found instead: {spxOptions[0]}");
            }
            if (spxOptions[1] != expectedContract3800)
            {
                throw new Exception($"Contract {expectedContract3800} was not found in the chain, found instead: {spxOptions[1]}");
            }

            Schedule.On(DateRules.Tomorrow, TimeRules.AfterMarketOpen(spx, 1), () =>
            {
                MarketOrder(spxOptions[0], 1);
                MarketOrder(spxOptions[1], -1);
            });
            Schedule.On(DateRules.Tomorrow, TimeRules.Noon, () =>
            {
                Liquidate();
            });
        }

        /// <summary>
        /// Ran at the end of the algorithm to ensure the algorithm has no holdings
        /// </summary>
        /// <exception cref="Exception">The algorithm has holdings</exception>
        public override void OnEndOfAlgorithm()
        {
            if (Portfolio.Invested)
            {
                throw new Exception($"Expected no holdings at end of algorithm, but are invested in: {string.Join(", ", Portfolio.Keys)}");
            }
        }

        /// <summary>
        /// This is used by the regression test system to indicate if the open source Lean repository has the required data to run this algorithm.
        /// </summary>
        public bool CanRunLocally { get; } = true;

        /// <summary>
        /// This is used by the regression test system to indicate which languages this algorithm is written in.
        /// </summary>
        public Language[] Languages { get; } = { Language.CSharp, Language.Python };

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "4"},
            {"Average Win", "0%"},
            {"Average Loss", "-0.06%"},
            {"Compounding Annual Return", "-1.552%"},
            {"Drawdown", "0.100%"},
            {"Expectancy", "-1"},
            {"Net Profit", "-0.110%"},
            {"Sharpe Ratio", "-3.525"},
            {"Probabilistic Sharpe Ratio", "0.518%"},
            {"Loss Rate", "100%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "-0.014"},
            {"Beta", "-0.004"},
            {"Annual Standard Deviation", "0.004"},
            {"Annual Variance", "0"},
            {"Information Ratio", "-0.453"},
            {"Tracking Error", "0.156"},
            {"Treynor Ratio", "3.397"},
            {"Total Fees", "$0.00"},
            {"Estimated Strategy Capacity", "$0"},
            {"Lowest Capacity Asset", "SPX XL80P3HB5O6M|SPX 31"},
            {"Fitness Score", "0"},
            {"Kelly Criterion Estimate", "0"},
            {"Kelly Criterion Probability Value", "0"},
            {"Sortino Ratio", "-4619.237"},
            {"Return Over Maximum Drawdown", "-14.266"},
            {"Portfolio Turnover", "0.005"},
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
            {"OrderListHash", "7d15f56731d38768ea81afac627f0657"}
        };
    }
}

