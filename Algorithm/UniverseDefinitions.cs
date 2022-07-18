﻿/*
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
using System.Collections.Generic;
using System.Linq;
using Python.Runtime;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;

namespace QuantConnect.Algorithm
{
    /// <summary>
    /// Provides helpers for defining universes in algorithms
    /// </summary>
    public class UniverseDefinitions
    {
        private readonly QCAlgorithm _algorithm;
        
        /// <summary>
        /// Gets a helper that provides methods for creating universes based on daily dollar volumes
        /// </summary>
        public DollarVolumeUniverseDefinitions DollarVolume { get; set; }
        
        /// <summary>
        /// Specifies that universe selection should not make changes on this iteration
        /// </summary>
        public Universe.UnchangedUniverse Unchanged => Universe.Unchanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniverseDefinitions"/> class
        /// </summary>
        /// <param name="algorithm">The algorithm instance, used for obtaining the default <see cref="UniverseSettings"/></param>
        public UniverseDefinitions(QCAlgorithm algorithm)
        {
            _algorithm = algorithm;
            DollarVolume = new DollarVolumeUniverseDefinitions(algorithm);
        }
        
        /// <summary>
        /// Creates a universe for the constituents of the provided <paramref name="etfTicker"/>
        /// </summary>
        /// <param name="etfTicker">Ticker of the ETF to get constituents for</param>
        /// <param name="market">Market of the ETF</param>
        /// <param name="universeSettings">Universe settings</param>
        /// <param name="universeFilterFunc">Function to filter universe results</param>
        /// <returns>New ETF constituents Universe</returns>
        public Universe ETF(
            string etfTicker, 
            string market = null, 
            UniverseSettings universeSettings = null, 
            Func<IEnumerable<ETFConstituentData>, IEnumerable<Symbol>> universeFilterFunc = null)
        {
            market ??= _algorithm.BrokerageModel.DefaultMarkets.TryGetValue(SecurityType.Equity, out var defaultMarket)
                ? defaultMarket
                : throw new Exception("No default market set for security type: Equity");

            var etfSymbol = new Symbol(
                SecurityIdentifier.GenerateEquity(
                    etfTicker,
                    market,
                    true,
                    mappingResolveDate: _algorithm.Time.Date),
                etfTicker);

            return ETF(etfSymbol, universeSettings, universeFilterFunc);
        }


        /// <summary>
        /// Creates a universe for the constituents of the provided <paramref name="etfTicker"/>
        /// </summary>
        /// <param name="etfTicker">Ticker of the ETF to get constituents for</param>
        /// <param name="market">Market of the ETF</param>
        /// <param name="universeSettings">Universe settings</param>
        /// <param name="universeFilterFunc">Function to filter universe results</param>
        /// <returns>New ETF constituents Universe</returns>
        public Universe ETF(
            string etfTicker,
            string market = null,
            UniverseSettings universeSettings = null,
            PyObject universeFilterFunc = null)
        {
            return ETF(etfTicker, market, universeSettings, universeFilterFunc.ConvertPythonUniverseFilterFunction<ETFConstituentData>());
        }

        /// <summary>
        /// Creates a universe for the constituents of the provided ETF <paramref name="symbol"/>
        /// </summary>
        /// <param name="symbol">ETF Symbol to get constituents for</param>
        /// <param name="universeSettings">Universe settings</param>
        /// <param name="universeFilterFunc">Function to filter universe results</param>
        /// <returns>New ETF constituents Universe</returns>
        public Universe ETF(
            Symbol symbol,
            UniverseSettings universeSettings = null, 
            Func<IEnumerable<ETFConstituentData>, IEnumerable<Symbol>> universeFilterFunc = null)
        {
            return new ETFConstituentsUniverse(symbol, universeSettings ?? _algorithm.UniverseSettings, universeFilterFunc);
        }
        
        /// <summary>
        /// Creates a universe for the constituents of the provided ETF <paramref name="symbol"/>
        /// </summary>
        /// <param name="symbol">ETF Symbol to get constituents for</param>
        /// <param name="universeSettings">Universe settings</param>
        /// <param name="universeFilterFunc">Function to filter universe results</param>
        /// <returns>New ETF constituents Universe</returns>
        public Universe ETF(
            Symbol symbol,
            UniverseSettings universeSettings = null, 
            PyObject universeFilterFunc = null)
        {
            return ETF(symbol, universeSettings, universeFilterFunc.ConvertPythonUniverseFilterFunction<ETFConstituentData>());
        }

        /// <summary>
        /// Creates a universe for the constituents of the provided <paramref name="indexTicker"/>
        /// </summary>
        /// <param name="indexTicker">Ticker of the index to get constituents for</param>
        /// <param name="market">Market of the index</param>
        /// <param name="universeSettings">Universe settings</param>
        /// <param name="universeFilterFunc">Function to filter universe results</param>
        /// <returns>New index constituents Universe</returns>
        public Universe Index(
            string indexTicker,
            string market = null,
            UniverseSettings universeSettings = null,
            Func<IEnumerable<ETFConstituentData>, IEnumerable<Symbol>> universeFilterFunc = null)
        {
            market ??= _algorithm.BrokerageModel.DefaultMarkets.TryGetValue(SecurityType.Index, out var defaultMarket)
                ? defaultMarket
                : throw new Exception("No default market set for security type: Index");

            return Index(
                Symbol.Create(indexTicker, SecurityType.Index, market),
                universeSettings, 
                universeFilterFunc);
        }
                
        /// <summary>
        /// Creates a universe for the constituents of the provided <paramref name="indexTicker"/>
        /// </summary>
        /// <param name="indexTicker">Ticker of the index to get constituents for</param>
        /// <param name="market">Market of the index</param>
        /// <param name="universeSettings">Universe settings</param>
        /// <param name="universeFilterFunc">Function to filter universe results</param>
        /// <returns>New index constituents Universe</returns>
        public Universe Index(
            string indexTicker,
            string market = null,
            UniverseSettings universeSettings = null,
            PyObject universeFilterFunc = null)
        {
            return Index(indexTicker, market, universeSettings, universeFilterFunc.ConvertPythonUniverseFilterFunction<ETFConstituentData>());
        }

        /// <summary>
        /// Creates a universe for the constituents of the provided <paramref name="indexSymbol"/>
        /// </summary>
        /// <param name="indexSymbol">Index Symbol to get constituents for</param>
        /// <param name="universeSettings">Universe settings</param>
        /// <param name="universeFilterFunc">Function to filter universe results</param>
        /// <returns>New index constituents Universe</returns>
        public Universe Index(
            Symbol indexSymbol,
            UniverseSettings universeSettings = null,
            Func<IEnumerable<ETFConstituentData>, IEnumerable<Symbol>> universeFilterFunc = null)
        {
            return new ETFConstituentsUniverse(indexSymbol, universeSettings ?? _algorithm.UniverseSettings, universeFilterFunc);
        }
        
        /// <summary>
        /// Creates a universe for the constituents of the provided <paramref name="indexSymbol"/>
        /// </summary>
        /// <param name="indexSymbol">Index Symbol to get constituents for</param>
        /// <param name="universeSettings">Universe settings</param>
        /// <param name="universeFilterFunc">Function to filter universe results</param>
        /// <returns>New index constituents Universe</returns>
        public Universe Index(
            Symbol indexSymbol,
            UniverseSettings universeSettings = null,
            PyObject universeFilterFunc = null)
        {
            return Index(indexSymbol, universeSettings, universeFilterFunc.ConvertPythonUniverseFilterFunction<ETFConstituentData>());
        }
        
        /// <summary>
        /// Creates a new fine universe that contains the constituents of QC500 index based onthe company fundamentals
        /// The algorithm creates a default tradable and liquid universe containing 500 US equities
        /// which are chosen at the first trading day of each month.
        /// </summary>
        /// <returns>A new coarse universe for the top count of stocks by dollar volume</returns>
        public Universe QC500
        {
            get
            {
                var lastMonth = -1;
                var numberOfSymbolsCoarse = 1000;
                var numberOfSymbolsFine = 500;
                var dollarVolumeBySymbol = new Dictionary<Symbol, decimal>();
                var symbol = Symbol.Create("qc-500", SecurityType.Equity, Market.USA);

                var coarseUniverse = new CoarseFundamentalUniverse(
                    symbol,
                    _algorithm.UniverseSettings,
                    coarse =>
                    {
                        if (_algorithm.Time.Month == lastMonth)
                        {
                            return Universe.Unchanged;
                        }

                        // The stocks must have fundamental data
                        // The stock must have positive previous-day close price
                        // The stock must have positive volume on the previous trading day
                        var sortedByDollarVolume =
                            (from x in coarse
                                where x.HasFundamentalData && x.Volume > 0 && x.Price > 0
                                orderby x.DollarVolume descending
                                select x).Take(numberOfSymbolsCoarse).ToList();

                        dollarVolumeBySymbol.Clear();
                        foreach (var x in sortedByDollarVolume)
                        {
                            dollarVolumeBySymbol[x.Symbol] = x.DollarVolume;
                        }

                        // If no security has met the QC500 criteria, the universe is unchanged.
                        // A new selection will be attempted on the next trading day as lastMonth is not updated
                        if (dollarVolumeBySymbol.Count == 0)
                        {
                            return Universe.Unchanged;
                        }

                        return dollarVolumeBySymbol.Keys;
                    });

                return new FineFundamentalFilteredUniverse(
                    coarseUniverse,
                    fine =>
                    {
                        // The company's headquarter must in the U.S.
                        // The stock must be traded on either the NYSE or NASDAQ 
                        // At least half a year since its initial public offering
                        // The stock's market cap must be greater than 500 million
                        var filteredFine =
                            (from x in fine
                                where x.CompanyReference.CountryId == "USA" &&
                                    (x.CompanyReference.PrimaryExchangeID == "NYS" ||
                                        x.CompanyReference.PrimaryExchangeID == "NAS") &&
                                    (_algorithm.Time - x.SecurityReference.IPODate).Days > 180 &&
                                    x.MarketCap > 500000000m
                                select x).ToList();

                        var count = filteredFine.Count;

                        // If no security has met the QC500 criteria, the universe is unchanged.
                        // A new selection will be attempted on the next trading day as lastMonth is not updated
                        if (count == 0)
                        {
                            return Universe.Unchanged;
                        }

                        // Update _lastMonth after all QC500 criteria checks passed
                        lastMonth = _algorithm.Time.Month;

                        var percent = numberOfSymbolsFine / (double) count;

                        // select stocks with top dollar volume in every single sector 
                        var topFineBySector =
                            (from x in filteredFine
                                // Group by sector
                                group x by x.CompanyReference.IndustryTemplateCode
                                into g
                                let y = from item in g
                                    orderby dollarVolumeBySymbol[item.Symbol] descending
                                    select item
                                let c = (int) Math.Ceiling(y.Count() * percent)
                                select new {g.Key, Value = y.Take(c)}
                            ).ToDictionary(x => x.Key, x => x.Value);

                        return topFineBySector.SelectMany(x => x.Value)
                            .OrderByDescending(x => dollarVolumeBySymbol[x.Symbol])
                            .Take(numberOfSymbolsFine)
                            .Select(x => x.Symbol);
                    });
            }
        }

        /// <summary>
        /// Creates a new coarse universe that contains the top count of stocks
        /// by daily dollar volume
        /// </summary>
        /// <param name="count">The number of stock to select</param>
        /// <param name="universeSettings">The settings for stocks added by this universe.
        /// Defaults to <see cref="QCAlgorithm.UniverseSettings"/></param>
        /// <returns>A new coarse universe for the top count of stocks by dollar volume</returns>
        public Universe Top(int count, UniverseSettings universeSettings = null)
        {
            universeSettings ??= _algorithm.UniverseSettings;
            
            var symbol = Symbol.Create("us-equity-dollar-volume-top-" + count, SecurityType.Equity, Market.USA);
            var config = new SubscriptionDataConfig(typeof(CoarseFundamental), symbol, Resolution.Daily, TimeZones.NewYork, TimeZones.NewYork, false, false, true);
            return new FuncUniverse(config, universeSettings, selectionData => (
                from c in selectionData.OfType<CoarseFundamental>()
                orderby c.DollarVolume descending
                select c.Symbol).Take(count)
                );
        }
    }
}
