// ============================================================================
// DORISE Sales Incentive Framework - Main Infrastructure Template
// ============================================================================
//
// "I choo-choo-choose you!" - Ralph Wiggum
// We choo-choo-choose Azure for our cloud infrastructure!
//
// This is the main orchestration template that deploys all Azure resources
// for the DSIF application using modular Bicep templates.
//
// Usage:
//   az deployment sub create \
//     --location eastus \
//     --template-file main.bicep \
//     --parameters @parameters/dev.bicepparam
//
// ============================================================================

targetScope = 'subscription'

// ============================================================================
// Parameters
// ============================================================================

@description('The environment name (dev, stg, prod)')
@allowed(['dev', 'stg', 'prod'])
param environment string

@description('The Azure region for resource deployment')
param location string = 'eastus'

@description('The project name used in resource naming')
param projectName string = 'dorise'

@description('SQL Server administrator login')
@secure()
param sqlAdminLogin string

@description('SQL Server administrator password')
@secure()
param sqlAdminPassword string

@description('Tags to apply to all resources')
param tags object = {
  Project: 'Dorise-Incentive'
  Environment: environment
  Owner: 'DSIF-Team'
  CostCenter: 'CC-12345'
  Application: 'DSIF'
  ManagedBy: 'Bicep'
}

// ============================================================================
// Variables
// ============================================================================

var resourceGroupName = 'rg-${projectName}-${environment}-${location}'

// SKU configurations per environment
var skuConfig = {
  dev: {
    appServicePlan: 'B1'
    sqlDatabase: 'Basic'
    sqlDtu: 5
    storageRedundancy: 'LRS'
    keyVaultSku: 'standard'
  }
  stg: {
    appServicePlan: 'S1'
    sqlDatabase: 'S1'
    sqlDtu: 20
    storageRedundancy: 'GRS'
    keyVaultSku: 'standard'
  }
  prod: {
    appServicePlan: 'P1v3'
    sqlDatabase: 'S2'
    sqlDtu: 50
    storageRedundancy: 'GRS'
    keyVaultSku: 'premium'
  }
}

var currentSku = skuConfig[environment]

// ============================================================================
// Resource Group
// ============================================================================

resource resourceGroup 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

// ============================================================================
// Module Deployments
// ============================================================================

// Monitoring resources (deployed first as other resources depend on it)
module monitoring 'modules/monitoring.bicep' = {
  scope: resourceGroup
  name: 'monitoring-${environment}'
  params: {
    projectName: projectName
    environment: environment
    location: location
    tags: tags
  }
}

// Storage Account
module storage 'modules/storage.bicep' = {
  scope: resourceGroup
  name: 'storage-${environment}'
  params: {
    projectName: projectName
    environment: environment
    location: location
    tags: tags
    storageRedundancy: currentSku.storageRedundancy
    logAnalyticsWorkspaceId: monitoring.outputs.logAnalyticsWorkspaceId
  }
}

// Key Vault
module keyVault 'modules/keyVault.bicep' = {
  scope: resourceGroup
  name: 'keyvault-${environment}'
  params: {
    projectName: projectName
    environment: environment
    location: location
    tags: tags
    skuName: currentSku.keyVaultSku
    logAnalyticsWorkspaceId: monitoring.outputs.logAnalyticsWorkspaceId
  }
}

// SQL Database
module sqlDatabase 'modules/sqlDatabase.bicep' = {
  scope: resourceGroup
  name: 'sql-${environment}'
  params: {
    projectName: projectName
    environment: environment
    location: location
    tags: tags
    administratorLogin: sqlAdminLogin
    administratorPassword: sqlAdminPassword
    skuName: currentSku.sqlDatabase
    skuCapacity: currentSku.sqlDtu
    logAnalyticsWorkspaceId: monitoring.outputs.logAnalyticsWorkspaceId
    keyVaultName: keyVault.outputs.keyVaultName
  }
}

// App Service
module appService 'modules/appService.bicep' = {
  scope: resourceGroup
  name: 'appservice-${environment}'
  params: {
    projectName: projectName
    environment: environment
    location: location
    tags: tags
    skuName: currentSku.appServicePlan
    applicationInsightsConnectionString: monitoring.outputs.applicationInsightsConnectionString
    applicationInsightsInstrumentationKey: monitoring.outputs.applicationInsightsInstrumentationKey
    keyVaultName: keyVault.outputs.keyVaultName
    logAnalyticsWorkspaceId: monitoring.outputs.logAnalyticsWorkspaceId
  }
}

// ============================================================================
// Outputs
// ============================================================================

output resourceGroupName string = resourceGroup.name
output appServiceName string = appService.outputs.appServiceName
output appServiceUrl string = appService.outputs.appServiceUrl
output sqlServerName string = sqlDatabase.outputs.sqlServerName
output sqlDatabaseName string = sqlDatabase.outputs.sqlDatabaseName
output keyVaultName string = keyVault.outputs.keyVaultName
output keyVaultUri string = keyVault.outputs.keyVaultUri
output storageAccountName string = storage.outputs.storageAccountName
output applicationInsightsName string = monitoring.outputs.applicationInsightsName
output logAnalyticsWorkspaceName string = monitoring.outputs.logAnalyticsWorkspaceName
