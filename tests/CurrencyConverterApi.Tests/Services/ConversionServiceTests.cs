using CurrencyConverter.API.Exceptions;
using CurrencyConverterApi.Configuration;
using CurrencyConverterApi.Models;
using CurrencyConverterApi.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CurrencyConverterApi.Tests.Services;

public class ConversionServiceTests
{
    private readonly Mock<IOptionsMonitor<ExchangeRateOptions>> _mockOptionsMonitor;
    private readonly Mock<ILogger<ConversionService>> _mockLogger;
    private readonly ConversionService _service;

    public ConversionServiceTests()
    {
        _mockOptionsMonitor = new Mock<IOptionsMonitor<ExchangeRateOptions>>();
        _mockLogger = new Mock<ILogger<ConversionService>>();
        _service = new ConversionService(_mockOptionsMonitor.Object, _mockLogger.Object);
    }

    private void SetupRates(Dictionary<string, decimal> rates)
    {
        var options = new ExchangeRateOptions { Rates = rates };
        _mockOptionsMonitor.Setup(m => m.CurrentValue).Returns(options);
    }

    [Fact]
    public void Convert_ValidRequest_ReturnsCorrectResult()
    {
        // Arrange
        SetupRates(new Dictionary<string, decimal> { { "USD_TO_INR", 74m } });
        var request = new ConversionRequest { SourceCurrency = "USD", TargetCurrency = "INR", Amount = 100 };

        // Act
        var result = _service.Convert(request);

        // Assert
        Assert.Equal(74m, result.ExchangeRate);
        Assert.Equal(7400m, result.ConvertedAmount);
    }

    [Fact]
    public void Convert_ValidRequest_RoundsToTwoDecimalPlaces()
    {
        // Arrange
        SetupRates(new Dictionary<string, decimal> { { "INR_TO_USD", 0.013m } });
        var request = new ConversionRequest { SourceCurrency = "INR", TargetCurrency = "USD", Amount = 1 };

        // Act
        var result = _service.Convert(request);

        // Assert
        Assert.Equal(0.01m, result.ConvertedAmount);
    }

    [Fact]
    public void Convert_ZeroAmount_ThrowsArgumentException()
    {
        // Arrange
        SetupRates(new Dictionary<string, decimal> { { "USD_TO_INR", 74m } });
        var request = new ConversionRequest { SourceCurrency = "USD", TargetCurrency = "INR", Amount = 0 };

        // Act & Assert
        var exception = Assert.Throws<CurrencyValidationException>(() => _service.Convert(request));
        Assert.Equal("Amount must be greater than zero.", exception.Message);
    }

    [Fact]
    public void Convert_NegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        SetupRates(new Dictionary<string, decimal> { { "USD_TO_INR", 74m } });
        var request = new ConversionRequest { SourceCurrency = "USD", TargetCurrency = "INR", Amount = -10 };

        // Act & Assert
        var exception = Assert.Throws<CurrencyValidationException>(() => _service.Convert(request));
        Assert.Equal("Amount must be greater than zero.", exception.Message);
    }

    [Fact]
    public void Convert_UnsupportedSourceCurrency_ThrowsArgumentException()
    {
        // Arrange
        SetupRates(new Dictionary<string, decimal> { { "GBP_TO_INR", 90m } });
        var request = new ConversionRequest { SourceCurrency = "GBP", TargetCurrency = "INR", Amount = 100 };

        // Act & Assert
        var exception = Assert.Throws<CurrencyValidationException>(() => _service.Convert(request));
        Assert.Contains("Currency 'GBP' is not supported", exception.Message);
        Assert.Contains("USD, INR, EUR", exception.Message);
    }

    [Fact]
    public void Convert_UnsupportedTargetCurrency_ThrowsArgumentException()
    {
        // Arrange
        SetupRates(new Dictionary<string, decimal> { { "USD_TO_JPY", 110m } });
        var request = new ConversionRequest { SourceCurrency = "USD", TargetCurrency = "JPY", Amount = 100 };

        // Act & Assert
        var exception = Assert.Throws<CurrencyValidationException>(() => _service.Convert(request));
        Assert.Contains("Currency 'JPY' is not supported", exception.Message);
        Assert.Contains("USD, INR, EUR", exception.Message);
    }

    [Fact]
    public void Convert_SameCurrency_ThrowsArgumentException()
    {
        // Arrange
        SetupRates(new Dictionary<string, decimal> { { "USD_TO_INR", 74m } });
        var request = new ConversionRequest { SourceCurrency = "USD", TargetCurrency = "USD", Amount = 100 };

        // Act & Assert
        var exception = Assert.Throws<CurrencyValidationException>(() => _service.Convert(request));
        Assert.Equal("SourceCurrency and TargetCurrency must be different.", exception.Message);
    }

    [Fact]
    public void Convert_MissingRateKey_ThrowsKeyNotFoundException()
    {
        // Arrange
        SetupRates(new Dictionary<string, decimal>());
        var request = new ConversionRequest { SourceCurrency = "USD", TargetCurrency = "INR", Amount = 100 };

        // Act & Assert
        var exception = Assert.Throws<KeyNotFoundException>(() => _service.Convert(request));
        Assert.Equal("No exchange rate found for USD_TO_INR.", exception.Message);
    }

    [Fact]
    public void Convert_CurrencyInputIsCaseInsensitive()
    {
        // Arrange
        SetupRates(new Dictionary<string, decimal> { { "USD_TO_INR", 74m } });
        var request = new ConversionRequest { SourceCurrency = "usd", TargetCurrency = "inr", Amount = 100 };

        // Act
        var result = _service.Convert(request);

        // Assert
        Assert.Equal(74m, result.ExchangeRate);
        Assert.Equal(7400m, result.ConvertedAmount);
    }

    [Fact]
    public void Convert_LargeAmount_ReturnsCorrectResult()
    {
        // Arrange
        SetupRates(new Dictionary<string, decimal> { { "USD_TO_INR", 74m } });
        var request = new ConversionRequest { SourceCurrency = "USD", TargetCurrency = "INR", Amount = 1_000_000 };

        // Act
        var result = _service.Convert(request);

        // Assert
        Assert.Equal(74m, result.ExchangeRate);
        Assert.Equal(74_000_000m, result.ConvertedAmount);
    }

    [Fact]
    public void Convert_DecimalAmount_RoundsCorrectly()
    {
        // Arrange
        SetupRates(new Dictionary<string, decimal> { { "USD_TO_INR", 74.5m } });
        var request = new ConversionRequest { SourceCurrency = "USD", TargetCurrency = "INR", Amount = 100.50m };

        // Act
        var result = _service.Convert(request);

        // Assert
        Assert.Equal(74.5m, result.ExchangeRate);
        Assert.Equal(7487.25m, result.ConvertedAmount);
    }

    [Fact]
    public void Convert_SmallAmount_RoundsCorrectly()
    {
        // Arrange
        SetupRates(new Dictionary<string, decimal> { { "USD_TO_INR", 74m } });
        var request = new ConversionRequest { SourceCurrency = "USD", TargetCurrency = "INR", Amount = 0.01m };

        // Act
        var result = _service.Convert(request);

        // Assert
        Assert.Equal(74m, result.ExchangeRate);
        Assert.Equal(0.74m, result.ConvertedAmount);
    }

    [Fact]
    public void Convert_HighPrecisionRate_RoundsCorrectly()
    {
        // Arrange
        SetupRates(new Dictionary<string, decimal> { { "INR_TO_EUR", 0.011m } });
        var request = new ConversionRequest { SourceCurrency = "INR", TargetCurrency = "EUR", Amount = 1000 };

        // Act
        var result = _service.Convert(request);

        // Assert
        Assert.Equal(0.011m, result.ExchangeRate);
        Assert.Equal(11m, result.ConvertedAmount);
    }

    [Fact]
    public void Convert_MixedCaseInput_Succeeds()
    {
        // Arrange
        SetupRates(new Dictionary<string, decimal> { { "USD_TO_INR", 74m } });
        var request = new ConversionRequest { SourceCurrency = "UsD", TargetCurrency = "InR", Amount = 100 };

        // Act
        var result = _service.Convert(request);

        // Assert
        Assert.Equal(74m, result.ExchangeRate);
        Assert.Equal(7400m, result.ConvertedAmount);
    }

    [Fact]
    public void Convert_ReverseDirection_Succeeds()
    {
        // Arrange
        SetupRates(new Dictionary<string, decimal> { { "INR_TO_USD", 0.013m } });
        var request = new ConversionRequest { SourceCurrency = "INR", TargetCurrency = "USD", Amount = 1000 };

        // Act
        var result = _service.Convert(request);

        // Assert
        Assert.Equal(0.013m, result.ExchangeRate);
        Assert.Equal(13m, result.ConvertedAmount);
    }

    [Fact]
    public void Convert_AllCurrencies_Supported()
    {
        // Test that all three currencies (USD, INR, EUR) are properly recognized
        SetupRates(new Dictionary<string, decimal>
        {
            { "USD_TO_INR", 74m },
            { "USD_TO_EUR", 0.85m },
            { "INR_TO_EUR", 0.011m }
        });

        var currenciesToTest = new[] { "USD", "INR", "EUR" };

        foreach (var currency in currenciesToTest)
        {
            // Act
            var request = new ConversionRequest { SourceCurrency = currency, TargetCurrency = "USD", Amount = 100 };

            // Assert - should not throw if currency is supported
            if (currency == "USD")
            {
                // USD to USD should throw (same currency)
                Assert.Throws<CurrencyValidationException>(() => _service.Convert(request));
            }
            else
            {
                // This will throw KeyNotFoundException because we don't have all pairs, but the point is the currency is recognized
                try
                {
                    _service.Convert(request);
                }
                catch (KeyNotFoundException)
                {
                    // Expected if the rate doesn't exist
                }
                catch (CurrencyValidationException)
                {
                    Assert.Fail($"Currency {currency} should be supported");
                }
            }
        }
    }

    [Fact]
    public void Convert_EmptySourceCurrency_ThrowsArgumentException()
    {
        // Arrange
        SetupRates(new Dictionary<string, decimal> { { "USD_TO_INR", 74m } });
        var request = new ConversionRequest { SourceCurrency = "", TargetCurrency = "INR", Amount = 100 };

        // Act & Assert
        var exception = Assert.Throws<CurrencyValidationException>(() => _service.Convert(request));
        Assert.Contains("not supported", exception.Message);
    }

    [Fact]
    public void Convert_EmptyTargetCurrency_ThrowsArgumentException()
    {
        // Arrange
        SetupRates(new Dictionary<string, decimal> { { "USD_TO_INR", 74m } });
        var request = new ConversionRequest { SourceCurrency = "USD", TargetCurrency = "", Amount = 100 };

        // Act & Assert
        var exception = Assert.Throws<CurrencyValidationException>(() => _service.Convert(request));
        Assert.Contains("not supported", exception.Message);
    }

    [Fact]
    public void Convert_NullSourceCurrency_ThrowsArgumentException()
    {
        // Arrange
        SetupRates(new Dictionary<string, decimal> { { "USD_TO_INR", 74m } });
        var request = new ConversionRequest { SourceCurrency = null, TargetCurrency = "INR", Amount = 100 };

        // Act & Assert
        var exception = Assert.Throws<CurrencyValidationException>(() => _service.Convert(request));
        Assert.Contains("not supported", exception.Message);
    }

    [Fact]
    public void Convert_NullTargetCurrency_ThrowsArgumentException()
    {
        // Arrange
        SetupRates(new Dictionary<string, decimal> { { "USD_TO_INR", 74m } });
        var request = new ConversionRequest { SourceCurrency = "USD", TargetCurrency = null, Amount = 100 };

        // Act & Assert
        var exception = Assert.Throws<CurrencyValidationException>(() => _service.Convert(request));
        Assert.Contains("not supported", exception.Message);
    }

    [Theory]
    [InlineData("USD", "INR", 100, 74)]
    [InlineData("USD", "EUR", 100, 85)]
    [InlineData("EUR", "USD", 100, 118)]
    public void Convert_MultipleValidPairs_Succeed(string source, string target, decimal amount, decimal expectedRate)
    {
        // Arrange
        var rateKey = $"{source}_TO_{target}";
        SetupRates(new Dictionary<string, decimal> { { rateKey, expectedRate } });
        var request = new ConversionRequest { SourceCurrency = source, TargetCurrency = target, Amount = amount };

        // Act
        var result = _service.Convert(request);

        // Assert
        Assert.Equal(expectedRate, result.ExchangeRate);
        Assert.Equal(amount * expectedRate, result.ConvertedAmount);
    }
}
