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

using System;
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using System.Collections.Generic;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Regression test asserting behavior of <see cref="Symbol.Canonical"/>
    /// </summary>
    public class OptionSymbolCanonicalRegressionAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private Symbol _optionContract;
        private Symbol _canonicalOptionContract;

        public override void Initialize()
        {
            SetStartDate(2014, 06, 05);
            SetEndDate(2014, 06, 09);

            var equitySymbol = AddEquity("TWX").Symbol;
            var contracts = OptionChainProvider.GetOptionContractList(equitySymbol, UtcTime).ToList();

            var callOptionSymbol = contracts
                .Where(c => c.ID.OptionRight == OptionRight.Call)
                .OrderBy(c => c.ID.Date)
                .First();
            _optionContract = AddOptionContract(callOptionSymbol).Symbol;
            _canonicalOptionContract = _optionContract.Canonical;
        }

        public override void OnData(Slice slice)
        {
            if (!ReferenceEquals(_canonicalOptionContract, _optionContract.Canonical))
            {
                throw new Exception("Canonical Symbol reference changed!");
            }

            _canonicalOptionContract = _optionContract.Canonical;
            if (slice.OptionChains.ContainsKey(_optionContract.Canonical))
            {
                var chain = slice.OptionChains[_optionContract.Canonical];
                if (!Portfolio.Invested)
                {
                    MarketOrder(_optionContract, 1);
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
        public Language[] Languages { get; } = { Language.CSharp };

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "1"},
            {"Average Win", "0%"},
            {"Average Loss", "0%"},
            {"Compounding Annual Return", "-11.148%"},
            {"Drawdown", "0.200%"},
            {"Expectancy", "0"},
            {"Net Profit", "-0.151%"},
            {"Sharpe Ratio", "0"},
            {"Probabilistic Sharpe Ratio", "0%"},
            {"Loss Rate", "0%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "0"},
            {"Beta", "0"},
            {"Annual Standard Deviation", "0"},
            {"Annual Variance", "0"},
            {"Information Ratio", "-22.995"},
            {"Tracking Error", "0.046"},
            {"Treynor Ratio", "0"},
            {"Total Fees", "$1.00"},
            {"Estimated Strategy Capacity", "$110000000.00"},
            {"Lowest Capacity Asset", "AOL VRKS95ENLBYE|AOL R735QTJ8XC9X"},
            {"Fitness Score", "0.007"},
            {"Kelly Criterion Estimate", "0"},
            {"Kelly Criterion Probability Value", "0"},
            {"Sortino Ratio", "79228162514264337593543950335"},
            {"Return Over Maximum Drawdown", "-85.302"},
            {"Portfolio Turnover", "0.014"},
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
            {"OrderListHash", "244df60d462d98848974044e20c75174"}
        };
    }
}
