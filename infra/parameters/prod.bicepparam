// ============================================================================
// Production Environment Parameters
// ============================================================================
//
// "That's where I saw the leprechaun. He tells me to burn things." - Ralph Wiggum
//
// Production environment - no burning allowed, only stable, reliable deployments!
//
// ============================================================================

using '../main.bicep'

param environment = 'prod'
param location = 'eastus'
param projectName = 'dorise'

// SQL credentials should be provided via secure parameters at deployment time
// az deployment sub create --parameters sqlAdminLogin=<value> sqlAdminPassword=<value>
param sqlAdminLogin = '' // Provide at deployment
param sqlAdminPassword = '' // Provide at deployment

param tags = {
  Project: 'Dorise-Incentive'
  Environment: 'prod'
  Owner: 'DSIF-Team'
  CostCenter: 'CC-12345'
  Application: 'DSIF'
  ManagedBy: 'Bicep'
  CreatedBy: 'GitHub-Actions'
}
