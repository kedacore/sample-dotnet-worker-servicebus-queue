terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.52"
    }
    helm = {
      source  = "hashicorp/helm"
      version = "~> 2.9"
    }
  }
}

provider "azurerm" {
  features {}
  subscription_id = var.subscription_id
}

provider "helm" {
  kubernetes {
    config_path = "~/.kube/config"
  }
}