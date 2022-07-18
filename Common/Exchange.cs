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

using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace QuantConnect
{
    /// <summary>
    /// Lean exchange definition
    /// </summary>
    public class Exchange
    {
        /// <summary>
        /// Unknown exchange value
        /// </summary>
        public static Exchange UNKNOWN { get; } = new (string.Empty, string.Empty, "UNKNOWN", string.Empty);

        /// <summary>
        /// National Association of Securities Dealers Automated Quotation.
        /// </summary>
        public static Exchange NASDAQ { get; }
            = new("NASDAQ", "Q", "National Association of Securities Dealers Automated Quotation", QuantConnect.Market.USA, SecurityType.Equity);

        /// <summary>
        /// Bats Global Markets, Better Alternative Trading System
        /// </summary>
        public static Exchange BATS { get; }
            = new("BATS", "Z", "Bats Global Markets, Better Alternative Trading System", QuantConnect.Market.USA, SecurityType.Equity);

        /// <summary>
        /// New York Stock Archipelago Exchange
        /// </summary>
        public static Exchange ARCA { get; }
            = new("ARCA", "P", "New York Stock Archipelago Exchange", QuantConnect.Market.USA, SecurityType.Equity);

        /// <summary>
        /// New York Stock Exchange
        /// </summary>
        public static Exchange NYSE { get; }
            = new("NYSE", "N", "New York Stock Exchange", QuantConnect.Market.USA, SecurityType.Equity);

        /// <summary>
        /// National Stock Exchange
        /// </summary>
        /// <remarks>Is now known as the NYSE National</remarks>
        public static Exchange NSX { get; }
            = new("NSE", "C", "National Stock Exchange", QuantConnect.Market.USA, SecurityType.Equity);

        /// <summary>
        /// The Financial Industry Regulatory Authority
        /// </summary>
        public static Exchange FINRA { get; }
            = new("FINRA", "D", "The Financial Industry Regulatory Authority", QuantConnect.Market.USA, SecurityType.Equity);

        /// <summary>
        /// Nasdaq International Securities Exchange
        /// </summary>
        public static Exchange ISE { get; }
            = new("ISE", "I", "Nasdaq International Securities Exchange", QuantConnect.Market.USA, SecurityType.Equity);

        /// <summary>
        /// Chicago Stock Exchange
        /// </summary>
        public static Exchange CSE { get; }
            = new("CSE", "M", "Chicago Stock Exchange", QuantConnect.Market.USA, SecurityType.Equity);

        /// <summary>
        /// The Chicago Board Options Exchange
        /// </summary>
        public static Exchange CBOE { get; }
            = new("CBOE", "W", "The Chicago Board Options Exchange", QuantConnect.Market.USA, SecurityType.Equity);

        /// <summary>
        /// The American Options Exchange
        /// </summary>
        public static Exchange NASDAQ_BX { get; }
            = new("NASDAQ_BX", "B", "National Association of Securities Dealers Automated Quotation BX", QuantConnect.Market.USA, SecurityType.Equity);

        /// <summary>
        /// The Securities Industry Automation Corporation
        /// </summary>
        public static Exchange SIAC { get; }
            = new("SIAC", "SIAC", "The Securities Industry Automation Corporation", QuantConnect.Market.USA, SecurityType.Equity);

        /// <summary>
        /// CBOE EDGA U.S. equities Exchange
        /// </summary>
        public static Exchange EDGA { get; }
            = new("EDGA", "J", "CBOE EDGA U.S. equities Exchange", QuantConnect.Market.USA, SecurityType.Equity);

        /// <summary>
        /// CBOE EDGX U.S. equities Exchange
        /// </summary>
        public static Exchange EDGX { get; }
            = new("EDGX", "K", "CBOE EDGX U.S. equities Exchange", QuantConnect.Market.USA, SecurityType.Equity);

        /// <summary>
        /// National Association of Securities Dealers Automated Quotation PSX
        /// </summary>
        public static Exchange NASDAQ_PSX { get; }
            = new("NASDAQ_PSX", "X", "National Association of Securities Dealers Automated Quotation PSX", QuantConnect.Market.USA, SecurityType.Equity);

        /// <summary>
        /// National Association of Securities Dealers Automated Quotation PSX
        /// </summary>
        public static Exchange BATS_Y { get; }
            = new("BATS_Y", "Y", "Bats Global Markets, Better Alternative Trading System", QuantConnect.Market.USA, SecurityType.Equity);

        /// <summary>
        /// The Boston Stock Exchange
        /// </summary>
        /// <remarks>Now NASDAQ OMX BX</remarks>
        public static Exchange BOSTON { get; }
            = new("BOSTON", "B", "The Boston Stock Exchange", QuantConnect.Market.USA, SecurityType.Equity);

        /// <summary>
        /// The American Stock Exchange
        /// </summary>
        /// <remarks>Now NYSE MKT</remarks>
        public static Exchange AMEX { get; }
            = new("AMEX", "A", "The American Stock Exchange", QuantConnect.Market.USA, SecurityType.Equity);

        /// <summary>
        /// Bombay Stock Exchange
        /// </summary>
        public static Exchange BSE { get; }
            = new("BSE", string.Empty, "Bombay Stock Exchange", QuantConnect.Market.India, SecurityType.Equity);

        /// <summary>
        /// National Stock Exchange of India
        /// </summary>
        public static Exchange NSE { get; }
            = new("NSE", string.Empty, "National Stock Exchange of India", QuantConnect.Market.India, SecurityType.Equity);

        /// <summary>
        /// The American Options Exchange
        /// </summary>
        /// <remarks>Now NYSE Amex Options</remarks>
        public static Exchange AMEX_Options { get; }
            = new("AMEX", "A", "The American Options Exchange", QuantConnect.Market.USA, SecurityType.Option);

        /// <summary>
        /// The Options Price Reporting Authority
        /// </summary>
        public static Exchange OPRA { get; }
            = new("OPRA", "O", "The Options Price Reporting Authority", QuantConnect.Market.USA, SecurityType.Option);

        /// <summary>
        /// CBOE Options Exchange
        /// </summary>
        public static Exchange C2 { get; }
            = new("C2", "W", "CBOE Options Exchange", QuantConnect.Market.USA, SecurityType.Option);

        /// <summary>
        /// Miami International Securities Options Exchange
        /// </summary>
        public static Exchange MIAX { get; }
            = new("MIAX", "M", "Miami International Securities Options Exchange", QuantConnect.Market.USA, SecurityType.Option);

        /// <summary>
        /// International Securities Options Exchange GEMINI
        /// </summary>
        public static Exchange ISE_GEMINI { get; }
            = new("ISE_GEMINI", "H", "International Securities Options Exchange GEMINI", QuantConnect.Market.USA, SecurityType.Option);

        /// <summary>
        /// International Securities Options Exchange MERCURY
        /// </summary>
        public static Exchange ISE_MERCURY { get; }
            = new("ISE_MERCURY", "J", "International Securities Options Exchange MERCURY", QuantConnect.Market.USA, SecurityType.Option);

        /// <summary>
        /// The Chicago Mercantile Exchange (CME), is an organized exchange for the trading of futures and options.
        /// </summary>
        public static Exchange CME { get; }
            = new("CME", "CME", "Futures and Options Chicago Mercantile Exchange", QuantConnect.Market.CME, SecurityType.Future, SecurityType.FutureOption);

        /// <summary>
        /// The Chicago Board of Trade (CBOT) is a commodity exchange
        /// </summary>
        public static Exchange CBOT { get; }
            = new("CBOT", "CBOT", " Chicago Board of Trade Commodity Exchange", QuantConnect.Market.CBOT, SecurityType.Future, SecurityType.FutureOption);

        /// <summary>
        /// Cboe Futures Exchange
        /// </summary>
        public static Exchange CFE { get; }
            = new("CFE", "CFE", "CFE Futures Exchange", QuantConnect.Market.CFE, SecurityType.Future);

        /// <summary>
        /// COMEX Commodity Exchange
        /// </summary>
        public static Exchange COMEX { get; }
            = new("COMEX", "COMEX", "COMEX Futures Exchange", QuantConnect.Market.COMEX, SecurityType.Future);

        /// <summary>
        /// The Intercontinental Exchange
        /// </summary>
        public static Exchange ICE { get; }
            = new("ICE", "ICE", "The Intercontinental Exchange", QuantConnect.Market.ICE, SecurityType.Future);

        /// <summary>
        /// New York Mercantile Exchange
        /// </summary>
        public static Exchange NYMEX { get; }
            = new("NYMEX", "NYMEX", "New York Mercantile Exchange", QuantConnect.Market.NYMEX, SecurityType.Future, SecurityType.FutureOption);

        /// <summary>
        /// London International Financial Futures and Options Exchange
        /// </summary>
        public static Exchange NYSELIFFE { get; }
            = new("NYSELIFFE", "NYSELIFFE", "London International Financial Futures and Options Exchange", QuantConnect.Market.NYSELIFFE, SecurityType.Future, SecurityType.FutureOption);

        /// <summary>
        /// Exchange description
        /// </summary>
        [JsonIgnore]
        public string Description { get; }

        /// <summary>
        /// The exchange short code
        /// </summary>
        public string Code { get; init; }

        /// <summary>
        /// The exchange name
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// The associated lean market <see cref="Market"/>
        /// </summary>
        public string Market { get; init; }

        /// <summary>
        /// Security types traded in this exchange
        /// </summary>
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public IReadOnlyList<SecurityType> SecurityTypes { get; init; } = new List<SecurityType>();

        /// <summary>
        /// Creates a new empty exchange instance
        /// </summary>
        /// <remarks>For json round trip serialization</remarks>
        private Exchange()
        {
        }

        /// <summary>
        /// Creates a new exchange instance
        /// </summary>
        private Exchange(string name, string code, string description, string market, params SecurityType[] securityTypes)
        {
            Name = name;
            Market = market;
            Description = description;
            SecurityTypes = securityTypes?.ToList() ?? new List<SecurityType>();
            Code = string.IsNullOrEmpty(code) ? name : code;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Returns the string representation of this exchange
        /// </summary>
        public static implicit operator string(Exchange exchange)
        {
            return ReferenceEquals(exchange, null) ? string.Empty : exchange.ToString();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            var exchange = obj as Exchange;
            if (ReferenceEquals(exchange, null) || ReferenceEquals(exchange, UNKNOWN))
            {
                // other is null or UNKNOWN (equivalents)
                // so we need to know how We compare with UNKNOWN
                return ReferenceEquals(this, UNKNOWN);
            }

            return Code == exchange.Code
                && Market == exchange.Market
                && SecurityTypes.All(exchange.SecurityTypes.Contains)
                && SecurityTypes.Count == exchange.SecurityTypes.Count;
        }

        /// <summary>
        /// Equals operator
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>True if both symbols are equal, otherwise false</returns>
        public static bool operator ==(Exchange left, Exchange right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }
            if (ReferenceEquals(left, null) || left.Equals(UNKNOWN))
            {
                return ReferenceEquals(right, null) || right.Equals(UNKNOWN);
            }
            return left.Equals(right);
        }

        /// <summary>
        /// Not equals operator
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>True if both symbols are not equal, otherwise false</returns>
        public static bool operator !=(Exchange left, Exchange right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Code.GetHashCode();
                hashCode = (hashCode * 397) ^ Market.GetHashCode();
                for (var i = 0; i < SecurityTypes.Count; i++)
                {
                    hashCode = (hashCode * 397) ^ SecurityTypes[i].GetHashCode();
                }
                return hashCode;
            }
        }
    }
}
