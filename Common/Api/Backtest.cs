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
using Newtonsoft.Json;
using QuantConnect.Statistics;
using System.Collections.Generic;
using QuantConnect.Optimizer.Parameters;

namespace QuantConnect.Api
{
    /// <summary>
    /// Backtest response packet from the QuantConnect.com API.
    /// </summary>
    public class Backtest : RestResponse
    {
        /// <summary>
        /// Name of the backtest
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Note on the backtest attached by the user
        /// </summary>
        [JsonProperty(PropertyName = "note")]
        public string Note { get; set; }

        /// <summary>
        /// Assigned backtest Id
        /// </summary>
        [JsonProperty(PropertyName = "backtestId")]
        public string BacktestId { get; set; }

        /// <summary>
        /// Boolean true when the backtest is completed.
        /// </summary>
        [JsonProperty(PropertyName = "completed")]
        public bool Completed { get; set; }

        /// <summary>
        /// Progress of the backtest in percent 0-1.
        /// </summary>
        [JsonProperty(PropertyName = "progress")]
        public decimal Progress { get; set; }

        /// <summary>
        /// Backtest error message
        /// </summary>
        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }

        /// <summary>
        /// Backtest error stacktrace
        /// </summary>
        [JsonProperty(PropertyName = "stacktrace")]
        public string StackTrace { get; set; }

        /// <summary>
        /// Backtest creation date and time
        /// </summary>
        [JsonProperty(PropertyName = "created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// Rolling window detailed statistics.
        /// </summary>
        [JsonProperty(PropertyName = "rollingWindow", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, AlgorithmPerformance> RollingWindow { get; set; }

        /// <summary>
        /// Rolling window detailed statistics.
        /// </summary>
        [JsonProperty(PropertyName = "totalPerformance", NullValueHandling = NullValueHandling.Ignore)]
        public AlgorithmPerformance TotalPerformance { get; set; }

        /// <summary>
        /// Contains population averages scores over the life of the algorithm
        /// </summary>
        [JsonProperty(PropertyName = "alphaRuntimeStatistics", NullValueHandling = NullValueHandling.Ignore)]
        public AlphaRuntimeStatistics AlphaRuntimeStatistics { get; set; }

        /// <summary>
        /// Charts updates for the live algorithm since the last result packet
        /// </summary>
        [JsonProperty(PropertyName = "charts", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, Chart> Charts { get; set; }

        /// <summary>
        /// Statistics information sent during the algorithm operations.
        /// </summary>
        /// <remarks>Intended for update mode -- send updates to the existing statistics in the result GUI. If statistic key does not exist in GUI, create it</remarks>
        [JsonProperty(PropertyName = "statistics", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, string> Statistics { get; set; }

        /// <summary>
        /// Runtime banner/updating statistics in the title banner of the live algorithm GUI.
        /// </summary>
        [JsonProperty(PropertyName = "runtimeStatistics", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, string> RuntimeStatistics { get; set; }

        /// <summary>
        /// Optimization parameters
        /// </summary>
        [JsonProperty(PropertyName = "parameterSet")]
        public ParameterSet ParameterSet { get; set; }
    }

    /// <summary>
    /// Wrapper class for Backtest/* endpoints JSON response
    /// Currently used by Backtest/Read and Backtest/Create
    /// </summary>
    public class BacktestResponseWrapper : RestResponse
    {
        /// <summary>
        /// Backtest Object
        /// </summary>
        [JsonProperty(PropertyName = "backtest")]
        public Backtest Backtest { get; set; }
    }

    /// <summary>
    /// Collection container for a list of backtests for a project
    /// </summary>
    public class BacktestList : RestResponse
    {
        /// <summary>
        /// Collection of summarized backtest objects
        /// </summary>
        [JsonProperty(PropertyName = "backtests")]
        public List<Backtest> Backtests { get; set; }
    }
}
