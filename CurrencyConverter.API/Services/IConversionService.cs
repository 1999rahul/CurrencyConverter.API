using CurrencyConverterApi.Models;

namespace CurrencyConverterApi.Services;

public interface IConversionService
{
    ConversionResponse Convert(ConversionRequest request);
}
