using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace ScanVXX.Models
{
    public class BaseData
    {
        public string baseUri {get;set;}

        public BaseData()
        {
            baseUri = "http://biz.yahoo.com/p/";
        }

        [JsonProperty(PropertyName = "Id")]
        public int id { get; set; }

        [JsonProperty(PropertyName = "Date")]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "URI")]
        public string URI { get; set; }

        //Sector
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        //1 Day Price Change %
        [JsonProperty(PropertyName = "OneDayPriceChangePercent")]
        public decimal OneDayPriceChangePercent { get; set; }

        //Market Cap 
        [JsonProperty(PropertyName = "MarketCap")]
        public string MarketCap { get; set; }

        //P/E 
        [JsonProperty(PropertyName = "PriceToEarnings")]
        public decimal PriceToEarnings { get; set; }

        //ROE % 
        [JsonProperty(PropertyName = "ROEPercent")]
        public decimal ROEPercent { get; set; }

        //Div. Yield % 
         [JsonProperty(PropertyName = "DivYieldPercent")]
        public decimal DivYieldPercent { get; set; }

        //Long-Term Debt to Equity
         [JsonProperty(PropertyName = "LongTermDebtToEquity")]
         public decimal LongTermDebtToEquity { get; set; }

        //Price to Book Value
        [JsonProperty(PropertyName = "PriceToBookValue")]
         public decimal PriceToBookValue { get; set; }

        //Net Profit Margin % (mrq)
        [JsonProperty(PropertyName = "NetProfitMarginPercentMRQ")]
        public decimal NetProfitMarginPercentMRQ { get; set; }

        //Price to Free Cash Flow (mrq)
        [JsonProperty(PropertyName = "PriceToFreeCashFlowMRQ")]
        public decimal PriceToFreeCashFlowMRQ { get; set; }

        protected string dateForSerialization;

        [OnDeserializing]
        void OnDeserializing(StreamingContext context)
        {
            this.dateForSerialization = "1900-01-01";
        }

        public decimal ScrubData(string value)
        {
            if (value == "") return 0;
            return Convert.ToDecimal(value);
        }
    }
}
