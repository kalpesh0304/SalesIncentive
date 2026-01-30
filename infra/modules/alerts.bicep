// ============================================================================
// Alert Rules Module
// ============================================================================
//
// "That's where I saw the Leprechaun. He told me to burn things." - Ralph Wiggum
// Alerts tell us when things are burning (metaphorically) so we can fix them!
//
// This module deploys:
// - Metric alerts for App Service
// - Metric alerts for SQL Database
// - Log-based alerts for exceptions
// - Action groups for notifications
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

@description('Application Insights resource ID')
param applicationInsightsId string

@description('App Service resource ID')
param appServiceId string

@description('SQL Database resource ID')
param sqlDatabaseId string = ''

@description('Email addresses for alert notifications')
param alertEmailAddresses array = []

@description('Enable alerts')
param enableAlerts bool = true

// ============================================================================
// Variables
// ============================================================================

var actionGroupName = 'ag-${projectName}-${environment}'
var alertPrefix = 'alert-${projectName}-${environment}'

// Severity levels: 0 = Critical, 1 = Error, 2 = Warning, 3 = Informational, 4 = Verbose
var severityCritical = 0
var severityError = 1
var severityWarning = 2

// ============================================================================
// Action Group
// ============================================================================

resource actionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = if (enableAlerts) {
  name: actionGroupName
  location: 'global'
  tags: tags
  properties: {
    groupShortName: 'DSIF-${environment}'
    enabled: true
    emailReceivers: [for (email, i) in alertEmailAddresses: {
      name: 'email-${i}'
      emailAddress: email
      useCommonAlertSchema: true
    }]
    // Add webhook receiver for external integrations
    webhookReceivers: []
    // Add SMS receivers for critical alerts
    smsReceivers: []
    // Add Azure app push notifications
    azureAppPushReceivers: []
  }
}

// ============================================================================
// App Service Alerts
// ============================================================================

// High CPU Alert
resource cpuAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = if (enableAlerts) {
  name: '${alertPrefix}-high-cpu'
  location: 'global'
  tags: tags
  properties: {
    description: 'Alert when CPU percentage exceeds 80% for 5 minutes'
    severity: severityWarning
    enabled: true
    scopes: [appServiceId]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'HighCPU'
          metricName: 'CpuPercentage'
          operator: 'GreaterThan'
          threshold: 80
          timeAggregation: 'Average'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

// High Memory Alert
resource memoryAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = if (enableAlerts) {
  name: '${alertPrefix}-high-memory'
  location: 'global'
  tags: tags
  properties: {
    description: 'Alert when memory percentage exceeds 80% for 5 minutes'
    severity: severityWarning
    enabled: true
    scopes: [appServiceId]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'HighMemory'
          metricName: 'MemoryPercentage'
          operator: 'GreaterThan'
          threshold: 80
          timeAggregation: 'Average'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

// HTTP 5xx Errors Alert
resource http5xxAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = if (enableAlerts) {
  name: '${alertPrefix}-http-5xx'
  location: 'global'
  tags: tags
  properties: {
    description: 'Alert when HTTP 5xx errors exceed 10 in 5 minutes'
    severity: severityError
    enabled: true
    scopes: [appServiceId]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'Http5xxErrors'
          metricName: 'Http5xx'
          operator: 'GreaterThan'
          threshold: 10
          timeAggregation: 'Total'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

// Response Time Alert
resource responseTimeAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = if (enableAlerts) {
  name: '${alertPrefix}-slow-response'
  location: 'global'
  tags: tags
  properties: {
    description: 'Alert when average response time exceeds 2 seconds'
    severity: severityWarning
    enabled: true
    scopes: [appServiceId]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'SlowResponse'
          metricName: 'HttpResponseTime'
          operator: 'GreaterThan'
          threshold: 2
          timeAggregation: 'Average'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

// Health Check Failed Alert
resource healthCheckAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = if (enableAlerts) {
  name: '${alertPrefix}-health-check-failed'
  location: 'global'
  tags: tags
  properties: {
    description: 'Alert when health check fails'
    severity: severityCritical
    enabled: true
    scopes: [appServiceId]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'HealthCheckFailed'
          metricName: 'HealthCheckStatus'
          operator: 'LessThan'
          threshold: 1
          timeAggregation: 'Average'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

// ============================================================================
// SQL Database Alerts (if SQL Database ID provided)
// ============================================================================

// DTU Percentage Alert
resource dtuAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = if (enableAlerts && !empty(sqlDatabaseId)) {
  name: '${alertPrefix}-high-dtu'
  location: 'global'
  tags: tags
  properties: {
    description: 'Alert when DTU percentage exceeds 80%'
    severity: severityWarning
    enabled: true
    scopes: [sqlDatabaseId]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'HighDTU'
          metricName: 'dtu_consumption_percent'
          operator: 'GreaterThan'
          threshold: 80
          timeAggregation: 'Average'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

// Database Connection Failed Alert
resource dbConnectionAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = if (enableAlerts && !empty(sqlDatabaseId)) {
  name: '${alertPrefix}-db-connection-failed'
  location: 'global'
  tags: tags
  properties: {
    description: 'Alert when database connections fail'
    severity: severityCritical
    enabled: true
    scopes: [sqlDatabaseId]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'ConnectionFailed'
          metricName: 'connection_failed'
          operator: 'GreaterThan'
          threshold: 5
          timeAggregation: 'Total'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

// Deadlock Alert
resource deadlockAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = if (enableAlerts && !empty(sqlDatabaseId)) {
  name: '${alertPrefix}-db-deadlock'
  location: 'global'
  tags: tags
  properties: {
    description: 'Alert when deadlocks are detected'
    severity: severityError
    enabled: true
    scopes: [sqlDatabaseId]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'Deadlock'
          metricName: 'deadlock'
          operator: 'GreaterThan'
          threshold: 0
          timeAggregation: 'Total'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

// ============================================================================
// Application Insights Log Alerts
// ============================================================================

// Exception Rate Alert
resource exceptionAlert 'Microsoft.Insights/scheduledQueryRules@2023-03-15-preview' = if (enableAlerts) {
  name: '${alertPrefix}-high-exceptions'
  location: location
  tags: tags
  properties: {
    displayName: 'High Exception Rate'
    description: 'Alert when exception rate exceeds threshold'
    severity: severityError
    enabled: true
    evaluationFrequency: 'PT5M'
    scopes: [applicationInsightsId]
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          query: '''
            exceptions
            | summarize ExceptionCount = count() by bin(timestamp, 5m)
            | where ExceptionCount > 50
          '''
          timeAggregation: 'Count'
          operator: 'GreaterThan'
          threshold: 0
          failingPeriods: {
            numberOfEvaluationPeriods: 1
            minFailingPeriodsToAlert: 1
          }
        }
      ]
    }
    actions: {
      actionGroups: [actionGroup.id]
    }
  }
}

// Failed Request Rate Alert
resource failedRequestAlert 'Microsoft.Insights/scheduledQueryRules@2023-03-15-preview' = if (enableAlerts) {
  name: '${alertPrefix}-high-failure-rate'
  location: location
  tags: tags
  properties: {
    displayName: 'High Request Failure Rate'
    description: 'Alert when request failure rate exceeds 5%'
    severity: severityError
    enabled: true
    evaluationFrequency: 'PT5M'
    scopes: [applicationInsightsId]
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          query: '''
            requests
            | summarize
                TotalRequests = count(),
                FailedRequests = countif(success == false)
            | extend FailureRate = (FailedRequests * 100.0) / TotalRequests
            | where FailureRate > 5
          '''
          timeAggregation: 'Count'
          operator: 'GreaterThan'
          threshold: 0
          failingPeriods: {
            numberOfEvaluationPeriods: 1
            minFailingPeriodsToAlert: 1
          }
        }
      ]
    }
    actions: {
      actionGroups: [actionGroup.id]
    }
  }
}

// Slow Dependency Alert
resource slowDependencyAlert 'Microsoft.Insights/scheduledQueryRules@2023-03-15-preview' = if (enableAlerts) {
  name: '${alertPrefix}-slow-dependency'
  location: location
  tags: tags
  properties: {
    displayName: 'Slow External Dependency'
    description: 'Alert when external dependencies are slow'
    severity: severityWarning
    enabled: true
    evaluationFrequency: 'PT5M'
    scopes: [applicationInsightsId]
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          query: '''
            dependencies
            | where duration > 5000
            | summarize SlowCalls = count() by target, type
            | where SlowCalls > 10
          '''
          timeAggregation: 'Count'
          operator: 'GreaterThan'
          threshold: 0
          failingPeriods: {
            numberOfEvaluationPeriods: 1
            minFailingPeriodsToAlert: 1
          }
        }
      ]
    }
    actions: {
      actionGroups: [actionGroup.id]
    }
  }
}

// ============================================================================
// Outputs
// ============================================================================

output actionGroupId string = enableAlerts ? actionGroup.id : ''
output actionGroupName string = enableAlerts ? actionGroup.name : ''
