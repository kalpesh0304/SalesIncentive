# Required Azure Permissions

This document lists the Azure role assignments that must be configured **manually** after infrastructure deployment.

## Overview

Role assignments have been removed from the Bicep templates to:
- Avoid permission errors during CI/CD deployments
- Allow for manual review and approval of access grants
- Support environments with restricted RBAC capabilities

## Required Role Assignments

### 1. App Service Managed Identity → Key Vault

| Setting | Value |
|---------|-------|
| **Scope** | Key Vault resource |
| **Role** | Key Vault Secrets User |
| **Role Definition ID** | `4633458b-17de-408a-b874-0445c86b69e6` |
| **Principal** | App Service System-Assigned Managed Identity |
| **Principal Type** | ServicePrincipal |

**Purpose:** Allows the App Service to read secrets from Key Vault for configuration (connection strings, API keys, etc.)

#### Azure CLI Command

```bash
# Get the App Service principal ID
APP_PRINCIPAL_ID=$(az webapp identity show \
  --name app-dsif-api-{environment} \
  --resource-group rg-dsif-{environment} \
  --query principalId -o tsv)

# Assign Key Vault Secrets User role
az role assignment create \
  --role "Key Vault Secrets User" \
  --assignee-object-id $APP_PRINCIPAL_ID \
  --assignee-principal-type ServicePrincipal \
  --scope /subscriptions/{subscription-id}/resourceGroups/rg-dsif-{environment}/providers/Microsoft.KeyVault/vaults/kv-dsif-{environment}
```

#### Azure Portal Steps

1. Navigate to the Key Vault resource
2. Go to **Access control (IAM)**
3. Click **+ Add** → **Add role assignment**
4. Select role: **Key Vault Secrets User**
5. Select members: Search for the App Service name (e.g., `app-dsif-api-dev`)
6. Click **Review + assign**

---

## Staging Slot Permissions (Non-Dev Environments)

For staging and production environments, the staging deployment slot also requires Key Vault access.

| Setting | Value |
|---------|-------|
| **Scope** | Key Vault resource |
| **Role** | Key Vault Secrets User |
| **Role Definition ID** | `4633458b-17de-408a-b874-0445c86b69e6` |
| **Principal** | Staging Slot System-Assigned Managed Identity |
| **Principal Type** | ServicePrincipal |

#### Azure CLI Command

```bash
# Get the Staging Slot principal ID
SLOT_PRINCIPAL_ID=$(az webapp identity show \
  --name app-dsif-api-{environment} \
  --resource-group rg-dsif-{environment} \
  --slot staging \
  --query principalId -o tsv)

# Assign Key Vault Secrets User role
az role assignment create \
  --role "Key Vault Secrets User" \
  --assignee-object-id $SLOT_PRINCIPAL_ID \
  --assignee-principal-type ServicePrincipal \
  --scope /subscriptions/{subscription-id}/resourceGroups/rg-dsif-{environment}/providers/Microsoft.KeyVault/vaults/kv-dsif-{environment}
```

---

## Environment-Specific Resources

Replace placeholders with actual values:

| Placeholder | Dev | Staging | Production |
|-------------|-----|---------|------------|
| `{environment}` | dev | stg | prod |
| `{subscription-id}` | Your Azure subscription ID | | |

### Resource Naming Convention

| Resource | Name Pattern | Example (Dev) |
|----------|--------------|---------------|
| Resource Group | `rg-dsif-{env}` | `rg-dsif-dev` |
| App Service | `app-dsif-api-{env}` | `app-dsif-api-dev` |
| Key Vault | `kv-dsif-{env}` | `kv-dsif-dev` |

---

## Verification

After assigning permissions, verify access:

```bash
# Test Key Vault access from App Service
az webapp config appsettings list \
  --name app-dsif-api-{environment} \
  --resource-group rg-dsif-{environment}

# Check role assignments on Key Vault
az role assignment list \
  --scope /subscriptions/{subscription-id}/resourceGroups/rg-dsif-{environment}/providers/Microsoft.KeyVault/vaults/kv-dsif-{environment} \
  --output table
```

---

## Troubleshooting

### Error: "Access denied to Key Vault"

1. Verify the managed identity is enabled on the App Service
2. Confirm the role assignment exists and is scoped correctly
3. Check if Key Vault has firewall rules blocking access
4. Ensure the role assignment has propagated (can take up to 5 minutes)

### Error: "Principal not found"

1. Ensure the App Service exists and has a system-assigned managed identity
2. The managed identity is created when the App Service is deployed
3. Check the principal ID matches the App Service identity

---

## Related Documentation

- [Azure RBAC built-in roles](https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles)
- [Key Vault RBAC](https://learn.microsoft.com/en-us/azure/key-vault/general/rbac-guide)
- [App Service Managed Identity](https://learn.microsoft.com/en-us/azure/app-service/overview-managed-identity)
