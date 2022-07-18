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

namespace QuantConnect.Securities.Option
{
    /// <summary>
    /// Static class contains common utility methods specific to symbols representing the option contracts
    /// </summary>
    public static class OptionSymbol
    {
        private static readonly Dictionary<string, byte> _optionExpirationErrorLog = new();

        /// <summary>
        /// Returns true if the option is a standard contract that expires 3rd Friday of the month
        /// </summary>
        /// <param name="symbol">Option symbol</param>
        /// <returns></returns>
        public static bool IsStandardContract(Symbol symbol)
        {
            return IsStandard(symbol);
        }

        /// <summary>
        /// Returns true if the option is a standard contract that expires 3rd Friday of the month
        /// </summary>
        /// <param name="symbol">Option symbol</param>
        /// <returns></returns>
        public static bool IsStandard(Symbol symbol)
        {
            var date = symbol.ID.Date;

            // first we find out the day of week of the first day in the month
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1).DayOfWeek;

            // find out the day of first Friday in this month
            var firstFriday = firstDayOfMonth == DayOfWeek.Saturday ? 7 : 6 - (int)firstDayOfMonth;

            // check if the expiration date is within the week containing 3rd Friday
            // we exclude monday, wednesday, and friday weeklys
            return firstFriday + 7 + 5 /*sat -> wed */ < date.Day && date.Day < firstFriday + 2 * 7 + 2 /* sat, sun*/;
        }

        /// <summary>
        /// Returns true if the option is a weekly contract that expires on Friday , except 3rd Friday of the month
        /// </summary>
        /// <param name="symbol">Option symbol</param>
        /// <returns></returns>
        public static bool IsWeekly(Symbol symbol)
        {
            return !IsStandard(symbol) && symbol.ID.Date.DayOfWeek == DayOfWeek.Friday;
        }

        /// <summary>
        /// Returns the last trading date for the option contract
        /// </summary>
        /// <param name="symbol">Option symbol</param>
        /// <returns></returns>
        public static DateTime GetLastDayOfTrading(Symbol symbol)
        {
            // The OCC proposed rule change: starting from 1 Feb 2015 standard monthly contracts
            // expire on 3rd Friday, not Saturday following 3rd Friday as it was before.
            // More details: https://www.sec.gov/rules/sro/occ/2013/34-69480.pdf

            int daysBefore = 0;
            var symbolDateTime = symbol.ID.Date;

            if (IsStandard(symbol) &&
                symbolDateTime.DayOfWeek == DayOfWeek.Saturday &&
                symbolDateTime < new DateTime(2015, 2, 1))
            {
                daysBefore--;
            }

            var exchangeHours = MarketHoursDatabase.FromDataFolder()
                                              .GetEntry(symbol.ID.Market, symbol, symbol.SecurityType)
                                              .ExchangeHours;

            while (!exchangeHours.IsDateOpen(symbolDateTime.AddDays(daysBefore)))
            {
                daysBefore--;
            }

            return symbolDateTime.AddDays(daysBefore).Date;
        }

        /// <summary>
        /// Returns true if the option contract is expired at the specified time
        /// </summary>
        /// <param name="symbol">The option contract symbol</param>
        /// <param name="currentTimeUtc">The current time (UTC)</param>
        /// <returns>True if the option contract is expired at the specified time, false otherwise</returns>
        public static bool IsOptionContractExpired(Symbol symbol, DateTime currentTimeUtc)
        {
            if (!symbol.SecurityType.IsOption())
            {
                return false;
            }

            var exchangeHours = MarketHoursDatabase.FromDataFolder()
                .GetExchangeHours(symbol.ID.Market, symbol, symbol.SecurityType);

            var currentTime = currentTimeUtc.ConvertFromUtc(exchangeHours.TimeZone);

            // Ideally we can calculate expiry on the date of the symbol ID, but if that exchange is not open on that day we 
            // will consider expired on the last trading day close before this; Example in AddOptionContractExpiresRegressionAlgorithm
            var expiryDay = exchangeHours.IsDateOpen(symbol.ID.Date)
                ? symbol.ID.Date
                : exchangeHours.GetPreviousTradingDay(symbol.ID.Date);

            var expiryTime = exchangeHours.GetNextMarketClose(expiryDay, false);

            // Once bug 6189 was solved in ´GetNextMarketClose()´ there was found possible bugs on some futures symbol.ID.Date or delisting/liquidation handle event.
            // Specifically see 'DelistingFutureOptionRegressionAlgorithm' where Symbol.ID.Date: 4/1/2012 00:00 ExpiryTime: 4/2/2012 16:00 for Milk 3 futures options.
            // See 'bug-milk-class-3-future-options-expiration' branch. So let's limit the expiry time to up to end of day of expiration
            if (expiryTime >= symbol.ID.Date.AddDays(1).Date)
            {
                lock (_optionExpirationErrorLog)
                {
                    if (symbol.ID.Underlying != null
                        // let's log this once per underlying and expiration date: avoiding the same log for multiple option contracts with different strikes/rights
                        && _optionExpirationErrorLog.TryAdd($"{symbol.ID.Underlying}-{symbol.ID.Date}", 1))
                    {
                        Logging.Log.Error($"OptionSymbol.IsOptionContractExpired(): limiting unexpected option expiration time for symbol {symbol.ID}. Symbol.ID.Date {symbol.ID.Date}. ExpiryTime: {expiryTime}");
                    }
                }
                expiryTime = symbol.ID.Date.AddDays(1).Date;
            }

            return currentTime >= expiryTime;
        }
    }
}
