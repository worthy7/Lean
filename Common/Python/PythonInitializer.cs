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
using System.Linq;
using Python.Runtime;
using QuantConnect.Logging;
using System.Collections.Generic;
using QuantConnect.Util;

namespace QuantConnect.Python
{
    /// <summary>
    /// Helper class for Python initialization
    /// </summary>
    public static class PythonInitializer
    {
        // Used to allow multiple Python unit and regression tests to be run in the same test run
        private static bool _isInitialized;

        // Used to hold pending path additions before Initialize is called
        private static List<string> _pendingPathAdditions = new List<string>();

        /// <summary>
        /// Initialize the Python.NET library
        /// </summary>
        public static void Initialize()
        {
            if (!_isInitialized)
            {
                Log.Trace("PythonInitializer.Initialize(): start...");
                PythonEngine.Initialize();

                // required for multi-threading usage
                PythonEngine.BeginAllowThreads();

                _isInitialized = true;
                Log.Trace("PythonInitializer.Initialize(): ended");

                AddPythonPaths(new []{ Environment.CurrentDirectory });
            }
        }

        /// <summary>
        /// Adds directories to the python path at runtime
        /// </summary>
        public static void AddPythonPaths(IEnumerable<string> paths)
        {
            if (paths == null)
            {
                return;
            }

            if (_isInitialized)
            {
                using (Py.GIL())
                {
                    _pendingPathAdditions.AddRange(paths);

                    // Generate the python code to add these to our path and execute
                    var code = string.Join(";", _pendingPathAdditions.Select(s => $"sys.path.append('{s}')"))
                        .Replace('\\', '/');

                    PythonEngine.Exec($"import sys;{code}");
                    _pendingPathAdditions.Clear();
                }
            }
            else
            {
                // Add these paths to our pending additions list
                _pendingPathAdditions.AddRange(paths);
            }
        }
    }
}
