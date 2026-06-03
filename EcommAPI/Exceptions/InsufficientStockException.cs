using EcommDTO;

namespace EcommAPI.Exceptions
{
    public class InsufficientStockException : Exception
    {
        public List<StockFailure> Failures { get; }

        public InsufficientStockException(
            List<StockFailure> failures)
        {
            Failures = failures;
        }
    }
}
