namespace Producer.Worker;

public interface IDocumentWorkerFactory
{
    IDocumentWorker Create();
}

public class DocumentWorkerFactory(IServiceProvider serviceProvider) : IDocumentWorkerFactory
{
    public IDocumentWorker Create()
    {
        return serviceProvider.GetRequiredService<IDocumentWorker>();
    }
}