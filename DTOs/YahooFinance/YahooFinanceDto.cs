using System.Text.Json.Serialization;

namespace server.DTOs.YahooFinance
{
    public class YahooFinanceResponse
    {
        [JsonPropertyName("chart")]
        public Chart Chart { get; set; } = new();
    }

    public class Chart
    {
        [JsonPropertyName("result")]
        public List<ChartResult> Result { get; set; } = new();

        [JsonPropertyName("error")]
        public object? Error { get; set; }
    }

    public class ChartResult
    {
        [JsonPropertyName("meta")]
        public Meta Meta { get; set; } = new();

        [JsonPropertyName("timestamp")]
        public List<long> Timestamp { get; set; } = new();

        [JsonPropertyName("indicators")]
        public Indicators Indicators { get; set; } = new();
    }

    public class Meta
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("regularMarketPrice")]
        public decimal? RegularMarketPrice { get; set; }

        [JsonPropertyName("previousClose")]
        public decimal? PreviousClose { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("exchangeName")]
        public string ExchangeName { get; set; } = string.Empty;
        public string? MarketState { get; set; }
        public long? RegularMarketTime { get; set; }
    }

    public class Indicators
    {
        [JsonPropertyName("quote")]
        public List<Quote> Quote { get; set; } = new();
    }

    public class Quote
    {
        [JsonPropertyName("open")]
        public List<decimal?> Open { get; set; } = new();

        [JsonPropertyName("high")]
        public List<decimal?> High { get; set; } = new();

        [JsonPropertyName("low")]
        public List<decimal?> Low { get; set; } = new();

        [JsonPropertyName("close")]
        public List<decimal?> Close { get; set; } = new();

        [JsonPropertyName("volume")]
        public List<long?> Volume { get; set; } = new();
    }

    public class YahooStockInfo
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal? RegularMarketPrice { get; set; }
        public decimal? PreviousClose { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string ExchangeName { get; set; } = string.Empty;
        public string MarketState { get; set; } = string.Empty;
        public DateTime RegularMarketTime { get; set; }
    }

    public class YahooStockDetailResponse
    {
        public QuoteSummary QuoteSummary { get; set; } = new();
    }

    public class QuoteSummary
    {
        public List<QuoteSummaryResult> Result { get; set; } = new();
        public object? Error { get; set; }
    }

    public class QuoteSummaryResult
    {
        public PriceData? Price { get; set; }
        public SummaryDetail? SummaryDetail { get; set; }
        public DefaultKeyStatistics? DefaultKeyStatistics { get; set; }
        public AssetProfile? AssetProfile { get; set; }
    }

    public class PriceData
    {
        public string? ShortName { get; set; }
        public string? Symbol { get; set; }
        public YahooValue? RegularMarketPrice { get; set; }
        public YahooValue? MarketCap { get; set; }
    }

    public class SummaryDetail
    {
        public YahooValue? TrailingPE { get; set; }
        public YahooValue? DividendYield { get; set; }
        public YahooValue? LastDividendValue { get; set; }
    }

    public class DefaultKeyStatistics
    {
        public YahooValue? PeRatio { get; set; }
        public YahooValue? MarketCap { get; set; }
    }

    public class AssetProfile
    {
        public string? LongName { get; set; }
        public string? Industry { get; set; }
        public string? Country { get; set; }
        public string? Sector { get; set; }
    }

    public class YahooValue
    {
        public long Raw { get; set; }
        public string? Fmt { get; set; }
    }
}
