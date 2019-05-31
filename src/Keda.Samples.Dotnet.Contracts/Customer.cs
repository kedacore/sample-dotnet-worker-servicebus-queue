using Newtonsoft.Json;

namespace Keda.Samples.Dotnet.Contracts
{
    public class Customer
    {
        [JsonProperty]
        public string FirstName { get; private set; }

        [JsonProperty]
        public string LastName { get; private set; }
    }
}
