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

using QuantConnect.Algorithm.Framework.Alphas;
using QuantConnect.Algorithm.Framework.Portfolio;
using QuantConnect.Algorithm.Framework.Risk;
using QuantConnect.Algorithm.Framework.Selection;
using QuantConnect.Data.Fundamental;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Orders;
using QuantConnect.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// This example algorithm defines its own custom coarse/fine fundamental selection model
    /// with equally weighted portfolio and a maximum sector exposure
    /// </summary>
    public class SectorExposureRiskFrameworkAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        public override void Initialize()
        {
            // Set requested data resolution
            UniverseSettings.Resolution = Resolution.Daily;

            SetStartDate(2014, 03, 25);
            SetEndDate(2014, 04, 07);
            SetCash(100000);

            SetUniverseSelection(new FineFundamentalUniverseSelectionModel(SelectCoarse, SelectFine));
            SetAlpha(new ConstantAlphaModel(InsightType.Price, InsightDirection.Up, QuantConnect.Time.OneDay));
            SetPortfolioConstruction(new EqualWeightingPortfolioConstructionModel());
            SetRiskManagement(new MaximumSectorExposureRiskManagementModel());
        }

        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            if (orderEvent.Status.IsFill())
            {
                Debug($"Order event: {orderEvent}. Holding value: {Securities[orderEvent.Symbol].Holdings.AbsoluteHoldingsValue}");
            }
        }

        private IEnumerable<Symbol> SelectCoarse(IEnumerable<CoarseFundamental> coarse)
        {
            var tickers = Time.Date < new DateTime(2014, 4, 1)
                ? new[] { "AAPL", "AIG", "IBM" }
                : new[] { "GOOG", "BAC", "SPY" };

            return tickers.Select(x => QuantConnect.Symbol.Create(x, SecurityType.Equity, Market.USA));
        }

        private IEnumerable<Symbol> SelectFine(IEnumerable<FineFundamental> fine) => fine.Select(f => f.Symbol);

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
            {"Total Trades", "22"},
            {"Average Win", "0.08%"},
            {"Average Loss", "-0.01%"},
            {"Compounding Annual Return", "-35.065%"},
            {"Drawdown", "2.100%"},
            {"Expectancy", "1.412"},
            {"Net Profit", "-1.643%"},
            {"Sharpe Ratio", "-3.58"},
            {"Probabilistic Sharpe Ratio", "9.142%"},
            {"Loss Rate", "62%"},
            {"Win Rate", "38%"},
            {"Profit-Loss Ratio", "5.43"},
            {"Alpha", "-0.298"},
            {"Beta", "-0.033"},
            {"Annual Standard Deviation", "0.081"},
            {"Annual Variance", "0.007"},
            {"Information Ratio", "-0.716"},
            {"Tracking Error", "0.133"},
            {"Treynor Ratio", "8.708"},
            {"Total Fees", "$34.09"},
            {"Estimated Strategy Capacity", "$19000000.00"},
            {"Lowest Capacity Asset", "AIG R735QTJ8XC9X"},
            {"Fitness Score", "0.005"},
            {"Kelly Criterion Estimate", "-6.919"},
            {"Kelly Criterion Probability Value", "0.697"},
            {"Sortino Ratio", "-4.518"},
            {"Return Over Maximum Drawdown", "-16.314"},
            {"Portfolio Turnover", "0.1"},
            {"Total Insights Generated", "27"},
            {"Total Insights Closed", "25"},
            {"Total Insights Analysis Completed", "25"},
            {"Long Insight Count", "27"},
            {"Short Insight Count", "0"},
            {"Long/Short Ratio", "100%"},
            {"Estimated Monthly Alpha Value", "$-3512937"},
            {"Total Accumulated Estimated Alpha Value", "$-1658887"},
            {"Mean Population Estimated Insight Value", "$-66355.47"},
            {"Mean Population Direction", "32%"},
            {"Mean Population Magnitude", "0%"},
            {"Rolling Averaged Population Direction", "57.5578%"},
            {"Rolling Averaged Population Magnitude", "0%"},
            {"OrderListHash", "7abdbe50d404c3f0ef7dfa6dcca6ff38"}
        };
    }
}
