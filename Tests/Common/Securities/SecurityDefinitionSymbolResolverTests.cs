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
 *
*/

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using QuantConnect.Configuration;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Securities;

namespace QuantConnect.Tests.Common.Securities
{
    [TestFixture]
    public class SecurityDefinitionSymbolResolverTests
    {
        private string _dataFolderConfig;
        private DirectoryInfo _testingDataDirectory;
        private SecurityDefinitionSymbolResolver _instance;
        private static readonly Dictionary<string, string> _tickerToSecurityIdentifier = new Dictionary<string, string>
        {
            {"AAPL", "AAPL R735QTJ8XC9X"},
            {"GOOG", "GOOCV VP83T1ZUHROL"},
            {"GOOCV", "GOOCV VP83T1ZUHROL"},
            {"QQQ", "QQQ RIWIV7K5Z9LX"},
            {"QQQQ", "QQQ RIWIV7K5Z9LX"}
        };

        [OneTimeSetUp]
        public void SetUp()
        {
            _testingDataDirectory = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "testing_data"));
            var symbolPropertiesDirectory = Directory.CreateDirectory(Path.Combine(_testingDataDirectory.FullName, "symbol-properties"));
            var securityDatabaseFilePath = Path.Combine(symbolPropertiesDirectory.FullName, "security-database.csv");
            _instance = new SecurityDefinitionSymbolResolver(TestGlobals.DataProvider, securityDatabaseFilePath);

            var securityDatabaseLines = string.Join("\n",
                "AAPL R735QTJ8XC9X,03783310,BBG000B9XRY4,2046251,US0378331005",
                "GOOG T1AZ164W5VTX,38259P50,BBG000BHSKN9,B020QX2,US38259P5089",
                "GOOCV VP83T1ZUHROL,38259P70,BBG002W96FT9,BKM4JZ7,US38259P7069",
                "QQQ RIWIV7K5Z9LX,73935A10,BBG000BSWKH7,BDQYP67,US46090E1038");
            File.WriteAllText(securityDatabaseFilePath,securityDatabaseLines);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _testingDataDirectory.Delete(true);
        }

        [Test]
        public void NoExistingSecurityDefinitions()
        {
            var date = DateTime.UtcNow;
            var definitionSymbolResolver = new SecurityDefinitionSymbolResolver(TestGlobals.DataProvider);
            Assert.IsNull(definitionSymbolResolver.ISIN("US46090E1038", date));
            Assert.IsNull(definitionSymbolResolver.CUSIP("03783310", date));
            Assert.IsNull(definitionSymbolResolver.CompositeFIGI("BBG000BSWKH7", date));
            Assert.IsNull(definitionSymbolResolver.SEDOL("bdqyp67", date));
        }
        
        [TestCase("03783310", 2021, 9, 9, "AAPL", Market.USA)]
        [TestCase("03783310", 2002, 9, 9, "AAPL", Market.USA)]
        [TestCase("03783310", 1995, 1, 1, "AAPL", Market.USA)]
        [TestCase("38259P70", 2021, 9, 9, "GOOG", Market.USA)]
        [TestCase("38259P70", 2014, 4, 3, "GOOG", Market.USA)]
        [TestCase("38259P70", 2014, 4, 2, "GOOCV", Market.USA)]
        [TestCase("38259P70", 2014, 3, 10, "GOOCV", Market.USA)]
        [TestCase("38259p70", 2014, 3, 28, "GOOCV", Market.USA)]
        [TestCase("38259P70", 2014, 3, 27, "GOOCV", Market.USA)]
        [TestCase("73935a10", 2021, 9, 9, "QQQ", Market.USA)]
        [TestCase("73935A10", 2012, 12, 21, "QQQ", Market.USA)]
        [TestCase("73935A10", 2011, 3, 23, "QQQ", Market.USA)]
        [TestCase("73935A10", 2011, 3, 22, "QQQQ", Market.USA)]
        [TestCase("73935A10", 2011, 3, 21, "QQQQ", Market.USA)]
        [TestCase("73935a10", 2005, 1, 1, "QQQQ", Market.USA)]
        [TestCase("73935A10", 2004, 12, 1, "QQQQ", Market.USA)]
        [TestCase("73935A10", 2004, 11, 30, "QQQ", Market.USA)]
        [TestCase("73935a10", 2004, 11, 29, "QQQ", Market.USA)]
        [TestCase("73935A10", 1999, 5, 21, "QQQ", Market.USA)]
        [TestCase("73935A10", 1999, 3, 11, "QQQ", Market.USA)]
        [TestCase("73935A10", 1999, 3, 10, "QQQ", Market.USA)]
        [TestCase("73935A10", 1998, 5, 21, "QQQ", Market.USA)]
        [TestCase("", 2021, 9, 9, null, Market.USA)]
        [TestCase("ABCDEF99", 2021, 9, 9, null, Market.USA)]
        [TestCase(null, 2021, 9, 9, null, Market.USA)]
        [TestCase("1", 2021, 9, 9, null, Market.USA)]
        public void ResolvesCUSIP(string cusip, int year, int month, int day, string expectedTicker, string expectedMarket)
        {
            var tradingDate = new DateTime(year, month, day);
            var expectedSid = expectedTicker == null
                ? null
                : _tickerToSecurityIdentifier[expectedTicker];
            
            AssertSymbol(_instance.CUSIP(cusip, tradingDate), expectedTicker, expectedSid, expectedMarket);
        }
        
        [TestCase("BBG000B9XRY4", 2021, 9, 9, "AAPL", Market.USA)]
        [TestCase("BBG000B9XRY4", 2002, 9, 9, "AAPL", Market.USA)]
        [TestCase("BBG000B9XRY4", 1995, 1, 1, "AAPL", Market.USA)]
        [TestCase("BBG002W96FT9", 2021, 9, 9, "GOOG", Market.USA)]
        [TestCase("BBG002W96FT9", 2014, 4, 3, "GOOG", Market.USA)]
        [TestCase("BBG002W96FT9", 2014, 4, 2, "GOOCV", Market.USA)]
        [TestCase("BBG002W96FT9", 2014, 3, 10, "GOOCV", Market.USA)]
        [TestCase("BBG002W96FT9", 2014, 3, 28, "GOOCV", Market.USA)]
        [TestCase("BBG002W96FT9", 2014, 3, 27, "GOOCV", Market.USA)]
        [TestCase("BBG000BSWKH7", 2021, 9, 9, "QQQ", Market.USA)]
        [TestCase("BBG000BSWKH7", 2012, 12, 21, "QQQ", Market.USA)]
        [TestCase("BBG000BSWKH7", 2011, 3, 23, "QQQ", Market.USA)]
        [TestCase("BBG000BSWKH7", 2011, 3, 22, "QQQQ", Market.USA)]
        [TestCase("BBG000BSWKH7", 2011, 3, 21, "QQQQ", Market.USA)]
        [TestCase("BBG000BSWKH7", 2005, 1, 1, "QQQQ", Market.USA)]
        [TestCase("BBG000BSWKH7", 2004, 12, 1, "QQQQ", Market.USA)]
        [TestCase("BBG000BSWKH7", 2004, 11, 30, "QQQ", Market.USA)]
        [TestCase("BBG000BSWKH7", 2004, 11, 29, "QQQ", Market.USA)]
        [TestCase("BBG000BSWKH7", 1999, 5, 21, "QQQ", Market.USA)]
        [TestCase("BBG000BSWKH7", 1999, 3, 11, "QQQ", Market.USA)]
        [TestCase("BBG000BSWKH7", 1999, 3, 10, "QQQ", Market.USA)]
        [TestCase("BBG000BSWKH7", 1998, 5, 21, "QQQ", Market.USA)]
        [TestCase("", 2021, 9, 9, null, Market.USA)]
        [TestCase("ABCDEF99", 2021, 9, 9, null, Market.USA)]
        [TestCase(null, 2021, 9, 9, null, Market.USA)]
        [TestCase("1", 2021, 9, 9, null, Market.USA)]
        [TestCase("bbg000BSWKH7", 1998, 5, 21, "QQQ", Market.USA)]
        [TestCase("BBG000BSwKH7", 1998, 5, 21, "QQQ", Market.USA)]
        public void ResolvesCompositeFIGI(string compositeFigi, int year, int month, int day, string expectedTicker, string expectedMarket)
        {
            var tradingDate = new DateTime(year, month, day);
            var expectedSid = expectedTicker == null
                ? null
                : _tickerToSecurityIdentifier[expectedTicker];
            
            AssertSymbol(_instance.CompositeFIGI(compositeFigi, tradingDate), expectedTicker, expectedSid, expectedMarket);
        }
        
        [TestCase("2046251", 2021, 9, 9, "AAPL", Market.USA)]
        [TestCase("2046251", 2002, 9, 9, "AAPL", Market.USA)]
        [TestCase("2046251", 1995, 1, 1, "AAPL", Market.USA)]
        [TestCase("BKM4JZ7", 2021, 9, 9, "GOOG", Market.USA)]
        [TestCase("BKM4JZ7", 2014, 4, 3, "GOOG", Market.USA)]
        [TestCase("BKM4JZ7", 2014, 4, 2, "GOOCV", Market.USA)]
        [TestCase("BKM4JZ7", 2014, 3, 10, "GOOCV", Market.USA)]
        [TestCase("BKM4JZ7", 2014, 3, 28, "GOOCV", Market.USA)]
        [TestCase("BKM4JZ7", 2014, 3, 27, "GOOCV", Market.USA)]
        [TestCase("BDQYp67", 2021, 9, 9, "QQQ", Market.USA)]
        [TestCase("BDQYP67", 2012, 12, 21, "QQQ", Market.USA)]
        [TestCase("BDQYP67", 2011, 3, 23, "QQQ", Market.USA)]
        [TestCase("BDqYP67", 2011, 3, 22, "QQQQ", Market.USA)]
        [TestCase("BDQYP67", 2011, 3, 21, "QQQQ", Market.USA)]
        [TestCase("BDQYP67", 2005, 1, 1, "QQQQ", Market.USA)]
        [TestCase("BDQYP67", 2004, 12, 1, "QQQQ", Market.USA)]
        [TestCase("bDQYP67", 2004, 11, 30, "QQQ", Market.USA)]
        [TestCase("BDQYP67", 2004, 11, 29, "QQQ", Market.USA)]
        [TestCase("bdqyp67", 1999, 5, 21, "QQQ", Market.USA)]
        [TestCase("BDQYP67", 1999, 3, 11, "QQQ", Market.USA)]
        [TestCase("BDQYP67", 1999, 3, 10, "QQQ", Market.USA)]
        [TestCase("BDQYP67", 1998, 5, 21, "QQQ", Market.USA)]
        [TestCase("", 2021, 9, 9, null, Market.USA)]
        [TestCase("ABCDEF99", 2021, 9, 9, null, Market.USA)]
        [TestCase(null, 2021, 9, 9, null, Market.USA)]
        [TestCase("1", 2021, 9, 9, null, Market.USA)]
        public void ResolvesSEDOL(string sedol, int year, int month, int day, string expectedTicker, string expectedMarket)
        {
            var tradingDate = new DateTime(year, month, day);
            var expectedSid = expectedTicker == null
                ? null
                : _tickerToSecurityIdentifier[expectedTicker];
            
            AssertSymbol(_instance.SEDOL(sedol, tradingDate), expectedTicker, expectedSid, expectedMarket);
        }
        
        [TestCase("US0378331005", 2021, 9, 9, "AAPL", Market.USA)]
        [TestCase("US0378331005", 2002, 9, 9, "AAPL", Market.USA)]
        [TestCase("US0378331005", 1995, 1, 1, "AAPL", Market.USA)]
        [TestCase("US38259P7069", 2021, 9, 9, "GOOG", Market.USA)]
        [TestCase("US38259P7069", 2014, 4, 3, "GOOG", Market.USA)]
        [TestCase("US38259P7069", 2014, 4, 2, "GOOCV", Market.USA)]
        [TestCase("US38259P7069", 2014, 3, 10, "GOOCV", Market.USA)]
        [TestCase("US38259P7069", 2014, 3, 28, "GOOCV", Market.USA)]
        [TestCase("US38259P7069", 2014, 3, 27, "GOOCV", Market.USA)]
        [TestCase("US46090E1038", 2021, 9, 9, "QQQ", Market.USA)]
        [TestCase("US46090E1038", 2012, 12, 21, "QQQ", Market.USA)]
        [TestCase("US46090E1038", 2011, 3, 23, "QQQ", Market.USA)]
        [TestCase("US46090E1038", 2011, 3, 22, "QQQQ", Market.USA)]
        [TestCase("US46090E1038", 2011, 3, 21, "QQQQ", Market.USA)]
        [TestCase("US46090E1038", 2005, 1, 1, "QQQQ", Market.USA)]
        [TestCase("US46090E1038", 2004, 12, 1, "QQQQ", Market.USA)]
        [TestCase("us46090E1038", 2004, 11, 30, "QQQ", Market.USA)]
        [TestCase("US46090E1038", 2004, 11, 29, "QQQ", Market.USA)]
        [TestCase("US46090E1038", 1999, 5, 21, "QQQ", Market.USA)]
        [TestCase("US46090e1038", 1999, 3, 11, "QQQ", Market.USA)]
        [TestCase("US46090E1038", 1999, 3, 10, "QQQ", Market.USA)]
        [TestCase("US46090E1038", 1998, 5, 21, "QQQ", Market.USA)]
        [TestCase("", 2021, 9, 9, null, Market.USA)]
        [TestCase("ABCDEF99", 2021, 9, 9, null, Market.USA)]
        [TestCase(null, 2021, 9, 9, null, Market.USA)]
        [TestCase("1", 2021, 9, 9, null, Market.USA)]
        public void ResolvesISIN(string isin, int year, int month, int day, string expectedTicker, string expectedMarket)
        {
            var tradingDate = new DateTime(year, month, day);
            var expectedSid = expectedTicker == null
                ? null
                : _tickerToSecurityIdentifier[expectedTicker];
            
            AssertSymbol(_instance.ISIN(isin, tradingDate), expectedTicker, expectedSid, expectedMarket);
        }

        private void AssertSymbol(Symbol actual, string expectedTicker, string expectedSid, string expectedMarket)
        {
            Assert.AreEqual(expectedTicker, actual?.Value);
            Assert.AreEqual(expectedSid, actual?.ID.ToString());
            Assert.AreEqual(expectedMarket, actual?.ID.Market ?? expectedMarket);
        }
    }
}
