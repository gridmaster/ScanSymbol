using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using ScanVXX.BulkLoad;
using ScanVXX.Models;
using ScanVXX.Web;

namespace ScanVXX
{
    class Program
    {
        #region tunable resources
        public static int multipliar = 100;
        public static decimal maxLoss = -0.04m;
        public static decimal maxPain = 0.92m;
        public static decimal profitSellStop = 0.99m;
        public static decimal splitTest = 200m;
        public static int numberOfMonths = -1;
        public static int maxAge = 30;

        public static DateTime startDate = new DateTime(2013, 11, 01);

        private static string dividends = "http://real-chart.finance.yahoo.com/table.csv?s=WFC&a=05&b=1&c=1972&d=04&e=29&f=2015&g=v&ignore=.csv";

        #region buySellMatrix
        public static bool buyOnOpen;
        public static bool buyOnTrigger;
        public static bool buyOnClose;
        public static bool sellOnOpen;
        public static bool sellOnTrigger;
        public static bool sellOnClose;
        #endregion buySellMatrix

        #endregion tunable resources

        static void Main(string[] args)
        {

            Console.WriteLine("Start: {0}", DateTime.Now);
            string symbol = "IBM";

            RunQuickExit(symbol);
            RunPatientExit(symbol);

            Console.ReadKey();
        }

        public static void RunQuickExit(string symbol)
        {
            int skip = 0;
            var date = DateTime.Now;
            decimal up = 0m;
            decimal down = 0m;
            int age = 1;
            Dictionary<DateTime, decimal> gainsLoses = new Dictionary<DateTime, decimal>();
            List<Daily> positions = new List<Daily>();
            Daily currentPosition = null;
            Daily lastPosition = new Daily();
            int i = 0;
            Daily dailyLow = new Daily()
            {
                Date = DateTime.Now,
                Open = 0m,
                Close = 0m,
                High = 0m,
                Low = 9999999.0m,
                Volume = 0,
                AdjClose = 0m
            };
            Daily longtermLow = new Daily();
            longtermLow = dailyLow;

            //if (args.Length > 0) symbol = args[0];
            //if (args.Length > 1) skip = Convert.ToInt32(args[0]);

            //example of complete uri
            //http://real-chart.finance.yahoo.com/table.csv?s=VXX&d=5&e=27&f=2015&g=d&a=0&b=30&c=2009&ignore=.csv

            string uriString = WebPage.GetUri(symbol, date, numberOfMonths);

            string today = DateTime.Now.Date.ToString().Substring(0, date.ToString().IndexOf(' '));
            string dayFile = string.Format(@"C:\Users\Jim\Documents\Visual Studio 2012\Projects\LoadSectorIndustrySymbol\Files\QuickExit {0} - {1}.csv", symbol.ToUpper(), today.Replace('/', '-'));

            //string[] dailyArray = System.IO.File.ReadAllLines(@"../../../Files/vxxtable.csv");
            string sPage = WebPage.Get(uriString);
            //string test = WebPage.TheDownload(uriString);
            string[] dailyArray = sPage.Split('\n');
            List<Daily> dailies = GetDailies(dailyArray);
            dailies.Reverse();

            using (StreamWriter sw = new StreamWriter(dayFile))
            {
                for (; i < dailies.Count; i++)
                {
                    if (i < skip) continue;

                    if (longtermLow.Low > dailies[i].Low)
                    {
                        longtermLow = dailies[i];
                    }

                    if (age > maxAge)
                    {
                        // if we go maxAge with no new low, reset to the lowest number in the last 30
                        dailyLow = longtermLow;
                        sw.WriteLine("{0} > {1}:, {2}, {3}, {4}, {5}, {6}", age, maxAge,
                                dailyLow.Date, dailyLow.Open, dailyLow.High, dailyLow.Low, dailyLow.Close);
                    }

                    // todays close / yesterdays close * 100 > 300 == split so restart calculations
                    if ((lastPosition.Close != 0 && ((lastPosition.Close / dailies[i].Close) * 100) > splitTest))
                    {
                        positions.Add(currentPosition);
                        gainsLoses.Add(dailies[i].Date, 0m);
                        currentPosition = null;

                        WriteOutput(sw, dailies[i], currentPosition, 0.0m, dailyLow.Low);
                        dailyLow = dailies[i];
                    }

                    // if we hit a new low
                    if (dailyLow.Low > dailies[i].Low)
                    {
                        dailyLow = dailies[i];

                        // if we don't have a position, start one
                        if (currentPosition == null)
                        {
                            currentPosition = new Daily
                            {
                                Date = dailies[i].Date,
                                Open = dailies[i].Open,
                                High = dailies[i].High,
                                Low = dailies[i].Low,
                                Close = dailies[i].Close,
                                Volume = dailies[i].Volume,
                                AdjClose = dailies[i].AdjClose
                            };
                        }

                        lastPosition = dailies[i];

                        age = 1; // reset age
                        longtermLow = dailyLow; // reset long term low
                        continue;
                    }

                    if (currentPosition != null)
                    {
                        // if we gain more than a $1
                        if (((decimal)dailies[i].High - (decimal)currentPosition.Low) > profitSellStop)
                        {
                            // if all is well sell at close
                            decimal gain = ((decimal)dailies[i].Close - (decimal)currentPosition.Low);
                            if (gain > profitSellStop)
                            {
                                gainsLoses.Add(dailies[i].Date, gain);
                                up += gain;
                                WriteOutput(sw, dailies[i], currentPosition, gain, currentPosition.Low);
                            }
                            else // this should not execute... I don't think, we now sell at close.
                            {   // profitSellStop + .01m because we actually test for > profitSellStop
                                gainsLoses.Add(dailies[i].Date, profitSellStop + .01m);
                                up += profitSellStop + .01m;
                                WriteOutput(sw, dailies[i], currentPosition, profitSellStop + .01m, dailyLow.Low);
                            }
                            positions.Add(currentPosition);
                            currentPosition = null;
                        }
                        else
                        {
                            // if we hit the maxLoss sell stop..
                            decimal LossSellStop = (decimal.Divide((decimal)dailies[i].Low, (decimal)currentPosition.Low) - 1);
                            if (LossSellStop < maxLoss)
                            {
                                decimal loss = dailies[i].Open - currentPosition.Low;
                                positions.Add(currentPosition);
                                // if open is > loss than maxLoss sell at open
                                if ((decimal.Divide((decimal)dailies[i].Open, (decimal)currentPosition.Low) - 1) < maxLoss)
                                {
                                    gainsLoses.Add(dailies[i].Date, loss);
                                    down += loss;
                                    WriteOutput(sw, dailies[i], currentPosition, loss, dailyLow.Low);
                                }
                                else
                                {// else sell at maxLoss
                                    loss = (currentPosition.Low * 0.92m) - currentPosition.Low;
                                    gainsLoses.Add(dailies[i].Date, loss);
                                    down += loss;
                                    WriteOutput(sw, dailies[i], currentPosition, loss, dailyLow.Low);
                                }
                                currentPosition = null;
                            }
                        }
                    }

                    //last thing to do...
                    lastPosition.Date = dailies[i].Date;
                    lastPosition.Open = dailies[i].Open;
                    lastPosition.High = dailies[i].High;
                    lastPosition.Low = dailies[i].Low;
                    lastPosition.Close = dailies[i].Close;
                    lastPosition.Volume = dailies[i].Volume;
                    lastPosition.AdjClose = dailies[i].AdjClose;
                }  // end for

                sw.WriteLine("");
                if (currentPosition != null)
                    sw.WriteLine(" Bought:, {0}, {1}, {2}, {3}, {4}",
                        currentPosition.Date, currentPosition.Open, currentPosition.High, currentPosition.Low, currentPosition.Close);
                sw.WriteLine(" Last:, {0}, {1}, {2}, {3}, {4}",
                        dailies[i - 1].Date, dailies[i - 1].Open, dailies[i - 1].High, dailies[i - 1].Low, dailies[i - 1].Close);
                sw.WriteLine("Daily Low:, {0}, {1}, {2}, {3}, {4}",
                        dailyLow.Date, dailyLow.Open, dailyLow.High, dailyLow.Low, dailyLow.Close);
                sw.WriteLine(" Date:, {0}", dailies[i - 1].Date);
                sw.WriteLine("  Low:, {0}", dailyLow.Low);
                sw.WriteLine("gains:, {0}", up * multipliar);
                sw.WriteLine(" loss:, {0}", down * multipliar);
                sw.WriteLine("  sum:, {0}", (up + down) * multipliar);
                sw.Close();
            }

            Console.WriteLine("   Symbol: {0}", symbol);
            if (currentPosition != null) Console.WriteLine("   Bought: {0}, {1}, {2}, {3}, {4}",
                    currentPosition.Date, currentPosition.Open, currentPosition.High, currentPosition.Low, currentPosition.Close);
            Console.WriteLine("     Last: {0}, {1}, {2}, {3}, {4}",
                    dailies[i - 1].Date, dailies[i - 1].Open, dailies[i - 1].High, dailies[i - 1].Low, dailies[i - 1].Close);
            Console.WriteLine("Daily Low: {0}, {1}, {2}, {3}, {4}",
                    dailyLow.Date, dailyLow.Open, dailyLow.High, dailyLow.Low, dailyLow.Close);
            Console.WriteLine("     Date: {0}", dailies[i - 1].Date);
            Console.WriteLine("      Low: {0}", dailyLow.Low);
            Console.WriteLine("    gains: {0}", up * multipliar);
            Console.WriteLine("     loss: {0}", down * multipliar);
            Console.WriteLine("      sum: {0}", (up + down) * multipliar);
            // Console.WriteLine("\nWin Loss Ratio: {0}", (up / Math.Abs(down)) * 100);

            Console.ReadKey();
        }

        public static void RunPatientExit(string symbol)
        {
            int skip = 0;
            decimal totalUp = 0m;
            decimal totalDown = 0m;
            int age = 1;
            Dictionary<DateTime, decimal> gainsLoses = new Dictionary<DateTime, decimal>();
            List<Daily> positions = new List<Daily>();
            Daily currentPosition = null;
            Daily lastPosition = new Daily();
            int i = 0;

            // for starting new positions
            Daily dailyLow = new Daily()
            {
                Date = DateTime.Now,
                Open = 0m,
                Close = 0m,
                High = 0m,
                Low = 9999999.0m,
                Volume = 0,
                AdjClose = 0m
            };
            Daily longtermLow = new Daily();
            longtermLow = dailyLow;

            // for delayed exit of new position
            Daily longTermHigh = new Daily();

            string dayFile = GetOutPutFile(symbol, "LongHold");
            List<Daily> dailies = GetDailyValues(symbol);

            using (StreamWriter sw = new StreamWriter(dayFile))
            {
                for (; i < dailies.Count; i++)
                {
                    if (i < skip) continue;

                    if (longtermLow.Low > dailies[i].Low)
                    {
                        longtermLow = dailies[i];
                    }

                    if (dailies[i].Date.ToString() == "10/6/2014 12:00:00 AM")
                    {
                        Console.WriteLine("boing!");
                    }

                    if (age > maxAge)
                    {
                        // if we go maxAge with no new low, reset to the lowest number in the last 30
                        dailyLow = longtermLow;
                        sw.WriteLine("{0} > {1}:, {2}, {3}, {4}, {5}, {6}", age, maxAge,
                                dailyLow.Date, dailyLow.Open, dailyLow.High, dailyLow.Low, dailyLow.Close);
                    }

                    // todays close / yesterdays close * 100 > 300 == split so restart calculations
                    if ((lastPosition.Close != 0 && ((lastPosition.Close / dailies[i].Close) * 100) > splitTest))
                    {
                        positions.Add(currentPosition);
                        gainsLoses.Add(dailies[i].Date, 0m);
                        currentPosition = null;

                        WriteOutput(sw, dailies[i], currentPosition, 0.0m, dailyLow.Low);
                        dailyLow = dailies[i];
                    }

                    if (currentPosition != null && longTermHigh != null)
                    {
                        // reset if necessary
                        if (dailies[i].High > longTermHigh.High) longTermHigh = dailies[i];
                    }

                    // if we hit a new low
                    if (dailyLow.Low > dailies[i].Low)
                    {
                        dailyLow = dailies[i];

                        // if we don't have a position, start one
                        if (currentPosition == null)
                        {
                            currentPosition = new Daily
                            {
                                Date = dailies[i].Date,
                                Open = dailies[i].Open,
                                High = dailies[i].High,
                                Low = dailies[i].Low,
                                Close = dailies[i].Close,
                                Volume = dailies[i].Volume,
                                AdjClose = dailies[i].AdjClose
                            };

                            longTermHigh = currentPosition;
                        }

                        lastPosition = dailies[i];

                        age = 1; // reset age
                        longtermLow = dailyLow; // reset long term low
                        continue;
                    }

                    if (currentPosition != null)
                    {
                        // if we gain more than a $1 save current High
                        if (((decimal)dailies[i].High - (decimal)currentPosition.Low) > profitSellStop)
                        {
                            if (longTermHigh == null)
                            {
                                longTermHigh = dailies[i];
                            }
                        }

                        // if longTermHigh has lost more than maxPain exit trade
                        if ((longTermHigh.High * maxPain) > dailies[i].High)
                        {
                            decimal gain = ((decimal)dailies[i].Close - (decimal)currentPosition.Low);
                            if (gain > profitSellStop)
                            {
                                gainsLoses.Add(dailies[i].Date, gain);
                                totalUp += gain;
                                WriteOutput(sw, dailies[i], currentPosition, gain, currentPosition.Low);
                                positions.Add(currentPosition);
                                currentPosition = longTermHigh = null;
                            }
                        }
                        else
                        {
                            // if we hit the maxLoss sell stop..
                            decimal LossSellStop = (decimal.Divide((decimal)dailies[i].Low, (decimal)currentPosition.Low) - 1);
                            if (LossSellStop < maxLoss)
                            {
                                decimal loss = dailies[i].Open - currentPosition.Low;
                                positions.Add(currentPosition);
                                // if open is > loss than maxLoss sell at open
                                if ((decimal.Divide((decimal)dailies[i].Open, (decimal)currentPosition.Low) - 1) < maxLoss)
                                {
                                    gainsLoses.Add(dailies[i].Date, loss);
                                    totalDown += loss;
                                    WriteOutput(sw, dailies[i], currentPosition, loss, dailyLow.Low);
                                }
                                else
                                {// else sell at maxLoss
                                    loss = (currentPosition.Low * maxPain) - currentPosition.Low;
                                    gainsLoses.Add(dailies[i].Date, loss);
                                    totalDown += loss;
                                    WriteOutput(sw, dailies[i], currentPosition, loss, dailyLow.Low);
                                }
                                currentPosition = null;
                            }
                        }
                    }

                    //last thing to do...
                    lastPosition.Date = dailies[i].Date;
                    lastPosition.Open = dailies[i].Open;
                    lastPosition.High = dailies[i].High;
                    lastPosition.Low = dailies[i].Low;
                    lastPosition.Close = dailies[i].Close;
                    lastPosition.Volume = dailies[i].Volume;
                    lastPosition.AdjClose = dailies[i].AdjClose;
                }  // end for

                sw.WriteLine("");
                if (currentPosition != null)
                    sw.WriteLine(" Bought:, {0}, {1}, {2}, {3}, {4}",
                        currentPosition.Date, currentPosition.Open, currentPosition.High, currentPosition.Low, currentPosition.Close);
                sw.WriteLine(" Last:, {0}, {1}, {2}, {3}, {4}",
                        dailies[i - 1].Date, dailies[i - 1].Open, dailies[i - 1].High, dailies[i - 1].Low, dailies[i - 1].Close);
                sw.WriteLine("Daily Low:, {0}, {1}, {2}, {3}, {4}",
                        dailyLow.Date, dailyLow.Open, dailyLow.High, dailyLow.Low, dailyLow.Close);
                sw.WriteLine(" Date:, {0}", dailies[i - 1].Date);
                sw.WriteLine("  Low:, {0}", dailyLow.Low);
                sw.WriteLine("gains:, {0}", totalUp * multipliar);
                sw.WriteLine(" loss:, {0}", totalDown * multipliar);
                sw.WriteLine("  sum:, {0}", (totalUp + totalDown) * multipliar);
                sw.Close();
            }

            Console.WriteLine("   Symbol: {0}", symbol);
            if (currentPosition != null) Console.WriteLine("   Bought: {0}, {1}, {2}, {3}, {4}",
                    currentPosition.Date, currentPosition.Open, currentPosition.High, currentPosition.Low, currentPosition.Close);
            Console.WriteLine("     Last: {0}, {1}, {2}, {3}, {4}",
                    dailies[i - 1].Date, dailies[i - 1].Open, dailies[i - 1].High, dailies[i - 1].Low, dailies[i - 1].Close);
            Console.WriteLine("Daily Low: {0}, {1}, {2}, {3}, {4}",
                    dailyLow.Date, dailyLow.Open, dailyLow.High, dailyLow.Low, dailyLow.Close);
            Console.WriteLine("     Date: {0}", dailies[i - 1].Date);
            Console.WriteLine("      Low: {0}", dailyLow.Low);
            Console.WriteLine("    gains: {0}", totalUp * multipliar);
            Console.WriteLine("     loss: {0}", totalDown * multipliar);
            Console.WriteLine("      sum: {0}", (totalUp + totalDown) * multipliar);
            // Console.WriteLine("\nWin Loss Ratio: {0}", (up / Math.Abs(down)) * 100);

            Console.ReadKey();
        }

        private static string GetOutPutFile(string symbol, string name)
        {
            string today = DateTime.Now.Date.ToString().Substring(0, DateTime.Now.ToString().IndexOf(' '));
            return string.Format(@"C:\Users\Jim\Documents\Visual Studio 2012\Projects\LoadSectorIndustrySymbol\Files\{0} {1} - {2}.csv", name, symbol.ToUpper(), today.Replace('/', '-'));
        }

        private static List<Daily> GetDailyValues(string symbol)
        {
            var date = DateTime.Now;

            string uriString = WebPage.GetUri(symbol, startDate, numberOfMonths);

            //string[] dailyArray = System.IO.File.ReadAllLines(@"../../../Files/vxxtable.csv");
            string sPage = WebPage.Get(uriString);
            string[] dailyArray = sPage.Split('\n');
            List<Daily> dailies = GetDailies(dailyArray);
            dailies.Reverse();
            return dailies;
        }

        public static void WriteOutput(StreamWriter sw, Daily today, Daily current, decimal value, decimal low)
        {
            if (current == null)
            {
                sw.WriteLine("Stock split:, {0}, {1}, {2}, {3}, {4}, Low: {5}",
                        today.Date, today.Open, today.High, today.Low, today.Close, low);
            }
            else
            {
                decimal newMultipliar = multipliar;
                if (current.Low < 30m) newMultipliar = multipliar * 2;

                sw.WriteLine("Bought:, {0}, {1}, {2}, {3}, {4}, Sold:, {5}, {6}, {7}, {8}, {9}, Low:, {10}, Value:, {11}",
                    current.Date, current.Open, current.High, current.Low, current.Close,
                    today.Date, today.Open, today.High, today.Low, today.Close, low, value * newMultipliar);
            }
        }

        public static List<Daily> GetDailies(string[] dailyArray)
        {
            List<Daily> dailies = new List<Daily>();

            foreach (string value in dailyArray)
            {
                if (string.IsNullOrEmpty(value)) return dailies;

                string[] line = value.Split(',');

                if (line[0] == "Date") continue;

                Daily daily = new Daily();
                daily.Date = Convert.ToDateTime(line[0]);
                daily.Open = Convert.ToDecimal(line[1]);
                daily.High = Convert.ToDecimal(line[2]);
                daily.Low = Convert.ToDecimal(line[3]);
                daily.Close = Convert.ToDecimal(line[4]);
                daily.Volume = Convert.ToInt32(line[5]);
                daily.AdjClose = Convert.ToDecimal(line[6]);
                dailies.Add(daily);
            }
            return dailies;
        }

        public static void RunPatientxExit(string symbol)
        {
            int skip = 0;
            var date = DateTime.Now;
            decimal up = 0m;
            decimal down = 0m;
            int age = 1;
            Dictionary<DateTime, decimal> gainsLoses = new Dictionary<DateTime, decimal>();
            List<Daily> positions = new List<Daily>();
            Daily currentPosition = null;
            Daily lastPosition = new Daily();
            int i = 0;

            // for starting new positions
            Daily dailyLow = new Daily()
            {
                Date = DateTime.Now,
                Open = 0m,
                Close = 0m,
                High = 0m,
                Low = 9999999.0m,
                Volume = 0,
                AdjClose = 0m
            };
            Daily longtermLow = new Daily();
            longtermLow = dailyLow;

            // for delayed exit of new position
            Daily longTermHigh = new Daily();

            string uriString = string.Format("{0}{1}&d={2}&e={3}&f={4}&g=d&a={5}&b={6}&c={7}&ignore=.csv",
                    @"http://real-chart.finance.yahoo.com/table.csv?s=",
                    symbol, date.Month, date.Day, date.Year,
                    date.AddMonths(numberOfMonths - 1).Month, date.AddDays(-1).AddMonths(numberOfMonths).Day, date.AddMonths(numberOfMonths).Year);

            string today = DateTime.Now.Date.ToString().Substring(0, date.ToString().IndexOf(' '));
            string dayFile = string.Format(@"C:\Users\Jim\Documents\Visual Studio 2012\Projects\LoadSectorIndustrySymbol\Files\LongHold {0} - {1}.csv", symbol.ToUpper(), today.Replace('/', '-'));

            string sPage = WebPage.Get(uriString);
            string[] dailyArray = sPage.Split('\n');
            List<Daily> dailies = GetDailies(dailyArray);
            dailies.Reverse();

            using (StreamWriter sw = new StreamWriter(dayFile))
            {
                for (; i < dailies.Count; i++)
                {
                    if (i < skip) continue;

                    if (longtermLow.Low > dailies[i].Low)
                    {
                        longtermLow = dailies[i];
                    }

                    if (age > maxAge)
                    {
                        // if we go maxAge with no new low, reset to the lowest number in the last 30
                        dailyLow = longtermLow;
                        sw.WriteLine("{0} > {1}:, {2}, {3}, {4}, {5}, {6}", age, maxAge,
                                dailyLow.Date, dailyLow.Open, dailyLow.High, dailyLow.Low, dailyLow.Close);
                    }

                    // todays close / yesterdays close * 100 > 300 == split so restart calculations
                    if ((lastPosition.Close != 0 && ((lastPosition.Close / dailies[i].Close) * 100) > splitTest))
                    {
                        positions.Add(currentPosition);
                        gainsLoses.Add(dailies[i].Date, 0m);
                        currentPosition = null;

                        WriteOutput(sw, dailies[i], currentPosition, 0.0m, dailyLow.Low);
                        dailyLow = dailies[i];
                    }

                    if (currentPosition != null && longTermHigh != null)
                    {
                        // reset if necessary
                        if (dailies[i].High > longTermHigh.High) longTermHigh = dailies[i];
                    }

                    // if we hit a new low
                    if (dailyLow.Low > dailies[i].Low)
                    {
                        dailyLow = dailies[i];

                        // if we don't have a position, start one
                        if (currentPosition == null)
                        {
                            currentPosition = new Daily
                            {
                                Date = dailies[i].Date,
                                Open = dailies[i].Open,
                                High = dailies[i].High,
                                Low = dailies[i].Low,
                                Close = dailies[i].Close,
                                Volume = dailies[i].Volume,
                                AdjClose = dailies[i].AdjClose
                            };

                            longTermHigh = currentPosition;
                        }

                        lastPosition = dailies[i];

                        age = 1; // reset age
                        longtermLow = dailyLow; // reset long term low
                        continue;
                    }

                    if (currentPosition != null)
                    {
                        // if we gain more than a $1 save current High
                        if (((decimal)dailies[i].High - (decimal)currentPosition.Low) > profitSellStop)
                        {
                            if (longTermHigh == null)
                            {
                                longTermHigh = dailies[i];
                            }
                        }

                        // if longTermHigh has lost more than maxPain exit trade
                        if ((longTermHigh.High * maxPain) > dailies[i].High)
                        {
                            decimal gain = ((decimal)dailies[i].Close - (decimal)currentPosition.Low);
                            if (gain > profitSellStop)
                            {
                                gainsLoses.Add(dailies[i].Date, gain);
                                up += gain;
                                WriteOutput(sw, dailies[i], currentPosition, gain, currentPosition.Low);
                                positions.Add(currentPosition);
                                currentPosition = longTermHigh = null;
                            }
                        }
                        else
                        {
                            // if we hit the maxLoss sell stop..
                            decimal LossSellStop = (decimal.Divide((decimal)dailies[i].Low, (decimal)currentPosition.Low) - 1);
                            if (LossSellStop < maxLoss)
                            {
                                decimal loss = dailies[i].Open - currentPosition.Low;
                                positions.Add(currentPosition);
                                // if open is > loss than maxLoss sell at open
                                if ((decimal.Divide((decimal)dailies[i].Open, (decimal)currentPosition.Low) - 1) < maxLoss)
                                {
                                    gainsLoses.Add(dailies[i].Date, loss);
                                    down += loss;
                                    WriteOutput(sw, dailies[i], currentPosition, loss, dailyLow.Low);
                                }
                                else
                                {// else sell at maxLoss
                                    loss = (currentPosition.Low * 0.92m) - currentPosition.Low;
                                    gainsLoses.Add(dailies[i].Date, loss);
                                    down += loss;
                                    WriteOutput(sw, dailies[i], currentPosition, loss, dailyLow.Low);
                                }
                                currentPosition = null;
                            }
                        }
                    }

                    //last thing to do...
                    lastPosition = dailies[i];
                }  // end for
                sw.Close();
            }
        }
    }
}
