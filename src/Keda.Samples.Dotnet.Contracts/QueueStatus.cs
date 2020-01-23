using Newtonsoft.Json;

namespace Keda.Samples.Dotnet.Contracts
{
    public class QueueStatus
    {
        [JsonProperty]
        public long MessageCount { get; set; }
    }
}
