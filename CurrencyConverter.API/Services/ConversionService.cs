using CurrencyConverter.API.Exceptions;
using CurrencyConverterApi.Configuration;
using CurrencyConverterApi.Models;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace CurrencyConverterApi.Services;

public class ConversionService : IConversionService
{
    private static readonly HashSet<string> SupportedCurrencies = new() { "USD", "INR", "EUR" };

    private readonly IOptionsMonitor<ExchangeRateOptions> _optionsMonitor;
    private readonly ILogger<ConversionService> _logger;

    public ConversionService(IOptionsMonitor<ExchangeRateOptions> optionsMonitor, ILogger<ConversionService> logger)
    {
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    public ConversionResponse Convert(ConversionRequest request)
    {
        // Normalize currencies to uppercase
        var sourceCurrency = request.SourceCurrency?.ToUpperInvariant() ?? string.Empty;
        var targetCurrency = request.TargetCurrency?.ToUpperInvariant() ?? string.Empty;

        // Validation: Amount must be greater than 0
        if (request.Amount <= 0)
        {
            throw new CurrencyValidationException("Amount must be greater than zero.");
        }

        // Validation: Source and Target currencies must be supported
        ValidateCurrency(sourceCurrency, nameof(request.SourceCurrency));
        ValidateCurrency(targetCurrency, nameof(request.TargetCurrency));

        // Validation: SourceCurrency and TargetCurrency must be different
        if (sourceCurrency == targetCurrency)
        {
            _logger.LogWarning("Validation error: SourceCurrency and TargetCurrency must be different.");
            throw new CurrencyValidationException("SourceCurrency and TargetCurrency must be different.");
        }

        // Build the rate key
        var rateKey = $"{sourceCurrency}_TO_{targetCurrency}";

        // Get the exchange rate
        var rates = _optionsMonitor.CurrentValue.Rates;
        if (!rates.TryGetValue(rateKey, out var exchangeRate))
        {
            _logger.LogWarning("Rate not found: No exchange rate found for {RateKey}.", rateKey);
            throw new KeyNotFoundException($"No exchange rate found for {rateKey}.");
        }

        // Calculate converted amount
        var convertedAmount = Math.Round(request.Amount * exchangeRate, 2);

        _logger.LogInformation("Conversion successful: {SourceCurrency} to {TargetCurrency}, Amount: {Amount}, ExchangeRate: {ExchangeRate}, ConvertedAmount: {ConvertedAmount}",
            sourceCurrency, targetCurrency, request.Amount, exchangeRate, convertedAmount);

        return new ConversionResponse
        {
            ExchangeRate = exchangeRate,
            ConvertedAmount = convertedAmount
        };
    }

    private static void ValidateCurrency(string currency, string paramName)
    {
        if (!SupportedCurrencies.Contains(currency))
            throw new CurrencyValidationException(
                $"Currency '{currency}' is not supported. Supported currencies: {string.Join(", ", SupportedCurrencies)}.");
    }
}
