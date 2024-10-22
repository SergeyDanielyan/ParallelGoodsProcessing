using System.Collections.Concurrent;
using System.IO;
using Domain;

namespace Infrastructure;

public class ProductRepository : IProductRepository
{
    private readonly ConcurrentQueue<string> _stringProducts = new ConcurrentQueue<string>();
    private readonly ConcurrentQueue<Product> _products = new ConcurrentQueue<Product>();
    private int _readLinesNumber = 0;
    private int _handledLinesNumber = 0;
    private int _writtenLinesNumber = 0;
    private bool _isRead = false;
    private object handlerLock = new object();
    
    
    private Product GetProductByString(string stringData)
    {
        int[] parsedData = Array.ConvertAll(stringData.Split(", "), int.Parse);
        if (parsedData.Length != 3)
        {
            throw new FormatException();
        }
        return new Product(parsedData[0], parsedData[1], parsedData[2]);
    }

    public async Task ReadInputFile(string inputPath, CancellationToken cancellationToken)
    {
        using (StreamReader sr = new StreamReader(inputPath, System.Text.Encoding.UTF8, false, 1024 * 1024))
        {
            string header = await sr.ReadLineAsync();
            if (header != "id, prediction, stock")
            {
                Console.WriteLine("The header is wrong");
            }
            while (!sr.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                _stringProducts.Enqueue(await sr.ReadLineAsync());
                Console.WriteLine($"The number of read products: {++_readLinesNumber}");
            }
        }
        Console.WriteLine("The file is read");
        _isRead = true;
    }

    public async Task HandleProducts(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            bool isDequed;
            string data;
            lock (handlerLock) {
                isDequed = _stringProducts.TryDequeue(out data);
            }
            if (isDequed)
            {
                try
                {
                    
                    lock (handlerLock) {
                        Product product = GetProductByString(data);
                        Thread.Sleep(50);
                        _products.Enqueue(product);
                        Console.WriteLine($"The number of handled products: {++_handledLinesNumber}");
                    }
                    if (_isRead && _handledLinesNumber == _readLinesNumber) {
                        break;
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("The line with wrong format is skipped");
                }
            }
            else
            {
                await Task.Delay(100);
            }
        }
    }

    public async Task WriteOutputFile(string outputPath, CancellationToken cancellationToken)
    {
        using (StreamWriter sw = new StreamWriter(outputPath))
        {
            await sw.WriteLineAsync("id, demand");
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_products.TryDequeue(out Product product))
                {
                    await sw.WriteLineAsync($"{product.Id}, {product.Demand}");
                    Console.WriteLine($"The number of written products: {++_writtenLinesNumber}");
                    if (_isRead && _writtenLinesNumber == _readLinesNumber) {
                        break; 
                    }
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }
    }
}