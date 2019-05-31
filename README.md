# .NET Core worker processing Service Bus Queue messages scaled by KEDA
A simple Docker container written in .NET that will receive messages from a Service Bus queue and scale via KEDA.

## Pre-requisites

- .NET Core 3.0 Preview 5
- Azure Subscription
- Kubernetes cluster
- [KEDA installed](https://github.com/kedacore/keda#setup) on the cluster

## Building & Running the sample locally on Docker

Build Docker container for order processor

```shell
⚡ tkerkhove@tomkerkhove C:\keda-sample-dotnet-worker-servicebus-queue
❯ docker build .\src --tag keda-sample-dotnet-worker-servicebus-queue --file .\src\Keda.Samples.Dotnet.QueueWorker\Dockerfile --no-cache
```

Run order processor locally
```shell
⚡ tkerkhove@tomkerkhove C:\keda-sample-dotnet-worker-servicebus-queue
❯ docker run --detach --env KEDA_SERVICEBUS_CONNECTIONSTRING="<connection-string>" --env KEDA_SERVICEBUS_QUEUENAME="orders" keda-sample-dotnet-worker-servicebus-queue
c6775c9383e56fc16da37b62ebbff0dc44d4019a53d282a1ef260a6d71022a32
```

Let's use the test orders via our `OrderGenerator` tool:
```shell
⚡ tkerkhove@tomkerkhove C:\keda-sample-dotnet-worker-servicebus-queue
❯ dotnet run --project .\src\Keda.Samples.Dotnet.OrderGenerator\Keda.Samples.Dotnet.OrderGenerator.csproj
Let's queue some orders, how many do you want?
2
Queuing order 719a7b19-f1f7-4f46-a543-8da9bfaf843d - A Hat for Reilly Davis
Queuing order 5c3a954c-c356-4cc9-b1d8-e31cd2c04a5a - A Salad for Savanna Rowe
That's it, see you later!
```

Logs indicate orders are being processed
```shell
⚡ tkerkhove@tomkerkhove C:\keda-sample-dotnet-worker-servicebus-queue
❯ docker logs c6775c9383e56fc16da37b62ebbff0dc44d4019a53d282a1ef260a6d71022a32
info: Keda.Samples.Dotnet.QueueWorker.OrdersQueueProcessor[0]
      Starting message pump at: 05/31/2019 09:03:21 +00:00
info: Keda.Samples.Dotnet.QueueWorker.OrdersQueueProcessor[0]
      Message pump started at: 05/31/2019 09:03:21 +00:00
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /app
info: Keda.Samples.Dotnet.QueueWorker.OrdersQueueProcessor[0]
      Received message d457cb2218864f60a3873c8d083c5bf4 with body {"Id":"719a7b19-f1f7-4f46-a543-8da9bfaf843d","Amount":605837337,"ArticleNumber":"Hat","Customer":{"FirstName":"Reilly","LastName":"Davis"}}
info: Keda.Samples.Dotnet.QueueWorker.OrdersQueueProcessor[0]
      Processing order 719a7b19-f1f7-4f46-a543-8da9bfaf843d for 605837337 units of Hat bought by Reilly Davis at: 05/31/2019 09:03:22 +00:00
info: Keda.Samples.Dotnet.QueueWorker.OrdersQueueProcessor[0]
      Order 719a7b19-f1f7-4f46-a543-8da9bfaf843d processed at: 05/31/2019 09:03:24 +00:00
info: Keda.Samples.Dotnet.QueueWorker.OrdersQueueProcessor[0]
      Message d457cb2218864f60a3873c8d083c5bf4 processed at: 05/31/2019 09:03:24 +00:00
info: Keda.Samples.Dotnet.QueueWorker.OrdersQueueProcessor[0]
      Received message f2fc0aae896249659bb921416886060c with body {"Id":"5c3a954c-c356-4cc9-b1d8-e31cd2c04a5a","Amount":1026689653,"ArticleNumber":"Salad","Customer":{"FirstName":"Savanna","LastName":"Rowe"}}
info: Keda.Samples.Dotnet.QueueWorker.OrdersQueueProcessor[0]
      Processing order 5c3a954c-c356-4cc9-b1d8-e31cd2c04a5a for 1026689653 units of Salad bought by Savanna Rowe at: 05/31/2019 09:03:24 +00:00
info: Keda.Samples.Dotnet.QueueWorker.OrdersQueueProcessor[0]
      Order 5c3a954c-c356-4cc9-b1d8-e31cd2c04a5a processed at: 05/31/2019 09:03:26 +00:00
info: Keda.Samples.Dotnet.QueueWorker.OrdersQueueProcessor[0]
      Message f2fc0aae896249659bb921416886060c processed at: 05/31/2019 09:03:26 +00:00
```  