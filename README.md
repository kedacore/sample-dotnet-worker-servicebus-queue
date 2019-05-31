# .NET Core worker processing Service Bus Queue messages scaled by KEDA
A simple Docker container written in .NET that will receive messages from a Service Bus queue and scale via KEDA.

## Pre-requisites

- .NET Core 3.0 Preview 5
- Azure Subscription
- Kubernetes cluster
- [KEDA installed](https://github.com/kedacore/keda#setup) on the cluster

## Building the sample

```shell
⚡ tkerkhove@tomkerkhove C:\keda
❯ dotnet build .\src\Keda.Samples.Dotnet.sln
```

## Sample order message

```json
{
  "id": "12345",
  "amount": "100",
  "articleNumber": "978-0395489321",
  "customer": {
    "firstName": "Bill",
    "lastName": "Bracket"
  }
}
```