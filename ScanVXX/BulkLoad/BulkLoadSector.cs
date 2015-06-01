using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using ScanVXX.Models;

namespace ScanVXX.BulkLoad
{
    public class BulkLoadSector : BaseBulkLoad, IDisposable
    {
        private static readonly string[] ColumnNames = new string[] { "Date", "LongTermDebtToEquity", "DivYieldPercent", 
            "MarketCap", "Name", "URI",  "NetProfitMarginPercentMRQ", "OneDayPriceChangePercent", "PriceToBookValue", "PriceToEarnings", "PriceToFreeCashFlowMRQ", "ROEPercent" };

        public BulkLoadSector() : base(ColumnNames)
        {
            
        }

        public DataTable LoadDataTableWithSectors(IEnumerable<Sector> dStats, DataTable dt)
        {
            foreach (var value in dStats)
            {
                var sValue = value.Date + "^" + value.LongTermDebtToEquity + "^"
                             + value.DivYieldPercent + "^" + value.MarketCap
                             + "^" + value.Name + "^" + value.URI + "^" + value.NetProfitMarginPercentMRQ + "^" + value.OneDayPriceChangePercent
                             + "^" + value.PriceToBookValue + "^" + value.PriceToEarnings + "^" + value.PriceToFreeCashFlowMRQ + "^" + value.ROEPercent;

                DataRow row = dt.NewRow();

                row.ItemArray = sValue.Split('^');

                dt.Rows.Add(row);
            }

            return dt;
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
        ~BulkLoadSector()
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