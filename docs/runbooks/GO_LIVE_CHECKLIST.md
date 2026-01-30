# Go-Live Checklist

**Document ID:** RB-GOLIVE-001
**Project:** Dorise Sales Incentive Framework (DSIF)
**Version:** 1.0
**Last Updated:** January 2025
**Owner:** Product Owner + DevOps Team

> *"When I grow up, I want to be a principal or a caterpillar."* - Ralph Wiggum
>
> When DSIF grows up (goes live), it will be a production-ready enterprise application!

---

## Table of Contents

1. [Overview](#1-overview)
2. [Pre-Go-Live Checklist](#2-pre-go-live-checklist)
3. [Go-Live Day Checklist](#3-go-live-day-checklist)
4. [Post-Go-Live Checklist](#4-post-go-live-checklist)
5. [Rollback Plan](#5-rollback-plan)
6. [Communication Plan](#6-communication-plan)
7. [Support Readiness](#7-support-readiness)

---

## 1. Overview

### 1.1 Purpose

This checklist ensures a smooth and successful go-live for the DSIF application. It covers all aspects from technical readiness to organizational preparation.

### 1.2 Go-Live Timeline

| Phase | Timing | Duration |
|-------|--------|----------|
| Pre-Go-Live Preparation | T-7 days | 1 week |
| Final Readiness Check | T-1 day | 1 day |
| Go-Live Execution | T-0 | 4-8 hours |
| Hypercare | T+1 to T+14 | 2 weeks |
| Stabilization | T+15 to T+30 | 2 weeks |

### 1.3 Go-Live Team

| Role | Responsibility | Primary Contact |
|------|----------------|-----------------|
| Go-Live Manager | Overall coordination | [TBD] |
| Technical Lead | Technical decisions | [TBD] |
| DevOps Lead | Infrastructure & deployment | [TBD] |
| DBA | Database operations | [TBD] |
| QA Lead | Testing verification | [TBD] |
| Support Lead | User support | [TBD] |
| Business Owner | Business sign-off | [TBD] |

---

## 2. Pre-Go-Live Checklist

### 2.1 Infrastructure Readiness (T-7 days)

| # | Item | Status | Owner | Notes |
|---|------|--------|-------|-------|
| 1 | Production Azure resources provisioned | ⬜ | DevOps | Run infra deployment |
| 2 | Production database created and configured | ⬜ | DBA | Apply security settings |
| 3 | SSL/TLS certificates installed | ⬜ | DevOps | Verify expiration dates |
| 4 | DNS records configured | ⬜ | DevOps | A records, CNAME |
| 5 | WAF rules configured and tested | ⬜ | DevOps | Review blocked requests |
| 6 | Backup and recovery tested | ⬜ | DBA | Document RTO/RPO |
| 7 | Monitoring and alerting configured | ⬜ | DevOps | Verify alert recipients |
| 8 | Log retention configured | ⬜ | DevOps | 90 days minimum |

### 2.2 Application Readiness (T-7 days)

| # | Item | Status | Owner | Notes |
|---|------|--------|-------|-------|
| 1 | All features developed and tested | ⬜ | Dev Team | Sprint backlog complete |
| 2 | UAT completed and signed off | ⬜ | QA/Business | Sign-off document |
| 3 | Performance testing passed | ⬜ | QA | Load test results |
| 4 | Security scan completed | ⬜ | Security | No critical findings |
| 5 | API documentation complete | ⬜ | Dev Team | Swagger/OpenAPI |
| 6 | Error messages reviewed | ⬜ | UX | User-friendly messages |
| 7 | Feature flags configured | ⬜ | Dev Team | Kill switches ready |

### 2.3 Data Readiness (T-5 days)

| # | Item | Status | Owner | Notes |
|---|------|--------|-------|-------|
| 1 | Data migration scripts tested | ⬜ | DBA | In staging environment |
| 2 | Data validation rules defined | ⬜ | Business | Acceptance criteria |
| 3 | Reference data loaded | ⬜ | DBA | Departments, roles, etc. |
| 4 | Test data cleaned | ⬜ | DBA | No test data in prod |
| 5 | Data backup before migration | ⬜ | DBA | Full backup |

### 2.4 Security Readiness (T-5 days)

| # | Item | Status | Owner | Notes |
|---|------|--------|-------|-------|
| 1 | Production secrets in Key Vault | ⬜ | DevOps | No secrets in code |
| 2 | Azure AD integration configured | ⬜ | DevOps | SSO working |
| 3 | RBAC roles assigned | ⬜ | Admin | User access reviewed |
| 4 | Network security rules verified | ⬜ | DevOps | NSG, firewall |
| 5 | Penetration test completed | ⬜ | Security | Report reviewed |

### 2.5 Organizational Readiness (T-3 days)

| # | Item | Status | Owner | Notes |
|---|------|--------|-------|-------|
| 1 | User training completed | ⬜ | Training | All user groups |
| 2 | Support team trained | ⬜ | Support | Runbooks reviewed |
| 3 | User documentation available | ⬜ | Docs | Help guides ready |
| 4 | Communication sent to users | ⬜ | Comms | Go-live notification |
| 5 | Escalation path confirmed | ⬜ | Support | On-call schedule |

---

## 3. Go-Live Day Checklist

### 3.1 Pre-Deployment (T-0, Morning)

| Time | Task | Status | Owner |
|------|------|--------|-------|
| 06:00 | Go-Live team standup | ⬜ | Go-Live Manager |
| 06:15 | Verify all systems healthy | ⬜ | DevOps |
| 06:30 | Final backup of staging data | ⬜ | DBA |
| 06:45 | Communication: Maintenance window starting | ⬜ | Comms |
| 07:00 | Begin maintenance window | ⬜ | DevOps |

### 3.2 Deployment (T-0, Deployment Window)

| Time | Task | Status | Owner |
|------|------|--------|-------|
| 07:00 | Deploy application to production | ⬜ | DevOps |
| 07:30 | Run database migrations | ⬜ | DBA |
| 08:00 | Execute data migration | ⬜ | DBA |
| 08:30 | Verify data migration | ⬜ | DBA |
| 09:00 | Run smoke tests | ⬜ | QA |
| 09:30 | Verify integrations | ⬜ | Dev Team |

### 3.3 Verification (T-0, Post-Deployment)

| Time | Task | Status | Owner |
|------|------|--------|-------|
| 10:00 | Health checks passing | ⬜ | DevOps |
| 10:15 | Core functionality verified | ⬜ | QA |
| 10:30 | Performance baseline captured | ⬜ | DevOps |
| 10:45 | Monitoring dashboards verified | ⬜ | DevOps |
| 11:00 | Business validation complete | ⬜ | Business |

### 3.4 Go-Live Decision

| Time | Task | Status | Owner |
|------|------|--------|-------|
| 11:30 | Go/No-Go decision meeting | ⬜ | Go-Live Manager |
| 11:45 | Sign-off from all stakeholders | ⬜ | All Leads |
| 12:00 | Communication: System live | ⬜ | Comms |

### 3.5 Post-Go-Live (T-0, Afternoon)

| Time | Task | Status | Owner |
|------|------|--------|-------|
| 12:00 | Enable user access | ⬜ | Admin |
| 12:30 | Monitor initial user activity | ⬜ | Support |
| 14:00 | First user feedback review | ⬜ | Support |
| 16:00 | End of day status meeting | ⬜ | Go-Live Manager |
| 17:00 | Handover to on-call team | ⬜ | DevOps |

---

## 4. Post-Go-Live Checklist

### 4.1 Day 1 (T+1)

| # | Item | Status | Owner |
|---|------|--------|-------|
| 1 | Review overnight alerts | ⬜ | DevOps |
| 2 | Check error rates | ⬜ | DevOps |
| 3 | Review user feedback | ⬜ | Support |
| 4 | Fix critical issues | ⬜ | Dev Team |
| 5 | Daily standup | ⬜ | Go-Live Manager |

### 4.2 Week 1 (Hypercare)

| # | Item | Status | Owner |
|---|------|--------|-------|
| 1 | Daily status meetings | ⬜ | Go-Live Manager |
| 2 | Monitor performance trends | ⬜ | DevOps |
| 3 | Address user issues | ⬜ | Support |
| 4 | Document lessons learned | ⬜ | All |
| 5 | Weekly stakeholder update | ⬜ | Go-Live Manager |

### 4.3 Week 2-4 (Stabilization)

| # | Item | Status | Owner |
|---|------|--------|-------|
| 1 | Reduce to weekly meetings | ⬜ | Go-Live Manager |
| 2 | Optimize based on metrics | ⬜ | Dev Team |
| 3 | Complete documentation | ⬜ | All |
| 4 | Conduct retrospective | ⬜ | All |
| 5 | Formal project closure | ⬜ | Go-Live Manager |

---

## 5. Rollback Plan

### 5.1 Rollback Decision Criteria

Rollback should be considered if:
- Critical business functionality is broken
- Data integrity issues discovered
- Security vulnerability exposed
- Performance is unacceptable (>5x baseline)
- More than 25% of users cannot work

### 5.2 Rollback Procedure

#### Quick Rollback (Within 2 hours of deployment)

```bash
# 1. Announce rollback
echo "Initiating rollback to previous version"

# 2. Swap slots (instant rollback)
az webapp deployment slot swap \
  --resource-group rg-dorise-prod-eastus \
  --name app-dorise-api-prod \
  --slot staging \
  --target-slot production

# 3. Verify health
curl https://app-dorise-api-prod.azurewebsites.net/health

# 4. Announce completion
echo "Rollback complete"
```

#### Database Rollback (If data migration ran)

```bash
# 1. Stop application
az webapp stop -g rg-dorise-prod-eastus -n app-dorise-api-prod

# 2. Restore database from pre-migration backup
az sql db restore \
  --resource-group rg-dorise-prod-eastus \
  --server sql-dorise-prod-eastus \
  --name sqldb-dorise-prod \
  --dest-name sqldb-dorise-prod-restored \
  --time "2025-01-15T06:00:00Z"  # Pre-migration time

# 3. Update connection string to restored DB

# 4. Restart application
az webapp start -g rg-dorise-prod-eastus -n app-dorise-api-prod
```

### 5.3 Rollback Communication

```
Subject: DSIF - Service Rollback in Progress

Dear Users,

We have identified an issue with the new release and are rolling back
to the previous version to ensure service stability.

Expected downtime: 15-30 minutes
We will notify you when the service is restored.

We apologize for any inconvenience.

DSIF Operations Team
```

---

## 6. Communication Plan

### 6.1 Stakeholder Communication Schedule

| When | What | Audience | Channel |
|------|------|----------|---------|
| T-7 | Go-live announcement | All users | Email |
| T-3 | Reminder + training links | All users | Email |
| T-1 | Final reminder | All users | Email |
| T-0 (start) | Maintenance starting | All users | Email + Status page |
| T-0 (end) | System live | All users | Email + Status page |
| T+1 | Day 1 status | Stakeholders | Email |
| T+7 | Week 1 summary | Stakeholders | Email |

### 6.2 Communication Templates

#### Go-Live Announcement (T-7)

```
Subject: DSIF Go-Live Scheduled for [DATE]

Dear Team,

We are pleased to announce that the Dorise Sales Incentive Framework (DSIF)
will go live on [DATE].

Key Information:
- Go-Live Date: [DATE]
- Maintenance Window: 07:00 - 12:00 UTC
- System URL: https://dsif.dorise.com

What to Expect:
- Brief system unavailability during maintenance window
- New features: [List key features]
- Training resources: [Link to training]

Questions?
Contact the DSIF Support team at dsif-support@dorise.com

Thank you,
DSIF Team
```

#### System Live Notification

```
Subject: DSIF is Now Live!

Dear Team,

The Dorise Sales Incentive Framework is now live and available at:
https://dsif.dorise.com

Getting Started:
1. Log in using your company credentials (SSO)
2. Review the quick start guide: [Link]
3. Contact support for any issues: dsif-support@dorise.com

Known Limitations:
- [List any known issues]

Thank you for your patience during this transition.

DSIF Team
```

---

## 7. Support Readiness

### 7.1 Support Team Schedule (Hypercare)

| Day | Hours (UTC) | Primary | Backup |
|-----|-------------|---------|--------|
| Mon-Fri | 06:00-18:00 | Support Team | Dev Team |
| Mon-Fri | 18:00-06:00 | On-Call | DevOps |
| Sat-Sun | All day | On-Call | DevOps |

### 7.2 Issue Escalation Matrix

| Severity | Response Time | Escalation Path |
|----------|---------------|-----------------|
| Critical | 15 min | Support → DevOps → CTO |
| High | 1 hour | Support → Dev Team Lead |
| Medium | 4 hours | Support → Dev Team |
| Low | 24 hours | Support |

### 7.3 Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| Cannot log in | Verify Azure AD access, clear browser cache |
| Slow performance | Check user's network, escalate if widespread |
| Calculation error | Verify input data, check calculation logs |
| Missing data | Check data migration status, verify permissions |
| Report not loading | Check browser console, try different browser |

### 7.4 Support Contacts

| Contact | Purpose | Availability |
|---------|---------|--------------|
| dsif-support@dorise.com | General support | Business hours |
| dsif-oncall@dorise.com | Critical issues | 24/7 |
| #dsif-support (Slack) | Quick questions | Business hours |

---

## Appendix A: Sign-Off Sheet

### Go-Live Approval

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Business Owner | | | |
| Technical Lead | | | |
| QA Lead | | | |
| DevOps Lead | | | |
| Security Lead | | | |
| Go-Live Manager | | | |

### Go-Live Decision

- [ ] **GO** - Proceed with go-live
- [ ] **NO-GO** - Postpone go-live

**Decision Date/Time:** ____________________

**Notes:**
_________________________________
_________________________________
_________________________________

---

**Document History:**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | Jan 2025 | Go-Live Team | Initial version |

*This document is part of the DSIF Operational Runbook series.*
