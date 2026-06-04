using CurrencyConverterApi.Models;
using CurrencyConverterApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverterApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ConvertController : ControllerBase
{
    private readonly IConversionService _conversionService;
    private readonly ILogger<ConvertController> _logger;

    public ConvertController(IConversionService conversionService, ILogger<ConvertController> logger)
    {
        _conversionService = conversionService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get([FromQuery] ConversionRequest request)
    {
        var result = _conversionService.Convert(request);
        return Ok(result);
    }
}
