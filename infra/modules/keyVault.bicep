// ============================================================================
// Key Vault Module
// ============================================================================
//
// "Me fail English? That's unpossible!" - Ralph Wiggum
//
// It's unpossible for secrets to leak when they're safely stored in Key Vault!
//
// This module deploys:
// - Azure Key Vault
// - Access policies
// - Diagnostic settings
//
// ============================================================================

// ============================================================================
// Parameters
// ============================================================================

@description('The project name')
param projectName string

@description('The environment name')
param environment string

@description('The Azure region')
param location string

@description('Resource tags')
param tags object

@description('Key Vault SKU name')
@allowed(['standard', 'premium'])
param skuName string = 'standard'

@description('Log Analytics Workspace ID for diagnostics')
param logAnalyticsWorkspaceId string

@description('Enable soft delete')
param enableSoftDelete bool = true

@description('Soft delete retention in days')
param softDeleteRetentionInDays int = 90

@description('Enable purge protection')
param enablePurgeProtection bool = true

// ============================================================================
// Variables
// ============================================================================

var keyVaultName = 'kv-${projectName}-${environment}'

// ============================================================================
// Resources
// ============================================================================

// Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: skuName
    }
    tenantId: subscription().tenantId
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: true
    enableSoftDelete: enableSoftDelete
    softDeleteRetentionInDays: softDeleteRetentionInDays
    enablePurgeProtection: enablePurgeProtection ? true : null
    enableRbacAuthorization: true
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

// Diagnostic Settings
resource diagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  scope: keyVault
  name: 'keyvault-diagnostics'
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: [
      {
        category: 'AuditEvent'
        enabled: true
      }
      {
        category: 'AzurePolicyEvaluationDetails'
        enabled: true
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}

// ============================================================================
// Outputs
// ============================================================================

output keyVaultId string = keyVault.id
output keyVaultName string = keyVault.name
output keyVaultUri string = keyVault.properties.vaultUri
