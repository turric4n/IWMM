namespace IWMM.Services.Abstractions
{
    public class FqdnResolverException : Exception
    {
        public FqdnResolverException(string? message) : base(message)
        {
        }
    }
}
