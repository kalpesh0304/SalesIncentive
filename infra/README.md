# Infrastructure Documentation

**Project:** Dorise Sales Incentive Framework (DSIF)
**Gate:** QG-4 (Infrastructure)

> *"I choo-choo-choose you!"* - Ralph Wiggum
>
> We choo-choo-choose Azure for our cloud infrastructure - reliable, scalable, and enterprise-ready!

---

## Overview

This folder contains all Infrastructure as Code (IaC) artifacts for the DSIF project, implemented using Azure Bicep templates.

---

## Folder Structure

```
infra/
├── main.bicep                    # Main orchestration template
├── modules/                      # Reusable Bicep modules
│   ├── appService.bicep          # App Service resources
│   ├── keyVault.bicep            # Key Vault resources
│   ├── monitoring.bicep          # Monitoring resources
│   ├── sqlDatabase.bicep         # SQL Database resources
│   └── storage.bicep             # Storage Account resources
├── parameters/                   # Environment-specific parameters
│   ├── dev.bicepparam            # Development parameters
│   ├── stg.bicepparam            # Staging parameters
│   └── prod.bicepparam           # Production parameters
└── README.md                     # This file
```

---

## Azure Resources

### Per Environment

| Resource | Dev Name | Stg Name | Prod Name |
|----------|----------|----------|-----------|
| Resource Group | rg-dorise-dev-eastus | rg-dorise-stg-eastus | rg-dorise-prod-eastus |
| App Service Plan | asp-dorise-dev | asp-dorise-stg | asp-dorise-prod |
| App Service | app-dorise-api-dev | app-dorise-api-stg | app-dorise-api-prod |
| SQL Server | sql-dorise-dev | sql-dorise-stg | sql-dorise-prod |
| SQL Database | sqldb-dorise-dev | sqldb-dorise-stg | sqldb-dorise-prod |
| Key Vault | kv-dorise-dev | kv-dorise-stg | kv-dorise-prod |
| App Insights | appi-dorise-dev | appi-dorise-stg | appi-dorise-prod |
| Log Analytics | log-dorise-dev | log-dorise-stg | log-dorise-prod |
| Storage Account | stdorisedev | stdorisestg | stdoriseprod |

### SKU Configuration

| Resource | Development | Staging | Production |
|----------|-------------|---------|------------|
| App Service Plan | B1 | S1 | P1v3 |
| SQL Database | Basic (5 DTU) | S1 (20 DTU) | S2 (50 DTU) |
| Storage Redundancy | LRS | GRS | GRS |
| Key Vault | Standard | Standard | Premium |

---

## Deployment

### Prerequisites

1. **Azure CLI** installed and configured
2. **Bicep CLI** (comes with Azure CLI 2.20+)
3. **Azure Subscription** with appropriate permissions
4. **Service Principal** for CI/CD (stored in GitHub Secrets)

### Manual Deployment

```bash
# Login to Azure
az login

# Set subscription
az account set --subscription "<subscription-name>"

# Validate templates
az bicep build --file infra/main.bicep

# Deploy to development
az deployment sub create \
  --location eastus \
  --template-file infra/main.bicep \
  --parameters infra/parameters/dev.bicepparam \
  --parameters sqlAdminLogin=<admin-login> \
  --parameters sqlAdminPassword=<admin-password>

# Deploy to staging
az deployment sub create \
  --location eastus \
  --template-file infra/main.bicep \
  --parameters infra/parameters/stg.bicepparam \
  --parameters sqlAdminLogin=<admin-login> \
  --parameters sqlAdminPassword=<admin-password>

# Deploy to production
az deployment sub create \
  --location eastus \
  --template-file infra/main.bicep \
  --parameters infra/parameters/prod.bicepparam \
  --parameters sqlAdminLogin=<admin-login> \
  --parameters sqlAdminPassword=<admin-password>
```

### What-If Preview

Always run a what-if preview before deploying:

```bash
az deployment sub what-if \
  --location eastus \
  --template-file infra/main.bicep \
  --parameters infra/parameters/dev.bicepparam \
  --parameters sqlAdminLogin=<admin-login> \
  --parameters sqlAdminPassword=<admin-password>
```

---

## CI/CD Pipelines

### GitHub Actions Workflows

| Workflow | File | Description | Trigger |
|----------|------|-------------|---------|
| CI | `.github/workflows/ci.yml` | Build, test, validate | Push, PR |
| CD Dev | `.github/workflows/cd-dev.yml` | Deploy to dev | CI success |
| CD Staging | `.github/workflows/cd-stg.yml` | Deploy to staging | Manual, tag |
| Infrastructure | `.github/workflows/infra.yml` | Deploy Azure resources | Manual |

### Required GitHub Secrets

| Secret | Description |
|--------|-------------|
| `AZURE_CREDENTIALS` | Service principal credentials (JSON) |
| `SQL_ADMIN_LOGIN` | SQL Server admin username |
| `SQL_ADMIN_PASSWORD` | SQL Server admin password |

### Creating Azure Credentials

```bash
# Create service principal
az ad sp create-for-rbac \
  --name "sp-dsif-github" \
  --role contributor \
  --scopes /subscriptions/<subscription-id> \
  --sdk-auth

# Copy the JSON output to GitHub Secrets as AZURE_CREDENTIALS
```

---

## Module Documentation

### main.bicep

The main orchestration template that deploys all resources.

**Parameters:**
- `environment`: Target environment (dev, stg, prod)
- `location`: Azure region (default: eastus)
- `projectName`: Project name for naming (default: dorise)
- `sqlAdminLogin`: SQL admin username (secure)
- `sqlAdminPassword`: SQL admin password (secure)
- `tags`: Resource tags

**Outputs:**
- Resource names and connection strings

### modules/appService.bicep

Deploys App Service Plan and Web App.

**Features:**
- .NET 8.0 Linux runtime
- System-assigned managed identity
- Application Insights integration
- Key Vault integration
- Staging slot (non-dev environments)
- Diagnostic logging

### modules/sqlDatabase.bicep

Deploys Azure SQL Server and Database.

**Features:**
- SQL Server with Azure AD authentication
- Database with appropriate SKU per environment
- Firewall rules (Azure services, dev all IPs)
- Auditing enabled
- Connection string stored in Key Vault

### modules/keyVault.bicep

Deploys Azure Key Vault.

**Features:**
- RBAC authorization
- Soft delete enabled
- Purge protection enabled
- Diagnostic logging

### modules/monitoring.bicep

Deploys monitoring resources.

**Features:**
- Log Analytics Workspace
- Application Insights (workspace-based)
- Configurable retention

### modules/storage.bicep

Deploys Azure Storage Account.

**Features:**
- Blob containers (exports, imports, backups)
- TLS 1.2 enforced
- Soft delete enabled
- Diagnostic logging

---

## Tagging Standard

All resources are tagged with:

```json
{
  "Project": "Dorise-Incentive",
  "Environment": "<dev|stg|prod>",
  "Owner": "DSIF-Team",
  "CostCenter": "CC-12345",
  "Application": "DSIF",
  "ManagedBy": "Bicep"
}
```

---

## Security Considerations

> *"My cat's breath smells like cat food."* - Ralph Wiggum
>
> Our security is configured exactly as expected - no surprises!

### Implemented Controls

| Control | Implementation |
|---------|----------------|
| Encryption at Rest | Enabled on SQL, Storage, Key Vault |
| Encryption in Transit | TLS 1.2 enforced |
| Managed Identity | App Service to Key Vault/SQL |
| RBAC | Key Vault uses RBAC authorization |
| Network | Azure Services bypass on firewalls |
| Secrets | Stored in Key Vault |
| Logging | All resources log to Log Analytics |

### Post-Deployment Security Tasks

1. [ ] Configure Azure AD authentication for SQL
2. [ ] Set up private endpoints (production)
3. [ ] Configure WAF (production)
4. [ ] Review firewall rules
5. [ ] Set up alerts

---

## Troubleshooting

### Common Issues

**Deployment fails with "ResourceNotFound"**
- Ensure the subscription is correct
- Check that the resource group exists

**"Key Vault already exists"**
- Key Vault names are globally unique
- Soft-deleted vaults must be purged first

**SQL connection fails**
- Check firewall rules
- Verify managed identity permissions

### Useful Commands

```bash
# List all resources in a resource group
az resource list --resource-group rg-dorise-dev-eastus --output table

# Check deployment status
az deployment sub list --output table

# View deployment errors
az deployment sub show --name <deployment-name> --query properties.error

# Purge deleted Key Vault
az keyvault purge --name kv-dorise-dev
```

---

## Related Documents

- `/docs/architecture/SOLUTION_ARCHITECTURE.md` - Solution design
- `/docs/architecture/SECURITY_ARCHITECTURE.md` - Security controls
- `/docs/gates/QG-4-CHECKLIST.md` - Infrastructure gate checklist
- `/GOVERNANCE.md` - Naming conventions and standards

---

## Next Steps

1. ✅ Create Bicep templates
2. ✅ Create CI/CD pipelines
3. ⏳ Provision Azure resources
4. ⏳ Deploy database schema
5. ⏳ Configure secrets in Key Vault
6. ⏳ Verify end-to-end deployment

---

*This documentation is part of the DSIF QG-4 (Infrastructure Gate) deliverables.*
