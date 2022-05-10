namespace Keda.Samples.Dotnet.OrderProcessor
{
    public enum AuthenticationMode
    {
        ConnectionString,
        ServicePrinciple,
        PodIdentity,
        WorkloadIdentity
    }
}
