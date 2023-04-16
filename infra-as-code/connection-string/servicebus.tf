resource "azurerm_servicebus_namespace" "namespace" {
  name                = "keda-sample-servicebus"
  location            = data.azurerm_resource_group.rg.location
  resource_group_name = data.azurerm_resource_group.rg.name
  sku                 = "Basic"

  tags = {
    source = "terraform"
  }
}

resource "azurerm_servicebus_queue" "orders" {
  name         = "orders"
  namespace_id = azurerm_servicebus_namespace.namespace.id
}

resource "azurerm_servicebus_queue_authorization_rule" "listen_send" {
  name     = "listen-send"
  queue_id = azurerm_servicebus_queue.orders.id

  listen = true
  send   = true
  manage = false
}

resource "azurerm_servicebus_queue_authorization_rule" "keda_monitor" {
  name     = "keda-monitor"
  queue_id = azurerm_servicebus_queue.orders.id

  listen = true
  send   = true
  manage = true
}