using server.Helpers;
using System.Text.Json.Serialization;

namespace server.DTOs.Fmp
{
    // FMP API Response DTOs
    public class FmpQuoteResponse
    {
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal ChangesPercentage { get; set; }
        public decimal Change { get; set; }
        public decimal DayLow { get; set; }
        public decimal DayHigh { get; set; }
        public decimal YearHigh { get; set; }
        public decimal YearLow { get; set; }
        public long MarketCap { get; set; }
        public decimal PriceAvg50 { get; set; }
        public decimal PriceAvg200 { get; set; }
        public string Exchange { get; set; } = string.Empty;
        public long Volume { get; set; }
        public long AvgVolume { get; set; }
        public decimal Open { get; set; }
        public decimal PreviousClose { get; set; }
        public decimal Eps { get; set; }
        public decimal Pe { get; set; }
        public string EarningsAnnouncement { get; set; } = string.Empty;
        public long SharesOutstanding { get; set; }
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime Timestamp { get; set; }
    }

    public class FmpProfileResponse
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Beta { get; set; }
        public long VolAvg { get; set; }
        public long MktCap { get; set; }
        public decimal LastDiv { get; set; }
        public string Range { get; set; } = string.Empty;
        public decimal Changes { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string Cik { get; set; } = string.Empty;
        public string Isin { get; set; } = string.Empty;
        public string Cusip { get; set; } = string.Empty;
        public string Exchange { get; set; } = string.Empty;
        public string ExchangeShortName { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Ceo { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string FullTimeEmployees { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Zip { get; set; } = string.Empty;
        public decimal DcfDiff { get; set; }
        public decimal Dcf { get; set; }
        public string Image { get; set; } = string.Empty;
        public DateTime IpoDate { get; set; }
        public bool DefaultImage { get; set; }
        public bool IsEtf { get; set; }
        public bool IsActivelyTrading { get; set; }
        public bool IsAdr { get; set; }
        public bool IsFund { get; set; }
    }

    public class FmpHistoricalPriceResponse
    {
        public string Symbol { get; set; } = string.Empty;
        public List<FmpDailyPrice> Historical { get; set; } = new();
    }

    public class FmpDailyPrice
    {
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal AdjClose { get; set; }
        public long Volume { get; set; }
        public decimal UnadjustedVolume { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercent { get; set; }
        public decimal Vwap { get; set; }
        public string Label { get; set; } = string.Empty;
        public decimal ChangeOverTime { get; set; }
    }

    public class FmpRealtimePrice
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Change { get; set; }
        public decimal ChangesPercentage { get; set; }
    }
}
