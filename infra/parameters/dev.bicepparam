// ============================================================================
// Development Environment Parameters
// ============================================================================
//
// "I bent my wookiee." - Ralph Wiggum
//
// Development environment - where we bend and shape our code into something great!
//
// ============================================================================

using '../main.bicep'

param environment = 'dev'
param location = 'eastus'
param projectName = 'dorise'

// SQL credentials should be provided via secure parameters at deployment time
// az deployment sub create --parameters sqlAdminLogin=<value> sqlAdminPassword=<value>
param sqlAdminLogin = '' // Provide at deployment
param sqlAdminPassword = '' // Provide at deployment

param tags = {
  Project: 'Dorise-Incentive'
  Environment: 'dev'
  Owner: 'DSIF-Team'
  CostCenter: 'CC-12345'
  Application: 'DSIF'
  ManagedBy: 'Bicep'
  CreatedBy: 'GitHub-Actions'
}
