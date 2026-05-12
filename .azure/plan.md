# Azure Deployment Plan

> **Status:** Ready for Validation

Generated: 2026-05-12T10:56:24+03:00

---

## 1. Project Overview

**Goal:** Add Azure infrastructure-as-code, upgrade the application to .NET 10, introduce .NET Aspire where it adds value, and enable deployment to a user-owned Azure subscription from GitHub Actions.

**Path:** Modernize Existing

---

## 2. Requirements

| Attribute | Value |
|-----------|-------|
| Classification | Development-ready, production-adjustable |
| Scale | Small |
| Budget | Cost-Optimized by default |
| **Subscription** | To be confirmed before validation/deployment |
| **Location** | To be confirmed before validation/deployment |

Notes:

- The existing app is an ASP.NET Core MVC classroom timer hosted on Azure App Service.
- The current repo has a GitHub Actions publish-profile workflow but no checked-in Bicep, ARM, azd, or Aspire projects.
- The target deployment experience should let a fork or copy of this repository deploy into the user's own subscription from GitHub Actions.
- The app currently depends on Azure App Service, Application Insights, Cosmos DB, Storage, Key Vault, and optionally Azure OpenAI plus a password reset endpoint.

---

## 3. Components Detected

| Component | Type | Technology | Path |
|-----------|------|------------|------|
| mct-timer | Web app | ASP.NET Core MVC, .NET 8 currently | `mct-timer/` |
| mct-timer.Tests | Tests | xUnit, .NET 8 currently | `mct-timer.Tests/` |
| GitHub Actions deployment | CI/CD | Azure Web Apps Deploy with publish profile placeholder | `.github/workflows/azure-webapps-dotnet-core.yml` |

---

## 4. Recipe Selection

**Selected:** GitHub Actions + Bicep, with azd-compatible configuration where useful

**Rationale:** The user wants deployment to their own subscription as a GitHub Action and asked for Bicep/ARM IaC. Bicep is the maintainable source format for ARM deployments. GitHub Actions will authenticate with Azure through OpenID Connect (OIDC) and run `az deployment` plus `dotnet publish`/Web App deployment steps. `azure.yaml` can still be added for local azd-oriented workflows, but GitHub Actions is the primary deployment path.

---

## 5. Architecture

**Stack:** App Service with managed Azure dependencies

### Service Mapping

| Component | Azure Service | SKU |
|-----------|---------------|-----|
| `mct-timer` | Azure App Service on Linux | B1 default parameter |
| User/profile/settings store | Azure Cosmos DB for NoSQL | Serverless default |
| Uploaded/generated backgrounds | Azure Storage account + Blob container | Standard LRS |
| Public background delivery | Storage static website endpoint parameter/output | Storage static website |
| Password encryption | Azure Key Vault key | Standard vault |
| Monitoring | Application Insights + Log Analytics | Consumption/default |
| App identity | System-assigned managed identity | N/A |
| CI/CD identity | Microsoft Entra app registration with federated GitHub credential | N/A |

### Supporting Services

| Service | Purpose |
|---------|---------|
| GitHub Actions | Provision and deploy to the user's Azure subscription |
| Azure Login Action | OIDC login without storing Azure passwords or publish profiles |
| Azure Developer CLI | Optional local provisioning/deployment workflow |
| Bicep modules | App Service, Cosmos DB, Storage, Key Vault, monitoring, RBAC |
| .NET Aspire AppHost | Local orchestration of the web project and Azure dependency placeholders |

### Important Design Decisions

- Keep Azure App Service as the production host because this is the current architecture and the existing app is a single MVC web app.
- Add Aspire AppHost for local developer experience and dashboard support, not as a forced migration to Azure Container Apps. ServiceDefaults were intentionally not wired into the production app because the current MVC startup already has explicit Application Insights configuration and the production target remains App Service.
- Provision core Azure resources with Bicep. Treat Azure OpenAI and password-reset email endpoint as configurable parameters unless the user explicitly wants those provisioned too, because Azure OpenAI availability, quota, and approval vary by subscription and region.
- Use managed identity for Azure resource access. Do not add storage keys, Cosmos keys, or Key Vault secrets for internal service access.
- Upgrade project target frameworks to .NET 10 and update CI/CD workflow runtime settings accordingly.
- Replace publish-profile deployment with OIDC-based GitHub Actions deployment. This requires users to create a Microsoft Entra app registration/federated credential once, then set repository variables/secrets such as `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `AZURE_LOCATION`, and an environment name.
- The workflow should support manual dispatch and pushes to `main`, so users can deploy from their own fork after configuring GitHub environments/secrets.

---

## 6. Execution Checklist

### Phase 1: Planning

- [x] Analyze workspace
- [x] Gather requirements from user prompt
- [ ] Confirm subscription and location with user before deployment
- [x] Scan codebase
- [x] Select recipe
- [x] Plan architecture
- [x] **User approved this plan**

### Phase 2: Execution

- [x] Upgrade `mct-timer` and `mct-timer.Tests` target frameworks to `net10.0`
- [x] Replace publish-profile CI/CD with OIDC GitHub Actions workflow for provisioning and deployment
- [x] Add `azure.yaml` for azd App Service deployment
- [x] Add Bicep infrastructure under `infra/`
- [x] Add App Service app settings that map to the existing `ConfigMng` and Application Insights keys
- [x] Add managed identity role assignments for Cosmos DB, Storage, and Key Vault
- [x] Add Aspire AppHost where it fits cleanly
- [x] Update the solution file
- [x] Update README deployment instructions for user-owned subscription, OIDC setup, required GitHub variables/secrets, and manual workflow dispatch
- [x] Update plan status to "Ready for Validation"

### Phase 3: Validation

- [ ] Invoke azure-validate skill
- [ ] All validation checks pass
- [ ] Update plan status to "Validated"
- [ ] Record validation proof below

### Phase 4: Deployment

- [ ] Invoke azure-deploy skill if the user wants this deployed from the session
- [ ] Deployment successful
- [ ] Update plan status to "Deployed"

---

## 7. Validation Proof

> **Required:** The azure-validate skill must populate this section before setting status to `Validated`.

| Check | Command Run | Result | Timestamp |
|-------|-------------|--------|-----------|
| Pending | Pending | Pending | Pending |

**Validated by:** Pending

**Validation timestamp:** Pending

---

## 8. Files to Generate or Update

| File | Purpose | Status |
|------|---------|--------|
| `.azure/plan.md` | This plan | Complete |
| `azure.yaml` | Optional AZD configuration for App Service deployment | Complete |
| `infra/main.bicep` | Main Bicep composition | Complete |
| `infra/main.parameters.json.template` | Example deployment parameters | Complete |
| `mct-timer/mct-timer.csproj` | .NET 10 upgrade | Complete |
| `mct-timer.Tests/mct-timer.Tests.csproj` | .NET 10 test upgrade | Complete |
| `mct-timer.AppHost/*` | Aspire AppHost | Complete |
| `.github/workflows/azure-webapps-dotnet-core.yml` | Replace with OIDC-based provision/deploy workflow for .NET 10 | Complete |
| `readme.md` | Deployment and Aspire usage docs | Complete |

---

## 9. Next Steps

> Current: Ready for Validation

1. Run Azure validation.
2. Confirm subscription/location before any real deployment.
3. Deploy through GitHub Actions after repository secrets and variables are configured.
