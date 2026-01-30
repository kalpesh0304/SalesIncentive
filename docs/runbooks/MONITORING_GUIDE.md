# Monitoring & Observability Guide

**Document ID:** RB-MONITOR-001
**Project:** Dorise Sales Incentive Framework (DSIF)
**Version:** 1.0
**Last Updated:** January 2025
**Owner:** DevOps Team

> *"I'm learnding!"* - Ralph Wiggum
>
> Monitoring helps us learn what our application is doing in production!

---

## Table of Contents

1. [Overview](#1-overview)
2. [Application Insights](#2-application-insights)
3. [Log Analytics](#3-log-analytics)
4. [Alert Rules](#4-alert-rules)
5. [Dashboards](#5-dashboards)
6. [Key Metrics](#6-key-metrics)
7. [Troubleshooting Queries](#7-troubleshooting-queries)

---

## 1. Overview

### 1.1 Monitoring Stack

| Component | Purpose | Location |
|-----------|---------|----------|
| Application Insights | APM, telemetry, tracing | Azure Portal |
| Log Analytics | Log aggregation, queries | Azure Portal |
| Azure Monitor | Metrics, alerts | Azure Portal |
| Dashboards | Visualization | Azure Portal |

### 1.2 Instrumentation

The DSIF application uses automatic and custom instrumentation:

```csharp
// Application Insights is configured in Program.cs
builder.Services.AddApplicationInsightsTelemetry();

// Custom telemetry example
_telemetryClient.TrackEvent("CalculationCompleted", new Dictionary<string, string>
{
    ["EmployeeId"] = employeeId.ToString(),
    ["Period"] = period,
    ["Amount"] = amount.ToString("C")
});
```

### 1.3 Resource Naming

| Environment | Application Insights | Log Analytics |
|-------------|---------------------|---------------|
| Development | appi-dorise-dev-eastus | log-dorise-dev |
| Staging | appi-dorise-stg-eastus | log-dorise-stg |
| Production | appi-dorise-prod-eastus | log-dorise-prod |

---

## 2. Application Insights

### 2.1 Access

```
Azure Portal → Application Insights → appi-dorise-{env}-eastus
```

### 2.2 Key Features

| Feature | Description | Path |
|---------|-------------|------|
| Live Metrics | Real-time telemetry | Live Metrics |
| Application Map | Service dependency visualization | Application Map |
| Failures | Error analysis | Failures |
| Performance | Request performance | Performance |
| Availability | Uptime monitoring | Availability |

### 2.3 Connection String Configuration

```bash
# Get connection string
az monitor app-insights component show \
  --resource-group rg-dorise-prod-eastus \
  --app appi-dorise-prod-eastus \
  --query connectionString -o tsv

# Set in App Service
az webapp config appsettings set \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod \
  --settings APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=xxx;IngestionEndpoint=xxx"
```

### 2.4 Custom Events

The application tracks custom events for business metrics:

| Event Name | Description | Properties |
|------------|-------------|------------|
| CalculationStarted | Calculation initiated | EmployeeId, Period |
| CalculationCompleted | Calculation finished | EmployeeId, Period, Amount |
| CalculationFailed | Calculation error | EmployeeId, Period, Error |
| ApprovalSubmitted | Approval workflow started | CalculationId, ApproverId |
| ApprovalCompleted | Approval finished | CalculationId, Status |
| UserLogin | User authentication | UserId, LoginType |

---

## 3. Log Analytics

### 3.1 Access

```
Azure Portal → Log Analytics workspaces → log-dorise-{env}
```

### 3.2 Common Tables

| Table | Description | Retention |
|-------|-------------|-----------|
| requests | HTTP requests | 30 days |
| exceptions | Application exceptions | 30 days |
| dependencies | External calls (SQL, HTTP) | 30 days |
| traces | Log messages | 30 days |
| customEvents | Custom business events | 30 days |
| customMetrics | Custom metrics | 30 days |
| performanceCounters | Server metrics | 30 days |

### 3.3 Saved Queries

Import queries from `/infra/monitoring/log-analytics-queries.kql`:

```bash
# Example: Query recent errors
az monitor log-analytics query \
  --workspace log-dorise-prod \
  --analytics-query "exceptions | where timestamp > ago(1h) | take 10"
```

---

## 4. Alert Rules

### 4.1 Configured Alerts

| Alert | Condition | Severity | Action |
|-------|-----------|----------|--------|
| High CPU | CPU > 80% for 5 min | Warning | Email |
| High Memory | Memory > 80% for 5 min | Warning | Email |
| HTTP 5xx Errors | > 10 errors in 5 min | Error | Email |
| Slow Response | Avg response > 2s | Warning | Email |
| Health Check Failed | Status < 1 | Critical | Email + PagerDuty |
| High Exception Rate | > 50 exceptions in 5 min | Error | Email |
| High Failure Rate | > 5% failure rate | Error | Email |
| Database DTU High | DTU > 80% | Warning | Email |
| Database Connection Failed | > 5 failures | Critical | Email + PagerDuty |
| Database Deadlock | Any deadlock | Error | Email |

### 4.2 Managing Alerts

```bash
# List all alert rules
az monitor metrics alert list \
  --resource-group rg-dorise-prod-eastus \
  --query "[].{Name:name, Enabled:enabled, Severity:severity}"

# Disable an alert (maintenance)
az monitor metrics alert update \
  --resource-group rg-dorise-prod-eastus \
  --name alert-dorise-prod-high-cpu \
  --enabled false

# Re-enable alert
az monitor metrics alert update \
  --resource-group rg-dorise-prod-eastus \
  --name alert-dorise-prod-high-cpu \
  --enabled true
```

### 4.3 Alert Notification Configuration

Alerts are sent to:
- **Email:** dsif-alerts@dorise.com
- **PagerDuty:** (Critical alerts only)
- **Slack:** #dsif-alerts (via webhook)

---

## 5. Dashboards

### 5.1 Available Dashboards

| Dashboard | Description | Audience |
|-----------|-------------|----------|
| DSIF Production Overview | Key metrics for prod | Operations |
| DSIF Performance | Detailed performance | Development |
| DSIF Business Metrics | Calculations, approvals | Business |

### 5.2 Dashboard Components

**Overview Dashboard includes:**
- Health check status
- Response time trends
- Request volume
- Error rates (4xx, 5xx)
- CPU utilization
- Memory utilization
- Active connections

### 5.3 Creating Custom Dashboards

1. Navigate to Azure Portal → Dashboards
2. Click "New dashboard"
3. Add tiles from Application Insights / Metrics
4. Save and share with team

---

## 6. Key Metrics

### 6.1 SLI/SLO Definitions

| Metric | SLI | SLO | Measurement |
|--------|-----|-----|-------------|
| Availability | Successful health checks / Total | 99.9% | Application Insights |
| Latency | P95 response time | < 500ms | Application Insights |
| Error Rate | Failed requests / Total | < 1% | Application Insights |
| Throughput | Requests per second | > 100 RPS | Application Insights |

### 6.2 Key Performance Indicators

| KPI | Target | Query |
|-----|--------|-------|
| API Availability | 99.9% | See queries below |
| API P95 Latency | < 500ms | See queries below |
| Error Rate | < 1% | See queries below |
| Calculation Success Rate | > 99% | Custom events |

### 6.3 KPI Queries

```kusto
// Availability (last 24 hours)
requests
| where timestamp > ago(24h)
| summarize
    TotalRequests = count(),
    SuccessfulRequests = countif(success == true)
| extend Availability = round(SuccessfulRequests * 100.0 / TotalRequests, 3)
| project Availability

// P95 Latency (last 24 hours)
requests
| where timestamp > ago(24h)
| summarize P95Latency = percentile(duration, 95)
| project P95LatencyMs = round(P95Latency, 0)

// Error Rate (last 24 hours)
requests
| where timestamp > ago(24h)
| summarize
    TotalRequests = count(),
    FailedRequests = countif(success == false)
| extend ErrorRate = round(FailedRequests * 100.0 / TotalRequests, 3)
| project ErrorRate
```

---

## 7. Troubleshooting Queries

### 7.1 Performance Issues

```kusto
// Find slow endpoints
requests
| where timestamp > ago(1h)
| summarize
    AvgDuration = avg(duration),
    P95Duration = percentile(duration, 95),
    Count = count()
    by name
| where P95Duration > 1000
| order by P95Duration desc
| take 10

// Find slow SQL queries
dependencies
| where timestamp > ago(1h)
| where type == "SQL"
| where duration > 1000
| project timestamp, target, name, duration, success
| order by duration desc
| take 20
```

### 7.2 Error Investigation

```kusto
// Recent exceptions with details
exceptions
| where timestamp > ago(1h)
| project
    timestamp,
    type,
    outerMessage,
    innermostMessage,
    operation_Id
| order by timestamp desc
| take 50

// Correlate exception with request
let operationId = "abc123";  // Get from exception
requests
| where operation_Id == operationId
| project timestamp, name, url, duration, resultCode

// Group exceptions by type
exceptions
| where timestamp > ago(24h)
| summarize Count = count() by type, outerMessage
| order by Count desc
```

### 7.3 Dependency Issues

```kusto
// Failed external calls
dependencies
| where timestamp > ago(1h)
| where success == false
| summarize Count = count() by type, target, resultCode
| order by Count desc

// Slow external services
dependencies
| where timestamp > ago(1h)
| summarize
    AvgDuration = avg(duration),
    P95Duration = percentile(duration, 95)
    by type, target
| where P95Duration > 2000
| order by P95Duration desc
```

### 7.4 User/Traffic Analysis

```kusto
// Requests by IP (detect abuse)
requests
| where timestamp > ago(1h)
| summarize Count = count() by client_IP
| order by Count desc
| take 20

// Failed login attempts
customEvents
| where timestamp > ago(24h)
| where name == "LoginFailed"
| summarize Count = count() by tostring(customDimensions["Username"])
| order by Count desc
```

---

## Appendix A: Quick Reference

### Useful Azure CLI Commands

```bash
# View live metrics
az monitor app-insights metrics show \
  --app appi-dorise-prod-eastus \
  --metric requests/count

# Query logs
az monitor log-analytics query \
  --workspace log-dorise-prod \
  --analytics-query "requests | where timestamp > ago(1h) | count"

# List alert rules
az monitor metrics alert list --resource-group rg-dorise-prod-eastus

# Check resource health
az resource show --ids /subscriptions/.../resourceGroups/rg-dorise-prod-eastus/...
```

### Portal Quick Links

- Application Insights: `https://portal.azure.com/#@tenant/resource/subscriptions/{sub}/resourceGroups/rg-dorise-prod-eastus/providers/microsoft.insights/components/appi-dorise-prod-eastus/overview`
- Log Analytics: `https://portal.azure.com/#@tenant/resource/subscriptions/{sub}/resourceGroups/rg-dorise-prod-eastus/providers/Microsoft.OperationalInsights/workspaces/log-dorise-prod/logs`

---

**Document History:**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | Jan 2025 | DevOps Team | Initial version |

*This document is part of the DSIF Operational Runbook series.*
