using CurrencyConverter.API.Middleware;
using CurrencyConverterApi.Configuration;
using CurrencyConverterApi.Middleware;
using CurrencyConverterApi.Services;
using Serilog;
using Serilog.Context;

var builder = WebApplication.CreateBuilder(args);

// ----- Serilog -----
builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration));

// ----- Configuration: exchangeRates.json + env var overrides -----
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("exchangeRates.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// ----- Bind ExchangeRateOptions -----
builder.Services.Configure<ExchangeRateOptions>(
    builder.Configuration.GetSection("ExchangeRates"));

// ----- Services -----
builder.Services.AddControllers();
builder.Services.AddScoped<IConversionService, ConversionService>();

var app = builder.Build();

// ----- Middleware pipeline -----
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();
app.MapControllers();

app.Run();
