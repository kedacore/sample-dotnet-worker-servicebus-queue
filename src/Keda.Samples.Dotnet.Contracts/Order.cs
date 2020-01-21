namespace Keda.Samples.Dotnet.Contracts
{
    public class Order
    {
        public string Id { get; set; }
        public int Amount { get; set; }
        public string ArticleNumber { get; set; }
        public Customer Customer { get; set; }
    }
}