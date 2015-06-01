using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ScanVXX.Models
{
    public class Sector : BaseData
    {

        public Sectors GetSectors(string[] rows)
        {
            Sectors sectors = new Sectors();

            string[] stringSeparators = new string[] { "<td" };

            var column1 = rows[1].Split(stringSeparators, StringSplitOptions.None);
            
            var column3 = rows[3].Split(stringSeparators, StringSplitOptions.None);

            DateTime date = DateTime.Now;

            for (int i = 2; i < rows.Length; i++)
            {
                var columns = rows[i].Split(stringSeparators, StringSplitOptions.None);
                Sector sector = new Sector()
                {
                    URI = columns[1].Split('<', '>')[2].Replace("a href=", baseUri),
                    Name = columns[1].Split('<', '>')[5],
                    OneDayPriceChangePercent = ScrubData(columns[2].Split('<', '>')[3]),
                    MarketCap = columns[3].Split('<', '>')[3],
                    PriceToEarnings = ScrubData(columns[4].Split('<', '>')[3]),
                    ROEPercent = ScrubData(columns[5].Split('<', '>')[3]),
                    DivYieldPercent = ScrubData(columns[6].Split('<', '>')[3]),
                    LongTermDebtToEquity = ScrubData(columns[7].Split('<', '>')[3]),
                    PriceToBookValue = ScrubData(columns[8].Split('<', '>')[3]),
                    NetProfitMarginPercentMRQ = ScrubData(columns[9].Split('<', '>')[3]),
                    PriceToFreeCashFlowMRQ = ScrubData(columns[10].Split('<', '>')[3]),
                    Date = date
                };
                sectors.Add(sector);
            }
            return sectors;
        }

        [OnSerializing]
        void OnSerializing(StreamingContext context)
        {
            base.dateForSerialization = this.Date.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
        }
    }
}

//  1 Day Price Change %
//  Market Cap
//  P/E
//  ROE %
//  Div. Yield %
//  Long-Term Debt to Equity
//  Price to Book Value
//  Profit Margin % (mrq)
//  Free Cash Flow (mrq)


//<th bgcolor=eeeeee width=10%><a href=s_pr1u.html><font face=verdana size=-2><b>1 Day Price<br>Change %</b></font></a></th>
//<th bgcolor=eeeeee width=10%><a href=s_mktu.html><font face=verdana size=-2><b>Market<br>Cap</b></font></a></th>
//<th bgcolor=eeeeee width=10%><a href=s_peeu.html><font face=verdana size=-2><b>P/E</b></font></a></th>
//<th bgcolor=eeeeee width=10%><a href=s_ttmu.html><font face=verdana size=-2><b>ROE %</b></font></a></th>
//<th bgcolor=eeeeee width=10%><a href=s_yieu.html><font face=verdana size=-2><b>Div. Yield %</b></font></a></th>
//<th bgcolor=eeeeee width=10%><a href=s_qtou.html><font face=verdana size=-2><b>Long-Term Debt to<br>Equity</b></font></a></th>
//<th bgcolor=eeeeee width=10%><a href=s_priu.html><font face=verdana size=-2><b>Price to<br>Book Value</b></font></a></th>
//<th bgcolor=eeeeee width=10%><a href=s_qpmu.html><font face=verdana size=-2><b>Net<br>Profit Margin % (mrq)</b></font></a></th>
//<th bgcolor=eeeeee width=10%><a href=s_prfu.html><font face=verdana size=-2><b>Price to<br>Free Cash Flow (mrq)</b></font></a></th>