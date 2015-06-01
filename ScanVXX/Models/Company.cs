using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ScanVXX.Models
{
    public class Company : BaseData, IDisposable
    {
        public string GeneralInfoURI { get; set; }
        public string Symbol { get; set; }

        public string Sector { get; set; }
        public int SectorId { get; set; }

        public string Industry { get; set; }
        public int IndustryId { get; set; }

        public string Exchange { get; set; }
        public int ExchangeId { get; set; }

        public Companies GetCompanies(string sector, string industry, string[] rows)
        {
            Companies companies = new Companies();

            string[] stringSeparators = new string[] { "<td" };
            
            DateTime date = DateTime.Now;

            Dictionary<string, DateTime> myDic = new Dictionary<string, DateTime>();

            for (int i = 5; i < rows.Length; i++)
            {
                var columns = rows[i].Split(stringSeparators, StringSplitOptions.None);

                Company company = new Company()
                {
                    URI = columns[1].Split('<', '>')[4].Substring(columns[1].Split('<', '>')[4].LastIndexOf("http")).Replace("\"", ""),
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
                    Symbol = columns[1].Split('(', ')')[1].Split('<', '>')[0] == "" ? columns[1].Split('(', ')')[1].Split('<', '>')[2] : columns[1].Split('(', ')')[1].Split('<', '>')[0],
                    Sector = sector,
                    SectorId = 0,
                    Industry = industry,
                    IndustryId = 0,
                    Exchange = Exchange,
                    ExchangeId = 0,
                    GeneralInfoURI = columns[1].Split('<', '>')[8].LastIndexOf("http") >= 0 ? columns[1].Split('<', '>')[8].Substring(columns[1].Split('<', '>')[8].LastIndexOf("http")).Replace("\"", "") : "",
                    Date = date,
                };

                try
                {
                    myDic.Add(company.Name + " " + company.Symbol, date); //  + " " + company.Symbol);
                    companies.Add(company);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(company.Name + ": " +ex.Message);
                }
            }
            return companies;
        }

        #region Implement IDisposable

        private IntPtr nativeResource = Marshal.AllocHGlobal(100);

        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // NOTE: Leave out the finalizer altogether if this class doesn't 
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are. 
        ~Company()
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
