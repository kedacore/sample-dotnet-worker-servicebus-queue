# .NET Core worker processing Azure Service Bus Queue scaled by KEDA with connection strings

A simple Docker container written in .NET that will receive messages from a Service Bus queue and scale via KEDA with connection strings.

The message processor will receive a single message at a time (per instance), and sleep for 2 second to simulate performing work. When adding a massive amount of queue messages, KEDA will drive the container to scale out according to the event source (Service Bus Queue).

> 💡 *If you want to learn how to scale this sample with KEDA 1.0, feel free to read about it [here](https://github.com/kedacore/sample-dotnet-worker-servicebus-queue/tree/keda-v1.0).*

_The sample can also be ran locally on Docker without KEDA, read our [documentation here](./src/)._

## A closer look at our KEDA Scaling

This is defined via the `ScaledObject` which is deployed along with our application.

```yaml
apiVersion: keda.sh/v1alpha1
kind: ScaledObject
metadata:
  name: order-processor-scaler
  labels:
    app: order-processor
    name: order-processor
spec:
  scaleTargetRef:
    name: order-processor
  # minReplicaCount: 0 Change to define how many minimum replicas you want
  maxReplicaCount: 10
  triggers:
  - type: azure-servicebus
    metadata:
      queueName: orders
      queueLength: '5'
    authenticationRef:
      name: trigger-auth-service-bus-orders
```

It defines the type of scale trigger we'd like to use, in our case `azure-servicebus`, and the scaling criteria. For our scenario we'd like to scale out if there are 5 or more messages in the `orders` queue with a maximum of 10 concurrent replicas which is defined via `maxReplicaCount`.

Next to that, it is referring to `trigger-auth-service-bus-orders` which is a `TriggerAuthentication` resource that defines how KEDA should authenticate to get the metrics:

```yaml
apiVersion: keda.sh/v1alpha1
kind: TriggerAuthentication
metadata:
  name: trigger-auth-service-bus-orders
spec:
  secretTargetRef:
  - parameter: connection
    name: secrets-order-management
    key: servicebus-order-management-connectionstring
```

In this case, we are telling KEDA to read the `connection` parameter from a Kubernetes secret with the name `secrets-order-management` and pass the value of the entry with key `servicebus-order-management-connectionstring`.

This allows us to not only re-use this authentication resource but also assign different permissions to KEDA than our app itself.

## Deploy manually

### Pre-requisites

- Azure CLI
- Azure Subscription
- .NET Core 3.0
- Kubernetes cluster with [KEDA v2.0+ installed](https://keda.sh/docs/2.0/deploy/)

### Setup

This setup will go through creating an Azure Service Bus queue  and deploying this consumer with the `ScaledObject` to scale via KEDA.  If you already have an Azure Service Bus namespace you can use your existing queues.

#### Creating a new Azure Service Bus namespace & queue

We will start by creating a new Azure Service Bus namespace:

```cli
❯ az servicebus namespace create --name <namespace-name> --resource-group <resource-group-name> --sku basic
```

After that, we create an `orders` queue in our namespace:

```cli
❯ az servicebus queue create --namespace-name <namespace-name> --name orders --resource-group <resource-group-name>
```

We need to be able to connect to our queue, so we create a new authorization rule with `Listen` permissions which our app will use to process messages.

```cli
❯ az servicebus queue authorization-rule create --resource-group <resource-group-name> --namespace-name <namespace-name> --queue-name orders --name order-consumer --rights Listen
```

Once the authorization rule is created, we can list the connection string as following:

```cli
❯ az servicebus queue authorization-rule keys list --resource-group <resource-group-name> --namespace-name <namespace-name> --queue-name orders --name order-consumer
{
  "aliasPrimaryConnectionString": null,
  "aliasSecondaryConnectionString": null,
  "keyName": "order-consumer",
  "primaryConnectionString": "Endpoint=sb://keda.servicebus.windows.net/;SharedAccessKeyName=order-consumer;SharedAccessKey=<redacted>;EntityPath=orders",
  "primaryKey": "<redacted>",
  "secondaryConnectionString": "Endpoint=sb://keda.servicebus.windows.net/;SharedAccessKeyName=order-consumer;SharedAccessKey=<redacted>;EntityPath=orders",
  "secondaryKey": "<redacted>"
}
```

Create a base64 representation of the connection string and update our Kubernetes secret in `deploy/connection-string/deploy-app.yaml`:

```cli
❯ echo -n "<connection string>" | base64
```

#### Deploying our order processor

We will start by creating a new Kubernetes namespace to run our order processor in:

```cli
❯ kubectl create namespace keda-dotnet-sample
namespace "keda-dotnet-sample" created
```

Before we can connect to our queue, we need to create a secret which contains the Service Bus connection string to the queue.

```cli
❯ kubectl apply -f deploy/connection-string/deploy-app.yaml --namespace keda-dotnet-sample
deployment.apps/order-processor created
secret/secrets-order-consumer created
```

Once created, you should be able to retrieve the secret:

```cli
❯ kubectl get secrets --namespace keda-dotnet-sample

NAME                  TYPE                                  DATA      AGE
secrets-order-consumer         Opaque                                1         24s
```

Next to that, you will see that our deployment shows up with one pods created:

```cli
❯ kubectl get deployments --namespace keda-dotnet-sample -o wide
NAME              DESIRED   CURRENT   UP-TO-DATE   AVAILABLE   AGE       CONTAINERS        IMAGES                                                   SELECTOR
order-processor   1         1         1           1           49s       order-processor   kedasamples/sample-dotnet-worker-servicebus-queue   app=order-processor
```

#### Deploying our autoscaling

First things first, we will create a new authorization rule with `Management` permissions so that KEDA can monitor it.

```cli
❯ az servicebus queue authorization-rule create --resource-group <resource-group-name> --namespace-name <namespace-name> --queue-name orders --name keda-monitor --rights Manage Send Listen
```

Get and encode the connection string as mentioned above and store it in `servicebus-order-management-connectionstring` for our secret in `deploy-autoscaling.yaml`.

We have our secret configured, defined a `TriggerAuthentication` for KEDA to authenticate with and defined how our app should scale with a `ScaledObject` - We are ready to go!

Now let's create everything:

```cli
❯ kubectl apply -f .\deploy/connection-string/deploy-autoscaling.yaml --namespace keda-dotnet-sample
triggerauthentication.keda.sh/trigger-auth-service-bus-orders created
secret/secrets-order-consumer configured
scaledobject.keda.sh/order-processor-scaler created
```

Once created, you will see that our deployment shows up with no pods created:

```cli
❯ kubectl get deployments --namespace keda-dotnet-sample -o wide
NAME              DESIRED   CURRENT   UP-TO-DATE   AVAILABLE   AGE       CONTAINERS        IMAGES                                                   SELECTOR
order-processor   0         0         0            0           49s       order-processor   kedasamples/sample-dotnet-worker-servicebus-queue   app=order-processor
```

This is because our queue is empty and KEDA scaled it down until there is work to do.

In that case, let's give generate some!

### Publishing messages to the queue

The following job will send messages to the "orders" queue on which the order processor is listening to. As the queue builds up, KEDA will help the horizontal pod autoscaler add more and more pods until the queue is drained. The order generator will allow you to specify how many messages you want to queue.

First you should clone the project:

```cli
❯ git clone https://github.com/kedacore/sample-dotnet-worker-servicebus-queue
❯ cd sample-dotnet-worker-servicebus-queue
```

Configure a connection string with `Send` permissions in the tool via your favorite text editor, in this case via Visual Studio Code:

```cli
❯ code .\src\Keda.Samples.Dotnet.OrderGenerator\Program.cs
```

Next, you can run the order generator via the CLI:

```cli
❯ dotnet run --project .\src\Keda.Samples.Dotnet.OrderGenerator\Keda.Samples.Dotnet.OrderGenerator.csproj
Let's queue some orders, how many do you want?
300
Queuing order 719a7b19-f1f7-4f46-a543-8da9bfaf843d - A Hat for Reilly Davis
Queuing order 5c3a954c-c356-4cc9-b1d8-e31cd2c04a5a - A Salad for Savanna Rowe
[...]

That's it, see you later!
```

Now that the messages are generated, you'll see that KEDA starts automatically scaling out your deployment:

```cli
❯ kubectl get deployments --namespace keda-dotnet-sample -o wide
NAME              DESIRED   CURRENT   UP-TO-DATE   AVAILABLE   AGE       CONTAINERS        IMAGES                                                   SELECTOR
order-processor   8         8         8            4           4m        order-processor   kedasamples/sample-dotnet-worker-servicebus-queue   app=order-processor
```

Eventually we will have 10 pods running processing messages in parallel:

```cli
❯ kubectl get pods --namespace keda-dotnet-sample
NAME                              READY     STATUS    RESTARTS   AGE
order-processor-65d5dd564-9wbph   1/1       Running   0          54s
order-processor-65d5dd564-czlqb   1/1       Running   0          39s
order-processor-65d5dd564-h2l5l   1/1       Running   0          54s
order-processor-65d5dd564-h6fcl   1/1       Running   0          24s
order-processor-65d5dd564-httnf   1/1       Running   0          1m
order-processor-65d5dd564-j64wq   1/1       Running   0          54s
order-processor-65d5dd564-ncwfd   1/1       Running   0          39s
order-processor-65d5dd564-q7tkt   1/1       Running   0          39s
order-processor-65d5dd564-t2g6x   1/1       Running   0          24s
order-processor-65d5dd564-v79x6   1/1       Running   0          39s
```

You can look at the logs for a given processor as following:

```cli
❯ kubectl logs order-processor-65d5dd564-httnf --namespace keda-dotnet-sample
info: Keda.Samples.Dotnet.OrderProcessor.OrdersQueueProcessor[0]
      Starting message pump at: 06/03/2019 12:32:14 +00:00
info: Keda.Samples.Dotnet.OrderProcessor.OrdersQueueProcessor[0]
      Message pump started at: 06/03/2019 12:32:14 +00:00
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /app
info: Keda.Samples.Dotnet.OrderProcessor.OrdersQueueProcessor[0]
      Received message 513b896fbe3b4085ad274d9c23e01842 with body {"Id":"7ff54254-a370-4697-8115-134e55ebdc65","Amount":1741776525,"ArticleNumber":"Chicken","Customer":{"FirstName":"Myrtis","LastName":"Balistreri"}}
info: Keda.Samples.Dotnet.OrderProcessor.OrdersQueueProcessor[0]
      Processing order 7ff54254-a370-4697-8115-134e55ebdc65 for 1741776525 units of Chicken bought by Myrtis Balistreri at: 06/03/2019 12:32:15 +00:00
info: Keda.Samples.Dotnet.OrderProcessor.OrdersQueueProcessor[0]
      Order 7ff54254-a370-4697-8115-134e55ebdc65 processed at: 06/03/2019 12:32:17 +00:00
info: Keda.Samples.Dotnet.OrderProcessor.OrdersQueueProcessor[0]
      Message 513b896fbe3b4085ad274d9c23e01842 processed at: 06/03/2019 12:32:17 +00:00
info: Keda.Samples.Dotnet.OrderProcessor.OrdersQueueProcessor[0]
      Received message 9d24f13cd5ec44e884efdc9ed4a8842d with body {"Id":"cd9fe9e4-f421-432d-9b19-b94dbf9090f5","Amount":-186606051,"ArticleNumber":"Shoes","Customer":{"FirstName":"Valerie","LastName":"Schaefer"}}
info: Keda.Samples.Dotnet.OrderProcessor.OrdersQueueProcessor[0]
      Processing order cd9fe9e4-f421-432d-9b19-b94dbf9090f5 for -186606051 units of Shoes bought by Valerie Schaefer at: 06/03/2019 12:32:17 +00:00
info: Keda.Samples.Dotnet.OrderProcessor.OrdersQueueProcessor[0]
      Order cd9fe9e4-f421-432d-9b19-b94dbf9090f5 processed at: 06/03/2019 12:32:19 +00:00
info: Keda.Samples.Dotnet.OrderProcessor.OrdersQueueProcessor[0]
      Message 9d24f13cd5ec44e884efdc9ed4a8842d processed at: 06/03/2019 12:32:19 +00:00
```

## Deploy with terraform

### Pre-requisites

- Terraform
- Azure Subscription
- .NET Core 3.0
- Kubernetes cluster with [KEDA v2.0+ installed](https://keda.sh/docs/2.0/deploy/)

### Setup

This setup will go through creating an Azure Service Bus queue and the necessary permissions with terraform and deploying this consumer with the `ScaledObject` to scale via KEDA. It will use the [helm chart created specifically](./deploy/helm/keda-servicebus-connection-string/) to create the necessary resources in the cluster.

1. It's necessary configure the necessary variables, modifying the file [terraform.tfvars](infra-as-code/connection-string/terraform.tfvars) with each of them:
   1. `location`: Azure location where the resources are going to be created
   2. `resource_group`: Azure resource group where the resources are going to be created
   3. `subscription_id`: Azure Account Subscription ID where the resources are going to be created
2. Download the necessary terraform dependencies:

      ```sh
      > terraform -chdir=infra-as-code/connection-string init
      ```

3. Calculate the terraform plan to view ther resources that are going to be created, using the file [terraform.tfvars](infra-as-code/connection-string/terraform.tfvars) after made the modifications as `--var-file`:

      ```sh
      > terraform -chdir=infra-as-code/connection-string plan --var-file=terraform.tfvars -out=terraform.plan 
      ```

4. Apply the terraform plan calculated in the previous step to create the necessary resources:

      ```sh
      > terraform -chdir=infra-as-code/connection-string apply "terraform.plan"
      ```

5. Once the terraform apply process have finished, it can be seen that a job has been created to queue messages in the queue:

      ```sh
      > kubectl -n keda-dotnet-sample get job -l app=order-processor -w
      NAME                       COMPLETIONS   DURATION   AGE
      order-generator-eqw54u4m   0/1                      0s
      order-generator-eqw54u4m   0/1           0s         0s
      order-generator-eqw54u4m   0/1           3s         3s
      order-generator-eqw54u4m   0/1           12s        12s
      order-generator-eqw54u4m   0/1           14s        14s
      order-generator-eqw54u4m   1/1           14s        14s
      ```

6. If the logs of this job are obtained you can see how messages have been queued:

      ```sh
      > kubectl -n keda-dotnet-sample logs order-generator-eqw54u4m-w2p6l
      Let's queue some orders, how many do you want?
      100 orders are going to be generated
      Queuing order c6d5ed3a-774c-451d-be86-578925f046d0 - A Chips for Melissa O'Reilly
      Queuing order 80da03a7-c070-40b4-af60-3f35753a7cd9 - A Pizza for Johnny Welch
      Queuing order 5cd01df3-b7af-4b28-a8b5-0821c0802d8d - A Sausages for Lee Roob
      Queuing order f507c7af-3382-4f15-905c-f4931c548c74 - A Chair for Mary Heller
      Queuing order 2c1684f2-91e7-4aab-ab01-c02194e25f39 - A Ball for Myrl McClure
      Queuing order b6a8d667-7fb4-465b-8069-98869f11b211 - A Fish for Manuela Champlin
      Queuing order 28bc7ca0-25cf-4e4d-978e-4ded66ed2890 - A Pants for Amelie Heathcote
      Queuing order 92bc0d56-14b7-4f6f-98fb-ae4153f98394 - A Tuna for Anita Mitchell
      Queuing order ec7362b3-f1f2-4f5e-a51c-3b2014643aa9 - A Shoes for Shanelle Botsford
      Queuing order 8626abea-3994-4b44-8875-04ba0015c979 - A Soap for Jackie Hansen
      Queuing order 4ae76dc1-a494-4111-b434-25874cd220be - A Ball for Kiarra Schuppe
      Queuing order 794e06e7-3a86-452f-9a15-f97f19dbff5c - A Gloves for Branson Runte
      Queuing order 41521b9d-77da-441e-9252-e807504617a0 - A Bacon for Geovany Heidenreich
      Queuing order 29486511-e574-4a0a-b180-1dd07c2348a3 - A Salad for Destiny Quigley
      Queuing order 90416461-8c1e-4d99-97e6-22b82c86257c - A Keyboard for Aryanna Nitzsche
      Queuing order 008f1c81-d344-45b2-a9d5-baec7ee72f6c - A Bacon for Luigi Gutmann
      Queuing order d2d44ea2-26f1-46ee-8f53-2c2ee7f13ecb - A Keyboard for Eusebio Hyatt
      Queuing order 8274ceaa-6bbe-44ce-b465-1e0f1dfca969 - A Bike for Dario Collins
      Queuing order 5e6d7539-a739-43ca-a570-379d6fb071fd - A Ball for Elenor Hane
      Queuing order ee19a215-e47d-4226-80c3-676b7d083ce6 - A Bike for Juvenal Halvorson
      Queuing order e5e6a883-fd36-412f-87ed-bd63307569da - A Hat for Nedra Schamberger
      Queuing order b61b05a4-9e0c-4b7e-9eda-c449bbbdd89f - A Tuna for Tomasa Sawayn
      Queuing order 5fb788c9-9ed3-41f3-8836-3ac9d778a027 - A Sausages for Rex Zieme
      Queuing order 6f54693a-0caa-42ca-9d5b-c60ca002e28e - A Towels for Xander Tremblay
      Queuing order f6818d61-356a-4408-be06-c88ff423323e - A Soap for Boyd Herman
      Queuing order e4eee103-f935-4358-8d84-3c79ed62bdf8 - A Soap for Nolan Krajcik
      Queuing order 97ea4dfa-7d3f-497d-ba02-f91e7e43cbda - A Car for Erna Lubowitz
      Queuing order e957c5db-9978-4d3b-8694-3091325cc0a1 - A Bike for Kathleen Balistreri
      Queuing order 76a78906-ccb9-4aed-8f16-6e23ed45cd9c - A Shirt for Ismael Adams
      Queuing order 5b3f150c-99ce-4e4f-b87b-541148cff1df - A Ball for Elinor McDermott
      Queuing order 0c709d86-d79a-449c-bb5a-3aa8cfc0e8fd - A Computer for Lou Gleason
      Queuing order 14094363-79f4-4a22-960f-2ab69dfebbca - A Cheese for Gloria Jakubowski
      Queuing order ffc4ee72-3954-4238-9887-703b3981abf8 - A Gloves for Jacklyn Carter
      Queuing order 309df831-ea1e-4adb-b95d-1947fbc1b9b1 - A Shirt for Dorothy Lockman
      Queuing order ecee4713-0102-41d3-8f78-fc8658673a03 - A Computer for Vicente Price
      Queuing order 4711bf82-6df2-484a-a58e-60a684e9f7e3 - A Gloves for Bettie Lowe
      Queuing order c5e8a228-2107-4684-af4e-94b776163805 - A Chips for Charley Bartoletti
      Queuing order 99c6128b-dcaf-4e5f-9d5d-7ab0a03926c4 - A Chicken for Angus Cummings
      Queuing order 5a725ebf-93c8-4d62-8ab1-d6f73debe35b - A Cheese for Burley Schoen
      Queuing order 09ccbb57-1b87-46c2-9c68-e672469b5409 - A Computer for Ernie Pollich
      Queuing order 2fded1e5-3883-4f66-8a45-1bc5d5ef171a - A Gloves for Nicholas Crona
      Queuing order bcc444d5-5611-4f63-82fd-cb2607f1c568 - A Chips for Treva Johnson
      Queuing order 6fa4a2e2-d26f-4176-9f1f-e6b08d85abe8 - A Soap for Rosella Lesch
      Queuing order e5d7ac33-9d66-4c9d-aa5c-792b41976ae9 - A Bike for Helmer Thiel
      Queuing order 73e7a762-e9ae-4cae-9cb2-1d9318bf4320 - A Ball for Benny Sporer
      Queuing order b73668ce-e0b3-4bab-87e9-772e901f7366 - A Bacon for Candido Crooks
      Queuing order 8fd39e05-1b5d-4974-8863-fc3155cdb270 - A Towels for Herbert Pfannerstill
      Queuing order 9dac0a12-f936-4276-8836-9c9d008ea8f7 - A Chair for Aileen Fritsch
      Queuing order 66f8593e-a04a-4dc5-aeb1-254a1fbdd213 - A Bacon for Luisa Walsh
      Queuing order 5f8dee17-0985-4c5c-a88b-07d817152074 - A Pants for Leonor Heaney
      Queuing order 7d34c8bb-00b5-4c95-ab96-aef7b6bf5eca - A Chair for Izabella Upton
      Queuing order 197e1226-b68c-4ba9-9297-5113d3d10eea - A Keyboard for Sylvester Weber
      Queuing order 84c1be12-0122-4f01-98cb-31643585850a - A Towels for Oren Anderson
      Queuing order 468fe208-4539-47b9-be17-b9340dd86da5 - A Towels for Carolyn Streich
      Queuing order 575b5f96-eb0f-4143-954e-424d0b0a2fd2 - A Soap for Cesar O'Kon
      Queuing order 8e73a307-3c84-47b6-a35f-f7f66d752ee9 - A Tuna for Cheyanne Runolfsdottir
      Queuing order 9cc57364-2a1f-4040-a507-af82580a21d0 - A Pants for Randal Zboncak
      Queuing order fd17a112-e9bf-490f-a735-e2497f2a1736 - A Tuna for Joseph Schimmel
      Queuing order b277f380-d68d-4211-b0dc-50fd3f79314f - A Ball for Arturo Klein
      Queuing order acb18f83-fae4-41ee-81dd-71f1cbe81202 - A Tuna for Loren Bechtelar
      Queuing order 6243ad1c-c416-411e-b854-cb9cb5113ba5 - A Mouse for Nathaniel Lebsack
      Queuing order 727017e1-c436-415d-9292-8808021a6a8d - A Bacon for Petra Hauck
      Queuing order 43ab6232-c76c-4eb4-898f-b5b2b8952ed7 - A Pants for Buddy Walsh
      Queuing order 17eda05b-2744-4a86-8884-66857a1b5c41 - A Car for Pamela Koch
      Queuing order e6a553ad-5ad0-4085-8ca0-3c768064aba7 - A Shoes for Frederique Cormier
      Queuing order 45fd69ea-621d-4f46-b58c-f9fbb87014cd - A Chips for Miracle Donnelly
      Queuing order 7a8c2b48-244f-49f6-b806-41078df6b12b - A Salad for Brant Larson
      Queuing order d92890a3-c80b-44ed-ba98-55f96e7457df - A Towels for Einar Nitzsche
      Queuing order cf948d5b-8518-4cbc-8f29-5a3da31ca243 - A Shoes for Chelsie Spencer
      Queuing order 5bafb7c4-78b3-446d-a182-d26dc481af9f - A Shoes for Carmine Schulist
      Queuing order f9f7acb2-0f67-4c34-b33a-3f82a44b803f - A Gloves for Chance Dooley
      Queuing order fdf78669-9792-4803-9f6c-4be825faadc9 - A Table for Laurie Harris
      Queuing order 9d8e4375-1054-4740-8a71-1052097a779b - A Chicken for Lavina Schumm
      Queuing order 03ff3354-0fc4-4405-acd2-ad4d181f6b6a - A Car for Litzy MacGyver
      Queuing order 46a7e326-8616-4a5d-b7af-e95558d95af4 - A Pizza for Buford Dickinson
      Queuing order e95a993c-6f97-4494-9393-80f448880196 - A Gloves for Julianne Schaden
      Queuing order bde68e04-c6c1-4b1f-a005-97a0e2a9bdfc - A Bacon for Sydni Stiedemann
      Queuing order 55f861c2-9127-41cf-8398-c8b16c5fc3a2 - A Gloves for Timothy Rosenbaum
      Queuing order aea85cf6-ea23-45b1-be8f-6b5012aef81f - A Sausages for Vickie Kuhic
      Queuing order 1941d43d-2bf1-4bdd-9838-d9ed7ebb071a - A Pizza for Frank Mann
      Queuing order 32c236cb-68a5-4a86-9480-ed8c38052d87 - A Gloves for Lavada Osinski
      Queuing order b7de9d3d-2bec-4fee-9c00-f9d65a632e0c - A Cheese for Willa Dickinson
      Queuing order 71ae5da2-0de1-46f7-80af-74ceda3d14fd - A Towels for Ramon West
      Queuing order 03b0ebc9-987b-4b98-86ae-a808e7f87170 - A Cheese for Nannie Zboncak
      Queuing order 8e8448e1-53ab-4665-b0c3-d14479cb6ab1 - A Computer for Santos Schinner
      Queuing order cdfacf78-c775-48e5-9ea9-3035bf7cf283 - A Hat for Mafalda Rice
      Queuing order a74afa0b-2749-4c84-ade3-3c7a8972c7af - A Fish for Vickie Schmidt
      Queuing order 46829d2b-baff-4797-908b-f79ed34e94e2 - A Bacon for David Rice
      Queuing order 8d408010-b6b5-47be-8037-c6f5bdb85560 - A Shoes for Murray Weissnat
      Queuing order 6cc667e1-1fd4-44d7-a587-243c96887137 - A Mouse for Adrian Barrows
      Queuing order 81683e8d-d05a-4ca3-bccf-26dc1f4909e9 - A Chips for Gail Kuvalis
      Queuing order fe261dcc-0fb0-4d1d-b9e5-0450328a214b - A Chicken for Polly Reinger
      Queuing order 1acac1cc-b13f-4ea2-b3bc-0502ec470077 - A Bacon for Delilah Mayer
      Queuing order c3ae937c-40a7-48e0-bfa1-0666fcb94fc6 - A Gloves for Adele Ortiz
      Queuing order 7ae03d51-2cfd-4c3e-94d8-4da557595afd - A Chair for Adalberto Schmeler
      Queuing order 6fa33e30-b6a4-4588-a659-e193919fd0f2 - A Computer for Tito Rowe
      Queuing order f803ebd3-cbe3-47c6-bc78-93ad7b7af3bb - A Keyboard for Lois Roob
      Queuing order ed80672b-b590-428b-bec8-4253a6a0c72a - A Sausages for Jacques Jacobs
      Queuing order d14e0129-6345-42f8-8536-64dc935557ce - A Towels for Johnson Leffler
      Queuing order 51e92eac-f4fb-4067-86b5-84a71a06118d - A Shoes for Carolanne Cartwright
      ```

7. If the deployment `order-processor` is checked you can see how the number of replicas is increasing:

      ```sh
      > kubectl -n keda-dotnet-sample get deployment order-processor -w
      NAME              READY   UP-TO-DATE   AVAILABLE   AGE
      order-processor   2/2     2            2           2m47s
      order-processor   2/0     2            2           3m
      order-processor   2/0     2            2           3m
      order-processor   0/0     0            0           3m
      ```

8. Now it is possible to go to the section [visualizing the service bus queue](#visualizing-the-service-bus-queue)

9. Once the tests have been completed, the resources must be deleted, using the following commands:

      ```sh
      > terraform -chdir=infra-as-code/connection-string plan -destroy --var-file=terraform.tfvars -out=terraform.destroy
      > terraform -chdir=infra-as-code/connection-string apply "terraform.destroy"
      ```

## Visualizing the service bus queue

There is also a web application included in the repository that shows a simple bar chart with the number of messages. The graph refreshes every 2 seconds, giving you a visualization how the queue initially builds up when orders are being sent to the service bus, and then when the autoscaler kicks in the queue will decrease in length quicker and quicker depending on how many replicas that have been created.

To build and run the web app locally, add the service bus connection string to appSettings.json and run the web application from Visual Studio.

There is also a docker image available, so you can also run it locally with the following command:

```cli
docker run -p 8080:80 -d -e OrderQueue__ConnectionString="KEDA_SERVICEBUS_QUEUE_CONNECTIONSTRING" kedasamples/sample-dotnet-web 
```

To deploy the web application to your Kubernetes cluster:

```cli
❯ kubectl apply -f .\deploy\deploy-web.yaml --namespace keda-dotnet-sample
deployment.apps/order-web created
service/kedasampleweb created
```

Get the public IP by running:

```cli
❯ kubectl get svc kedasampleweb --namespace keda-dotnet-sample
NAME            TYPE           CLUSTER-IP   EXTERNAL-IP     PORT(S)        AGE
kedasampleweb   LoadBalancer   10.0.37.60   52.157.87.179   80:30919/TCP   117s
```

You'll need to wait a short while until the public IP is created and shown in the output.

![Visualize message queue](/images/kedaweb.png)

## Cleaning up resources

### Delete the application

```cli
❯ kubectl delete -f deploy/connection-string/deploy-autoscaling.yaml --namespace keda-dotnet-sample
❯ kubectl delete -f deploy/connection-string/deploy-app.yaml --namespace keda-dotnet-sample
❯ kubectl delete namespace keda-dotnet-sample
```

### Delete the Azure Service Bus namespace

```cli
❯ az servicebus namespace delete --name <namespace-name> --resource-group <resource-group-name>
```

### Uninstall KEDA

```cli
❯ helm uninstall keda --namespace keda
❯ kubectl delete customresourcedefinition  scaledobjects.keda.sh
❯ kubectl delete customresourcedefinition  triggerauthentications.keda.sh
❯ kubectl delete namespace keda
```
