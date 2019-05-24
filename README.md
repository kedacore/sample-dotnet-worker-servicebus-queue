# .NET Core worker processing Service Bus Queue scaled by KEDA
A simple Docker container written in .NET that will receive messages from a Service Bus queue and scale via KEDA.

## Pre-requisites

- Azure Subscription
- Kubernetes cluster
- [KEDA installed](https://github.com/kedacore/keda#setup) on the cluster
