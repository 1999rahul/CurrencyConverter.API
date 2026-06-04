namespace CurrencyConverter.API.Exceptions
{
    public class CurrencyValidationException : Exception
    {
        public CurrencyValidationException(string message) : base(message) { }
    }
}
