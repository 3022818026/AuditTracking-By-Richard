# AuditTracking 审计跟踪管理系统

> Design By **Richard_Xia**

AuditTracking 是一套基于 **ASP.NET Core Web API + Vue 3 + SQL Server** 开发的审计跟踪管理系统，围绕企业内部审计业务，形成了“审计计划—审计问题—整改措施—整改验证—问题关闭”的完整业务闭环。

系统不仅实现了常规的增删改查，还重点处理了业务状态流转、软删除、回收站、操作日志、统计仪表盘、JWT 登录认证、用户身份追踪等实际后台管理系统中常见的问题。

---

## 一、项目简介

### 1.1 项目目标

本项目的目标不是简单制作一个数据录入页面，而是完成一套结构清晰、业务完整、可持续扩展的审计跟踪系统。

系统主要解决以下问题：

- 审计计划如何统一创建和管理
- 审计过程中发现的问题如何持续跟踪
- 整改措施如何提交、审批和完成
- 整改结果如何进行验证
- 问题何时关闭或重新进入整改
- 用户操作如何留下完整的追溯记录
- 未登录用户如何被阻止访问业务接口
- 管理人员如何快速了解整体审计执行情况

### 1.2 核心业务流程

```text
审计计划
    ↓
审计问题
    ↓
整改措施
    ↓
整改验证
    ↓
问题关闭或返回整改
```

完整流程如下：

```text
创建审计计划
→ 启动审计计划
→ 创建审计问题
→ 问题进入整改
→ 创建整改措施
→ 提交整改措施
→ 审批整改措施
→ 完成整改措施
→ 问题进入待验证
→ 创建整改验证
→ 验证通过后问题自动关闭
→ 验证不通过或需要补充材料时重新进入整改
```

---

## 二、技术栈

### 2.1 后端技术

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Bearer Authentication
- PasswordHasher
- DataAnnotations
- 全局异常处理中间件
- User Secrets
- ASP.NET Core 内置 OpenAPI

### 2.2 前端技术

- Vue 3
- TypeScript
- Vite
- Vue Router
- Pinia
- Element Plus
- Axios

### 2.3 数据库技术

- SQL Server
- EF Core Code First
- Migration 数据库迁移
- 唯一索引
- 外键约束
- 软删除
- 操作日志表
- 数据库事务

---

## 三、整体架构

本项目采用前后端分离架构。

```text
AuditTracking-ReMedy
├─ AuditTracking.Api              # ASP.NET Core 后端项目
├─ audit-tracking-web             # Vue 3 前端项目
└─ xxx.md
```

整体调用关系如下：

```text
浏览器
  ↓
Vue 3 前端
  ↓ Axios
/api/*
  ↓ Vite Proxy
ASP.NET Core Web API
  ↓ Entity Framework Core
SQL Server
```

### 3.1 采用前后端分离的原因

采用前后端分离主要基于以下考虑：

1. 前端页面与后端业务职责更加清晰
2. 后端接口可以独立测试
3. 前端可以独立构建和部署
4. 后续可以接入移动端或其他客户端
5. 有利于统一处理认证、权限和日志
6. 避免页面代码与数据库逻辑直接耦合
7. 便于项目后续扩展和维护

---

## 四、后端项目结构

后端项目位于：

```text
AuditTracking.Api
```

主要目录结构如下：

```text
AuditTracking.Api
├─ Common
│  └─ ApiResponse.cs
├─ Controllers
│  ├─ AuthController.cs
│  ├─ AuditPlansController.cs
│  ├─ AuditIssuesController.cs
│  ├─ CorrectiveActionsController.cs
│  ├─ RectificationVerificationsController.cs
│  ├─ DashboardController.cs
│  └─ DatabaseController.cs
├─ Data
│  └─ AppDbContext.cs
├─ Dtos
│  ├─ Auth
│  ├─ Dashboard
│  ├─ AuditPlans
│  ├─ AuditIssues
│  ├─ CorrectiveActions
│  └─ RectificationVerifications
├─ Entities
│  ├─ AppUser.cs
│  ├─ AuditPlan.cs
│  ├─ AuditIssue.cs
│  ├─ CorrectiveAction.cs
│  ├─ RectificationVerification.cs
│  ├─ AuditPlanOperationLog.cs
│  ├─ AuditIssueOperationLog.cs
│  ├─ CorrectiveActionOperationLog.cs
│  └─ RectificationVerificationOperationLog.cs
├─ Middleware
│  └─ ExceptionHandlingMiddleware.cs
├─ Options
│  └─ JwtOptions.cs
├─ Services
│  ├─ CurrentUserService.cs
│  └─ Auth
│     ├─ IJwtTokenService.cs
│     ├─ JwtTokenService.cs
│     └─ DefaultAdminInitializer.cs
├─ Migrations
├─ Program.cs
└─ AuditTracking.Api.csproj
```

---

## 五、后端分层设计

### 5.1 Entity 实体层

Entity 对应数据库中的数据表。

主要实体包括：

```text
AuditPlan
AuditIssue
CorrectiveAction
RectificationVerification
AppUser
```

实体主要负责描述：

- 数据库字段
- 数据类型
- 字段长度
- 表之间的关联关系
- 导航属性
- 基础数据结构

实体不直接承担前端页面展示职责。

---

### 5.2 DTO 数据传输层

DTO 用于接口输入和输出。

本项目没有直接将所有实体完整暴露给前端，而是根据不同接口场景设计 DTO，例如：

```text
CreateAuditIssueDto
UpdateAuditIssueDto
AuditIssueQueryDto
ChangeAuditIssueStatusDto
```

这样设计的优点包括：

- 限制前端可以修改的字段
- 防止实体内部结构全部暴露
- 区分新增和修改时的校验规则
- 便于增加接口参数校验
- 降低前后端耦合
- 避免前端随意修改状态、创建时间等敏感字段

---

### 5.3 Controller 控制器层

Controller 主要负责：

- 接收 HTTP 请求
- 校验接口参数
- 校验业务状态
- 调用 Entity Framework Core
- 执行业务状态流转
- 写入操作日志
- 返回统一响应结果

Controller 中不应堆积与当前模块无关的逻辑。

---

### 5.4 Service 服务层

当前项目中的 Service 主要承担以下功能：

- JWT Token 创建
- 当前登录用户识别
- 默认管理员初始化
- 认证相关公共逻辑

随着项目继续扩展，可以进一步将复杂业务规则从 Controller 中拆分至 Service 层。

---

### 5.5 AppDbContext 数据访问层

`AppDbContext` 负责：

- 注册 DbSet
- 配置数据表
- 配置字段长度
- 配置唯一索引
- 配置外键关系
- 配置删除规则
- 配置全局软删除过滤器
- 管理数据库迁移

---

## 六、核心业务实体

## 6.1 审计计划 AuditPlan

审计计划是整个审计流程的起点。

主要字段包括：

```text
Id
AuditNo
Title
AuditType
PlannedDate
Auditee
Auditor
Status
Result
Remark
CreatedAt
UpdatedAt
CompletedAt
ClosedAt
IsDeleted
DeletedAt
DeletedBy
```

状态流转如下：

```text
Draft → InProgress / Cancelled
InProgress → Completed / Cancelled
Completed → Closed
```

状态含义：

- `Draft`：草稿
- `InProgress`：进行中
- `Completed`：已完成
- `Closed`：已关闭
- `Cancelled`：已取消

其中：

- 审计计划完成时可以填写审计结果
- 审计计划关闭后表示业务流程正常结束
- 取消状态表示该计划不再继续执行

---

## 6.2 审计问题 AuditIssue

审计问题属于某一个审计计划。

主要字段包括：

```text
Id
AuditPlanId
IssueNo
Title
Description
IssueType
Severity
ResponsibleDepartment
ResponsiblePerson
DueDate
Status
CreatedAt
UpdatedAt
ClosedAt
IsDeleted
DeletedAt
DeletedBy
```

状态流转如下：

```text
Open → Rectifying / Rejected
Rectifying → PendingVerification / Rejected
PendingVerification → Closed / Rectifying
Closed → 终态
Rejected → 终态
```

状态含义：

- `Open`：问题已创建
- `Rectifying`：整改中
- `PendingVerification`：待验证
- `Closed`：问题已关闭
- `Rejected`：问题被驳回

---

## 6.3 整改措施 CorrectiveAction

整改措施属于某一个审计问题。

主要字段包括：

```text
Id
AuditIssueId
ActionNo
ActionDescription
ResponsibleDepartment
ResponsiblePerson
PlannedCompletionDate
ActualCompletionDate
CompletionDescription
Status
SubmittedAt
ApprovedAt
CompletedAt
CreatedAt
UpdatedAt
IsDeleted
DeletedAt
DeletedBy
```

状态流转如下：

```text
Draft → Submitted
Submitted → Approved / Rejected
Rejected → Draft
Approved → Completed
Completed → 终态
```

状态含义：

- `Draft`：草稿
- `Submitted`：已提交
- `Approved`：已审批
- `Rejected`：已驳回
- `Completed`：已完成

该状态流程模拟了实际企业整改措施从编写、提交、审批到最终完成的过程。

---

## 6.4 整改验证 RectificationVerification

整改验证同时关联：

```text
AuditIssue
CorrectiveAction
```

主要字段包括：

```text
Id
AuditIssueId
CorrectiveActionId
VerificationNo
VerificationResult
VerificationComment
Verifier
VerifiedAt
IsPassed
CreatedAt
UpdatedAt
IsDeleted
DeletedAt
DeletedBy
```

验证结果包括：

```text
Passed
Failed
NeedMoreEvidence
```

验证结果与问题状态联动规则如下：

```text
Passed
→ AuditIssue.Status = Closed
→ 设置 ClosedAt

Failed
→ AuditIssue.Status = Rectifying
→ 清空 ClosedAt

NeedMoreEvidence
→ AuditIssue.Status = Rectifying
→ 清空 ClosedAt
```

整改验证和审计问题状态更新位于同一数据库事务中。

这样可以避免出现以下不一致情况：

```text
验证记录保存成功
但审计问题状态更新失败
```

或：

```text
审计问题状态已关闭
但验证记录没有保存成功
```

---

## 6.5 用户 AppUser

用户实体用于系统登录和身份管理。

主要字段包括：

```text
Id
UserName
DisplayName
PasswordHash
Role
IsActive
CreatedAt
CreatedBy
UpdatedAt
UpdatedBy
LastLoginAt
```

当前角色包括：

```text
Admin
User
```

角色含义：

- `Admin`：管理员
- `User`：普通用户

系统不会保存明文密码，只保存经过密码哈希处理后的 `PasswordHash`。

---

## 七、数据库设计

### 7.1 主要数据表

```text
AppUsers
AuditPlans
AuditIssues
CorrectiveActions
RectificationVerifications
AuditPlanOperationLogs
AuditIssueOperationLogs
CorrectiveActionOperationLogs
RectificationVerificationOperationLogs
__EFMigrationsHistory
```

### 7.2 业务表关系

```text
AuditPlan
  └─ AuditIssues
       └─ CorrectiveActions
            └─ RectificationVerifications
```

整改验证同时关联审计问题和整改措施。

更完整的关系如下：

```text
AuditPlan 1 ─── N AuditIssue

AuditIssue 1 ─── N CorrectiveAction

AuditIssue 1 ─── N RectificationVerification

CorrectiveAction 1 ─── N RectificationVerification
```

---

### 7.3 唯一索引

以下业务编号设置唯一索引：

```text
AuditNo
IssueNo
ActionNo
VerificationNo
UserName
```

设置唯一索引的原因：

- 防止业务编号重复
- 防止用户名重复
- 保证数据唯一性
- 减少人工校验遗漏
- 将数据一致性约束下沉到数据库

---

### 7.4 外键删除策略

项目主要采用：

```text
DeleteBehavior.Restrict
```

采用限制删除而不是级联删除的原因：

- 不希望删除父记录时自动清空整条业务链
- 审计业务强调历史追溯
- 删除操作应该通过软删除完成
- 避免误操作导致大量数据丢失
- 保证关联数据完整性

---

## 八、软删除与回收站设计

系统没有直接物理删除核心业务数据，而是采用软删除机制。

通用软删除字段：

```text
IsDeleted
DeletedAt
DeletedBy
```

普通查询通过全局查询过滤器自动排除：

```text
IsDeleted = true
```

回收站查询使用：

```csharp
IgnoreQueryFilters()
```

恢复操作会清空：

```text
DeletedAt
DeletedBy
```

并将：

```text
IsDeleted = false
```

采用软删除的原因：

- 防止误删
- 支持数据恢复
- 保留审计历史
- 满足追溯要求
- 避免破坏关联关系

---

## 九、操作日志设计

每个核心模块都有独立的操作日志表。

主要记录以下操作：

```text
Create
Update
StatusChange
Delete
Restore
```

日志字段包括：

```text
OperationType
BeforeData
AfterData
Operator
Remark
CreatedAt
```

其中：

```text
BeforeData
AfterData
```

使用 JSON 字符串保存关键字段快照。

通过日志可以回答以下问题：

- 谁进行了操作
- 在什么时间操作
- 操作前的数据是什么
- 操作后的数据是什么
- 操作类型是什么
- 是否发生了状态变化

系统同时保留两类日志：

### 数据库操作日志

用于业务审计和历史追溯。

### ILogger 系统日志

用于：

- 系统运行记录
- 异常排查
- 调试
- 服务启动信息
- 数据库执行信息

---

## 十、统一响应与异常处理

系统使用统一响应结构：

```json
{
  "success": true,
  "message": "操作成功",
  "data": {},
  "errors": null
}
```

后端通过统一响应类返回：

```text
ApiResponse<T>
ApiResponse.Ok(...)
ApiResponse.Fail(...)
```

统一响应的优点：

- 前端处理逻辑一致
- 错误信息格式统一
- 减少页面中的重复判断
- 后续接口扩展更加方便

系统同时配置了全局异常处理中间件：

```text
ExceptionHandlingMiddleware
```

主要作用：

- 捕获未处理异常
- 返回统一 JSON
- 避免将异常堆栈直接暴露给前端
- 减少 Controller 中重复的 try-catch
- 提高系统可维护性

---

## 十一、身份认证与安全设计

### 11.1 登录方式

系统采用：

```text
JWT Bearer Authentication
```

登录流程如下：

```text
用户输入用户名和密码
→ 后端查询 AppUser
→ PasswordHasher 验证密码
→ 验证账号是否启用
→ 生成 JWT
→ 前端保存 Token
→ 后续请求携带 Bearer Token
→ 后端验证 Token
→ 允许访问受保护接口
```

---

### 11.2 JWT Claims

Token 中包含：

```text
NameIdentifier：用户 ID
Name：用户名
GivenName：显示名称
Role：用户角色
```

Token 中不会包含：

```text
密码
PasswordHash
数据库连接字符串
JWT SigningKey
其他敏感配置
```

---

### 11.3 密码存储

系统使用：

```text
PasswordHasher<AppUser>
```

进行密码哈希。

项目禁止使用：

```text
明文密码
MD5
SHA1
直接 SHA256
```

原因是普通哈希算法不适合直接存储用户密码，而 `PasswordHasher` 会自动处理盐值和安全参数。

---

### 11.4 受保护接口

以下业务 Controller 使用：

```csharp
[Authorize]
```

包括：

```text
AuditPlansController
AuditIssuesController
CorrectiveActionsController
RectificationVerificationsController
DashboardController
DatabaseController
```

认证接口权限如下：

```text
POST /api/auth/login
→ AllowAnonymous

GET /api/auth/me
→ Authorize

POST /api/auth/users
→ Authorize(Roles = "Admin")

PUT /api/auth/change-password
→ Authorize
```

这意味着：

- 登录接口允许匿名访问
- 获取当前用户必须登录
- 创建用户仅管理员可以执行
- 修改密码必须登录
- 所有业务接口必须携带有效 Token

---

### 11.5 当前用户识别

当前用户识别优先级如下：

```text
JWT Claims
→ X-User-Name
→ System
```

JWT Claims 优先级最高。

即使客户端传入 `X-User-Name`，也不能覆盖已经认证的 JWT 用户身份。

---

### 11.6 User Secrets

以下敏感配置不会写入源码：

```text
数据库连接字符串
JWT SigningKey
初始管理员账号
初始管理员密码
```

本地开发环境使用：

```text
dotnet user-secrets
```

进行配置。

这样可以避免：

- 密码进入 Git
- 密钥上传到远程仓库
- 数据库账号泄露
- 不同电脑之间配置冲突

---

## 十二、前端项目结构

前端项目位于：

```text
audit-tracking-web
```

主要目录结构如下：

```text
src
├─ api
│  ├─ auth.ts
│  ├─ dashboard.ts
│  ├─ audit-plans.ts
│  ├─ auditIssues.ts
│  ├─ correctiveActions.ts
│  └─ rectificationVerifications.ts
├─ components
│  └─ auth
│     └─ ChangePasswordDialog.vue
├─ layouts
│  └─ AdminLayout.vue
├─ router
│  └─ index.ts
├─ stores
│  ├─ index.ts
│  └─ auth.ts
├─ types
│  ├─ api.ts
│  ├─ auth.ts
│  ├─ dashboard.ts
│  ├─ audit-plan.ts
│  ├─ auditIssue.ts
│  ├─ correctiveAction.ts
│  └─ rectificationVerification.ts
├─ utils
│  ├─ request.ts
│  └─ auth.ts
├─ views
│  ├─ LoginView.vue
│  ├─ DashboardView.vue
│  ├─ AuditIssuesView.vue
│  ├─ CorrectiveActionsView.vue
│  ├─ RectificationVerificationsView.vue
│  └─ audit-plans
│     └─ AuditPlanListView.vue
├─ App.vue
└─ main.ts
```

---

## 十三、前端架构设计

### 13.1 API 层

所有接口集中存放在：

```text
src/api
```

页面组件不会直接调用 Axios。

例如：

```text
getAuditIssues
createAuditIssue
updateAuditIssue
changeAuditIssueStatus
deleteAuditIssue
restoreAuditIssue
```

这样做的优点：

- 统一管理接口路径
- 统一维护请求参数类型
- 避免页面中重复编写请求
- 后端接口变化时更容易修改
- 便于后续增加缓存、重试和日志

---

### 13.2 request.ts

`request.ts` 负责：

- 配置 `baseURL`
- 自动附加 JWT
- 统一解析后端响应
- 统一处理错误
- 统一处理 401
- 统一处理 403
- 统一处理网络错误
- 统一处理请求超时

前端统一请求：

```text
/api/*
```

开发环境下通过 Vite 代理到后端。

---

### 13.3 Pinia Auth Store

认证状态由 Pinia 统一管理。

主要状态包括：

```text
accessToken
currentUser
expiresAt
isAuthenticated
loginLoading
```

主要操作包括：

```text
initializeAuth
login
loadCurrentUser
logout
clearAuth
```

页面刷新后，认证恢复流程如下：

```text
从 localStorage 读取 Token
→ 检查 Token 是否过期
→ 未过期时调用 /auth/me
→ 获取当前用户
→ 恢复登录状态
```

这样可以避免用户刷新页面后立即退出登录。

---

### 13.4 路由守卫

业务路由通过：

```text
meta.requiresAuth
```

进行保护。

未登录用户访问业务页面时：

```text
跳转 /login
```

并记录原始目标地址。

登录成功后：

```text
返回原目标页面
```

例如：

```text
未登录访问 /audit-plans
→ 跳转 /login?redirect=/audit-plans
→ 登录成功
→ 返回 /audit-plans
```

---

### 13.5 页面布局

系统采用后台管理系统常见布局：

```text
左侧菜单
顶部用户区域
主内容区域
```

品牌区域显示：

```text
Design By
Richard_Xia
```

顶部用户区域显示：

```text
显示名称
角色
修改密码
退出登录
```

---

## 十四、核心前端页面

### 14.1 登录页面

登录页面功能包括：

- 用户名必填校验
- 密码必填校验
- 回车登录
- 密码显示和隐藏
- 记住用户名
- 登录按钮加载状态
- 错误密码提示
- 网络错误中文提示
- 登录成功后跳转
- 已登录用户不能重复进入登录页
- 不保存密码
- 不输出 Token

---

### 14.2 仪表盘页面

仪表盘主要展示：

- 审计计划总数
- 进行中计划数量
- 逾期计划数量
- 审计问题总数
- 整改中问题数量
- 待验证问题数量
- 逾期问题数量
- 整改措施总数
- 已完成整改措施数量
- 逾期整改措施数量
- 整改验证总数
- 验证通过数量
- 整改措施完成率
- 各模块状态分布
- 最近审计计划
- 最近审计问题

---

### 14.3 审计计划页面

主要功能：

- 查询
- 分页
- 状态筛选
- 风险统计
- 新增
- 编辑
- 详情
- 状态变更
- 删除
- 回收站
- 恢复
- 操作日志

---

### 14.4 审计问题页面

主要功能：

- 按审计计划筛选
- 按状态筛选
- 按严重程度筛选
- 新增
- 编辑
- 详情
- 合法状态流转
- 删除
- 回收站
- 恢复
- 操作日志

---

### 14.5 整改措施页面

主要功能：

- 按审计问题筛选
- 新增
- 编辑
- 详情
- 提交
- 审批
- 驳回
- 完成
- 删除
- 回收站
- 恢复
- 操作日志

---

### 14.6 整改验证页面

主要功能：

- 按问题筛选
- 按整改措施筛选
- 按验证结果筛选
- 新增
- 编辑
- 详情
- 删除
- 回收站
- 恢复
- 操作日志

创建验证记录时：

- 只显示当前问题下的整改措施
- 只允许选择状态为 `Completed` 的整改措施
- 前端提示验证结果会影响问题状态
- 实际状态修改由后端完成

---

### 14.7 修改密码功能

修改密码 Dialog 包含：

```text
当前密码
新密码
确认新密码
```

校验规则包括：

- 三项必填
- 新旧密码不能相同
- 两次新密码必须一致
- 确认密码不会提交到后端
- 修改过程中显示 loading
- 防止重复提交
- 不保存密码
- 不在控制台输出密码

修改成功后：

```text
显示“密码修改成功，请重新登录”
→ 清除 Token
→ 清除当前用户
→ 清除过期时间
→ 跳转 /login
```

---

## 十五、仪表盘统计设计

后端提供统一统计接口：

```text
GET /api/dashboard/summary
```

一次返回：

- 各模块总数
- 各状态数量
- 逾期数量
- 整改完成率
- 最近审计计划
- 最近审计问题

没有让前端分别调用大量统计接口，原因是：

- 减少请求次数
- 减少页面加载等待
- 保持统计口径一致
- 由后端统一负责数据计算
- 降低前端逻辑复杂度

---

## 十六、项目制作思路

### 16.1 先梳理业务，再写代码

项目开发初期没有直接制作页面，而是先梳理完整业务链：

```text
审计计划
→ 审计问题
→ 整改措施
→ 整改验证
```

先明确：

- 哪些对象互相关联
- 每个对象有哪些状态
- 状态之间如何变化
- 哪些操作会影响其他模块
- 哪些字段需要保留历史

这样可以避免：

- 页面完成后数据库结构不断变化
- 状态字段反复修改
- 前端大量返工
- 后端接口缺少统一规则
- 业务流程出现逻辑冲突

---

### 16.2 按模块逐个完成闭环

每个业务模块按照统一顺序开发：

```text
Entity
→ AppDbContext
→ DTO
→ Controller
→ Migration
→ 后端编译
→ 前端类型
→ API
→ 页面
→ 前后端联调
```

不同时开发多个未稳定模块。

这种方式的优点：

- 每完成一个模块就可以运行
- 出现问题时容易定位
- 减少多个模块同时报错
- 保证业务逐步稳定
- 降低返工成本

---

### 16.3 状态流转必须由后端决定

前端只展示允许的状态选项，但真正的业务校验仍然放在后端。

原因是：

- 前端规则可以被绕过
- 用户可以直接调用 API
- 后续可能存在其他客户端
- 后端必须是最终规则来源

因此项目采用：

```text
前端过滤合法选项
+
后端再次校验状态流转
```

两层保护同时存在。

---

### 16.4 关键业务联动必须使用事务

整改验证会自动修改审计问题状态。

因此以下操作需要位于同一事务中：

```text
创建或修改验证记录
+
修改 AuditIssue 状态
+
写入操作日志
```

使用事务可以保证：

```text
全部成功
或
全部失败
```

避免出现部分数据保存成功的情况。

---

### 16.5 删除优先采用软删除

审计系统强调追溯性。

因此核心业务数据不直接物理删除，而是采用：

```text
软删除
+
回收站
+
恢复
```

这样既可以满足用户删除需求，又能够保留历史数据。

---

### 16.6 操作日志与业务同步建设

操作日志不是项目结束后临时补充，而是在每个核心模块完成时同步建设。

每个模块都考虑：

```text
新增记录日志
修改记录日志
状态变更日志
删除日志
恢复日志
```

这样可以保证所有核心操作都可追踪。

---

### 16.7 认证能力在业务稳定后加入

项目早期先使用：

```text
X-User-Name
```

完成基本操作人记录。

核心业务稳定后再加入：

```text
AppUser
JWT
PasswordHasher
Authorize
路由守卫
修改密码
401/403 处理
```

这样做的原因：

- 前期减少认证系统对业务开发的干扰
- 先确保核心业务闭环正确
- 后期统一完成安全收尾
- 保留兼容机制，减少已有代码改动

最终系统以 JWT 用户身份为准。

---

### 16.8 数据库迁移优于手工建表

所有数据库结构变化均通过 Entity Framework Core Migration 完成。

优势包括：

- 数据库结构变化可追踪
- 新电脑可以快速创建数据库
- 不依赖人工逐表建库
- 便于团队协作
- 便于版本管理
- 便于部署和恢复

---

### 16.9 使用虚拟数据进行演示

项目不需要复制真实企业数据库。

在新电脑中执行：

```text
Update-Database
```

即可创建结构一致的本地数据库。

之后通过系统页面录入虚拟数据。

这样可以：

- 避免泄露真实业务数据
- 避免依赖公司服务器权限
- 便于个人演示
- 便于作品集展示
- 便于离线开发和测试

---

### 16.10 每完成一个阶段都进行构建

开发过程中每完成一个阶段都会执行：

后端：

```powershell
dotnet build
```

前端：

```powershell
npm run build
```

这样可以及时发现：

- C# 编译错误
- TypeScript 类型错误
- Vue 模板错误
- 路由配置错误
- API 类型不一致
- 依赖问题

避免将大量问题堆积到项目最后。

---

## 十七、运行环境要求

### 17.1 后端环境

- .NET 10 SDK
- SQL Server
- SQL Server Management Studio
- Visual Studio 2022 或更高版本

### 17.2 前端环境

- Node.js
- npm
- VS Code 或 Visual Studio

---

## 十八、本地启动方式

### 18.1 克隆项目

```bash
git clone <repository-url>
cd AuditTracking-ReMedy
```

---

### 18.2 配置数据库连接字符串

使用本地默认 SQL Server：

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=AuditTracking_Local;Trusted_Connection=True;TrustServerCertificate=True;" --project AuditTracking.Api/AuditTracking.Api.csproj
```

使用 SQL Server Express：

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost\SQLEXPRESS;Database=AuditTracking_Local;Trusted_Connection=True;TrustServerCertificate=True;" --project AuditTracking.Api/AuditTracking.Api.csproj
```

使用 SQL Server 账号密码时：

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=服务器地址;Database=AuditTracking_Local;User Id=数据库账号;Password=数据库密码;TrustServerCertificate=True;" --project AuditTracking.Api/AuditTracking.Api.csproj
```

不要将真实连接字符串提交到 Git。

---

### 18.3 配置 JWT

```powershell
dotnet user-secrets set "Jwt:Issuer" "AuditTracking.Api" --project AuditTracking.Api/AuditTracking.Api.csproj
```

```powershell
dotnet user-secrets set "Jwt:Audience" "AuditTracking.Web" --project AuditTracking.Api/AuditTracking.Api.csproj
```

```powershell
dotnet user-secrets set "Jwt:SigningKey" "<至少64字符的高强度随机密钥>" --project AuditTracking.Api/AuditTracking.Api.csproj
```

```powershell
dotnet user-secrets set "Jwt:ExpirationMinutes" "120" --project AuditTracking.Api/AuditTracking.Api.csproj
```

---

### 18.4 配置初始管理员

```powershell
dotnet user-secrets set "InitialAdmin:UserName" "admin" --project AuditTracking.Api/AuditTracking.Api.csproj
```

```powershell
dotnet user-secrets set "InitialAdmin:DisplayName" "系统管理员" --project AuditTracking.Api/AuditTracking.Api.csproj
```

```powershell
dotnet user-secrets set "InitialAdmin:Password" "<高强度初始密码>" --project AuditTracking.Api/AuditTracking.Api.csproj
```

注意：

- 初始管理员只会在系统中不存在管理员时创建
- 管理员已经创建后，修改 User Secrets 中的密码不会自动修改数据库中的现有密码
- 已存在管理员应通过系统中的“修改密码”功能更换密码

---

### 18.5 创建数据库

使用 .NET CLI：

```powershell
dotnet ef database update --project AuditTracking.Api/AuditTracking.Api.csproj --startup-project AuditTracking.Api/AuditTracking.Api.csproj
```

也可以在 Visual Studio 的 Package Manager Console 中执行：

```powershell
Update-Database
```

数据库创建完成后会包含：

```text
AppUsers
AuditPlans
AuditIssues
CorrectiveActions
RectificationVerifications
各模块操作日志表
__EFMigrationsHistory
```

---

### 18.6 启动后端

在项目根目录执行：

```powershell
dotnet restore
```

```powershell
dotnet build AuditTracking.Api/AuditTracking.Api.csproj
```

```powershell
dotnet run --project AuditTracking.Api/AuditTracking.Api.csproj
```

后端默认可能监听：

```text
https://localhost:7002
http://localhost:5194
```

实际地址以控制台输出为准。

---

### 18.7 安装前端依赖

```powershell
cd audit-tracking-web
```

```powershell
npm install
```

---

### 18.8 启动前端

```powershell
npm run dev
```

浏览器访问：

```text
http://localhost:5173
```

---

## 十九、Vite 开发代理

前端通过 Vite Proxy 将：

```text
/api
```

转发到后端。

示例：

```ts
server: {
  proxy: {
    '/api': {
      target: 'https://localhost:7002',
      changeOrigin: true,
      secure: false
    }
  }
}
```

注意：

- `target` 必须与后端实际监听地址一致
- 后端为 HTTPS 时通常需要设置 `secure: false`
- 修改代理配置后需要重新启动 `npm run dev`

---

## 二十、项目构建

### 20.1 后端构建

```powershell
dotnet build AuditTracking.Api/AuditTracking.Api.csproj
```

构建成功应显示：

```text
Build succeeded.
0 Error(s)
```

---

### 20.2 前端构建

```powershell
cd audit-tracking-web
npm run build
```

构建成功后生成：

```text
dist
```

目录。

---

## 二十一、首次使用建议

推荐按照以下顺序录入测试数据：

```text
1. 创建审计计划
2. 将审计计划变为 InProgress
3. 创建审计问题
4. 将审计问题变为 Rectifying
5. 创建整改措施
6. 提交整改措施
7. 审批整改措施
8. 完成整改措施
9. 将审计问题变为 PendingVerification
10. 创建整改验证
11. 验证结果选择 Passed
12. 检查审计问题是否自动变为 Closed
```

也可以测试：

```text
验证结果选择 Failed
→ 审计问题重新变为 Rectifying
```

以及：

```text
验证结果选择 NeedMoreEvidence
→ 审计问题重新变为 Rectifying
```

---

## 二十二、安全注意事项

1. 不要提交 User Secrets
2. 不要提交数据库密码
3. 不要提交 JWT SigningKey
4. 不要在 README 中写真实密码
5. 不要在控制台打印 Token
6. 不要在数据库中保存明文密码
7. 不要使用弱初始管理员密码
8. 不要将真实公司数据上传至公开仓库
9. 生产环境应使用环境变量或专业密钥管理服务
10. 生产环境应启用 HTTPS
11. 生产环境不应长期使用默认管理员密码
12. 项目公开前应检查 Git 历史中是否存在敏感信息

---

## 二十三、已实现功能

- [x] 审计计划管理
- [x] 审计问题管理
- [x] 整改措施管理
- [x] 整改验证管理
- [x] 完整业务状态流转
- [x] 验证结果与问题状态联动
- [x] 分页查询
- [x] 多条件筛选
- [x] 详情查看
- [x] 新增和编辑
- [x] 软删除
- [x] 回收站
- [x] 数据恢复
- [x] 操作日志
- [x] 统计仪表盘
- [x] JWT 登录
- [x] 当前用户信息
- [x] 退出登录
- [x] 修改密码
- [x] 修改密码后强制重新登录
- [x] 路由守卫
- [x] 401 自动跳转登录
- [x] 403 权限提示
- [x] 默认管理员初始化
- [x] Admin 和 User 基础角色
- [x] 业务接口统一认证保护
- [x] 数据库 Migration
- [x] 前后端统一响应处理
- [x] 全局异常处理

---

## 二十四、后续可扩展功能

- [ ] 用户管理页面
- [ ] 用户启用和禁用
- [ ] 管理员重置密码
- [ ] 更细粒度角色权限
- [ ] 部门数据权限
- [ ] 附件上传和下载
- [ ] 审计证据管理
- [ ] Excel 导入和导出
- [ ] PDF 审计报告生成
- [ ] 邮件提醒
- [ ] 到期提醒
- [ ] 自动化测试
- [ ] 操作日志高级检索
- [ ] ECharts 可视化
- [ ] Docker 部署
- [ ] CI/CD
- [ ] Refresh Token
- [ ] Token 黑名单
- [ ] 多环境配置
- [ ] 生产环境部署
- [ ] 数据备份和恢复
- [ ] 系统监控和异常告警

---

## 二十五、项目开发经验总结

AuditTracking 的开发过程遵循以下思路：

```text
先梳理业务
→ 再设计数据库
→ 再实现后端
→ 再开发前端
→ 再完成联调
→ 最后补充认证和安全能力
```

项目从最初的审计计划管理，逐步扩展为：

```text
完整业务闭环
操作追溯
状态约束
自动联动
统计展示
用户认证
接口保护
修改密码
```

整个开发过程中最重要的经验包括：

1. 状态机必须先设计，再写页面
2. 业务规则必须由后端最终决定
3. 前端只负责展示和交互，不应成为唯一校验层
4. 关键联动必须使用数据库事务
5. 核心业务操作都应保留日志
6. 审计类数据删除应优先采用软删除
7. 前后端字段必须以实际接口为准
8. DTO 可以避免实体被直接暴露
9. 数据库迁移比手工建表更加可靠
10. 登录功能不仅是一个登录页面
11. 完整登录系统还包括 Token、路由守卫、接口认证、401、403 和当前用户识别
12. 每完成一个模块都应该立即构建和联调
13. 不要等整个项目完成后再一次性排错
14. 页面效果应服务于业务，不应为了视觉效果牺牲可读性
15. 真实企业数据与个人演示数据应严格分离

---

## 二十六、项目完成情况

当前项目已具备：

```text
完整业务流程
真实数据库
前后端联调
状态流转
操作日志
统计仪表盘
登录认证
权限保护
修改密码
软删除与恢复
```

按照课程项目、实习展示、个人作品集和内部演示系统的标准，项目已经具备完整首版系统的基本能力。

---

## 二十七、作者

```text
Richard_Xia @ Suzhou University of Science and Technology
```

---

## 二十八、免责声明

本项目主要用于学习、演示和内部开发实践。

如用于真实生产环境，还应进一步完成：

- 安全审计
- 权限细分
- 数据备份
- 日志留存策略
- 专业密钥管理
- HTTPS 部署
- 自动化测试
- 异常监控
- 数据脱敏
- 合规审查
- 生产环境运维方案
