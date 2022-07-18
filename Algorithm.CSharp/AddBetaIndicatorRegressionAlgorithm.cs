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
using QuantConnect.Data;
using QuantConnect.Indicators;
using QuantConnect.Interfaces;
using QuantConnect.Orders;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Regression test to explain how Beta indicator works
    /// </summary>
    public class AddBetaIndicatorRegressionAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private Beta _beta;
        private SimpleMovingAverage _sma;
        private decimal _lastSMAValue;

        public override void Initialize()
        {
            SetStartDate(2013, 10, 07);
            SetEndDate(2013, 10, 15);
            SetCash(10000);

            AddEquity("IBM");
            AddEquity("SPY");

            EnableAutomaticIndicatorWarmUp = true;
            _beta = B("IBM", "SPY", 3, Resolution.Daily);
            _sma = SMA("SPY", 3, Resolution.Daily);
            _lastSMAValue = 0;

            if (!_beta.IsReady)
            {
                throw new Exception("_beta indicator was expected to be ready");
            }
        }

        public override void OnData(Slice data)
        {
            if (!Portfolio.Invested)
            {
                var price = data["IBM"].Close;
                Buy("IBM", 10);
                LimitOrder("IBM", 10, price * 0.1m);
                StopMarketOrder("IBM", 10, price / 0.1m);
            }
            
            if (_beta.Current.Value < 0m || _beta.Current.Value > 2.80m)
            {
                throw new Exception($"_beta value was expected to be between 0 and 2.80 but was {_beta.Current.Value}");
            }

            Log($"Beta between IBM and SPY is: {_beta.Current.Value}");
        }

        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            var order = Transactions.GetOrderById(orderEvent.OrderId);
            var goUpwards = _lastSMAValue < _sma.Current.Value;
            _lastSMAValue = _sma.Current.Value;

            if (order.Status == OrderStatus.Filled)
            {
                if (order.Type == OrderType.Limit && Math.Abs(_beta.Current.Value - 1) < 0.2m && goUpwards)
                {
                    Transactions.CancelOpenOrders(order.Symbol);
                }
            }

            if (order.Status == OrderStatus.Canceled)
            {
                Log(orderEvent.ToString());
            }
        }

        /// <summary>
        /// This is used by the regression test system to indicate if the open source Lean repository has the required data to run this algorithm.
        /// </summary>
        public bool CanRunLocally { get; } = true;

        /// <summary>
        /// This is used by the regression test system to indicate which languages this algorithm is written in.
        /// </summary>
        public virtual Language[] Languages { get; } = { Language.CSharp};

        /// <summary>
        /// Data Points count of all timeslices of algorithm
        /// </summary>
        public long DataPoints => 10977;

        /// <summary>
        /// Data Points count of the algorithm history
        /// </summary>
        public int AlgorithmHistoryDataPoints => 11;

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "1"},
            {"Average Win", "0%"},
            {"Average Loss", "0%"},
            {"Compounding Annual Return", "12.939%"},
            {"Drawdown", "0.300%"},
            {"Expectancy", "0"},
            {"Net Profit", "0.289%"},
            {"Sharpe Ratio", "4.233"},
            {"Probabilistic Sharpe Ratio", "68.349%"},
            {"Loss Rate", "0%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "0.035"},
            {"Beta", "0.122"},
            {"Annual Standard Deviation", "0.024"},
            {"Annual Variance", "0.001"},
            {"Information Ratio", "-3.181"},
            {"Tracking Error", "0.142"},
            {"Treynor Ratio", "0.842"},
            {"Total Fees", "$1.00"},
            {"Estimated Strategy Capacity", "$35000000.00"},
            {"Lowest Capacity Asset", "IBM R735QTJ8XC9X"},
            {"Fitness Score", "0.022"},
            {"Kelly Criterion Estimate", "0"},
            {"Kelly Criterion Probability Value", "0"},
            {"Sortino Ratio", "8.508"},
            {"Return Over Maximum Drawdown", "58.894"},
            {"Portfolio Turnover", "0.022"},
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
            {"OrderListHash", "bd88c6a0e10c7e146b05377205101a12"}
        };
    }
}
