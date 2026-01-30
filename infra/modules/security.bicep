// ============================================================================
// Security Configuration Module
// ============================================================================
//
// "I bent my Wookie." - Ralph Wiggum
// Don't bend your security - keep it strong with these configurations!
//
// This module deploys security resources:
// - Azure Front Door with WAF
// - Private Endpoints
// - Network Security Groups
// - Managed Identity configurations
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

@description('App Service resource ID')
param appServiceId string

@description('App Service default hostname')
param appServiceHostname string

@description('SQL Server resource ID')
param sqlServerId string = ''

@description('Key Vault resource ID')
param keyVaultId string = ''

@description('Storage Account resource ID')
param storageAccountId string = ''

@description('Virtual Network ID (for private endpoints)')
param vnetId string = ''

@description('Subnet ID for private endpoints')
param privateEndpointSubnetId string = ''

@description('Enable WAF')
param enableWaf bool = true

@description('Enable Private Endpoints')
param enablePrivateEndpoints bool = false

@description('WAF Mode (Detection or Prevention)')
@allowed(['Detection', 'Prevention'])
param wafMode string = 'Prevention'

// ============================================================================
// Variables
// ============================================================================

var frontDoorName = 'fd-${projectName}-${environment}'
var wafPolicyName = 'waf${projectName}${environment}'
var nsgName = 'nsg-${projectName}-${environment}'

// ============================================================================
// WAF Policy
// ============================================================================

resource wafPolicy 'Microsoft.Network/FrontDoorWebApplicationFirewallPolicies@2022-05-01' = if (enableWaf) {
  name: wafPolicyName
  location: 'global'
  tags: tags
  sku: {
    name: 'Premium_AzureFrontDoor'
  }
  properties: {
    policySettings: {
      enabledState: 'Enabled'
      mode: wafMode
      requestBodyCheck: 'Enabled'
      maxRequestBodySizeInKb: 128
      customBlockResponseStatusCode: 403
      customBlockResponseBody: base64('{"error": "Request blocked by WAF policy"}')
    }
    customRules: {
      rules: [
        // Rate limiting rule
        {
          name: 'RateLimitRule'
          priority: 100
          enabledState: 'Enabled'
          ruleType: 'RateLimitRule'
          rateLimitDurationInMinutes: 1
          rateLimitThreshold: 1000
          matchConditions: [
            {
              matchVariable: 'RequestUri'
              operator: 'Contains'
              negateCondition: false
              matchValue: ['/api/']
              transforms: ['Lowercase']
            }
          ]
          action: 'Block'
        }
        // Block specific countries (example - customize as needed)
        {
          name: 'GeoBlockRule'
          priority: 200
          enabledState: 'Disabled' // Enable and customize as needed
          ruleType: 'MatchRule'
          matchConditions: [
            {
              matchVariable: 'RemoteAddr'
              operator: 'GeoMatch'
              negateCondition: false
              matchValue: ['XX'] // Replace with country codes to block
            }
          ]
          action: 'Block'
        }
        // Block known bad user agents
        {
          name: 'BlockBadBots'
          priority: 300
          enabledState: 'Enabled'
          ruleType: 'MatchRule'
          matchConditions: [
            {
              matchVariable: 'RequestHeader'
              selector: 'User-Agent'
              operator: 'Contains'
              negateCondition: false
              matchValue: [
                'sqlmap'
                'nikto'
                'nmap'
                'masscan'
                'zgrab'
              ]
              transforms: ['Lowercase']
            }
          ]
          action: 'Block'
        }
      ]
    }
    managedRules: {
      managedRuleSets: [
        // OWASP Core Rule Set
        {
          ruleSetType: 'Microsoft_DefaultRuleSet'
          ruleSetVersion: '2.1'
          ruleSetAction: 'Block'
          ruleGroupOverrides: []
        }
        // Bot protection
        {
          ruleSetType: 'Microsoft_BotManagerRuleSet'
          ruleSetVersion: '1.0'
          ruleSetAction: 'Block'
          ruleGroupOverrides: []
        }
      ]
    }
  }
}

// ============================================================================
// Azure Front Door
// ============================================================================

resource frontDoorProfile 'Microsoft.Cdn/profiles@2023-05-01' = if (enableWaf) {
  name: frontDoorName
  location: 'global'
  tags: tags
  sku: {
    name: 'Premium_AzureFrontDoor'
  }
  properties: {
    originResponseTimeoutSeconds: 60
  }
}

resource frontDoorEndpoint 'Microsoft.Cdn/profiles/afdEndpoints@2023-05-01' = if (enableWaf) {
  parent: frontDoorProfile
  name: 'endpoint-${projectName}-${environment}'
  location: 'global'
  tags: tags
  properties: {
    enabledState: 'Enabled'
  }
}

resource frontDoorOriginGroup 'Microsoft.Cdn/profiles/originGroups@2023-05-01' = if (enableWaf) {
  parent: frontDoorProfile
  name: 'origingroup-api'
  properties: {
    loadBalancingSettings: {
      sampleSize: 4
      successfulSamplesRequired: 3
      additionalLatencyInMilliseconds: 50
    }
    healthProbeSettings: {
      probePath: '/health'
      probeRequestType: 'GET'
      probeProtocol: 'Https'
      probeIntervalInSeconds: 30
    }
    sessionAffinityState: 'Disabled'
  }
}

resource frontDoorOrigin 'Microsoft.Cdn/profiles/originGroups/origins@2023-05-01' = if (enableWaf) {
  parent: frontDoorOriginGroup
  name: 'origin-appservice'
  properties: {
    hostName: appServiceHostname
    httpPort: 80
    httpsPort: 443
    originHostHeader: appServiceHostname
    priority: 1
    weight: 1000
    enabledState: 'Enabled'
    enforceCertificateNameCheck: true
  }
}

resource frontDoorRoute 'Microsoft.Cdn/profiles/afdEndpoints/routes@2023-05-01' = if (enableWaf) {
  parent: frontDoorEndpoint
  name: 'route-default'
  properties: {
    originGroup: {
      id: frontDoorOriginGroup.id
    }
    supportedProtocols: ['Https']
    patternsToMatch: ['/*']
    forwardingProtocol: 'HttpsOnly'
    linkToDefaultDomain: 'Enabled'
    httpsRedirect: 'Enabled'
  }
  dependsOn: [frontDoorOrigin]
}

resource frontDoorSecurityPolicy 'Microsoft.Cdn/profiles/securityPolicies@2023-05-01' = if (enableWaf) {
  parent: frontDoorProfile
  name: 'securitypolicy-waf'
  properties: {
    parameters: {
      type: 'WebApplicationFirewall'
      wafPolicy: {
        id: wafPolicy.id
      }
      associations: [
        {
          domains: [
            {
              id: frontDoorEndpoint.id
            }
          ]
          patternsToMatch: ['/*']
        }
      ]
    }
  }
}

// ============================================================================
// Network Security Group
// ============================================================================

resource networkSecurityGroup 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
  name: nsgName
  location: location
  tags: tags
  properties: {
    securityRules: [
      // Allow HTTPS from Front Door
      {
        name: 'AllowFrontDoorHttps'
        properties: {
          priority: 100
          direction: 'Inbound'
          access: 'Allow'
          protocol: 'Tcp'
          sourcePortRange: '*'
          destinationPortRange: '443'
          sourceAddressPrefix: 'AzureFrontDoor.Backend'
          destinationAddressPrefix: '*'
          description: 'Allow HTTPS from Azure Front Door'
        }
      }
      // Allow Azure Load Balancer
      {
        name: 'AllowAzureLoadBalancer'
        properties: {
          priority: 110
          direction: 'Inbound'
          access: 'Allow'
          protocol: '*'
          sourcePortRange: '*'
          destinationPortRange: '*'
          sourceAddressPrefix: 'AzureLoadBalancer'
          destinationAddressPrefix: '*'
          description: 'Allow Azure Load Balancer'
        }
      }
      // Deny all other inbound
      {
        name: 'DenyAllInbound'
        properties: {
          priority: 4096
          direction: 'Inbound'
          access: 'Deny'
          protocol: '*'
          sourcePortRange: '*'
          destinationPortRange: '*'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          description: 'Deny all other inbound traffic'
        }
      }
    ]
  }
}

// ============================================================================
// Private Endpoints (when enabled)
// ============================================================================

// SQL Server Private Endpoint
resource sqlPrivateEndpoint 'Microsoft.Network/privateEndpoints@2023-05-01' = if (enablePrivateEndpoints && !empty(sqlServerId) && !empty(privateEndpointSubnetId)) {
  name: 'pe-${projectName}-sql-${environment}'
  location: location
  tags: tags
  properties: {
    subnet: {
      id: privateEndpointSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: 'plsc-sql'
        properties: {
          privateLinkServiceId: sqlServerId
          groupIds: ['sqlServer']
        }
      }
    ]
  }
}

// Key Vault Private Endpoint
resource kvPrivateEndpoint 'Microsoft.Network/privateEndpoints@2023-05-01' = if (enablePrivateEndpoints && !empty(keyVaultId) && !empty(privateEndpointSubnetId)) {
  name: 'pe-${projectName}-kv-${environment}'
  location: location
  tags: tags
  properties: {
    subnet: {
      id: privateEndpointSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: 'plsc-kv'
        properties: {
          privateLinkServiceId: keyVaultId
          groupIds: ['vault']
        }
      }
    ]
  }
}

// Storage Account Private Endpoint
resource storagePrivateEndpoint 'Microsoft.Network/privateEndpoints@2023-05-01' = if (enablePrivateEndpoints && !empty(storageAccountId) && !empty(privateEndpointSubnetId)) {
  name: 'pe-${projectName}-storage-${environment}'
  location: location
  tags: tags
  properties: {
    subnet: {
      id: privateEndpointSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: 'plsc-storage'
        properties: {
          privateLinkServiceId: storageAccountId
          groupIds: ['blob']
        }
      }
    ]
  }
}

// ============================================================================
// Outputs
// ============================================================================

output wafPolicyId string = enableWaf ? wafPolicy.id : ''
output wafPolicyName string = enableWaf ? wafPolicy.name : ''
output frontDoorId string = enableWaf ? frontDoorProfile.id : ''
output frontDoorEndpointHostname string = enableWaf ? frontDoorEndpoint.properties.hostName : ''
output nsgId string = networkSecurityGroup.id
output nsgName string = networkSecurityGroup.name
