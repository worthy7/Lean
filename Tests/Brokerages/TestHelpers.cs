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
using QuantConnect.Data.Market;
using QuantConnect.Securities;

namespace QuantConnect.Tests.Brokerages
{
    public class TestsHelpers
    {
        public static Security GetSecurity(decimal price = 1m, SecurityType securityType = SecurityType.Crypto, Resolution resolution = Resolution.Minute, string symbol = "BTCUSD", string market = Market.GDAX, string quoteCurrency = "USD")
        {
            return new Security(
                SecurityExchangeHours.AlwaysOpen(TimeZones.Utc),
                CreateConfig(symbol, market, securityType, resolution),
                new Cash(quoteCurrency, 1000, price),
                SymbolPropertiesDatabase.FromDataFolder().GetSymbolProperties(market, symbol, SecurityType.Crypto, quoteCurrency),
                ErrorCurrencyConverter.Instance,
                RegisteredSecurityDataTypesProvider.Null,
                new SecurityCache()
            );
        }

        private static SubscriptionDataConfig CreateConfig(string symbol, string market, SecurityType securityType = SecurityType.Crypto, Resolution resolution = Resolution.Minute)
        {
            return new SubscriptionDataConfig(typeof(TradeBar), Symbol.Create(symbol, securityType, market), resolution, TimeZones.Utc, TimeZones.Utc,
            false, true, false);
        }
    }
}
