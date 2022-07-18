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
using QuantConnect.Util;
using QuantConnect.Logging;
using QuantConnect.Interfaces;
using QuantConnect.Securities;
using System.Collections.Generic;

namespace QuantConnect.Lean.Engine.DataFeeds
{
    /// <summary>
    /// Base backtesting cache provider which will source symbols from local zip files
    /// </summary>
    public abstract class BacktestingChainProvider
    {
        // see https://github.com/QuantConnect/Lean/issues/6384
        private static readonly TickType[] DataTypes = new[] { TickType.Quote, TickType.OpenInterest, TickType.Trade };
        private bool _loggedPreviousTradableDate;

        /// <summary>
        /// The data cache instance to use
        /// </summary>
        protected IDataCacheProvider DataCacheProvider { get; }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        protected BacktestingChainProvider(IDataCacheProvider dataCacheProvider)
        {
            DataCacheProvider = dataCacheProvider;
        }

        /// <summary>
        /// Get the contract symbols associated with the given canonical symbol and date
        /// </summary>
        /// <param name="canonicalSymbol">The canonical symbol</param>
        /// <param name="date">The date to search for</param>
        protected IEnumerable<Symbol> GetSymbols(Symbol canonicalSymbol, DateTime date)
        {
            IEnumerable<string> entries = null;
            foreach (var tickType in DataTypes)
            {
                // build the zip file name and fetch it with our provider
                var zipFileName = LeanData.GenerateZipFilePath(Globals.DataFolder, canonicalSymbol, date, Resolution.Minute, tickType);
                try
                {
                    entries = DataCacheProvider.GetZipEntries(zipFileName);
                }
                catch
                {
                    // the cache provider will throw if the file isn't available TODO: it's api should be more like TryGetZipEntries
                }

                if (entries != null)
                {
                    break;
                }
            }

            if (entries == null)
            {
                var mhdb = MarketHoursDatabase.FromDataFolder();
                if (mhdb.TryGetEntry(canonicalSymbol.ID.Market, canonicalSymbol, canonicalSymbol.SecurityType, out var entry) && !entry.ExchangeHours.IsDateOpen(date))
                {
                    if (!_loggedPreviousTradableDate)
                    {
                        _loggedPreviousTradableDate = true;
                        Log.Trace($"BacktestingCacheProvider.GetSymbols(): {date} is not a tradable date for {canonicalSymbol}. When requesting contracts" +
                            $" for non tradable dates, will return contracts of previous tradable date.");
                    }

                    // be user friendly, will return contracts from the previous tradable date
                    foreach (var symbols in GetSymbols(canonicalSymbol, Time.GetStartTimeForTradeBars(entry.ExchangeHours, date, Time.OneDay, 1, false, entry.DataTimeZone)))
                    {
                        yield return symbols;
                    }
                    yield break;
                }

                if (Log.DebuggingEnabled)
                {
                    Log.Debug($"BacktestingCacheProvider.GetSymbols(): found no source of contracts for {canonicalSymbol} for date {date.ToString(DateFormat.EightCharacter)} for any tick type");
                }
                yield break;
            }

            // generate and return the contract symbol for each zip entry
            foreach (var zipEntryName in entries)
            {
                yield return LeanData.ReadSymbolFromZipEntry(canonicalSymbol, Resolution.Minute, zipEntryName);
            }
        }
    }
}
