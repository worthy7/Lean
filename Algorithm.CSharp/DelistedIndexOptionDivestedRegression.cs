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
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;

namespace QuantConnect.Algorithm.CSharp
{
    public class DelistedIndexOptionDivestedRegression : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private Symbol _spx;
        private Symbol _optionSymbol;
        private DateTime _optionExpiry = DateTime.MaxValue;
        private string _ticker;
        private bool _addOption = true;
        private bool _receivedWarning;

        public override void Initialize()
        {
            SetStartDate(2021, 1, 3);  //Set Start Date
            SetEndDate(2021, 1, 20);    //Set End Date

            _ticker = "SPX";
            var spxSecurity = AddIndex(_ticker, Resolution.Minute);
            spxSecurity.SetDataNormalizationMode(DataNormalizationMode.Raw);
            _spx = spxSecurity.Symbol;
        }

        public override void OnData(Slice slice)
        {
            if (!slice.ContainsKey(_spx))
            {
                return;
            }

            if (_addOption)
            {
                var contracts = OptionChainProvider.GetOptionContractList(_spx, Time);
                contracts = contracts.Where(x =>
                    x.ID.OptionRight == OptionRight.Put &&
                    x.ID.Date.Date == new DateTime(2021, 1, 15));

                var option = AddIndexOptionContract(contracts.First(), Resolution.Minute);
                _optionExpiry = option.Expiry;
                _optionSymbol = option.Symbol;


                _addOption = false;
            }

            if (slice.ContainsKey(_optionSymbol))
            {
                if (!Portfolio.Invested)
                {
                    SetHoldings(_optionSymbol, 0.25);
                }

                // Verify the order of delisting; warning then delisting
                Delisting delisting;
                if (slice.Delistings.TryGetValue(_optionSymbol, out delisting))
                {
                    switch (delisting.Type)
                    {
                        case DelistingType.Warning:
                            _receivedWarning = true;
                            break;
                        case DelistingType.Delisted:
                            if (!_receivedWarning)
                            {
                                throw new Exception("Did not receive warning before delisting");
                            }
                            break;
                    }
                }

                // Verify we aren't receiving expired option data.
                if (_optionExpiry < Time.Date)
                {
                    throw new Exception($"Received expired contract {_optionSymbol} expired: {_optionExpiry} current time: {Time}");
                }
            }

        }
        public override void OnEndOfAlgorithm()
        {
            foreach (var holding in Portfolio.Values)
            {
                Log($"Holding {holding.Symbol.Value}; Invested: {holding.Invested}; Quantity: {holding.Quantity}");

                if (holding.Symbol == _optionSymbol && holding.Invested)
                {
                    throw new Exception($"Index option {_optionSymbol.Value} is still invested after delisting");
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
            {"Total Trades", "2"},
            {"Average Win", "0%"},
            {"Average Loss", "-26.40%"},
            {"Compounding Annual Return", "-99.821%"},
            {"Drawdown", "45.600%"},
            {"Expectancy", "-1"},
            {"Net Profit", "-26.400%"},
            {"Sharpe Ratio", "-0.557"},
            {"Probabilistic Sharpe Ratio", "20.162%"},
            {"Loss Rate", "100%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "-0.564"},
            {"Beta", "-0.663"},
            {"Annual Standard Deviation", "1.752"},
            {"Annual Variance", "3.069"},
            {"Information Ratio", "-0.906"},
            {"Tracking Error", "1.763"},
            {"Treynor Ratio", "1.472"},
            {"Total Fees", "$0.00"},
            {"Estimated Strategy Capacity", "$1000000.00"},
            {"Lowest Capacity Asset", "SPX 31KC0UJFONTBI|SPX 31"},
            {"Fitness Score", "0.005"},
            {"Kelly Criterion Estimate", "0"},
            {"Kelly Criterion Probability Value", "0"},
            {"Sortino Ratio", "-0.249"},
            {"Return Over Maximum Drawdown", "-2.699"},
            {"Portfolio Turnover", "0.016"},
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
            {"OrderListHash", "4ae4c1d8e4054c41908fd36e893699a6"}
        };
    }
}
