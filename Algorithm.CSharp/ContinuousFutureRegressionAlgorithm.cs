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
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Orders;
using QuantConnect.Interfaces;
using QuantConnect.Securities;
using QuantConnect.Data.Market;
using System.Collections.Generic;
using QuantConnect.Securities.Future;
using QuantConnect.Data.UniverseSelection;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Continuous Futures Regression algorithm. Asserting and showcasing the behavior of adding a continuous future
    /// </summary>
    public class ContinuousFutureRegressionAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private List<SymbolChangedEvent> _mappings = new();
        private Symbol _currentMappedSymbol;
        private Future _continuousContract;
        private DateTime _lastMonth;

        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            SetStartDate(2013, 7, 1);
            SetEndDate(2014, 1, 1);

            _continuousContract = AddFuture(Futures.Indices.SP500EMini,
                dataNormalizationMode: DataNormalizationMode.BackwardsRatio,
                dataMappingMode: DataMappingMode.LastTradingDay,
                contractDepthOffset: 0
            );
        }

        /// <summary>
        /// OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        /// </summary>
        /// <param name="data">Slice object keyed by symbol containing the stock data</param>
        public override void OnData(Slice data)
        {
            // we subtract a minute cause we can get data on the market close, from the previous minute
            if (!_continuousContract.Exchange.DateTimeIsOpen(Time.AddMinutes(-1)))
            {
                if (data.Bars.Count > 0 || data.QuoteBars.Count > 0)
                {
                    throw new Exception($"We are getting data during closed market!");
                }
            }

            var currentlyMappedSecurity = Securities[_continuousContract.Mapped];

            if (data.Keys.Count != 1)
            {
                throw new Exception($"We are getting data for more than one symbols! {string.Join(",", data.Keys.Select(symbol => symbol))}");
            }

            foreach (var changedEvent in data.SymbolChangedEvents.Values)
            {
                if (changedEvent.Symbol == _continuousContract.Symbol)
                {
                    _mappings.Add(changedEvent);
                    Log($"{Time} - SymbolChanged event: {changedEvent}");

                    if (_currentMappedSymbol == _continuousContract.Mapped)
                    {
                        throw new Exception($"Continuous contract current symbol did not change! {_continuousContract.Mapped}");
                    }

                    var currentExpiration = changedEvent.Symbol.Underlying.ID.Date;
                    var frontMonthExpiration = FuturesExpiryFunctions.FuturesExpiryFunction(_continuousContract.Symbol)(Time.AddMonths(1));

                    if (currentExpiration != frontMonthExpiration.Date)
                    {
                        throw new Exception($"Unexpected current mapped contract expiration {currentExpiration}" +
                            $" @ {Time} it should be AT front month expiration {frontMonthExpiration}");
                    }
                }
            }
            if (_lastMonth.Month != Time.Month && currentlyMappedSecurity.HasData)
            {
                _lastMonth = Time;

                Log($"{Time}- {currentlyMappedSecurity.GetLastData()}");
                if (Portfolio.Invested)
                {
                    Liquidate();
                }
                else
                {
                    // This works because we set this contract as tradable, even if it's a canonical security
                    Buy(currentlyMappedSecurity.Symbol, 1);
                }

                if(Time.Month == 1 && Time.Year == 2013)
                {
                    var response = History(new[] { _continuousContract.Symbol }, 60 * 24 * 90);
                    if (!response.Any())
                    {
                        throw new Exception("Unexpected empty history response");
                    }
                }
            }

            _currentMappedSymbol = _continuousContract.Mapped;
        }

        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            if (orderEvent.Status == OrderStatus.Filled)
            {
                Log($"{orderEvent}");
            }
        }

        public override void OnSecuritiesChanged(SecurityChanges changes)
        {
            Debug($"{Time}-{changes}");
            if (changes.AddedSecurities.Any(security => security.Symbol != _continuousContract.Symbol)
                || changes.RemovedSecurities.Any(security => security.Symbol != _continuousContract.Symbol))
            {
                throw new Exception($"We got an unexpected security changes {changes}");
            }
        }

        public override void OnEndOfAlgorithm()
        {
            var expectedMappingCounts = 2;
            if (_mappings.Count != expectedMappingCounts)
            {
                throw new Exception($"Unexpected symbol changed events: {_mappings.Count}, was expecting {expectedMappingCounts}");
            }

            var securities = Securities.Values.Where(sec => !sec.IsTradable && !sec.Symbol.IsCanonical() && sec.Symbol.SecurityType == SecurityType.Future).ToList();
            if (securities.Count != 1)
            {
                throw new Exception("We should have a non tradable future contract security!");
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
        /// Data Points count of all timeslices of algorithm
        /// </summary>
        public long DataPoints => 875590;

        /// <summary>
        /// Data Points count of the algorithm history
        /// </summary>
        public int AlgorithmHistoryDataPoints => 0;

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "3"},
            {"Average Win", "1.21%"},
            {"Average Loss", "0%"},
            {"Compounding Annual Return", "2.392%"},
            {"Drawdown", "1.600%"},
            {"Expectancy", "0"},
            {"Net Profit", "1.199%"},
            {"Sharpe Ratio", "0.775"},
            {"Probabilistic Sharpe Ratio", "40.287%"},
            {"Loss Rate", "0%"},
            {"Win Rate", "100%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "-0.006"},
            {"Beta", "0.099"},
            {"Annual Standard Deviation", "0.022"},
            {"Annual Variance", "0"},
            {"Information Ratio", "-2.726"},
            {"Tracking Error", "0.076"},
            {"Treynor Ratio", "0.169"},
            {"Total Fees", "$5.55"},
            {"Estimated Strategy Capacity", "$67000000.00"},
            {"Lowest Capacity Asset", "ES VMKLFZIH2MTD"},
            {"Fitness Score", "0.01"},
            {"Kelly Criterion Estimate", "0"},
            {"Kelly Criterion Probability Value", "0"},
            {"Sortino Ratio", "0.516"},
            {"Return Over Maximum Drawdown", "1.935"},
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
            {"OrderListHash", "8ad040c62ad255e4f9cd423364147e85"}
        };
    }
}
