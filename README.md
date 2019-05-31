# .NET Core worker processing Service Bus Queue scaled by KEDA
A simple Docker container written in .NET that will receive messages from a Service Bus queue and scale via KEDA.

The message processor will receive a single message at a time (per instance), and sleep for 2 second to simulate performing work. When adding a massive amount of queue messages, KEDA will drive the container to scale out according to the event source (Service Bus Queue).

_The sample can also be ran locally on Docker without KEDA, read our [documentation here](./src/)._

## Pre-requisites

- .NET Core 3.0 Preview 5
- Azure Subscription
- Kubernetes cluster
- [KEDA installed](https://github.com/kedacore/keda#setup) on the cluster

## Setup
Coming soon.

### Create an Azure Service Bus Namespace & Queue

```cli
az servicebus namespace create --name <namespace-name> --resource-group <resource-group-name> --sku basic
az servicebus queue create --namespace-name <namespace-name> --name orders --resource-group <resource-group-name>
```


## Cleaning up resources

#### Delete the Order Processor

```cli
kubectl delete -f .\deploy\deploy-queue-processor.yaml
```

#### Delete the Azure Service Bus namespace

```cli
az servicebus namespace delete --name <namespace-name> --resource-group <resource-group-name>
```

#### Uninstall KEDA

```cli
helm delete --purge keda
kubectl delete customresourcedefinition  scaledobjects.keda.k8s.io
kubectl delete namespace keda
```