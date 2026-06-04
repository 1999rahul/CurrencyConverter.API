namespace CurrencyConverterApi.Configuration;

public class ExchangeRateOptions
{
    public Dictionary<string, decimal> Rates { get; set; } = new();
}
