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

using System.Collections.Generic;
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Securities;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// This regression algorithm verifies automatic option contract assignment behavior.
    /// </summary>
    /// <meta name="tag" content="regression test" />
    /// <meta name="tag" content="options" />
    /// <meta name="tag" content="using data" />
    /// <meta name="tag" content="filter selection" />
    public class OptionAssignmentRegressionAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private Security Stock;

        private Security CallOption;
        private Symbol CallOptionSymbol;

        private Security PutOption;
        private Symbol PutOptionSymbol;

        public override void Initialize()
        {
            SetStartDate(2015, 12, 23);
            SetEndDate(2015, 12, 28);
            SetCash(100000);
            Stock = AddEquity("GOOG", Resolution.Minute);

            var contracts = OptionChainProvider.GetOptionContractList(Stock.Symbol, UtcTime).ToList();

            PutOptionSymbol = contracts
                .Where(c => c.ID.OptionRight == OptionRight.Put)
                .OrderBy(c => c.ID.Date)
                .First(c => c.ID.StrikePrice == 800m);

            CallOptionSymbol = contracts
                .Where(c => c.ID.OptionRight == OptionRight.Call)
                .OrderBy(c => c.ID.Date)
                .First(c => c.ID.StrikePrice == 600m);

            PutOption = AddOptionContract(PutOptionSymbol);
            CallOption = AddOptionContract(CallOptionSymbol);
        }

        public override void OnData(Slice data)
        {
            if (!Portfolio.Invested && Stock.Price != 0 && PutOption.Price != 0 && CallOption.Price != 0)
            {
                // this gets executed on start and after each auto-assignment, finally ending with expiration assignment
                MarketOrder(PutOptionSymbol, -1);
                MarketOrder(CallOptionSymbol, -1);
            }
        }

        public bool CanRunLocally { get; } = true;
        public Language[] Languages { get; } = {Language.CSharp};

        /// <summary>
        /// Data Points count of all timeslices of algorithm
        /// </summary>
        public long DataPoints => 4745;

        /// <summary>
        /// Data Points count of the algorithm history
        /// </summary>
        public int AlgorithmHistoryDataPoints => 0;

        public Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "24"},
            {"Average Win", "9.60%"},
            {"Average Loss", "-16.86%"},
            {"Compounding Annual Return", "-75.533%"},
            {"Drawdown", "2.300%"},
            {"Expectancy", "0.046"},
            {"Net Profit", "-2.162%"},
            {"Sharpe Ratio", "-6.761"},
            {"Probabilistic Sharpe Ratio", "1.125%"},
            {"Loss Rate", "33%"},
            {"Win Rate", "67%"},
            {"Profit-Loss Ratio", "0.57"},
            {"Alpha", "-0.01"},
            {"Beta", "0.455"},
            {"Annual Standard Deviation", "0.014"},
            {"Annual Variance", "0"},
            {"Information Ratio", "6.047"},
            {"Tracking Error", "0.015"},
            {"Treynor Ratio", "-0.207"},
            {"Total Fees", "$12.00"},
            {"Estimated Strategy Capacity", "$1100000.00"},
            {"Lowest Capacity Asset", "GOOCV 305RBQ20WHPNQ|GOOCV VP83T1ZUHROL"},
            {"Fitness Score", "0.057"},
            {"Kelly Criterion Estimate", "0"},
            {"Kelly Criterion Probability Value", "0"},
            {"Sortino Ratio", "-3.876"},
            {"Return Over Maximum Drawdown", "-35.706"},
            {"Portfolio Turnover", "3.258"},
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
            {"OrderListHash", "a0e8eeee1c31968b773ebdf47bb996df"}
        };
    }
}
