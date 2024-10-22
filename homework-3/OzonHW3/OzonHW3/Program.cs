using Domain;
using Infrastructure;

namespace OzonHW3;

class Program
{
    private static string _inputFilePath = Path.Combine("..", "..", "..", "..", "data", "input.txt");
    private static string _outputFilePath = Path.Combine("..", "..", "..", "..", "data", "output.txt");
    private static int _maxDegreeOfParallelism = Environment.ProcessorCount * 2;
    
    public static void ChangeMaxDegreeOfParallelism(int maxDegreeOfParallelism) {
        _maxDegreeOfParallelism = maxDegreeOfParallelism;
    }
    
    static async Task Main(string[] args)
    {
        IProductRepository productRepository = new ProductRepository();
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        Task input = Task.Run(() => productRepository.ReadInputFile(_inputFilePath, cancellationToken));
        
        Task[] tasks = Enumerable.Range(0, _maxDegreeOfParallelism)
            .Select(i => Task.Run(() => productRepository.HandleProducts(cancellationToken)))
            .ToArray();
        
        Task output = Task.Run(() => productRepository.WriteOutputFile(_outputFilePath, cancellationToken));

        
        Console.CancelKeyPress += (sender, e) =>
                    {
                        Console.WriteLine("Отмена расчета...");
                        cancellationTokenSource.Cancel();
                        e.Cancel = true;
                    };
        
        try
        {
            await Task.WhenAll(input, output);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Расчет отменен.");
        }
        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Расчет отменен.");
        }
    }
}