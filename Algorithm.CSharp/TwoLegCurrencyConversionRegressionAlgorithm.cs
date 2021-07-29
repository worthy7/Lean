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
using QuantConnect.Data;
using QuantConnect.Interfaces;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Regression algorithm which tests that a two leg currency conversion happens correctly
    /// </summary>
    public class TwoLegCurrencyConversionRegressionAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private Symbol _ethUsdSymbol;
        private Symbol _ltcUsdSymbol;

        public override void Initialize()
        {
            SetStartDate(2018, 04, 04);
            SetEndDate(2018, 04, 04);

            // GDAX doesn't have LTCETH or ETHLTC, but they do have ETHUSD and LTCUSD to form a path between ETH and LTC
            SetAccountCurrency("ETH");
            SetCash("ETH", 100000);
            SetCash("LTC", 100000);
            SetCash("USD", 100000);

            _ethUsdSymbol = AddCrypto("ETHUSD", Resolution.Minute).Symbol;
            _ltcUsdSymbol = AddCrypto("LTCUSD", Resolution.Minute).Symbol;
        }

        public override void OnData(Slice slice)
        {
            if (!Portfolio.Invested)
            {
                MarketOrder(_ltcUsdSymbol, 1);
            }
        }

        public override void OnEndOfAlgorithm()
        {
            var ltcCash = Portfolio.CashBook["LTC"];

            var conversionSymbols = ltcCash.CurrencyConversion.ConversionRateSecurities
                .Select(x => x.Symbol)
                .ToList();

            if (conversionSymbols.Count != 2)
            {
                throw new Exception(
                    $"Expected two conversion rate securities for LTC to ETH, is {conversionSymbols.Count}");
            }

            if (conversionSymbols[0] != _ltcUsdSymbol)
            {
                throw new Exception(
                    $"Expected first conversion rate security from LTC to ETH to be {_ltcUsdSymbol}, is {conversionSymbols[0]}");
            }

            if (conversionSymbols[1] != _ethUsdSymbol)
            {
                throw new Exception(
                    $"Expected second conversion rate security from LTC to ETH to be {_ethUsdSymbol}, is {conversionSymbols[1]}");
            }

            var ltcUsdValue = Securities[_ltcUsdSymbol].GetLastData().Value;
            var ethUsdValue = Securities[_ethUsdSymbol].GetLastData().Value;

            var expectedConversionRate = ltcUsdValue / ethUsdValue;
            var actualConversionRate = ltcCash.ConversionRate;

            if (actualConversionRate != expectedConversionRate)
            {
                throw new Exception(
                    $"Expected conversion rate from LTC to ETH to be {expectedConversionRate}, is {actualConversionRate}");
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
            {"Compounding Annual Return", "0%"},
            {"Drawdown", "0%"},
            {"Expectancy", "0"},
            {"Net Profit", "0%"},
            {"Sharpe Ratio", "0"},
            {"Probabilistic Sharpe Ratio", "0%"},
            {"Loss Rate", "0%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "0"},
            {"Beta", "0"},
            {"Annual Standard Deviation", "0"},
            {"Annual Variance", "0"},
            {"Information Ratio", "0"},
            {"Tracking Error", "0"},
            {"Treynor Ratio", "0"},
            {"Total Fees", "$0.00"},
            {"Estimated Strategy Capacity", "$2000.00"},
            {"Lowest Capacity Asset", "LTCUSD XJ"},
            {"Fitness Score", "0"},
            {"Kelly Criterion Estimate", "0"},
            {"Kelly Criterion Probability Value", "0"},
            {"Sortino Ratio", "79228162514264337593543950335"},
            {"Return Over Maximum Drawdown", "-139.899"},
            {"Portfolio Turnover", "0"},
            {"Total Insights Generated", "0"},
            {"Total Insights Closed", "0"},
            {"Total Insights Analysis Completed", "0"},
            {"Long Insight Count", "0"},
            {"Short Insight Count", "0"},
            {"Long/Short Ratio", "100%"},
            {"Estimated Monthly Alpha Value", "Ξ0"},
            {"Total Accumulated Estimated Alpha Value", "Ξ0"},
            {"Mean Population Estimated Insight Value", "Ξ0"},
            {"Mean Population Direction", "0%"},
            {"Mean Population Magnitude", "0%"},
            {"Rolling Averaged Population Direction", "0%"},
            {"Rolling Averaged Population Magnitude", "0%"},
            {"OrderListHash", "9ffe1c1c11cbeaa5cc2d18048c4f3049"}
        };
    }
}
