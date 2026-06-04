namespace CurrencyConverterApi.Models;

public class ConversionRequest
{
    public string SourceCurrency { get; set; } = string.Empty;

    public string TargetCurrency { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}
