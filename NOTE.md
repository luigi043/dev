## **InsureX Database Schema Overview**

### **Schemas**

* `tenant` – Gerencia tenants, usuários, permissões e API keys
* `audit` – Logs e evidências
* `compliance` – Regras, verificações, alertas e histórico
* `workflow` – Casos, tarefas, comentários, anexos
* `integration` – Webhooks e configurações de conectores
* `reporting` – Dashboards e relatórios
* `dbo` – Identidade, ativos, políticas, notificações

---

### **tenant**

* **Tenants** `(Id)` → central de tenants
* **UserPermissions** `(TenantId, UserId)` → permissões específicas
* **ApiKeys** `(TenantId)` → integrações via API

---

### **dbo**

* **AspNetUsers** `(Id, TenantId)` → usuários, link para `Tenants`
* **AspNetRoles**, **AspNetUserRoles**, **AspNetRoleClaims**, **AspNetUserClaims**, **AspNetUserLogins**, **AspNetUserTokens** → gerenciamento de identidade
* **Assets** `(Id, TenantId)` → registro de ativos (veículos, equipamentos, propriedades)
* **AssetDocuments** `(AssetId, TenantId)` → documentos relacionados aos ativos
* **Policies** `(AssetId, TenantId)` → apólices vinculadas a ativos
* **PolicyClaims** `(PolicyId, TenantId)` → sinistros das apólices
* **Notifications** `(TenantId, UserId)` → notificações para usuários

---

### **compliance**

* **Rules** `(TenantId)` → regras de compliance
* **Checks** `(AssetId, RuleId, TenantId)` → verificações realizadas
* **History** `(AssetId, TenantId)` → histórico de alterações
* **Alerts** `(AssetId, RuleId, TenantId)` → alertas gerados

---

### **workflow**

* **Cases** `(AssetId, PolicyId, AlertId, TenantId)` → casos de workflow (não conformidade, sinistros, inspeções)
* **Tasks** `(CaseId, TenantId)` → tarefas dentro de casos
* **Comments** `(EntityType, EntityId, UserId)` → comentários sobre Cases, Tasks, Alerts ou Assets
* **Attachments** `(EntityType, EntityId)` → arquivos vinculados a qualquer entidade

---

### **audit**

* **Logs** `(TenantId, UserId)` → auditoria de eventos
* **Evidence** `(TenantId)` → evidências e arquivos relacionados

---

### **integration**

* **WebhookEvents** `(TenantId)` → eventos recebidos via webhook
* **ConnectorConfigs** `(TenantId)` → configurações de integração com terceiros

---

### **reporting**

* **DashboardSnapshots** `(TenantId)` → snapshots de métricas
* **Views**:

  * `vw_AssetCompliance` → compliance por ativo
  * `vw_PolicyStatus` → status das apólices
  * `vw_DashboardSummary` → visão geral por tenant

---

### **Relacionamentos principais**

* `AspNetUsers.TenantId → Tenants.Id`
* `Assets.TenantId → Tenants.Id`
* `Policies.AssetId → Assets.Id`
* `PolicyClaims.PolicyId → Policies.Id`
* `Checks.AssetId → Assets.Id`
* `Checks.RuleId → Rules.Id`
* `Alerts.AssetId → Assets.Id`
* `Alerts.RuleId → Rules.Id`
* `Cases.AssetId → Assets.Id`
* `Cases.PolicyId → Policies.Id`
* `Tasks.CaseId → Cases.Id`
* `Comments.EntityId → Cases/Tasks/Alerts/Assets`
* `Attachments.EntityId → Cases/Tasks/Alerts/Assets`



dotnet clean
dotnet build
dotnet run --project src/InsureX.Web

cd src/InsureX.Api
dotnet run