# Deployment Runbook

**Document ID:** RB-DEPLOY-001
**Project:** Dorise Sales Incentive Framework (DSIF)
**Version:** 1.0
**Last Updated:** January 2025
**Owner:** DevOps Team

> *"I'm Idaho!"* - Ralph Wiggum
>
> Know where you're deploying - this runbook helps you navigate deployment like a pro!

---

## Table of Contents

1. [Overview](#1-overview)
2. [Pre-Deployment Checklist](#2-pre-deployment-checklist)
3. [Environment Details](#3-environment-details)
4. [Deployment Procedures](#4-deployment-procedures)
5. [Post-Deployment Validation](#5-post-deployment-validation)
6. [Rollback Procedures](#6-rollback-procedures)
7. [Troubleshooting](#7-troubleshooting)
8. [Contacts](#8-contacts)

---

## 1. Overview

### 1.1 Purpose

This runbook provides step-by-step procedures for deploying the DSIF application to all environments. It ensures consistent, repeatable deployments with minimal risk.

### 1.2 Scope

- Development environment deployments
- Staging environment deployments
- Production environment deployments
- Infrastructure deployments (Bicep/ARM)

### 1.3 Deployment Strategy

| Environment | Strategy | Approval Required | Rollback Method |
|-------------|----------|-------------------|-----------------|
| Development | Direct Deploy | No | Redeploy |
| Staging | Slot Deploy | No | Slot Swap |
| Production | Blue-Green (Slots) | Yes (2 approvers) | Slot Swap |

---

## 2. Pre-Deployment Checklist

### 2.1 Code Readiness

- [ ] All unit tests passing in CI pipeline
- [ ] Code review completed and approved
- [ ] Version number updated (semantic versioning)
- [ ] CHANGELOG.md updated with release notes
- [ ] No critical or high security vulnerabilities
- [ ] Feature flags configured (if applicable)

### 2.2 Environment Readiness

- [ ] Target environment is accessible
- [ ] Database migrations are ready (if applicable)
- [ ] Configuration/secrets are updated in Key Vault
- [ ] Sufficient resources available (CPU, memory, storage)
- [ ] Backup of current deployment completed

### 2.3 Communication

- [ ] Stakeholders notified of deployment window
- [ ] Support team aware of deployment
- [ ] Rollback plan communicated to team
- [ ] Monitoring dashboards accessible

### 2.4 Documentation

- [ ] Deployment ticket created (JIRA/Azure DevOps)
- [ ] Release notes prepared
- [ ] Known issues documented

---

## 3. Environment Details

### 3.1 Development

| Component | Value |
|-----------|-------|
| Resource Group | `rg-dorise-dev-eastus` |
| App Service | `app-dorise-api-dev` |
| URL | `https://app-dorise-api-dev.azurewebsites.net` |
| SQL Server | `sql-dorise-dev-eastus` |
| Database | `sqldb-dorise-dev` |
| Key Vault | `kv-dorise-dev-eastus` |
| App Insights | `appi-dorise-dev-eastus` |

### 3.2 Staging

| Component | Value |
|-----------|-------|
| Resource Group | `rg-dorise-stg-eastus` |
| App Service | `app-dorise-api-stg` |
| URL | `https://app-dorise-api-stg.azurewebsites.net` |
| Staging Slot | `https://app-dorise-api-stg-staging.azurewebsites.net` |
| SQL Server | `sql-dorise-stg-eastus` |
| Database | `sqldb-dorise-stg` |
| Key Vault | `kv-dorise-stg-eastus` |
| App Insights | `appi-dorise-stg-eastus` |

### 3.3 Production

| Component | Value |
|-----------|-------|
| Resource Group | `rg-dorise-prod-eastus` |
| App Service | `app-dorise-api-prod` |
| URL | `https://app-dorise-api-prod.azurewebsites.net` |
| Staging Slot | `https://app-dorise-api-prod-staging.azurewebsites.net` |
| SQL Server | `sql-dorise-prod-eastus` |
| Database | `sqldb-dorise-prod` |
| Key Vault | `kv-dorise-prod-eastus` |
| App Insights | `appi-dorise-prod-eastus` |

---

## 4. Deployment Procedures

### 4.1 Automated Deployment (Recommended)

#### Development Deployment

```bash
# Trigger via GitHub Actions
# Option 1: Push to main branch (automatic)
git push origin main

# Option 2: Manual trigger
gh workflow run "CD - Deploy to Dev" --ref main -f reason="Manual deployment"
```

#### Staging Deployment

```bash
# Create a release tag
git tag -a v1.2.3 -m "Release v1.2.3"
git push origin v1.2.3

# Or manual trigger
gh workflow run "CD - Deploy to Staging" -f reason="Staging release" -f skip_swap=true
```

#### Production Deployment

```bash
# Create a GitHub release (triggers automatic deployment)
gh release create v1.2.3 --title "v1.2.3" --notes "Release notes here"

# Or manual trigger (requires approval)
gh workflow run "CD - Deploy to Production" \
  -f version="v1.2.3" \
  -f reason="Scheduled production release"
```

### 4.2 Manual Deployment (Emergency Only)

> ⚠️ **Warning:** Manual deployments should only be used when automated pipelines are unavailable.

#### Prerequisites

```bash
# Install required tools
az --version  # Azure CLI 2.50+
dotnet --version  # .NET 8.0 SDK

# Login to Azure
az login
az account set --subscription "DSIF-Production"
```

#### Build and Publish

```bash
# Clone repository
git clone https://github.com/dorise/sales-incentive.git
cd sales-incentive

# Checkout specific version
git checkout v1.2.3

# Restore and build
dotnet restore src/Dorise.Incentive.sln
dotnet build src/Dorise.Incentive.sln --configuration Release

# Publish
dotnet publish src/Dorise.Incentive.Api/Dorise.Incentive.Api.csproj \
  --configuration Release \
  --output ./publish
```

#### Deploy to Azure

```bash
# Deploy to staging slot first
az webapp deployment source config-zip \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod \
  --slot staging \
  --src ./publish.zip

# Verify staging slot
curl https://app-dorise-api-prod-staging.azurewebsites.net/health

# Swap to production (after verification)
az webapp deployment slot swap \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod \
  --slot staging \
  --target-slot production
```

### 4.3 Database Migrations

```bash
# Generate migration script
dotnet ef migrations script \
  --project src/Dorise.Incentive.Infrastructure \
  --startup-project src/Dorise.Incentive.Api \
  --idempotent \
  --output migrations.sql

# Review the script before execution!

# Apply to production (via Azure Portal or sqlcmd)
sqlcmd -S sql-dorise-prod-eastus.database.windows.net \
  -d sqldb-dorise-prod \
  -U admin_user \
  -P "password" \
  -i migrations.sql
```

### 4.4 Infrastructure Deployment

```bash
# Validate Bicep templates
az bicep build --file infra/main.bicep

# Preview changes (what-if)
az deployment sub what-if \
  --location eastus \
  --template-file infra/main.bicep \
  --parameters infra/parameters/prod.bicepparam

# Deploy infrastructure
az deployment sub create \
  --location eastus \
  --template-file infra/main.bicep \
  --parameters infra/parameters/prod.bicepparam \
  --name "dsif-prod-$(date +%Y%m%d-%H%M%S)"
```

---

## 5. Post-Deployment Validation

### 5.1 Health Checks

```bash
# API Health endpoint
curl -s https://app-dorise-api-prod.azurewebsites.net/health

# Expected response
# {"status":"Healthy","checks":[...]}

# API Info endpoint
curl -s https://app-dorise-api-prod.azurewebsites.net/api/info
```

### 5.2 Smoke Tests

| Test | Endpoint | Expected |
|------|----------|----------|
| Health Check | `/health` | 200 OK |
| API Info | `/api/info` | 200 OK with version |
| API Version | `/api/version` | Deployed version |
| Swagger | `/swagger` | 200 OK |

### 5.3 Functional Verification

```bash
# Test authentication (if applicable)
curl -H "Authorization: Bearer $TOKEN" \
  https://app-dorise-api-prod.azurewebsites.net/api/v1/employees

# Check Application Insights for errors
az monitor app-insights query \
  --app appi-dorise-prod-eastus \
  --analytics-query "exceptions | where timestamp > ago(30m) | count"
```

### 5.4 Monitoring Verification

- [ ] Application Insights showing traffic
- [ ] No new exceptions in last 15 minutes
- [ ] Response times within SLA (<500ms p95)
- [ ] Error rate below threshold (<1%)
- [ ] Database connections healthy

---

## 6. Rollback Procedures

### 6.1 Quick Rollback (Slot Swap)

**Use when:** Issues discovered within 30 minutes of deployment

```bash
# Via GitHub Actions (recommended)
gh workflow run "Rollback - Emergency Rollback" \
  -f environment="prod" \
  -f rollback-method="slot-swap" \
  -f reason="Critical bug in v1.2.3" \
  -f confirm="prod"

# Via Azure CLI (manual)
az webapp deployment slot swap \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod \
  --slot staging \
  --target-slot production
```

### 6.2 Redeploy Previous Version

**Use when:** Slot swap not possible or previous version needed

```bash
# Via GitHub Actions
gh workflow run "CD - Deploy to Production" \
  -f version="v1.2.2" \
  -f reason="Rollback from v1.2.3"

# Via Azure CLI
git checkout v1.2.2
dotnet publish src/Dorise.Incentive.Api/Dorise.Incentive.Api.csproj \
  --configuration Release \
  --output ./publish

az webapp deployment source config-zip \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod \
  --src ./publish.zip
```

### 6.3 Database Rollback

> ⚠️ **Warning:** Database rollbacks are complex and may cause data loss.

```bash
# Option 1: Point-in-time restore
az sql db restore \
  --resource-group rg-dorise-prod-eastus \
  --server sql-dorise-prod-eastus \
  --name sqldb-dorise-prod \
  --dest-name sqldb-dorise-prod-restored \
  --time "2025-01-15T10:00:00Z"

# Option 2: Apply rollback migration
dotnet ef migrations script \
  --project src/Dorise.Incentive.Infrastructure \
  FromMigration ToMigration \
  --output rollback.sql
```

### 6.4 Rollback Decision Matrix

| Scenario | Action | Estimated Time |
|----------|--------|----------------|
| UI bug, API working | Slot swap | 2 minutes |
| API errors, data OK | Slot swap | 2 minutes |
| Database migration issue | Restore + redeploy | 30-60 minutes |
| Data corruption | Point-in-time restore | 60-120 minutes |
| Infrastructure issue | Bicep redeploy | 15-30 minutes |

---

## 7. Troubleshooting

### 7.1 Common Issues

#### Deployment Fails - "Package deployment failed"

```bash
# Check deployment logs
az webapp log deployment show \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod

# Common causes:
# - Package too large (max 500MB for zip deploy)
# - Insufficient disk space
# - File locks from running application
```

#### Application Won't Start

```bash
# Check application logs
az webapp log tail \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod

# Check for missing configuration
az webapp config appsettings list \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod

# Restart application
az webapp restart \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod
```

#### Database Connection Issues

```bash
# Test connectivity
az sql db show-connection-string \
  --server sql-dorise-prod-eastus \
  --name sqldb-dorise-prod \
  --client ado.net

# Check firewall rules
az sql server firewall-rule list \
  --resource-group rg-dorise-prod-eastus \
  --server sql-dorise-prod-eastus
```

#### High Response Times

```bash
# Check App Service metrics
az monitor metrics list \
  --resource /subscriptions/.../resourceGroups/rg-dorise-prod-eastus/providers/Microsoft.Web/sites/app-dorise-api-prod \
  --metric "HttpResponseTime" \
  --interval PT1M

# Scale up if needed
az webapp update \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod \
  --set siteConfig.numberOfWorkers=4
```

### 7.2 Log Locations

| Log Type | Location |
|----------|----------|
| Application Logs | Application Insights / Log Analytics |
| Deployment Logs | Azure Portal > App Service > Deployment Center |
| Platform Logs | Azure Portal > App Service > Diagnose and solve problems |
| Database Logs | Azure Portal > SQL Database > Query Performance Insight |
| GitHub Actions | GitHub > Actions > Workflow Run |

---

## 8. Contacts

### 8.1 On-Call Rotation

| Role | Contact | Hours |
|------|---------|-------|
| Primary On-Call | oncall@dorise.com | 24/7 |
| DevOps Lead | devops-lead@dorise.com | Business Hours |
| DBA | dba@dorise.com | Business Hours |

### 8.2 Escalation Path

```
Level 1: On-Call Engineer (15 min response)
    ↓
Level 2: DevOps Lead (30 min response)
    ↓
Level 3: Engineering Manager (1 hour response)
    ↓
Level 4: VP Engineering (Critical issues only)
```

### 8.3 External Contacts

| Service | Contact | Support Portal |
|---------|---------|----------------|
| Azure Support | Microsoft Premier | https://portal.azure.com/#blade/Microsoft_Azure_Support |
| GitHub | GitHub Enterprise | https://support.github.com |

---

## Appendix A: Quick Reference Commands

```bash
# View deployment status
az webapp show --resource-group rg-dorise-prod-eastus --name app-dorise-api-prod --query state

# List deployment slots
az webapp deployment slot list --resource-group rg-dorise-prod-eastus --name app-dorise-api-prod

# View app settings
az webapp config appsettings list --resource-group rg-dorise-prod-eastus --name app-dorise-api-prod

# Stream logs
az webapp log tail --resource-group rg-dorise-prod-eastus --name app-dorise-api-prod

# Restart app
az webapp restart --resource-group rg-dorise-prod-eastus --name app-dorise-api-prod

# Scale out
az webapp update --resource-group rg-dorise-prod-eastus --name app-dorise-api-prod --set siteConfig.numberOfWorkers=4
```

---

**Document History:**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | Jan 2025 | DevOps Team | Initial version |

*This document is part of the DSIF Operational Runbook series.*
