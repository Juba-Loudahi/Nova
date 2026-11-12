namespace Nova.DI;

public class DependencyInjectionException : InvalidOperationException
{
    public DependencyInjectionException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}
