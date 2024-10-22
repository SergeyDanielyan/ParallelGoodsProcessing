namespace Domain;

public class Product
{
    public int Id { get; private set; }
    public int Prediction { get; private set; }
    public int Stock { get; private set; }

    public Product(int id, int prediction, int stock)
    {
        Id = id;
        Prediction = prediction;
        Stock = stock;
    }

    public int Demand
    {
        get
        {
            return Math.Max(Prediction - Stock, 0);
        }
    }

}