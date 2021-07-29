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
using System.Reflection;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Orders;
using QuantConnect.Securities;
using QuantConnect.Securities.Option;
using QuantConnect.Securities.Volatility;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// This regression algorithm tests In The Money (ITM) index option expiry for calls.
    /// We test to make sure that index options have greeks enabled, same as equity options.
    /// </summary>
    public class IndexOptionCallITMGreeksExpiryRegressionAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private bool _invested;
        private int _onDataCalls;
        private Symbol _spx;
        private Option _spxOption;
        private Symbol _expectedOptionContract;

        public override void Initialize()
        {
            SetStartDate(2021, 1, 4);
            SetEndDate(2021, 1, 31);

            var spx = AddIndex("SPX", Resolution.Minute);
            spx.VolatilityModel = new StandardDeviationOfReturnsVolatilityModel(60, Resolution.Minute, TimeSpan.FromMinutes(1));
            _spx = spx.Symbol;

            // Select an index option expiring ITM, and adds it to the algorithm.
            _spxOption = AddIndexOptionContract(OptionChainProvider.GetOptionContractList(_spx, Time)
                .Where(x => x.ID.StrikePrice <= 3200m && x.ID.OptionRight == OptionRight.Call && x.ID.Date.Year == 2021 && x.ID.Date.Month == 1)
                .OrderByDescending(x => x.ID.StrikePrice)
                .Take(1)
                .Single(), Resolution.Minute);

            _spxOption.PriceModel = OptionPriceModels.BlackScholes();

            _expectedOptionContract = QuantConnect.Symbol.CreateOption(_spx, Market.USA, OptionStyle.European, OptionRight.Call, 3200m, new DateTime(2021, 1, 15));
            if (_spxOption.Symbol != _expectedOptionContract)
            {
                throw new Exception($"Contract {_expectedOptionContract} was not found in the chain");
            }
        }

        public override void OnData(Slice data)
        {
            // Let the algo warmup, but without using SetWarmup. Otherwise, we get
            // no contracts in the option chain
            if (_invested || _onDataCalls++ < 40)
            {
                return;
            }

            if (data.OptionChains.Count == 0)
            {
                return;
            }
            if (data.OptionChains.Values.All(o => o.Contracts.Values.Any(c => !data.ContainsKey(c.Symbol))))
            {
                return;
            }
            if (data.OptionChains.Values.First().Contracts.Count == 0)
            {
                throw new Exception($"No contracts found in the option {data.OptionChains.Keys.First()}");
            }

            var deltas = data.OptionChains.Values.OrderByDescending(y => y.Contracts.Values.Sum(x => x.Volume)).First().Contracts.Values.Select(x => x.Greeks.Delta).ToList();
            var gammas = data.OptionChains.Values.OrderByDescending(y => y.Contracts.Values.Sum(x => x.Volume)).First().Contracts.Values.Select(x => x.Greeks.Gamma).ToList();
            var lambda = data.OptionChains.Values.OrderByDescending(y => y.Contracts.Values.Sum(x => x.Volume)).First().Contracts.Values.Select(x => x.Greeks.Lambda).ToList();
            var rho = data.OptionChains.Values.OrderByDescending(y => y.Contracts.Values.Sum(x => x.Volume)).First().Contracts.Values.Select(x => x.Greeks.Rho).ToList();
            var theta = data.OptionChains.Values.OrderByDescending(y => y.Contracts.Values.Sum(x => x.Volume)).First().Contracts.Values.Select(x => x.Greeks.Theta).ToList();
            var vega = data.OptionChains.Values.OrderByDescending(y => y.Contracts.Values.Sum(x => x.Volume)).First().Contracts.Values.Select(x => x.Greeks.Vega).ToList();

            // The commented out test cases all return zero.
            // This is because of failure to evaluate the greeks in the option pricing model, most likely
            // due to us not clearing the default 30 day requirement for the volatility model to start being updated.
            if (deltas.Any(d => d == 0))
            {
                throw new AggregateException("Option contract Delta was equal to zero");
            }
            // Delta is 1, therefore we expect a gamma of 0
            if (gammas.Any(g => deltas.Any() && deltas[0] == 1 ? g != 0 : g == 0))
            {
                throw new AggregateException("Option contract Gamma was equal to zero");
            }
            if (lambda.Any(l => l == 0))
            {
                throw new AggregateException("Option contract Lambda was equal to zero");
            }
            if (rho.Any(r => r == 0))
            {
                throw new AggregateException("Option contract Rho was equal to zero");
            }
            if (theta.Any(t => t == 0))
            {
                throw new AggregateException("Option contract Theta was equal to zero");
            }
            // The strike is far away from the underlying asset's price, and we're very close to expiry.
            // Zero is an expected value here.
            if (vega.Any(v => v != 0))
            {
                throw new AggregateException("Option contract Vega was equal to zero");
            }

            if (!_invested)
            {
                SetHoldings(data.OptionChains.Values.First().Contracts.Values.First().Symbol, 1);
                _invested = true;
            }
        }

        /// <summary>
        /// Ran at the end of the algorithm to ensure the algorithm has no holdings
        /// </summary>
        /// <exception cref="Exception">The algorithm has holdings</exception>
        public override void OnEndOfAlgorithm()
        {
            if (Portfolio.Invested)
            {
                throw new Exception($"Expected no holdings at end of algorithm, but are invested in: {string.Join(", ", Portfolio.Keys)}");
            }
            if (!_invested)
            {
                throw new Exception($"Never checked greeks, maybe we have no option data?");
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
            {"Total Trades", "2"},
            {"Average Win", "0%"},
            {"Average Loss", "-56.91%"},
            {"Compounding Annual Return", "44.906%"},
            {"Drawdown", "9.800%"},
            {"Expectancy", "-1"},
            {"Net Profit", "2.644%"},
            {"Sharpe Ratio", "7.691"},
            {"Probabilistic Sharpe Ratio", "91.027%"},
            {"Loss Rate", "100%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "3.219"},
            {"Beta", "-0.229"},
            {"Annual Standard Deviation", "0.417"},
            {"Annual Variance", "0.174"},
            {"Information Ratio", "6.893"},
            {"Tracking Error", "0.457"},
            {"Treynor Ratio", "-14.008"},
            {"Total Fees", "$0.00"},
            {"Estimated Strategy Capacity", "$44000000.00"},
            {"Lowest Capacity Asset", "SPX XL80P3GHDZXQ|SPX 31"},
            {"Fitness Score", "0.023"},
            {"Kelly Criterion Estimate", "0"},
            {"Kelly Criterion Probability Value", "0"},
            {"Sortino Ratio", "0.535"},
            {"Return Over Maximum Drawdown", "5.789"},
            {"Portfolio Turnover", "0.03"},
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
            {"OrderListHash", "3cccf8c2409ee8a9020ba79a6c45742a"}
        };
    }
}

