namespace Domain;

public interface IProductRepository
{
    Task ReadInputFile(string inputPath, CancellationToken cancellationToken);
    Task HandleProducts(CancellationToken cancellationToken);
    Task WriteOutputFile(string outputPath, CancellationToken cancellationToken);
}