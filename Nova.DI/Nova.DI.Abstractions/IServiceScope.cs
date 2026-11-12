namespace Nova.DI;

public interface IServiceScope : IDisposable, IAsyncDisposable
{
    IServiceProvider ServiceProvider { get; }
}
