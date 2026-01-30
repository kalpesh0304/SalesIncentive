# Maintenance Runbook

**Document ID:** RB-MAINT-001
**Project:** Dorise Sales Incentive Framework (DSIF)
**Version:** 1.0
**Last Updated:** January 2025
**Owner:** DevOps Team

> *"Sleep! That's where I'm a Viking!"* - Ralph Wiggum
>
> Scheduled maintenance keeps systems healthy while you sleep like a Viking!

---

## Table of Contents

1. [Overview](#1-overview)
2. [Scheduled Maintenance](#2-scheduled-maintenance)
3. [Database Maintenance](#3-database-maintenance)
4. [Application Maintenance](#4-application-maintenance)
5. [Infrastructure Maintenance](#5-infrastructure-maintenance)
6. [Security Maintenance](#6-security-maintenance)
7. [Backup & Recovery](#7-backup--recovery)
8. [Monitoring & Alerts](#8-monitoring--alerts)

---

## 1. Overview

### 1.1 Purpose

This runbook provides procedures for routine maintenance tasks that keep the DSIF application running optimally. Regular maintenance prevents incidents and ensures system reliability.

### 1.2 Maintenance Windows

| Environment | Window | Frequency |
|-------------|--------|-----------|
| Development | Any time | As needed |
| Staging | Weekdays 6-8 AM UTC | Weekly |
| Production | Sundays 2-6 AM UTC | Monthly |

### 1.3 Maintenance Categories

| Category | Frequency | Impact | Downtime Required |
|----------|-----------|--------|-------------------|
| Database Index Maintenance | Weekly | Low | No |
| Log Cleanup | Daily | None | No |
| Certificate Renewal | As needed | Medium | Minimal |
| Security Patches | Monthly | Medium | Possible |
| Major Upgrades | Quarterly | High | Yes |

---

## 2. Scheduled Maintenance

### 2.1 Daily Tasks

#### Automated (via scheduled-tasks.yml)

| Task | Time (UTC) | Duration |
|------|------------|----------|
| Log rotation | 02:00 | ~5 min |
| Health checks | 02:15 | ~2 min |
| Temp file cleanup | 02:30 | ~5 min |

#### Manual Verification

```bash
# Check health of all environments
for env in dev stg prod; do
  echo "Checking $env..."
  curl -s "https://app-dorise-api-$env.azurewebsites.net/health"
done

# Review overnight alerts
az monitor activity-log list \
  --start-time $(date -d "yesterday" +%Y-%m-%dT00:00:00Z) \
  --end-time $(date +%Y-%m-%dT00:00:00Z) \
  --query "[?level=='Error' || level=='Critical']"
```

### 2.2 Weekly Tasks

#### Monday Morning Review

- [ ] Review weekend alerts and incidents
- [ ] Check disk space usage
- [ ] Review error rates and trends
- [ ] Check certificate expiration dates
- [ ] Review security scan results

```bash
# Check disk space
az webapp show -g rg-dorise-prod-eastus -n app-dorise-api-prod \
  --query siteConfig.detailedErrorLoggingEnabled

# Check SSL certificate expiration
az webapp config ssl list -g rg-dorise-prod-eastus \
  --query "[].{Name:name, Expiration:expirationDate}"

# Check for security vulnerabilities
gh workflow run "Scheduled - Maintenance Tasks" -f task="security-scan"
```

#### Weekly Database Maintenance

```bash
# Run index maintenance (staging first)
gh workflow run "Scheduled - Maintenance Tasks" -f task="dependency-check"
```

### 2.3 Monthly Tasks

#### First Sunday of Month (Production)

**Pre-Maintenance:**
- [ ] Notify stakeholders 1 week in advance
- [ ] Prepare rollback plan
- [ ] Verify backup completeness
- [ ] Schedule change request

**During Maintenance:**
- [ ] Apply security patches
- [ ] Run database maintenance
- [ ] Update dependencies
- [ ] Clear caches
- [ ] Rotate logs

**Post-Maintenance:**
- [ ] Verify all services healthy
- [ ] Run smoke tests
- [ ] Send completion notification

### 2.4 Quarterly Tasks

- [ ] Major version upgrades (.NET, packages)
- [ ] Infrastructure review and right-sizing
- [ ] Disaster recovery test
- [ ] Security audit
- [ ] Performance baseline update

---

## 3. Database Maintenance

### 3.1 Index Maintenance

#### Check Index Health

```sql
-- Check index fragmentation
SELECT
    OBJECT_NAME(ips.object_id) AS TableName,
    i.name AS IndexName,
    ips.avg_fragmentation_in_percent AS Fragmentation,
    ips.page_count AS Pages
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
WHERE ips.avg_fragmentation_in_percent > 10
ORDER BY ips.avg_fragmentation_in_percent DESC;
```

#### Rebuild Indexes

```sql
-- Rebuild highly fragmented indexes (>30%)
ALTER INDEX ALL ON dbo.Employee REBUILD WITH (ONLINE = ON);
ALTER INDEX ALL ON dbo.Calculation REBUILD WITH (ONLINE = ON);
ALTER INDEX ALL ON dbo.IncentivePlan REBUILD WITH (ONLINE = ON);

-- Reorganize moderately fragmented indexes (10-30%)
ALTER INDEX ALL ON dbo.AuditLog REORGANIZE;
```

### 3.2 Statistics Update

```sql
-- Update statistics for all tables
EXEC sp_updatestats;

-- Update specific table statistics
UPDATE STATISTICS dbo.Calculation WITH FULLSCAN;
UPDATE STATISTICS dbo.Employee WITH FULLSCAN;
```

### 3.3 Query Performance

```sql
-- Find slow queries
SELECT TOP 20
    qs.execution_count,
    qs.total_elapsed_time / qs.execution_count AS avg_elapsed_time,
    SUBSTRING(qt.text, (qs.statement_start_offset/2)+1,
        ((CASE qs.statement_end_offset
            WHEN -1 THEN DATALENGTH(qt.text)
            ELSE qs.statement_end_offset
        END - qs.statement_start_offset)/2)+1) AS query_text
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) qt
ORDER BY avg_elapsed_time DESC;

-- Find missing indexes
SELECT
    mig.index_group_handle,
    mid.statement AS table_name,
    mid.equality_columns,
    mid.inequality_columns,
    mid.included_columns,
    migs.user_seeks,
    migs.avg_user_impact
FROM sys.dm_db_missing_index_groups mig
JOIN sys.dm_db_missing_index_group_stats migs ON migs.group_handle = mig.index_group_handle
JOIN sys.dm_db_missing_index_details mid ON mig.index_handle = mid.index_handle
WHERE mid.database_id = DB_ID()
ORDER BY migs.avg_user_impact DESC;
```

### 3.4 Data Cleanup

```sql
-- Archive old audit logs (keep 90 days)
DELETE FROM dbo.AuditLog
WHERE CreatedAt < DATEADD(DAY, -90, GETUTCDATE());

-- Clean up expired sessions
DELETE FROM dbo.UserSession
WHERE ExpiresAt < GETUTCDATE();

-- Archive old calculations (move to archive table)
INSERT INTO dbo.CalculationArchive
SELECT * FROM dbo.Calculation
WHERE CreatedAt < DATEADD(YEAR, -2, GETUTCDATE());

DELETE FROM dbo.Calculation
WHERE CreatedAt < DATEADD(YEAR, -2, GETUTCDATE());
```

### 3.5 Database Size Management

```sql
-- Check database size
SELECT
    DB_NAME() AS DatabaseName,
    SUM(size * 8 / 1024) AS SizeMB,
    SUM(CAST(FILEPROPERTY(name, 'SpaceUsed') AS INT) * 8 / 1024) AS UsedMB
FROM sys.database_files
GROUP BY DB_NAME();

-- Check table sizes
SELECT
    t.name AS TableName,
    s.row_count AS RowCounts,
    SUM(a.total_pages) * 8 AS TotalSpaceKB
FROM sys.tables t
JOIN sys.indexes i ON t.object_id = i.object_id
JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
JOIN sys.allocation_units a ON p.partition_id = a.container_id
JOIN sys.dm_db_partition_stats s ON p.partition_id = s.partition_id
GROUP BY t.name, s.row_count
ORDER BY TotalSpaceKB DESC;
```

---

## 4. Application Maintenance

### 4.1 Cache Management

#### Clear Application Cache

```bash
# Clear Redis cache
az redis force-reboot \
  --resource-group rg-dorise-prod-eastus \
  --name redis-dorise-prod \
  --reboot-type AllNodes

# Or selective cache invalidation via API
curl -X POST "https://app-dorise-api-prod.azurewebsites.net/api/admin/cache/invalidate" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"keys": ["employees:*", "plans:*"]}'
```

#### Cache Warmup

```bash
# Trigger cache warmup
curl -X POST "https://app-dorise-api-prod.azurewebsites.net/api/admin/cache/warmup" \
  -H "Authorization: Bearer $ADMIN_TOKEN"
```

### 4.2 Log Management

#### View Application Logs

```bash
# Stream live logs
az webapp log tail \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod

# Download logs
az webapp log download \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod \
  --log-file logs.zip
```

#### Configure Log Retention

```bash
# Set log retention (days)
az webapp config appsettings set \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod \
  --settings WEBSITE_HTTPLOGGING_RETENTION_DAYS=30
```

### 4.3 Application Restart

```bash
# Graceful restart
az webapp restart \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod

# Hard restart (if graceful fails)
az webapp stop \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod

sleep 10

az webapp start \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod
```

### 4.4 Dependency Updates

```bash
# Check for outdated packages
dotnet list src/Dorise.Incentive.sln package --outdated

# Check for vulnerable packages
dotnet list src/Dorise.Incentive.sln package --vulnerable

# Update packages (test in dev first!)
dotnet add src/Dorise.Incentive.Api/Dorise.Incentive.Api.csproj package PackageName --version X.Y.Z
```

---

## 5. Infrastructure Maintenance

### 5.1 App Service Maintenance

#### Check App Service Health

```bash
# View App Service metrics
az monitor metrics list \
  --resource /subscriptions/xxx/resourceGroups/rg-dorise-prod-eastus/providers/Microsoft.Web/sites/app-dorise-api-prod \
  --metric "CpuPercentage,MemoryPercentage,Http5xx" \
  --interval PT1H

# Check App Service Plan usage
az appservice plan show \
  --resource-group rg-dorise-prod-eastus \
  --name plan-dorise-prod \
  --query "[sku, numberOfWorkers, numberOfSites]"
```

#### Scale App Service

```bash
# Scale out (add instances)
az webapp update \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod \
  --set siteConfig.numberOfWorkers=4

# Scale up (larger instance)
az appservice plan update \
  --resource-group rg-dorise-prod-eastus \
  --name plan-dorise-prod \
  --sku P2V3
```

### 5.2 SQL Database Maintenance

#### Check DTU Usage

```bash
az sql db show \
  --resource-group rg-dorise-prod-eastus \
  --server sql-dorise-prod-eastus \
  --name sqldb-dorise-prod \
  --query "[currentServiceObjectiveName, maxSizeBytes, status]"
```

#### Scale Database

```bash
# Scale up DTU
az sql db update \
  --resource-group rg-dorise-prod-eastus \
  --server sql-dorise-prod-eastus \
  --name sqldb-dorise-prod \
  --service-objective S3
```

### 5.3 Redis Cache Maintenance

```bash
# Check Redis metrics
az redis show \
  --resource-group rg-dorise-prod-eastus \
  --name redis-dorise-prod \
  --query "[hostName, port, sku, provisioningState]"

# Force reboot if needed
az redis force-reboot \
  --resource-group rg-dorise-prod-eastus \
  --name redis-dorise-prod \
  --reboot-type AllNodes
```

### 5.4 Key Vault Maintenance

```bash
# List expiring secrets
az keyvault secret list \
  --vault-name kv-dorise-prod-eastus \
  --query "[?attributes.expires!=null && attributes.expires<'$(date -d '+30 days' +%Y-%m-%dT%H:%M:%SZ)']"

# Rotate secret
az keyvault secret set \
  --vault-name kv-dorise-prod-eastus \
  --name "ConnectionString" \
  --value "new-connection-string"
```

---

## 6. Security Maintenance

### 6.1 Certificate Management

#### Check Certificate Expiration

```bash
# List all certificates
az webapp config ssl list \
  --resource-group rg-dorise-prod-eastus \
  --query "[].{Name:name, Thumbprint:thumbprint, Expiration:expirationDate}"

# Check specific domain
openssl s_client -connect app-dorise-api-prod.azurewebsites.net:443 2>/dev/null | \
  openssl x509 -noout -dates
```

#### Renew Certificate

```bash
# For managed certificates (auto-renewal)
az webapp config ssl bind \
  --certificate-type Managed \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod \
  --hostname api.dorise.com

# For custom certificates
az webapp config ssl upload \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod \
  --certificate-file new-cert.pfx \
  --certificate-password "password"
```

### 6.2 Access Reviews

#### Monthly Access Review Checklist

- [ ] Review Azure AD group memberships
- [ ] Audit Key Vault access policies
- [ ] Check App Service deployment credentials
- [ ] Review SQL Server admin accounts
- [ ] Audit GitHub repository access

```bash
# List Key Vault access policies
az keyvault show \
  --name kv-dorise-prod-eastus \
  --query "properties.accessPolicies"

# List SQL Server admins
az sql server ad-admin list \
  --resource-group rg-dorise-prod-eastus \
  --server sql-dorise-prod-eastus
```

### 6.3 Security Scanning

```bash
# Run security scan via GitHub Actions
gh workflow run "Scheduled - Maintenance Tasks" -f task="security-scan"

# Run local dependency audit
dotnet list src/Dorise.Incentive.sln package --vulnerable
```

### 6.4 Firewall Rules Review

```bash
# List SQL firewall rules
az sql server firewall-rule list \
  --resource-group rg-dorise-prod-eastus \
  --server sql-dorise-prod-eastus

# Remove old/unused rules
az sql server firewall-rule delete \
  --resource-group rg-dorise-prod-eastus \
  --server sql-dorise-prod-eastus \
  --name "old-rule-name"
```

---

## 7. Backup & Recovery

### 7.1 Backup Status

#### Check SQL Database Backups

```bash
# List available restore points
az sql db show \
  --resource-group rg-dorise-prod-eastus \
  --server sql-dorise-prod-eastus \
  --name sqldb-dorise-prod \
  --query "earliestRestoreDate"

# Check backup retention
az sql db show \
  --resource-group rg-dorise-prod-eastus \
  --server sql-dorise-prod-eastus \
  --name sqldb-dorise-prod \
  --query "backupStorageRedundancy"
```

### 7.2 Test Recovery (Quarterly)

```bash
# Create test restore
az sql db restore \
  --resource-group rg-dorise-prod-eastus \
  --server sql-dorise-prod-eastus \
  --name sqldb-dorise-prod \
  --dest-name sqldb-dorise-restore-test \
  --time "$(date -d '1 hour ago' +%Y-%m-%dT%H:%M:%SZ)"

# Verify restore
az sql db show \
  --resource-group rg-dorise-prod-eastus \
  --server sql-dorise-prod-eastus \
  --name sqldb-dorise-restore-test

# Clean up test restore
az sql db delete \
  --resource-group rg-dorise-prod-eastus \
  --server sql-dorise-prod-eastus \
  --name sqldb-dorise-restore-test \
  --yes
```

### 7.3 Geo-Replication Status

```bash
# Check geo-replication
az sql db replica list-links \
  --resource-group rg-dorise-prod-eastus \
  --server sql-dorise-prod-eastus \
  --name sqldb-dorise-prod
```

---

## 8. Monitoring & Alerts

### 8.1 Alert Rules Review

```bash
# List all alert rules
az monitor metrics alert list \
  --resource-group rg-dorise-prod-eastus \
  --query "[].{Name:name, Enabled:enabled, Severity:severity}"
```

### 8.2 Dashboard Review

Monthly review of Azure Dashboard:
- [ ] Response time trends
- [ ] Error rate trends
- [ ] Resource utilization
- [ ] User activity patterns

### 8.3 Log Analytics Queries

```kusto
// Check for repeated errors
exceptions
| where timestamp > ago(7d)
| summarize count() by problemId
| where count_ > 100
| order by count_ desc

// Check database performance trends
dependencies
| where timestamp > ago(7d)
| where type == "SQL"
| summarize avg(duration), percentile(duration, 95) by bin(timestamp, 1d)
| render timechart

// Check request volume trends
requests
| where timestamp > ago(30d)
| summarize count() by bin(timestamp, 1d)
| render timechart
```

---

## Appendix A: Maintenance Calendar

### Annual Schedule

| Month | Week | Activity |
|-------|------|----------|
| January | 1 | Annual security audit |
| January | 2 | .NET version review |
| April | 1 | Q1 DR test |
| July | 1 | Q2 DR test, Mid-year review |
| October | 1 | Q3 DR test |
| December | 2 | Year-end maintenance freeze prep |

### Monthly Schedule

| Week | Day | Activity |
|------|-----|----------|
| 1 | Sunday | Production maintenance window |
| 1 | Monday | Monthly review meeting |
| 2 | Wednesday | Security patch review |
| 3 | Friday | Dependency update review |
| 4 | Friday | Infrastructure cost review |

---

## Appendix B: Quick Reference Commands

```bash
# Health check all environments
for env in dev stg prod; do curl -s "https://app-dorise-api-$env.azurewebsites.net/health"; done

# Restart production
az webapp restart -g rg-dorise-prod-eastus -n app-dorise-api-prod

# Check production logs
az webapp log tail -g rg-dorise-prod-eastus -n app-dorise-api-prod

# Scale out production
az webapp update -g rg-dorise-prod-eastus -n app-dorise-api-prod --set siteConfig.numberOfWorkers=4

# Clear Redis cache
az redis force-reboot -g rg-dorise-prod-eastus -n redis-dorise-prod --reboot-type AllNodes

# Check certificate expiration
az webapp config ssl list -g rg-dorise-prod-eastus --query "[].expirationDate"
```

---

**Document History:**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | Jan 2025 | DevOps Team | Initial version |

*This document is part of the DSIF Operational Runbook series.*
