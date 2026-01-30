// ============================================================================
// Azure Dashboard Module
// ============================================================================
//
// "My cat's breath smells like cat food." - Ralph Wiggum
// Our dashboard smells like... metrics! Fresh, informative metrics!
//
// This module deploys an Azure Dashboard for monitoring DSIF application
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

@description('SQL Database resource ID (optional)')
param sqlDatabaseId string = ''

@description('Log Analytics Workspace ID')
param logAnalyticsWorkspaceId string

// ============================================================================
// Variables
// ============================================================================

var dashboardName = 'dash-${projectName}-${environment}'

// ============================================================================
// Dashboard Resource
// ============================================================================

resource dashboard 'Microsoft.Portal/dashboards@2020-09-01-preview' = {
  name: dashboardName
  location: location
  tags: union(tags, {
    'hidden-title': 'DSIF ${toUpper(environment)} Dashboard'
  })
  properties: {
    lenses: [
      {
        order: 0
        parts: [
          // Row 1: Health Overview
          {
            position: {
              x: 0
              y: 0
              colSpan: 4
              rowSpan: 3
            }
            metadata: {
              type: 'Extension/HubsExtension/PartType/MarkdownPart'
              inputs: []
              settings: {
                content: {
                  settings: {
                    content: '''
# DSIF ${environment} Dashboard

**Environment:** ${environment}
**Last Updated:** Auto-refresh

## Quick Links
- [Application Insights](${applicationInsightsId})
- [Log Analytics](${logAnalyticsWorkspaceId})

---
*"I'm learnding!" - Ralph Wiggum*
'''
                  }
                }
              }
            }
          }
          // Health Status Tile
          {
            position: {
              x: 4
              y: 0
              colSpan: 4
              rowSpan: 3
            }
            metadata: {
              type: 'Extension/Microsoft_Azure_Monitoring/PartType/MetricsChartPart'
              inputs: [
                {
                  name: 'resourceId'
                  value: appServiceId
                }
                {
                  name: 'chartDefinition'
                  value: {
                    v2charts: [
                      {
                        metrics: [
                          {
                            resourceMetadata: {
                              id: appServiceId
                            }
                            name: 'HealthCheckStatus'
                            aggregationType: 4 // Average
                            namespace: 'Microsoft.Web/sites'
                            metricVisualization: {
                              displayName: 'Health Check Status'
                              color: '#47BDF5'
                            }
                          }
                        ]
                        title: 'Health Check Status'
                        visualization: {
                          chartType: 2 // Line chart
                          legendVisualization: {
                            isVisible: true
                            position: 2
                            hideSubtitle: false
                          }
                        }
                      }
                    ]
                  }
                }
              ]
            }
          }
          // Response Time Chart
          {
            position: {
              x: 8
              y: 0
              colSpan: 4
              rowSpan: 3
            }
            metadata: {
              type: 'Extension/Microsoft_Azure_Monitoring/PartType/MetricsChartPart'
              inputs: [
                {
                  name: 'resourceId'
                  value: appServiceId
                }
                {
                  name: 'chartDefinition'
                  value: {
                    v2charts: [
                      {
                        metrics: [
                          {
                            resourceMetadata: {
                              id: appServiceId
                            }
                            name: 'HttpResponseTime'
                            aggregationType: 4 // Average
                            namespace: 'Microsoft.Web/sites'
                            metricVisualization: {
                              displayName: 'Response Time'
                              color: '#7E58FF'
                            }
                          }
                        ]
                        title: 'Response Time (seconds)'
                        visualization: {
                          chartType: 2
                          legendVisualization: {
                            isVisible: true
                            position: 2
                          }
                        }
                      }
                    ]
                  }
                }
              ]
            }
          }
          // Row 2: Request Metrics
          // Requests Chart
          {
            position: {
              x: 0
              y: 3
              colSpan: 6
              rowSpan: 4
            }
            metadata: {
              type: 'Extension/Microsoft_Azure_Monitoring/PartType/MetricsChartPart'
              inputs: [
                {
                  name: 'resourceId'
                  value: appServiceId
                }
                {
                  name: 'chartDefinition'
                  value: {
                    v2charts: [
                      {
                        metrics: [
                          {
                            resourceMetadata: {
                              id: appServiceId
                            }
                            name: 'Requests'
                            aggregationType: 1 // Total
                            namespace: 'Microsoft.Web/sites'
                            metricVisualization: {
                              displayName: 'Requests'
                              color: '#00BCF2'
                            }
                          }
                        ]
                        title: 'Request Volume'
                        visualization: {
                          chartType: 3 // Area chart
                          legendVisualization: {
                            isVisible: true
                            position: 2
                          }
                        }
                      }
                    ]
                  }
                }
              ]
            }
          }
          // Error Rates Chart
          {
            position: {
              x: 6
              y: 3
              colSpan: 6
              rowSpan: 4
            }
            metadata: {
              type: 'Extension/Microsoft_Azure_Monitoring/PartType/MetricsChartPart'
              inputs: [
                {
                  name: 'resourceId'
                  value: appServiceId
                }
                {
                  name: 'chartDefinition'
                  value: {
                    v2charts: [
                      {
                        metrics: [
                          {
                            resourceMetadata: {
                              id: appServiceId
                            }
                            name: 'Http4xx'
                            aggregationType: 1 // Total
                            namespace: 'Microsoft.Web/sites'
                            metricVisualization: {
                              displayName: '4xx Errors'
                              color: '#FCD116'
                            }
                          }
                          {
                            resourceMetadata: {
                              id: appServiceId
                            }
                            name: 'Http5xx'
                            aggregationType: 1 // Total
                            namespace: 'Microsoft.Web/sites'
                            metricVisualization: {
                              displayName: '5xx Errors'
                              color: '#E81123'
                            }
                          }
                        ]
                        title: 'HTTP Errors'
                        visualization: {
                          chartType: 2
                          legendVisualization: {
                            isVisible: true
                            position: 2
                          }
                        }
                      }
                    ]
                  }
                }
              ]
            }
          }
          // Row 3: Resource Utilization
          // CPU Chart
          {
            position: {
              x: 0
              y: 7
              colSpan: 4
              rowSpan: 4
            }
            metadata: {
              type: 'Extension/Microsoft_Azure_Monitoring/PartType/MetricsChartPart'
              inputs: [
                {
                  name: 'resourceId'
                  value: appServiceId
                }
                {
                  name: 'chartDefinition'
                  value: {
                    v2charts: [
                      {
                        metrics: [
                          {
                            resourceMetadata: {
                              id: appServiceId
                            }
                            name: 'CpuPercentage'
                            aggregationType: 4 // Average
                            namespace: 'Microsoft.Web/sites'
                            metricVisualization: {
                              displayName: 'CPU %'
                              color: '#0078D4'
                            }
                          }
                        ]
                        title: 'CPU Utilization'
                        visualization: {
                          chartType: 2
                          legendVisualization: {
                            isVisible: true
                            position: 2
                          }
                        }
                      }
                    ]
                  }
                }
              ]
            }
          }
          // Memory Chart
          {
            position: {
              x: 4
              y: 7
              colSpan: 4
              rowSpan: 4
            }
            metadata: {
              type: 'Extension/Microsoft_Azure_Monitoring/PartType/MetricsChartPart'
              inputs: [
                {
                  name: 'resourceId'
                  value: appServiceId
                }
                {
                  name: 'chartDefinition'
                  value: {
                    v2charts: [
                      {
                        metrics: [
                          {
                            resourceMetadata: {
                              id: appServiceId
                            }
                            name: 'MemoryPercentage'
                            aggregationType: 4 // Average
                            namespace: 'Microsoft.Web/sites'
                            metricVisualization: {
                              displayName: 'Memory %'
                              color: '#00A2AD'
                            }
                          }
                        ]
                        title: 'Memory Utilization'
                        visualization: {
                          chartType: 2
                          legendVisualization: {
                            isVisible: true
                            position: 2
                          }
                        }
                      }
                    ]
                  }
                }
              ]
            }
          }
          // Connections Chart
          {
            position: {
              x: 8
              y: 7
              colSpan: 4
              rowSpan: 4
            }
            metadata: {
              type: 'Extension/Microsoft_Azure_Monitoring/PartType/MetricsChartPart'
              inputs: [
                {
                  name: 'resourceId'
                  value: appServiceId
                }
                {
                  name: 'chartDefinition'
                  value: {
                    v2charts: [
                      {
                        metrics: [
                          {
                            resourceMetadata: {
                              id: appServiceId
                            }
                            name: 'AppConnections'
                            aggregationType: 4 // Average
                            namespace: 'Microsoft.Web/sites'
                            metricVisualization: {
                              displayName: 'Connections'
                              color: '#8764B8'
                            }
                          }
                        ]
                        title: 'Active Connections'
                        visualization: {
                          chartType: 2
                          legendVisualization: {
                            isVisible: true
                            position: 2
                          }
                        }
                      }
                    ]
                  }
                }
              ]
            }
          }
        ]
      }
    ]
    metadata: {
      model: {
        timeRange: {
          value: {
            relative: {
              duration: 24
              timeUnit: 1 // Hours
            }
          }
          type: 'MsPortalFx.Composition.Configuration.ValueTypes.TimeRange'
        }
        filterLocale: {
          value: 'en-us'
        }
        filters: {
          value: {
            MsPortalFx_TimeRange: {
              model: {
                format: 'local'
                granularity: 'auto'
                relative: '24h'
              }
              displayCache: {
                name: 'Local Time'
                value: 'Past 24 hours'
              }
            }
          }
        }
      }
    }
  }
}

// ============================================================================
// Outputs
// ============================================================================

output dashboardId string = dashboard.id
output dashboardName string = dashboard.name
