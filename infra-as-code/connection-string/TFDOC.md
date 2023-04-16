## Requirements

| Name | Version |
|------|---------|
| <a name="requirement_azurerm"></a> [azurerm](#requirement\_azurerm) | ~> 3.52 |
| <a name="requirement_helm"></a> [helm](#requirement\_helm) | ~> 2.9 |

## Providers

| Name | Version |
|------|---------|
| <a name="provider_azurerm"></a> [azurerm](#provider\_azurerm) | ~> 3.52 |
| <a name="provider_helm"></a> [helm](#provider\_helm) | ~> 2.9 |

## Modules

No modules.

## Resources

| Name | Type |
|------|------|
| [azurerm_servicebus_namespace.namespace](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/servicebus_namespace) | resource |
| [azurerm_servicebus_queue.orders](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/servicebus_queue) | resource |
| [azurerm_servicebus_queue_authorization_rule.keda_monitor](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/servicebus_queue_authorization_rule) | resource |
| [azurerm_servicebus_queue_authorization_rule.listen_send](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/servicebus_queue_authorization_rule) | resource |
| [helm_release.keda](https://registry.terraform.io/providers/hashicorp/helm/latest/docs/resources/release) | resource |
| [helm_release.keda_dotnet_sample](https://registry.terraform.io/providers/hashicorp/helm/latest/docs/resources/release) | resource |
| [azurerm_resource_group.rg](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/resource_group) | data source |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_location"></a> [location](#input\_location) | Azure location where the resources are going to be created | `string` | `"West Europe"` | no |
| <a name="input_resource_group"></a> [resource\_group](#input\_resource\_group) | Azure resource group where the resources are going to be created | `string` | n/a | yes |
| <a name="input_subscription_id"></a> [subscription\_id](#input\_subscription\_id) | Azure Account Subscription ID where the resources are going to be created | `string` | n/a | yes |

## Outputs

No outputs.
