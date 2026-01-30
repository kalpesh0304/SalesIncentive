// ============================================================================
// Staging Environment Parameters
// ============================================================================
//
// "I sleep in a drawer!" - Ralph Wiggum
//
// Staging environment - where our code sleeps before going to production!
//
// ============================================================================

using '../main.bicep'

param environment = 'stg'
param location = 'eastus'
param projectName = 'dorise'

// SQL credentials should be provided via secure parameters at deployment time
// az deployment sub create --parameters sqlAdminLogin=<value> sqlAdminPassword=<value>
param sqlAdminLogin = '' // Provide at deployment
param sqlAdminPassword = '' // Provide at deployment

param tags = {
  Project: 'Dorise-Incentive'
  Environment: 'stg'
  Owner: 'DSIF-Team'
  CostCenter: 'CC-12345'
  Application: 'DSIF'
  ManagedBy: 'Bicep'
  CreatedBy: 'GitHub-Actions'
}
