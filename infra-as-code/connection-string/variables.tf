variable "location" {
  description = "Azure location where the resources are going to be created"
  type        = string
  default     = "West Europe"
}

variable "resource_group" {
  description = "Azure resource group where the resources are going to be created"
  type        = string
}

variable "subscription_id" {
  description = "Azure Account Subscription ID where the resources are going to be created"
  type        = string
}