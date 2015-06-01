using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ScanVXX.Models
{
    public class Industry : BaseData, IDisposable
    {
        public string MoreInfoLink { get; set; }

        public string Sector { get; set; }
        public int SectorId { get; set; }

        public Industries GetIndustries(string sector, string[] rows)
        {
            Industries industries = new Industries();

            string[] stringSeparators = new string[] { "<td" };

            DateTime date = DateTime.Now;

            for (int i = 4; i < rows.Length; i++)
            {
                var columns = rows[i].Split(stringSeparators, StringSplitOptions.None);

                Industry industry = new Industry()
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
                    MoreInfoLink = columns[11].Split('<', '>')[4].Replace("a href=", ""),
                    Sector = sector,
                    SectorId = 0,
                    Date = date
                };

                industries.Add(industry);
            }
            return industries;
        }

        #region Implement IDisposable

        private IntPtr nativeResource = Marshal.AllocHGlobal(100);
        //More Info

        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // NOTE: Leave out the finalizer altogether if this class doesn't 
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are. 
        ~Industry()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
            }
            // free native resources if there are any.
            if (nativeResource != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(nativeResource);
                nativeResource = IntPtr.Zero;
            }
        }
        #endregion Implement IDisposable
    }
}
