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
using QuantConnect.Data;
using QuantConnect.Interfaces;
using System.Collections.Generic;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Regression algorithm asserting the split is handled correctly. Specifically GH issue #5765, where cash
    /// difference applied due to share count difference was using the split reference price instead of the new price,
    /// increasing cash holdings by a higher amount than it should have
    /// </summary>
    public class SplitPartialShareRegressionAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private decimal _cash;
        private SplitType? _splitType;
        public override void Initialize()
        {
            SetStartDate(2014, 06, 05);
            SetEndDate(2014, 06, 09);

            UniverseSettings.DataNormalizationMode = DataNormalizationMode.Raw;

            AddEquity("AAPL");
        }

        public override void OnData(Slice data)
        {
            foreach (var dataSplit in data.Splits)
            {
                if (_splitType == null || _splitType < dataSplit.Value.Type)
                {
                    _splitType = dataSplit.Value.Type;

                    if (_splitType == SplitType.Warning && _cash != Portfolio.CashBook[Currencies.USD].Amount)
                    {
                        throw new Exception("Unexpected cash amount change before split");
                    }

                    if (_splitType == SplitType.SplitOccurred)
                    {
                        var newCash = Portfolio.CashBook[Currencies.USD].Amount;
                        if (_cash == newCash || newCash - _cash >= dataSplit.Value.SplitFactor * dataSplit.Value.ReferencePrice)
                        {
                            throw new Exception("Unexpected cash amount change after split");
                        }
                    }
                }
                else
                {
                    throw new Exception($"Unexpected split event {dataSplit.Value.Type}");
                }
            }

            if (!Portfolio.Invested)
            {
                Buy("AAPL", 1);
                _cash = Portfolio.CashBook[Currencies.USD].Amount;
            }
        }

        public override void OnEndOfAlgorithm()
        {
            if (_splitType == null)
            {
                throw new Exception("No split was emitted!");
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
            {"Compounding Annual Return", "0.562%"},
            {"Drawdown", "3.200%"},
            {"Expectancy", "0"},
            {"Net Profit", "0.007%"},
            {"Sharpe Ratio", "7.5"},
            {"Probabilistic Sharpe Ratio", "0%"},
            {"Loss Rate", "0%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "0.036"},
            {"Beta", "-0.026"},
            {"Annual Standard Deviation", "0.001"},
            {"Annual Variance", "0"},
            {"Information Ratio", "-22.218"},
            {"Tracking Error", "0.047"},
            {"Treynor Ratio", "-0.342"},
            {"Total Fees", "$1.00"},
            {"Estimated Strategy Capacity", "$4200000000.00"},
            {"Lowest Capacity Asset", "AAPL R735QTJ8XC9X"},
            {"Fitness Score", "0.003"},
            {"Kelly Criterion Estimate", "0"},
            {"Kelly Criterion Probability Value", "0"},
            {"Sortino Ratio", "79228162514264337593543950335"},
            {"Return Over Maximum Drawdown", "79228162514264337593543950335"},
            {"Portfolio Turnover", "0.003"},
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
            {"OrderListHash", "e2718d95499fcbdb51cabc32d6e28202"}
        };
    }
}
