using System.Collections.Generic;
using System.Data;
using ScanVXX.Models;

namespace ScanVXX.BulkLoad
{
    public class BulkLoadIndustry : BaseBulkLoad
    {
        private static readonly string[] ColumnNames = new string[]
            {
                "Date", "LongTermDebtToEquity", "DivYieldPercent",
                "MarketCap", "Name", "URI", "NetProfitMarginPercentMRQ", 
                "OneDayPriceChangePercent", "PriceToBookValue", "PriceToEarnings",
                "PriceToFreeCashFlowMRQ", "ROEPercent", "Sector", 
                "SectorId", "MoreInfoLink"
            };

        public BulkLoadIndustry() : base(ColumnNames)
        {

        }

        public DataTable LoadDataTableWithIndustries(IEnumerable<Industry> dStats, DataTable dt)
        {
            foreach (var value in dStats)
            {
                var sValue = value.Date + "^" + value.LongTermDebtToEquity + "^" + value.DivYieldPercent
                              + "^" + value.MarketCap + "^" + value.Name + "^" + value.URI + "^" + value.NetProfitMarginPercentMRQ 
                              + "^" + value.OneDayPriceChangePercent + "^" + value.PriceToBookValue + "^" + value.PriceToEarnings 
                              + "^" + value.PriceToFreeCashFlowMRQ + "^" + value.ROEPercent + "^" + value.Sector 
                              + "^" + value.SectorId + "^" + value.MoreInfoLink;

                DataRow row = dt.NewRow();

                row.ItemArray = sValue.Split('^');

                dt.Rows.Add(row);
            }

            return dt;
        }
    }
}