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
using QuantConnect.Algorithm.Framework.Alphas;
using QuantConnect.Algorithm.Framework.Execution;
using QuantConnect.Algorithm.Framework.Portfolio;
using QuantConnect.Algorithm.Framework.Selection;
using QuantConnect.Orders;
using QuantConnect.Interfaces;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Regression algorithm testing portfolio construction model control over rebalancing,
    /// specifying a date rules, see GH 4075.
    /// </summary>
    public class PortfolioRebalanceOnDateRulesRegressionAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            UniverseSettings.Resolution = Resolution.Daily;

            // Order margin value has to have a minimum of 0.5% of Portfolio value, allows filtering out small trades and reduce fees.
            // Commented so regression algorithm is more sensitive
            //Settings.MinimumOrderMarginPortfolioPercentage = 0.005m;

            SetStartDate(2015, 1, 1);
            SetEndDate(2017, 1, 1);

            Settings.RebalancePortfolioOnInsightChanges = false;
            Settings.RebalancePortfolioOnSecurityChanges = false;

            SetUniverseSelection(new CustomUniverseSelectionModel(
                "CustomUniverseSelectionModel",
                time => new List<string> { "AAPL", "IBM", "FB", "SPY" }
            ));
            SetAlpha(new ConstantAlphaModel(InsightType.Price, InsightDirection.Up, TimeSpan.FromMinutes(20), 0.025, null));
            SetPortfolioConstruction(new EqualWeightingPortfolioConstructionModel(DateRules.Every(DayOfWeek.Wednesday)));
            SetExecution(new ImmediateExecutionModel());
        }

        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            if (orderEvent.Status == OrderStatus.Submitted)
            {
                Debug($"{orderEvent}");
                if (UtcTime.DayOfWeek != DayOfWeek.Wednesday)
                {
                    throw new Exception($"{UtcTime} {orderEvent.Symbol} {UtcTime.DayOfWeek}");
                }
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
            {"Total Trades", "362"},
            {"Average Win", "0.06%"},
            {"Average Loss", "-0.03%"},
            {"Compounding Annual Return", "11.407%"},
            {"Drawdown", "18.200%"},
            {"Expectancy", "1.300"},
            {"Net Profit", "24.116%"},
            {"Sharpe Ratio", "0.689"},
            {"Probabilistic Sharpe Ratio", "29.629%"},
            {"Loss Rate", "24%"},
            {"Win Rate", "76%"},
            {"Profit-Loss Ratio", "2.01"},
            {"Alpha", "0.106"},
            {"Beta", "0.006"},
            {"Annual Standard Deviation", "0.154"},
            {"Annual Variance", "0.024"},
            {"Information Ratio", "0.219"},
            {"Tracking Error", "0.201"},
            {"Treynor Ratio", "16.79"},
            {"Total Fees", "$366.83"},
            {"Estimated Strategy Capacity", "$40000000.00"},
            {"Lowest Capacity Asset", "IBM R735QTJ8XC9X"},
            {"Fitness Score", "0.002"},
            {"Kelly Criterion Estimate", "0"},
            {"Kelly Criterion Probability Value", "1"},
            {"Sortino Ratio", "0.949"},
            {"Return Over Maximum Drawdown", "0.625"},
            {"Portfolio Turnover", "0.003"},
            {"Total Insights Generated", "2028"},
            {"Total Insights Closed", "2024"},
            {"Total Insights Analysis Completed", "2024"},
            {"Long Insight Count", "2028"},
            {"Short Insight Count", "0"},
            {"Long/Short Ratio", "100%"},
            {"Estimated Monthly Alpha Value", "$0"},
            {"Total Accumulated Estimated Alpha Value", "$0"},
            {"Mean Population Estimated Insight Value", "$0"},
            {"Mean Population Direction", "0%"},
            {"Mean Population Magnitude", "0%"},
            {"Rolling Averaged Population Direction", "0%"},
            {"Rolling Averaged Population Magnitude", "0%"},
            {"OrderListHash", "20d5c49aff16826f5a7fba8f3b9c23f2"}
        };
    }
}
