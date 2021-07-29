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
*/

using System;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Orders;
using QuantConnect.Orders.Fees;
using QuantConnect.Orders.TimeInForces;
using QuantConnect.Securities;
using QuantConnect.Util;
using static QuantConnect.StringExtensions;


namespace QuantConnect.Brokerages
{
    /// <summary>
    /// Brokerage Model implementation for Zerodha
    /// </summary>
    public class ZerodhaBrokerageModel : DefaultBrokerageModel
    {
        private readonly Type[] _supportedTimeInForces =
        {
            typeof(GoodTilCanceledTimeInForce),
            typeof(DayTimeInForce),
            typeof(GoodTilDateTimeInForce)
        };

        private const decimal _maxLeverage = 7m;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZerodhaBrokerageModel"/> class
        /// </summary>
        /// <param name="accountType">The type of account to be modelled, defaults to
        /// <see cref="AccountType.Margin"/></param>
        public ZerodhaBrokerageModel(AccountType accountType = AccountType.Margin) : base(accountType)
        {
        }

        /// <summary>
        /// Returns true if the brokerage would be able to execute this order at this time assuming
        /// market prices are sufficient for the fill to take place. This is used to emulate the
        /// brokerage fills in backtesting and paper trading. For example some brokerages may not perform
        /// executions during extended market hours. This is not intended to be checking whether or not
        /// the exchange is open, that is handled in the Security.Exchange property.
        /// </summary>
        /// <param name="security"></param>
        /// <param name="order">The order to test for execution</param>
        /// <returns>True if the brokerage would be able to perform the execution, false otherwise</returns>
        public override bool CanExecuteOrder(Security security, Order order)
        {

            // validate security type
            if (security.Type != SecurityType.Equity)
            {
                return false;
            }

           
            // validate time in force
            if (!_supportedTimeInForces.Contains(order.TimeInForce.GetType()))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if the brokerage could accept this order. This takes into account
        /// order type, security type, and order size limits.
        /// </summary>
        /// <remarks>
        /// For example, a brokerage may have no connectivity at certain times, or an order rate/size limit
        /// </remarks>
        /// <param name="security">The security being ordered</param>
        /// <param name="order">The order to be processed</param>
        /// <param name="message">If this function returns false, a brokerage message detailing why the order may not be submitted</param>
        /// <returns>True if the brokerage could process the order, false otherwise</returns>
        public override bool CanSubmitOrder(Security security, Order order, out BrokerageMessageEvent message)
        {
            message = null;

            // validate security type
            if (security.Type != SecurityType.Equity)
            {
                message = new BrokerageMessageEvent(BrokerageMessageType.Warning, "NotSupported",
                    Invariant($"The {nameof(ZerodhaBrokerageModel)} does not support {security.Type} security type.")
                );

                return false;
            }

           
            // validate time in force
            if (!_supportedTimeInForces.Contains(order.TimeInForce.GetType()))
            {
                message = new BrokerageMessageEvent(BrokerageMessageType.Warning, "NotSupported",
                    Invariant($"The {nameof(ZerodhaBrokerageModel)} does not support {order.TimeInForce.GetType().Name} time in force.")
                );

                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if the brokerage would allow updating the order as specified by the request
        /// </summary>
        /// <param name="security">The security of the order</param>
        /// <param name="order">The order to be updated</param>
        /// <param name="request">The requested update to be made to the order</param>
        /// <param name="message">If this function returns false, a brokerage message detailing why the order may not be updated</param>
        /// <returns>True if the brokerage would allow updating the order, false otherwise</returns>
        public override bool CanUpdateOrder(Security security, Order order, UpdateOrderRequest request, out BrokerageMessageEvent message)
        {
            message = null;
             return true;
        }

        /// <summary>
        /// Gets a map of the default markets to be used for each security type
        /// </summary>
        public override IReadOnlyDictionary<SecurityType, string> DefaultMarkets { get; } = GetDefaultMarkets();



        /// <summary>
        /// Gets a new buying power model for the security, returning the default model with the security's configured leverage.
        /// For cash accounts, leverage = 1 is used.
        /// For margin trading, max leverage = 7
        /// </summary>
        /// <param name="security">The security to get a buying power model for</param>
        /// <returns>The buying power model for this brokerage/security</returns>
        public override IBuyingPowerModel GetBuyingPowerModel(Security security)
        {
            return AccountType == AccountType.Cash
                ? (IBuyingPowerModel)new CashBuyingPowerModel()
                : new SecurityMarginModel(_maxLeverage);
        }

        /// <summary>
        /// Zerodha global leverage rule
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        public override decimal GetLeverage(Security security)
        {
            if (AccountType == AccountType.Cash || security.IsInternalFeed() || security.Type == SecurityType.Base)
            {
                return 1m;
            }

            if (security.Type == SecurityType.Equity || security.Type == SecurityType.Future || security.Type == SecurityType.Option)
            {
                return _maxLeverage;
            }

            throw new ArgumentException($"Invalid security type: {security.Type}", nameof(security));
        }

        /// <summary>
        /// Provides Zerodha fee model
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        public override IFeeModel GetFeeModel(Security security)
        {
            return new ZerodhaFeeModel();
        }

        private static IReadOnlyDictionary<SecurityType, string> GetDefaultMarkets()
        {
            var map = DefaultMarketMap.ToDictionary();
            map[SecurityType.Equity] = Market.India;

            return map.ToReadOnlyDictionary();
        }
    }
}
