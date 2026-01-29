# Dorise Sales Incentive Framework (DSIF)

[![Build Status](https://github.com/dorise/sales-incentive/workflows/CI/badge.svg)](https://github.com/dorise/sales-incentive/actions)
[![Azure](https://img.shields.io/badge/Azure-Deployed-blue)](https://azure.microsoft.com)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com)

**An enterprise-grade sales incentive calculation and management system built on Microsoft Azure.**

---

## 📋 Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Documentation](#documentation)
- [Development Workflow](#development-workflow)
- [Quality Gates](#quality-gates)
- [Contributing](#contributing)
- [Support](#support)

---

## Overview

The **Dorise Sales Incentive Framework (DSIF)** is a cloud-native application designed to automate the calculation, management, and governance of sales incentives. It replaces manual, error-prone processes with an auditable, configurable, and integration-ready solution.

### Business Problem

| Challenge | Impact |
|-----------|--------|
| No Audit Trail | Cannot verify historical calculations; compliance risk |
| Formula Replication | Calculation logic duplicated across systems |
| Manual Governance | No tracking of approvals, payments, or amendments |
| Integration Roadblocks | Cannot automate data posting to external systems |
| Audit & Integrity Risks | No immutable records; point-in-time values not captured |

### Solution

DSIF provides:
- **Automated Calculations** - Configurable incentive plans with threshold-based slabs
- **Immutable Audit Trail** - Every calculation, approval, and amendment is recorded
- **Self-Service Configuration** - Business users can modify parameters without IT intervention
- **Integration-Ready APIs** - RESTful APIs for external system connectivity
- **Historical Accuracy** - Point-in-time snapshots for any historical period

---

## Key Features

### 👥 Employee Management
- Hierarchical employee structure (Company → Zone → Store → Employee)
- Role and designation management
- Split incentive shares between employees
- Assignment history with effective dates

### 📊 Incentive Configuration
- Configurable incentive plans by role
- Threshold-based slabs (sales volume, production, sell-through rate)
- Formula modifications through configuration
- Version history for all plan changes

### 🧮 Calculation Engine
- Automated batch calculations (daily/weekly/monthly)
- Point-in-time value capture
- Immutable calculation records
- Manual recalculation with amendment tracking

### ✅ Approval Workflow
- Multi-level approval workflow
- Status tracking (Pending, Approved, Rejected, On Hold)
- Full audit trail with approver details and timestamps

### 📈 Reporting & Visualization
- Executive dashboards with key metrics
- Historical incentive record viewing
- Export functionality (Excel, PDF)
- Drill-down from summary to detail

---

## Technology Stack

### Core Technologies

| Component | Technology | Version |
|-----------|------------|---------|
| Runtime | .NET | 8.0 |
| Language | C# | 12 |
| Framework | ASP.NET Core | 8.0 |
| ORM | Entity Framework Core | 8.0 |
| Serverless | Azure Functions | 4.x (Isolated) |

### Azure Services

| Service | Purpose |
|---------|---------|
| Azure App Service | Web API hosting |
| Azure SQL Database | Relational data storage |
| Azure Functions | Background processing |
| Azure Entra ID | Authentication & SSO |
| Azure Key Vault | Secrets management |
| Azure Blob Storage | File storage |
| Application Insights | Monitoring & diagnostics |
| Azure Cache for Redis | Session & data caching |

### Development Tools

| Tool | Purpose |
|------|---------|
| Visual Studio / VS Code | IDE |
| Azure CLI | Azure management |
| Git | Version control |
| GitHub Actions | CI/CD pipelines |
| Bicep | Infrastructure as Code |

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         PRESENTATION                            │
│              Blazor Web Frontend / React SPA                    │
├─────────────────────────────────────────────────────────────────┤
│                           API LAYER                             │
│                    .NET 8 Web API (REST)                        │
│     Employee | Incentive | Calculation | Approval | Reporting   │
├─────────────────────────────────────────────────────────────────┤
│                        SERVICE LAYER                            │
│              Azure Functions (Scheduled Jobs)                   │
├─────────────────────────────────────────────────────────────────┤
│                      PERSISTENCE LAYER                          │
│           Azure SQL Database | Blob Storage | Redis             │
├─────────────────────────────────────────────────────────────────┤
│                       SECURITY LAYER                            │
│          Entra ID | Key Vault | Managed Identity                │
└─────────────────────────────────────────────────────────────────┘
```

---

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)
- Azure subscription with appropriate permissions

### Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/dorise/sales-incentive.git
   cd sales-incentive
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore src/Dorise.Incentive.sln
   ```

3. **Configure local settings**
   ```bash
   cp src/Dorise.Incentive.Api/appsettings.Development.json.example \
      src/Dorise.Incentive.Api/appsettings.Development.json
   ```
   Update the connection strings and Azure settings.

4. **Run database migrations**
   ```bash
   dotnet ef database update --project src/Dorise.Incentive.Infrastructure
   ```

5. **Start the application**
   ```bash
   dotnet run --project src/Dorise.Incentive.Api
   ```

6. **Access the API**
   - Swagger UI: `https://localhost:5001/swagger`
   - Health Check: `https://localhost:5001/health`

### Deploy to Azure

```bash
# Login to Azure
az login

# Deploy infrastructure
az deployment group create \
  -g rg-dorise-dev-eastus \
  -f infra/bicep/main.bicep \
  -p infra/bicep/parameters/dev.bicepparam

# Deploy application (via GitHub Actions or manually)
dotnet publish src/Dorise.Incentive.Api -c Release -o ./publish
az webapp deploy --resource-group rg-dorise-dev-eastus \
  --name app-dorise-api-dev --src-path ./publish
```

---

## Project Structure

```
dorise-sales-incentive/
├── src/
│   ├── Dorise.Incentive.Api/           # ASP.NET Core Web API
│   ├── Dorise.Incentive.Core/          # Business logic & domain
│   ├── Dorise.Incentive.Infrastructure/# Data access & external services
│   ├── Dorise.Incentive.Functions/     # Azure Functions
│   └── Dorise.Incentive.Shared/        # Shared DTOs & utilities
│
├── tests/
│   ├── Dorise.Incentive.UnitTests/
│   ├── Dorise.Incentive.IntegrationTests/
│   └── Dorise.Incentive.E2ETests/
│
├── infra/
│   ├── bicep/                          # Infrastructure as Code
│   │   ├── main.bicep
│   │   ├── modules/
│   │   └── parameters/
│   └── scripts/
│
├── docs/
│   ├── architecture/                   # Architecture documents
│   ├── functional/                     # Requirements & user stories
│   ├── technical/                      # API specs & technical docs
│   ├── design/                         # RACI, decisions, tracking
│   ├── testing/                        # Test strategy & cases
│   ├── runbooks/                       # Operational procedures
│   └── gates/                          # Quality gate checklists
│
├── .github/workflows/                  # CI/CD pipelines
├── CLAUDE.md                           # AI assistant instructions
├── GATE_STATUS.md                      # Quality gate tracker
├── GOVERNANCE.md                       # Project governance
└── README.md                           # This file
```

---

## Documentation

| Document | Description | Location |
|----------|-------------|----------|
| **GOVERNANCE.md** | Project governance, naming conventions, team roles | `/GOVERNANCE.md` |
| **CLAUDE.md** | AI-assisted development guidelines | `/CLAUDE.md` |
| **GATE_STATUS.md** | Quality gate status tracker | `/GATE_STATUS.md` |
| **Requirements** | Functional & non-functional requirements | `/docs/functional/` |
| **Architecture** | Solution, data, and security architecture | `/docs/architecture/` |
| **API Design** | REST API specifications | `/docs/technical/API_DESIGN.md` |
| **Runbooks** | Deployment and incident response | `/docs/runbooks/` |

---

## Development Workflow

### Branch Strategy

| Branch | Purpose |
|--------|---------|
| `main` | Production-ready code |
| `develop` | Integration branch |
| `feature/*` | New features |
| `bugfix/*` | Bug fixes |
| `release/*` | Release preparation |

### Commit Convention

```
<type>(<scope>): <subject>

Types: feat, fix, docs, style, refactor, test, chore
Scope: api, core, infra, functions, web
```

Example: `feat(api): add employee assignment endpoint`

### Pull Request Process

1. Create feature branch from `develop`
2. Implement changes with tests
3. Ensure CI pipeline passes
4. Request code review
5. Merge after approval

---

## Quality Gates

This project enforces **mandatory quality gates** before proceeding to each phase.

| Gate | Phase | Owner | Key Deliverables |
|------|-------|-------|------------------|
| QG-0 | Foundation | Project Manager | README, GOVERNANCE, RACI |
| QG-1 | Requirements | Product Owner | Requirements, User Stories |
| QG-2 | Architecture | Solution Architect | Architecture Docs, API Design |
| QG-3 | Technical | Lead Developer | CLAUDE.md, Technical Specs |
| QG-4 | Infrastructure | DevOps Engineer | Bicep templates, CI/CD |
| QG-5 | Testing | QA Lead | Test Strategy, 80% Coverage |
| QG-6 | Operations | DevOps + PO | Runbooks, UAT Sign-off |

**Current Status:** Check [GATE_STATUS.md](./GATE_STATUS.md) for the latest gate status.

> ⚠️ **Note:** Code generation and phase work is blocked until the required quality gate is passed.

---

## Contributing

We welcome contributions! Please follow these guidelines:

1. **Read the governance document** - Understand naming conventions and standards
2. **Check quality gates** - Ensure you're working within an unlocked phase
3. **Follow coding standards** - See `CLAUDE.md` for detailed guidelines
4. **Write tests** - Maintain >80% coverage for business logic
5. **Update documentation** - Keep docs in sync with code changes

### Code Style

- Follow Microsoft C# coding conventions
- Use async/await for all I/O operations
- Include XML documentation comments
- Write unit tests with the code

---

## Support

### Getting Help

- **Documentation:** Check the `/docs` folder
- **Issues:** Create a GitHub issue for bugs or feature requests
- **Teams Channel:** #dorise-dev (internal)

### Team Contacts

| Role | Contact |
|------|---------|
| Product Owner | [TBD] |
| Solution Architect | [TBD] |
| Lead Developer | [TBD] |
| DevOps Engineer | [TBD] |

---

## License

This project is proprietary software. All rights reserved.

---

## Acknowledgments

- Microsoft Azure documentation and samples
- Anthropic Claude for AI-assisted development
- The Dorise development team

---

**Project:** Dorise Sales Incentive Framework (DSIF)  
**Version:** 1.0  
**Last Updated:** January 2025
