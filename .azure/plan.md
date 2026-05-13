# Azure Deployment Plan

> **Status:** Validated

Generated: 2026-05-13T12:52:07+03:00

---

## 1. Project Overview

**Goal:** Remove the Azure App Service dependency and deploy the existing ASP.NET Core MVC app to Azure Container Apps from GitHub Actions, preserving current functionality while avoiding App Service Plan quota issues.

**Path:** Modernize Existing

---

## 2. Requirements

| Attribute | Value |
|-----------|-------|
| Classification | Development-ready, production-adjustable |
| Scale | Small |
| Budget | Cost-Optimized by default |
| **Subscription** | User-owned subscription via GitHub Actions OIDC |
| **Location** | GitHub variable/input, default `eastus2` |

Notes:

- The current app is still an ASP.NET Core MVC web app, not a SPA.
- Rewriting to SPA + APIs would be a larger product refactor. This plan removes App Service first by containerizing the existing app.
- Azure Container Apps Consumption still has Azure quota considerations, but it avoids App Service Plan worker quota and should be more suitable for low-traffic usage.

---

## 3. Components Detected

| Component | Type | Technology | Path |
|-----------|------|------------|------|
| mct-timer | Web app | ASP.NET Core MVC, .NET 10 | `mct-timer/` |
| mct-timer.Tests | Tests | xUnit, .NET 10 | `mct-timer.Tests/` |
| mct-timer.AppHost | Local orchestration | .NET Aspire AppHost | `mct-timer.AppHost/` |
| GitHub Actions deployment | CI/CD | OIDC + Azure CLI + Bicep | `.github/workflows/azure-webapps-dotnet-core.yml` |

---

## 4. Recipe Selection

**Selected:** GitHub Actions + Bicep + Azure Container Apps

**Rationale:** The user wants to remove the App Service dependency and continue deploying from GitHub Actions to their own Azure subscription. Container Apps can host the existing MVC app as a container with less application rewrite than SPA migration. Bicep remains the IaC source of truth.

---

## 5. Architecture

**Stack:** Azure Container Apps Consumption with managed Azure dependencies

### Service Mapping

| Component | Azure Service | SKU |
|-----------|---------------|-----|
| `mct-timer` | Azure Container Apps | Consumption |
| Container images | Azure Container Registry | Basic |
| User/profile/settings store | Azure Cosmos DB for NoSQL | Serverless |
| Uploaded/generated backgrounds | Azure Storage account + Blob container | Standard LRS |
| Public background delivery | Storage static website endpoint | Storage static website |
| Password encryption | Azure Key Vault key | Standard vault |
| Monitoring | Application Insights + Log Analytics | Consumption/default |
| App identity | User-assigned managed identity on Container App | N/A |
| CI/CD identity | Microsoft Entra app registration with federated GitHub credential | N/A |

### Supporting Services

| Service | Purpose |
|---------|---------|
| GitHub Actions | Build container, push to ACR, provision/deploy Container App |
| Azure Login Action | OIDC login without Azure passwords or publish profiles |
| Bicep | Container Apps, ACR, Cosmos, Storage, Key Vault, monitoring, RBAC |
| .NET Aspire AppHost | Local orchestration/dashboard support |

### Important Design Decisions

- Preserve the existing MVC app and public timer behavior; do not rewrite to SPA in this pass.
- Add a Linux Dockerfile for the .NET 10 web app and expose port `8080`.
- Replace App Service Plan/Web App resources with Azure Container Apps Environment, Container App, and Azure Container Registry.
- Use GitHub Actions to build and push a container image to ACR, then deploy Bicep with the image tag.
- Assign the Container App managed identity access to Cosmos DB, Storage, Key Vault, and ACR pull.
- Continue to support OIDC deployment from a user-owned subscription.
- Update docs to explain that App Service is no longer required.

---

## 6. Execution Checklist

### Phase 1: Planning

- [x] Analyze workspace
- [x] Gather requirements from user prompt
- [x] Scan codebase
- [x] Select recipe
- [x] Plan architecture
- [x] **User approved this plan**

### Phase 2: Execution

- [x] Add Dockerfile and `.dockerignore`
- [x] Replace App Service Bicep resources with ACR, Container Apps Environment, and Container App
- [x] Move app settings into Container App environment variables/secrets
- [x] Update RBAC assignments from Web App identity to Container App identity
- [x] Update GitHub Actions to build/push image and deploy Container Apps instead of Web App package deployment
- [x] Update `azure.yaml` host metadata from App Service to Container Apps
- [x] Update README deployment and quota guidance
- [x] Update plan status to "Ready for Validation"

### Phase 3: Validation

- [x] Build/test solution
- [x] Build Docker image locally if Docker is available
- [x] Build Bicep template
- [x] Run workflow syntax/diff checks
- [x] Invoke azure-validate skill before any deployment

### Phase 4: Deployment

- [ ] Deploy via GitHub Actions after secrets/variables are configured
- [ ] Update plan status to "Deployed" after successful deployment

---

## 7. Validation Proof

> **Required:** The azure-validate skill must populate this section before setting status to `Validated`.

| Check | Command Run | Result | Timestamp |
|-------|-------------|--------|-----------|
| Bicep build | `az bicep build --file .\infra\main.bicep` | Passed | Current session |
| .NET tests | `dotnet test .\mct-timer.Tests\mct-timer.Tests.csproj --verbosity minimal` | Passed | Current session |
| Diff whitespace | `git --no-pager diff --check` | Passed | Current session |
| Deployment error fix | Enabled storage-account blob public access required by static website hosting for `$web` background delivery | Passed local Bicep validation | Current session |
| Docker availability | `docker --version` | Docker CLI not installed locally; image build is delegated to `az acr build` in GitHub Actions | Current session |

**Validated by:** azure-validate skill with local CLI checks

**Validation timestamp:** Current session

---

## 8. Files to Generate or Update

| File | Purpose | Status |
|------|---------|--------|
| `.azure/plan.md` | This plan | Complete |
| `mct-timer/Dockerfile` | Container build for MVC app | Complete |
| `.dockerignore` | Efficient container build context | Complete |
| `infra/main.bicep` | Container Apps IaC | Complete |
| `.github/workflows/azure-webapps-dotnet-core.yml` | Container build/push/deploy workflow | Complete |
| `azure.yaml` | Container Apps metadata | Complete |
| `readme.md` | App-Service-free deployment docs | Complete |

---

## 9. Next Steps

> Current: Validated

1. Commit and push the Container Apps migration.
2. Deploy via GitHub Actions after secrets/variables are configured.
