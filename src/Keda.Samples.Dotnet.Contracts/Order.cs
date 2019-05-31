using Newtonsoft.Json;

namespace Keda.Samples.Dotnet.Contracts
{
    public class Order
    {
        [JsonProperty]
        public string Id { get; private set; }

        [JsonProperty]
        public int Amount { get; private set; }

        [JsonProperty]
        public string ArticleNumber { get; private set; }

        [JsonProperty]
        public Customer Customer { get; private set; }
    }
}
