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

using QuantConnect.Data;
using System.Collections.Generic;
using QuantConnect.Interfaces;
using QuantConnect.Orders;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Basic template India algorithm simply initializes the date range and cash. This is a skeleton
    /// framework you can use for designing an algorithm.
    /// </summary>
    /// <meta name="tag" content="using data" />
    /// <meta name="tag" content="using quantconnect" />
    /// <meta name="tag" content="trading and orders" />
    public class BasicTemplateIndiaAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            SetAccountCurrency("INR");  //Set Account Currency
            SetStartDate(2019, 1, 23);  //Set Start Date
            SetEndDate(2019, 10, 31);   //Set End Date
            SetCash(100000);            //Set Strategy Cash

            // Find more symbols here: http://quantconnect.com/data
            // Equities Resolutions: Tick, Second, Minute, Hour, Daily.
            AddEquity("YESBANK", Resolution.Minute, Market.India);

            //Set Order Prperties as per the requirements for order placement
            DefaultOrderProperties = new IndiaOrderProperties(exchange: Exchange.NSE);
            //override default productType value set in config.json if needed - order specific productType value
            //DefaultOrderProperties = new IndiaOrderProperties(exchange: Exchange.NSE, IndiaOrderProperties.IndiaProductType.CNC);

            // General Debug statement for acknowledgement
            Debug("Intialization Done");
        }

        /// <summary>
        /// OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        /// </summary>
        /// <param name="data">Slice object keyed by symbol containing the stock data</param>
        public override void OnData(Slice data)
        {
            if (!Portfolio.Invested)
            {
                var marketTicket = MarketOrder("YESBANK", 1);
            }
        }

        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            if (orderEvent.Status.IsFill())
            {
                Debug($"Purchased Complete: {orderEvent.Symbol}");
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
        public long DataPoints => 29524;

        /// <summary>
        /// Data Points count of the algorithm history
        /// </summary>
        public int AlgorithmHistoryDataPoints => 0;

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "1"},
            {"Average Win", "0%"},
            {"Average Loss", "0%"},
            {"Compounding Annual Return", "-0.010%"},
            {"Drawdown", "0.000%"},
            {"Expectancy", "0"},
            {"Net Profit", "-0.008%"},
            {"Sharpe Ratio", "-1.183"},
            {"Probabilistic Sharpe Ratio", "0.001%"},
            {"Loss Rate", "0%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "0"},
            {"Beta", "0"},
            {"Annual Standard Deviation", "0"},
            {"Annual Variance", "0"},
            {"Information Ratio", "-1.183"},
            {"Tracking Error", "0"},
            {"Treynor Ratio", "0"},
            {"Total Fees", "₹6.00"},
            {"Estimated Strategy Capacity", "₹61000000000.00"},
            {"Lowest Capacity Asset", "YESBANK UL"},
            {"Fitness Score", "0"},
            {"Kelly Criterion Estimate", "0"},
            {"Kelly Criterion Probability Value", "0"},
            {"Sortino Ratio", "-0.247"},
            {"Return Over Maximum Drawdown", "-1.104"},
            {"Portfolio Turnover", "0"},
            {"Total Insights Generated", "0"},
            {"Total Insights Closed", "0"},
            {"Total Insights Analysis Completed", "0"},
            {"Long Insight Count", "0"},
            {"Short Insight Count", "0"},
            {"Long/Short Ratio", "100%"},
            {"Estimated Monthly Alpha Value", "₹0"},
            {"Total Accumulated Estimated Alpha Value", "₹0"},
            {"Mean Population Estimated Insight Value", "₹0"},
            {"Mean Population Direction", "0%"},
            {"Mean Population Magnitude", "0%"},
            {"Rolling Averaged Population Direction", "0%"},
            {"Rolling Averaged Population Magnitude", "0%"},
            {"OrderListHash", "6cc69218edd7bd461678b9ee0c575db5"}
        };
    }
}
