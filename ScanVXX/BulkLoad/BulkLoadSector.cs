using System.Collections.Generic;
using System.Data;
using ScanVXX.Models;

namespace ScanVXX.BulkLoad
{
    public class BulkLoadSector : BaseBulkLoad
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
    }
}