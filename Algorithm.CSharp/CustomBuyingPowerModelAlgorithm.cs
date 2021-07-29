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
using QuantConnect.Interfaces;
using QuantConnect.Securities;
using System.Collections.Generic;
using QuantConnect.Data;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Demonstration of using custom buying power model in backtesting.
    /// QuantConnect allows you to model all orders as deeply and accurately as you need.
    /// </summary>
    /// <meta name="tag" content="trading and orders" />
    /// <meta name="tag" content="transaction fees and slippage" />
    /// <meta name="tag" content="custom buying power models" />
    public class CustomBuyingPowerModelAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private Symbol _spy;

        public override void Initialize()
        {
            SetStartDate(2013, 10, 01);
            SetEndDate(2013, 10, 31);
            var security = AddEquity("SPY", Resolution.Hour);
            _spy = security.Symbol;

            // set the buying power model
            security.SetBuyingPowerModel(new CustomBuyingPowerModel());
        }

        public override void OnData(Slice slice)
        {
            if (Portfolio.Invested)
            {
                return;
            }

            var quantity = CalculateOrderQuantity(_spy, 1m);
            if (quantity % 100 != 0)
            {
                throw new Exception($"CustomBuyingPowerModel only allow quantity that is multiple of 100 and {quantity} was found");
            }

            // We normally get insufficient buying power model, but the
            // CustomBuyingPowerModel always says that there is sufficient buying power for the orders
            MarketOrder(_spy, quantity * 10);
        }

        public class CustomBuyingPowerModel : BuyingPowerModel
        {
            public override GetMaximumOrderQuantityResult GetMaximumOrderQuantityForTargetBuyingPower(
                GetMaximumOrderQuantityForTargetBuyingPowerParameters parameters)
            {
                var quantity = base.GetMaximumOrderQuantityForTargetBuyingPower(parameters).Quantity;
                quantity = Math.Floor(quantity / 100) * 100;
                return new GetMaximumOrderQuantityResult(quantity);
            }

            public override HasSufficientBuyingPowerForOrderResult HasSufficientBuyingPowerForOrder(
                HasSufficientBuyingPowerForOrderParameters parameters)
            {
                // if portfolio doesn't have enough buying power:
                //     parameters.Insufficient()

                // this model never allows a lack of funds get in the way of buying securities
                return parameters.Sufficient();
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
            {"Total Trades", "1"},
            {"Average Win", "0%"},
            {"Average Loss", "0%"},
            {"Compounding Annual Return", "4775.196%"},
            {"Drawdown", "21.600%"},
            {"Expectancy", "0"},
            {"Net Profit", "38.619%"},
            {"Sharpe Ratio", "33.779"},
            {"Probabilistic Sharpe Ratio", "77.029%"},
            {"Loss Rate", "0%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "32.812"},
            {"Beta", "8.756"},
            {"Annual Standard Deviation", "1.11"},
            {"Annual Variance", "1.231"},
            {"Information Ratio", "37.501"},
            {"Tracking Error", "0.985"},
            {"Treynor Ratio", "4.281"},
            {"Total Fees", "$30.00"},
            {"Estimated Strategy Capacity", "$22000000.00"},
            {"Lowest Capacity Asset", "SPY R735QTJ8XC9X"},
            {"Fitness Score", "0.395"},
            {"Kelly Criterion Estimate", "0"},
            {"Kelly Criterion Probability Value", "0"},
            {"Sortino Ratio", "98.148"},
            {"Return Over Maximum Drawdown", "384.626"},
            {"Portfolio Turnover", "0.395"},
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
            {"OrderListHash", "3df007afa8125770e8f1a49263af90a2"}
        };
    }
}
