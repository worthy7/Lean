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
using QuantConnect.Algorithm.Framework.Alphas;
using QuantConnect.Algorithm.Framework.Portfolio;
using QuantConnect.Data.Fundamental;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Interfaces;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Regression algorithm used to test a fine and coarse selection methods
    /// returning <see cref="Universe.Unchanged"/>
    /// </summary>
    public class UniverseUnchangedRegressionAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private const int NumberOfSymbolsFine = 2;

        public override void Initialize()
        {
            UniverseSettings.Resolution = Resolution.Daily;

            // Order margin value has to have a minimum of 0.5% of Portfolio value, allows filtering out small trades and reduce fees.
            // Commented so regression algorithm is more sensitive
            //Settings.MinimumOrderMarginPortfolioPercentage = 0.005m;

            SetStartDate(2014, 03, 25);
            SetEndDate(2014, 04, 07);

            SetAlpha(new ConstantAlphaModel(InsightType.Price, InsightDirection.Up, TimeSpan.FromDays(1), 0.025, null));
            SetPortfolioConstruction(new EqualWeightingPortfolioConstructionModel());
            AddUniverse(CoarseSelectionFunction, FineSelectionFunction);
        }

        public IEnumerable<Symbol> CoarseSelectionFunction(IEnumerable<CoarseFundamental> coarse)
        {
            // the first and second selection
            if (Time.Date <= new DateTime(2014, 3, 26))
            {
                return new List<Symbol>
                {
                    QuantConnect.Symbol.Create("AAPL", SecurityType.Equity, Market.USA),
                    QuantConnect.Symbol.Create("AIG", SecurityType.Equity, Market.USA),
                    QuantConnect.Symbol.Create("IBM", SecurityType.Equity, Market.USA)
                };
            }
            // will skip fine selection
            return Universe.Unchanged;
        }

        public IEnumerable<Symbol> FineSelectionFunction(IEnumerable<FineFundamental> fine)
        {
            // just the first selection
            if (Time.Date == new DateTime(2014, 3, 25))
            {
                var sortedByPeRatio = fine.OrderByDescending(x => x.ValuationRatios.PERatio);
                var topFine = sortedByPeRatio.Take(NumberOfSymbolsFine);
                return topFine.Select(x => x.Symbol);
            }
            // the second selection will return unchanged, in the following fine selection will be skipped
            return Universe.Unchanged;
        }

        // assert security changes, throw if called more than once
        public override void OnSecuritiesChanged(SecurityChanges changes)
        {
            if (changes.AddedSecurities.Count != 2
                || Time != new DateTime(2014, 3, 25)
                || changes.AddedSecurities.All(security => security.Symbol != QuantConnect.Symbol.Create("IBM", SecurityType.Equity, Market.USA))
                || changes.AddedSecurities.All(security => security.Symbol != QuantConnect.Symbol.Create("AAPL", SecurityType.Equity, Market.USA)))
            {
                throw new Exception("Unexpected security changes");
            }
            Log($"OnSecuritiesChanged({Time:o}):: {changes}");
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
            {"Total Trades", "19"},
            {"Average Win", "0.00%"},
            {"Average Loss", "0.00%"},
            {"Compounding Annual Return", "-5.481%"},
            {"Drawdown", "2.100%"},
            {"Expectancy", "0.949"},
            {"Net Profit", "-0.216%"},
            {"Sharpe Ratio", "-0.263"},
            {"Probabilistic Sharpe Ratio", "39.639%"},
            {"Loss Rate", "33%"},
            {"Win Rate", "67%"},
            {"Profit-Loss Ratio", "1.92"},
            {"Alpha", "-0.049"},
            {"Beta", "-0.067"},
            {"Annual Standard Deviation", "0.137"},
            {"Annual Variance", "0.019"},
            {"Information Ratio", "0.914"},
            {"Tracking Error", "0.175"},
            {"Treynor Ratio", "0.536"},
            {"Total Fees", "$33.21"},
            {"Estimated Strategy Capacity", "$57000000.00"},
            {"Lowest Capacity Asset", "IBM R735QTJ8XC9X"},
            {"Fitness Score", "0.023"},
            {"Kelly Criterion Estimate", "-1.72"},
            {"Kelly Criterion Probability Value", "0.534"},
            {"Sortino Ratio", "-0.81"},
            {"Return Over Maximum Drawdown", "-2.587"},
            {"Portfolio Turnover", "0.084"},
            {"Total Insights Generated", "22"},
            {"Total Insights Closed", "20"},
            {"Total Insights Analysis Completed", "20"},
            {"Long Insight Count", "22"},
            {"Short Insight Count", "0"},
            {"Long/Short Ratio", "100%"},
            {"Estimated Monthly Alpha Value", "$-815359.3"},
            {"Total Accumulated Estimated Alpha Value", "$-385030.8"},
            {"Mean Population Estimated Insight Value", "$-19251.54"},
            {"Mean Population Direction", "30%"},
            {"Mean Population Magnitude", "30%"},
            {"Rolling Averaged Population Direction", "42.9939%"},
            {"Rolling Averaged Population Magnitude", "42.9939%"},
            {"OrderListHash", "0b0afe138397f324a48170acb2ec6ebf"}
        };
    }
}
