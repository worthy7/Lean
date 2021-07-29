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

using System.Collections.Generic;
using QuantConnect.Data;
using QuantConnect.Interfaces;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// This is a regression algorithm for CFD assets which have the exchange time zone ahead of the data time zone.
    /// </summary>
    public class CfdTimeZonesRegressionAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private Symbol _symbol;

        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            SetAccountCurrency("EUR");

            SetStartDate(2019, 2, 20);
            SetEndDate(2019, 2, 21);
            SetCash("EUR", 100000);

            _symbol = AddCfd("DE30EUR", Resolution.Minute, Market.Oanda).Symbol;

            SetBenchmark(_symbol);
        }

        /// <summary>
        /// OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        /// </summary>
        /// <param name="data">Slice object keyed by symbol containing the stock data</param>
        public override void OnData(Slice data)
        {
            if (Time.Minute % 10 != 0) return;

            if (!Portfolio.Invested)
            {
                MarketOrder(_symbol, 1m);
            }
            else
            {
                Liquidate();
            }
        }

        /// <summary>
        /// This is used by the regression test system to indicate if the open source Lean repository has the required data to run this algorithm.
        /// </summary>
        public bool CanRunLocally { get; } = true;

        /// <summary>
        /// This is used by the regression test system to indicate which languages this algorithm is written in.
        /// </summary>
        public Language[] Languages { get; } = { Language.CSharp };

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "251"},
            {"Average Win", "0.01%"},
            {"Average Loss", "-0.01%"},
            {"Compounding Annual Return", "-37.465%"},
            {"Drawdown", "0.300%"},
            {"Expectancy", "-0.285"},
            {"Net Profit", "-0.257%"},
            {"Sharpe Ratio", "-40.568"},
            {"Probabilistic Sharpe Ratio", "0%"},
            {"Loss Rate", "66%"},
            {"Win Rate", "34%"},
            {"Profit-Loss Ratio", "1.08"},
            {"Alpha", "0"},
            {"Beta", "0"},
            {"Annual Standard Deviation", "0.006"},
            {"Annual Variance", "0"},
            {"Information Ratio", "-40.568"},
            {"Tracking Error", "0.006"},
            {"Treynor Ratio", "0"},
            {"Total Fees", "$0.00"},
            {"Estimated Strategy Capacity", "$660000.00"},
            {"Lowest Capacity Asset", "DE30EUR 8I"},
            {"Fitness Score", "0.002"},
            {"Kelly Criterion Estimate", "0"},
            {"Kelly Criterion Probability Value", "0"},
            {"Sortino Ratio", "-23.868"},
            {"Return Over Maximum Drawdown", "-170.818"},
            {"Portfolio Turnover", "12.673"},
            {"Total Insights Generated", "0"},
            {"Total Insights Closed", "0"},
            {"Total Insights Analysis Completed", "0"},
            {"Long Insight Count", "0"},
            {"Short Insight Count", "0"},
            {"Long/Short Ratio", "100%"},
            {"Estimated Monthly Alpha Value", "€0"},
            {"Total Accumulated Estimated Alpha Value", "€0"},
            {"Mean Population Estimated Insight Value", "€0"},
            {"Mean Population Direction", "0%"},
            {"Mean Population Magnitude", "0%"},
            {"Rolling Averaged Population Direction", "0%"},
            {"Rolling Averaged Population Magnitude", "0%"},
            {"OrderListHash", "6745cf313aa3ef780d052ca3ba933c6c"}
        };
    }
}
