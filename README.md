# .NET Core worker processing Service Bus Queue scaled by KEDA
A simple Docker container written in .NET that will receive messages from a Service Bus queue and scale via KEDA.

The message processor will receive a single message at a time (per instance), and sleep for 2 second to simulate performing work. When adding a massive amount of queue messages, KEDA will drive the container to scale out according to the event source (Service Bus Queue).

## Pre-requisites

- .NET Core 3.0 Preview 5
- Azure Subscription
- Kubernetes cluster
- [KEDA installed](https://github.com/kedacore/keda#setup) on the cluster

## Setup

Coming soon.

Read our [documentation here](./src/) on how to run it locally on Docker.
