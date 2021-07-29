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
using QuantConnect.Algorithm.Framework.Execution;
using QuantConnect.Algorithm.Framework.Portfolio;
using QuantConnect.Algorithm.Framework.Selection;
using QuantConnect.Orders;
using QuantConnect.Interfaces;
using QuantConnect.Securities;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Regression algorithm testing portfolio construction model control over rebalancing,
    /// specifying a custom rebalance function that returns null in some cases, see GH 4075.
    /// </summary>
    public class PortfolioRebalanceOnCustomFuncRegressionAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private DateTime _lastRebalanceTime;

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
            SetEndDate(2018, 1, 1);

            Settings.RebalancePortfolioOnInsightChanges = false;
            Settings.RebalancePortfolioOnSecurityChanges = false;

            SetUniverseSelection(new CustomUniverseSelectionModel("CustomUniverseSelectionModel",
                time => new List<string> { "AAPL", "IBM", "FB", "SPY", "AIG", "BAC", "BNO" }
            ));
            SetAlpha(new ConstantAlphaModel(InsightType.Price, InsightDirection.Up, TimeSpan.FromMinutes(20), 0.025, null));
            SetPortfolioConstruction(new EqualWeightingPortfolioConstructionModel(
                time =>
                {
                    // for performance only run rebalance logic once a week
                    if (time.DayOfWeek != DayOfWeek.Monday)
                    {
                        return null;
                    }

                    if (_lastRebalanceTime == default(DateTime))
                    {
                        // initial rebalance
                        _lastRebalanceTime = time;
                        return time;
                    }

                    var deviation = 0m;
                    var count = Securities.Values.Count(security => security.Invested);
                    if (count > 0)
                    {
                        _lastRebalanceTime = time;
                        var portfolioValuePerSecurity = Portfolio.TotalPortfolioValue / count;
                        foreach (var security in Securities.Values.Where(security => security.Invested))
                        {
                            var reservedBuyingPowerForCurrentPosition = security.BuyingPowerModel.GetReservedBuyingPowerForPosition(
                                                                            new ReservedBuyingPowerForPositionParameters(security)).AbsoluteUsedBuyingPower
                                                                        // see GH issue 4107
                                                                        * security.BuyingPowerModel.GetLeverage(security);
                            // we sum up deviation for each security
                            deviation += (portfolioValuePerSecurity - reservedBuyingPowerForCurrentPosition) / portfolioValuePerSecurity;
                        }

                        // if securities are deviated 1.5% from their theoretical share of TotalPortfolioValue we rebalance
                        if (deviation >= 0.015m)
                        {
                            return time;
                        }
                    }
                    return null;
                }));
            SetExecution(new ImmediateExecutionModel());
        }

        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            Debug($"{orderEvent}");
            if (orderEvent.Status == OrderStatus.Submitted)
            {
                if (UtcTime - _lastRebalanceTime > TimeSpan.Zero || UtcTime.DayOfWeek != DayOfWeek.Monday)
                {
                    throw new Exception($"{UtcTime} {orderEvent.Symbol} {UtcTime - _lastRebalanceTime}");
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
            {"Total Trades", "34"},
            {"Average Win", "0.60%"},
            {"Average Loss", "0%"},
            {"Compounding Annual Return", "18.537%"},
            {"Drawdown", "9.300%"},
            {"Expectancy", "0"},
            {"Net Profit", "66.479%"},
            {"Sharpe Ratio", "1.755"},
            {"Probabilistic Sharpe Ratio", "88.014%"},
            {"Loss Rate", "0%"},
            {"Win Rate", "100%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "0.16"},
            {"Beta", "-0.051"},
            {"Annual Standard Deviation", "0.088"},
            {"Annual Variance", "0.008"},
            {"Information Ratio", "0.377"},
            {"Tracking Error", "0.147"},
            {"Treynor Ratio", "-3.035"},
            {"Total Fees", "$48.31"},
            {"Estimated Strategy Capacity", "$320000.00"},
            {"Lowest Capacity Asset", "BNO UN3IMQ2JU1YD"},
            {"Fitness Score", "0.001"},
            {"Kelly Criterion Estimate", "0"},
            {"Kelly Criterion Probability Value", "1"},
            {"Sortino Ratio", "2.128"},
            {"Return Over Maximum Drawdown", "2.002"},
            {"Portfolio Turnover", "0.001"},
            {"Total Insights Generated", "5327"},
            {"Total Insights Closed", "5320"},
            {"Total Insights Analysis Completed", "5320"},
            {"Long Insight Count", "5327"},
            {"Short Insight Count", "0"},
            {"Long/Short Ratio", "100%"},
            {"Estimated Monthly Alpha Value", "$0"},
            {"Total Accumulated Estimated Alpha Value", "$0"},
            {"Mean Population Estimated Insight Value", "$0"},
            {"Mean Population Direction", "0%"},
            {"Mean Population Magnitude", "0%"},
            {"Rolling Averaged Population Direction", "0%"},
            {"Rolling Averaged Population Magnitude", "0%"},
            {"OrderListHash", "a0076f3bae4c6f0be8805df052020fb9"}
        };
    }
}
