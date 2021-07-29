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

namespace QuantConnect.Securities.Positions
{
    /// <summary>
    /// Defines parameters for <see cref="IPositionGroupBuyingPowerModel.GetInitialMarginRequirement"/>
    /// </summary>
    public class PositionGroupInitialMarginParameters
    {
        /// <summary>
        /// Gets the algorithm's portfolio manager
        /// </summary>
        public SecurityPortfolioManager Portfolio { get; }

        /// <summary>
        /// Gets the position group
        /// </summary>
        public IPositionGroup PositionGroup { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PositionGroupInitialMarginParameters"/> class
        /// </summary>
        /// <param name="portfolio">The algorithm's portfolio manager</param>
        /// <param name="positionGroup">The position group</param>
        public PositionGroupInitialMarginParameters(
            SecurityPortfolioManager portfolio,
            IPositionGroup positionGroup
            )
        {
            Portfolio = portfolio;
            PositionGroup = positionGroup;
        }
    }
}
