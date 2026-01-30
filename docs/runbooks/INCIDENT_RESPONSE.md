# Incident Response Runbook

**Document ID:** RB-INCIDENT-001
**Project:** Dorise Sales Incentive Framework (DSIF)
**Version:** 1.0
**Last Updated:** January 2025
**Owner:** DevOps Team + Engineering

> *"The doctor said I wouldn't have so many nose bleeds if I kept my finger outta there."* - Ralph Wiggum
>
> Prevention is better than cure - but when incidents happen, this runbook helps you respond!

---

## Table of Contents

1. [Overview](#1-overview)
2. [Incident Classification](#2-incident-classification)
3. [Response Procedures](#3-response-procedures)
4. [Common Incidents](#4-common-incidents)
5. [Communication Templates](#5-communication-templates)
6. [Post-Incident Process](#6-post-incident-process)
7. [Contacts & Escalation](#7-contacts--escalation)

---

## 1. Overview

### 1.1 Purpose

This runbook provides structured procedures for responding to incidents affecting the DSIF application. It ensures rapid response, clear communication, and systematic resolution.

### 1.2 Scope

- Application outages and errors
- Performance degradation
- Security incidents
- Data issues
- Infrastructure failures

### 1.3 Incident Response Principles

1. **Safety First** - Protect data and systems
2. **Communicate Early** - Keep stakeholders informed
3. **Document Everything** - Create audit trail
4. **Fix Forward When Possible** - Prefer fixes over rollbacks
5. **Learn and Improve** - Every incident is a learning opportunity

---

## 2. Incident Classification

### 2.1 Severity Levels

| Severity | Impact | Response Time | Examples |
|----------|--------|---------------|----------|
| **SEV-1 Critical** | Complete service outage | 15 minutes | App completely down, data breach |
| **SEV-2 High** | Major functionality impaired | 30 minutes | Calculations failing, auth broken |
| **SEV-3 Medium** | Minor functionality impaired | 2 hours | Reports slow, minor UI bugs |
| **SEV-4 Low** | Minimal impact | 24 hours | Cosmetic issues, documentation |

### 2.2 Severity Decision Tree

```
Is the service completely unavailable?
├── YES → SEV-1
└── NO
    └── Is core functionality (calculations, approvals) broken?
        ├── YES → SEV-2
        └── NO
            └── Are users significantly impacted?
                ├── YES → SEV-3
                └── NO → SEV-4
```

### 2.3 Incident Categories

| Category | Description | Primary Team |
|----------|-------------|--------------|
| **Availability** | Service down or unreachable | DevOps |
| **Performance** | Slow response, timeouts | DevOps + Dev |
| **Functionality** | Features not working | Development |
| **Security** | Unauthorized access, vulnerabilities | Security + DevOps |
| **Data** | Data corruption, loss, inconsistency | DBA + Dev |
| **Integration** | External service failures | Development |

---

## 3. Response Procedures

### 3.1 Initial Response (First 15 Minutes)

#### Step 1: Acknowledge

```bash
# Check monitoring alerts
# Review Application Insights
# Verify the incident is real (not false positive)
```

#### Step 2: Assess Severity

Use the decision tree above to classify the incident.

#### Step 3: Notify

**SEV-1/SEV-2:** Immediately notify via:
- Slack: #dsif-incidents
- PagerDuty: Auto-escalation
- Email: dsif-oncall@dorise.com

**SEV-3/SEV-4:** Create ticket and notify during business hours.

#### Step 4: Create Incident Ticket

```markdown
**Title:** [SEV-X] Brief description
**Time Detected:** YYYY-MM-DD HH:MM UTC
**Impact:** Number of users/transactions affected
**Symptoms:** What users are experiencing
**Initial Assessment:** What we know so far
```

### 3.2 Investigation Phase

#### Gather Information

```bash
# Check application health
curl https://app-dorise-api-prod.azurewebsites.net/health

# Check recent deployments
az webapp deployment list \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod

# Check recent errors in Application Insights
az monitor app-insights query \
  --app appi-dorise-prod-eastus \
  --analytics-query "
    exceptions
    | where timestamp > ago(1h)
    | summarize count() by problemId
    | top 10 by count_
  "

# Check performance metrics
az monitor app-insights query \
  --app appi-dorise-prod-eastus \
  --analytics-query "
    requests
    | where timestamp > ago(1h)
    | summarize avg(duration), percentile(duration, 95) by bin(timestamp, 5m)
  "
```

#### Common Investigation Queries

```kusto
// Find error spike
exceptions
| where timestamp > ago(1h)
| summarize count() by bin(timestamp, 1m)
| render timechart

// Find slow requests
requests
| where timestamp > ago(1h)
| where duration > 5000
| project timestamp, name, duration, resultCode
| top 100 by duration

// Find failed requests
requests
| where timestamp > ago(1h)
| where success == false
| summarize count() by resultCode, name
| top 20 by count_

// Database dependency issues
dependencies
| where timestamp > ago(1h)
| where type == "SQL"
| where success == false
| summarize count() by target, resultCode
```

### 3.3 Mitigation Actions

#### Quick Mitigations

| Issue | Mitigation | Command |
|-------|------------|---------|
| Memory issues | Restart app | `az webapp restart -g rg-dorise-prod-eastus -n app-dorise-api-prod` |
| Bad deployment | Slot swap rollback | `az webapp deployment slot swap -g rg-dorise-prod-eastus -n app-dorise-api-prod -s staging` |
| Traffic spike | Scale out | `az webapp update -g rg-dorise-prod-eastus -n app-dorise-api-prod --set siteConfig.numberOfWorkers=4` |
| Database overload | Enable read replicas | Enable via Azure Portal |
| Cache issues | Clear Redis cache | `az redis force-reboot -g rg-dorise-prod-eastus -n redis-dorise-prod` |

#### Rollback Procedure

```bash
# Quick rollback via slot swap
az webapp deployment slot swap \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod \
  --slot staging \
  --target-slot production

# Verify rollback
curl https://app-dorise-api-prod.azurewebsites.net/health
curl https://app-dorise-api-prod.azurewebsites.net/api/version
```

### 3.4 Resolution & Recovery

#### Verification Steps

1. Health check returns 200
2. Error rate back to normal (<1%)
3. Response times within SLA
4. Core functionality tested
5. No new errors in logs

#### Recovery Checklist

- [ ] Root cause identified
- [ ] Fix applied (or rolled back)
- [ ] Monitoring confirms recovery
- [ ] Affected users can resume work
- [ ] Stakeholders notified of resolution

---

## 4. Common Incidents

### 4.1 Application Not Responding

**Symptoms:** 5xx errors, timeouts, health check failing

**Investigation:**
```bash
# Check app state
az webapp show -g rg-dorise-prod-eastus -n app-dorise-api-prod --query state

# Check for crashes
az monitor app-insights query --app appi-dorise-prod-eastus \
  --analytics-query "exceptions | where timestamp > ago(30m) | count"

# Check resource usage
az webapp show -g rg-dorise-prod-eastus -n app-dorise-api-prod --query usageState
```

**Resolution:**
1. Restart application
2. If persists, check recent deployments
3. Rollback if deployment-related
4. Scale out if resource-related

### 4.2 Database Connection Failures

**Symptoms:** "Cannot open database" errors, timeouts

**Investigation:**
```bash
# Check SQL server status
az sql server show -g rg-dorise-prod-eastus -n sql-dorise-prod-eastus

# Check DTU usage
az sql db show -g rg-dorise-prod-eastus -s sql-dorise-prod-eastus -n sqldb-dorise-prod \
  --query "[currentServiceObjectiveName, maxSizeBytes]"

# Check firewall rules
az sql server firewall-rule list -g rg-dorise-prod-eastus -s sql-dorise-prod-eastus
```

**Resolution:**
1. Verify connection string in Key Vault
2. Check firewall rules include App Service IPs
3. Scale up database if DTU exhausted
4. Check for blocking queries

### 4.3 High Response Times

**Symptoms:** P95 latency > 2s, user complaints about slowness

**Investigation:**
```kusto
// Find slow endpoints
requests
| where timestamp > ago(1h)
| summarize avg(duration), percentile(duration, 95) by name
| where percentile_duration_95 > 2000
| order by percentile_duration_95 desc

// Check database dependency
dependencies
| where timestamp > ago(1h)
| where type == "SQL"
| summarize avg(duration) by target
| order by avg_duration desc
```

**Resolution:**
1. Identify slow queries and optimize
2. Enable/increase caching
3. Scale out application
4. Scale up database

### 4.4 Authentication Failures

**Symptoms:** 401/403 errors, "Unable to validate token"

**Investigation:**
```bash
# Check Azure AD configuration
az webapp config appsettings list -g rg-dorise-prod-eastus -n app-dorise-api-prod \
  --query "[?contains(name, 'AzureAd')]"

# Check token signing keys
# Azure AD sometimes rotates keys
```

**Resolution:**
1. Verify Azure AD app registration
2. Check client ID and tenant ID
3. Restart application to refresh keys
4. Check token expiration settings

### 4.5 Calculation Errors

**Symptoms:** Incorrect incentive calculations, calculation failures

**Investigation:**
```kusto
// Find calculation errors
traces
| where timestamp > ago(1h)
| where message contains "Calculation"
| where severityLevel >= 3
| project timestamp, message, customDimensions

// Check for data issues
customEvents
| where timestamp > ago(1h)
| where name == "CalculationFailed"
| project timestamp, customDimensions
```

**Resolution:**
1. Check calculation engine logs
2. Verify incentive plan configuration
3. Check for data validation errors
4. Review recent plan changes

### 4.6 Security Incident

**Symptoms:** Unauthorized access attempts, suspicious activity

**Immediate Actions:**
1. **DO NOT** make changes that destroy evidence
2. Preserve logs and screenshots
3. Notify Security Team immediately
4. Document timeline of events

**Investigation:**
```kusto
// Check for suspicious logins
customEvents
| where timestamp > ago(24h)
| where name == "UserLogin"
| summarize count() by tostring(customDimensions["UserId"]), bin(timestamp, 1h)
| where count_ > 50

// Check for unusual API access
requests
| where timestamp > ago(24h)
| summarize count() by client_IP
| where count_ > 10000
```

**Escalation:**
- Security Team: security@dorise.com
- Legal (if data breach): legal@dorise.com

---

## 5. Communication Templates

### 5.1 Initial Incident Notification

```
🚨 INCIDENT: [SEV-X] [Brief Description]

Impact: [What users are experiencing]
Status: Investigating
Started: [Time] UTC

Team is actively investigating. Updates every 30 minutes.

Incident Commander: [Name]
```

### 5.2 Status Update

```
📢 UPDATE: [SEV-X] [Brief Description]

Status: [Investigating/Mitigating/Monitoring]
Progress: [What we've learned/done]
ETA: [Estimated resolution time if known]

Next update in 30 minutes.
```

### 5.3 Resolution Notification

```
✅ RESOLVED: [Brief Description]

Resolution: [What fixed the issue]
Duration: [Start time - End time]
Impact: [Number of users/transactions affected]

Post-incident review scheduled for [date].
```

### 5.4 Customer-Facing Communication

```
Subject: Service Disruption - Dorise Incentive System

Dear Users,

We are currently experiencing [brief description of issue] with the
Dorise Sales Incentive system. Our team is actively working on
resolving this issue.

Impact: [What functionality is affected]
Workaround: [If any available]

We apologize for any inconvenience and will provide updates every
30 minutes.

Thank you for your patience.
Dorise Support Team
```

---

## 6. Post-Incident Process

### 6.1 Incident Timeline

Document the complete timeline:

```markdown
| Time (UTC) | Event |
|------------|-------|
| 10:00 | First alert received |
| 10:05 | On-call acknowledged |
| 10:15 | Severity classified as SEV-2 |
| 10:20 | Root cause identified |
| 10:30 | Mitigation applied |
| 10:45 | Service restored |
| 11:00 | Monitoring confirmed recovery |
```

### 6.2 Post-Incident Review (PIR)

Schedule within 3 business days for SEV-1/SEV-2.

**PIR Template:**

```markdown
# Post-Incident Review: [Incident Title]

## Summary
- **Date:** [Date]
- **Duration:** [Duration]
- **Severity:** [SEV-X]
- **Impact:** [Users/Revenue affected]

## Timeline
[Detailed timeline]

## Root Cause
[Technical explanation of what went wrong]

## Contributing Factors
- [Factor 1]
- [Factor 2]

## What Went Well
- [Good response/process]

## What Could Be Improved
- [Improvement opportunity]

## Action Items
| Action | Owner | Due Date |
|--------|-------|----------|
| [Action 1] | [Name] | [Date] |

## Lessons Learned
[Key takeaways]
```

### 6.3 Action Item Tracking

All action items must be:
- Tracked in JIRA/Azure DevOps
- Assigned to an owner
- Given a due date
- Reviewed in weekly ops meeting

---

## 7. Contacts & Escalation

### 7.1 Escalation Matrix

| Severity | First Responder | Escalate After | Second Level | Escalate After | Third Level |
|----------|-----------------|----------------|--------------|----------------|-------------|
| SEV-1 | On-Call | 15 min | DevOps Lead | 30 min | VP Engineering |
| SEV-2 | On-Call | 30 min | DevOps Lead | 1 hour | Engineering Manager |
| SEV-3 | On-Call | 2 hours | Team Lead | 4 hours | Engineering Manager |
| SEV-4 | Assigned Dev | 24 hours | Team Lead | - | - |

### 7.2 Contact Directory

| Role | Name | Phone | Email |
|------|------|-------|-------|
| On-Call (Primary) | Rotation | +1-XXX-XXX-XXXX | oncall@dorise.com |
| DevOps Lead | [Name] | +1-XXX-XXX-XXXX | devops-lead@dorise.com |
| Engineering Manager | [Name] | +1-XXX-XXX-XXXX | eng-manager@dorise.com |
| DBA | [Name] | +1-XXX-XXX-XXXX | dba@dorise.com |
| Security Team | [Name] | +1-XXX-XXX-XXXX | security@dorise.com |

### 7.3 External Support

| Service | Support Portal | Support Level |
|---------|----------------|---------------|
| Azure | https://portal.azure.com/#blade/Microsoft_Azure_Support | Premier |
| GitHub Enterprise | https://support.github.com | Enterprise |
| Auth0 (if used) | https://support.auth0.com | Professional |

### 7.4 Communication Channels

| Channel | Purpose | URL |
|---------|---------|-----|
| Slack #dsif-incidents | Real-time incident updates | slack.dorise.com |
| Slack #dsif-oncall | On-call coordination | slack.dorise.com |
| PagerDuty | Alert escalation | pagerduty.com |
| Status Page | Customer communication | status.dorise.com |

---

## Appendix A: Incident Response Checklist

### Quick Reference

```
□ Acknowledge alert
□ Assess severity (SEV-1 to SEV-4)
□ Notify appropriate channels
□ Create incident ticket
□ Begin investigation
□ Apply mitigation
□ Verify resolution
□ Notify stakeholders
□ Complete incident documentation
□ Schedule PIR (if SEV-1/SEV-2)
```

---

**Document History:**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | Jan 2025 | DevOps Team | Initial version |

*This document is part of the DSIF Operational Runbook series.*
