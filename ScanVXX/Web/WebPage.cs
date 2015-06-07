using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;

namespace ScanVXX.Web
{
    class WebPage
    {
        public static string Get(string uri)
        {
            string results = "N/A";

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                StreamReader sr = new StreamReader(resp.GetResponseStream());
                results = sr.ReadToEnd();
                sr.Close();
            }
            catch (Exception ex)
            {
                results = ex.Message;
            }
            return results;
        }

        //example of complete uri
        //http://real-chart.finance.yahoo.com/table.csv?s=VXX&d=5&e=27&f=2015&g=d&a=0&b=30&c=2009&ignore=.csv
        public static string GetUri(string symbol, DateTime date, int numberOfMonths)
        {
            return string.Format("{0}{1}&d={2}&e={3}&f={4}&g=d&a={5}&b={6}&c={7}&ignore=.csv",
                      @"http://real-chart.finance.yahoo.com/table.csv?s=", symbol,
                      date.Month, date.Day, date.Year,
                      date.AddMonths(numberOfMonths - 1).Month, date.AddDays(-1).AddMonths(numberOfMonths).Day, date.AddMonths(numberOfMonths).Year);
        }

    }
}
