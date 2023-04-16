resource "helm_release" "keda" {
  name       = "keda"
  repository = "https://kedacore.github.io/charts"
  chart      = "keda"
  version    = "2.6.2"

  namespace        = "keda"
  wait             = true
  create_namespace = true

  depends_on = [
    azurerm_servicebus_queue.orders,
    azurerm_servicebus_queue_authorization_rule.listen_send,
    azurerm_servicebus_queue_authorization_rule.keda_monitor
  ]
}

resource "helm_release" "keda_dotnet_sample" {
  name  = "orders-test"
  chart = "${path.module}/../../deploy/helm/keda-servicebus-connection-string"

  namespace        = "keda-dotnet-sample"
  wait             = true
  create_namespace = true

  set_sensitive {
    name  = "orderProcessor.servicebusConnectionString"
    value = azurerm_servicebus_queue_authorization_rule.listen_send.primary_connection_string
    type  = "string"
  }

  set_sensitive {
    name  = "orderConsumer.servicebusConnectionString"
    value = azurerm_servicebus_queue_authorization_rule.keda_monitor.primary_connection_string
    type  = "string"
  }

  set {
    name  = "orderGenerator.queueName"
    value = azurerm_servicebus_queue.orders.name
    type  = "string"
  }

  depends_on = [
    helm_release.keda
  ]
}