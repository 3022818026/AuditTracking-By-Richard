# AuditTracking-ReMedy 任务执行记录

## 任务背景
- 项目仓库：AuditTracking-ReMedy
- 前端项目目录：audit-tracking-web
- 后端项目目录：AuditTracking.Api
- 本次工作重点：围绕审计计划、审计问题、整改措施三个前端模块完成基础页面与接口接入，且严格限制仅修改前端代码。

## 本次执行的主要内容

### 1. 前端基础结构
- 保留现有布局、菜单、路由和 `request.ts`。
- 维持 `X-User-Name: frontend-user` 请求头。
- 所有前端接口调用通过现有 `request.ts` 封装完成，未直接使用 `axios`。

### 2. 审计计划模块
- 复用并完善审计计划列表页面相关前端能力。
- 重点处理表格布局与列宽：
  - 调整列宽与列顺序
  - 使中间字段不被异常压缩
  - 操作列固定右侧
  - 增加横向滚动容器，适配较窄屏幕
- 目标是保持功能不变，仅优化列表展示体验。

### 3. 审计问题模块
- 读取后端 `AuditIssuesController` 的实际接口定义。
- 新增前端类型定义文件：
  - `src/types/auditIssue.ts`
- 新增前端 API 封装文件：
  - `src/api/auditIssues.ts`
- 新增页面：
  - `src/views/AuditIssuesView.vue`
- 实现内容包括：
  - 查询与重置
  - 审计计划下拉选择
  - 分页列表
  - 新增/编辑
  - 详情
  - 状态变更
  - 删除
  - 回收站与恢复
  - 操作日志

### 4. 整改措施模块
- 读取后端 `CorrectiveActionsController` 的实际接口定义。
- 新增前端类型定义文件：
  - `src/types/correctiveAction.ts`
- 新增前端 API 封装文件：
  - `src/api/correctiveActions.ts`
- 新增页面：
  - `src/views/CorrectiveActionsView.vue`
- 实现内容包括：
  - 查询与重置
  - 审计问题下拉选择
  - 分页列表
  - 新增/编辑
  - 详情
  - 状态变更
  - 删除
  - 回收站与恢复
  - 操作日志

## 关键文件清单

### 前端新增/修改
- `audit-tracking-web/src/types/auditIssue.ts`
- `audit-tracking-web/src/api/auditIssues.ts`
- `audit-tracking-web/src/views/AuditIssuesView.vue`
- `audit-tracking-web/src/types/correctiveAction.ts`
- `audit-tracking-web/src/api/correctiveActions.ts`
- `audit-tracking-web/src/views/CorrectiveActionsView.vue`
- `audit-tracking-web/src/views/audit-plans/AuditPlanListView.vue`
- `audit-tracking-web/src/utils/request.ts`

## 后端读取情况
- 读取并参考了以下控制器与 DTO：
  - `AuditTracking.Api/Controllers/AuditIssuesController.cs`
  - `AuditTracking.Api/Controllers/CorrectiveActionsController.cs`
  - `AuditTracking.Api/Dtos/AuditIssues/*`
  - `AuditTracking.Api/Dtos/CorrectiveActions/*`
  - `AuditTracking.Api/Entities/AuditIssue.cs`
  - `AuditTracking.Api/Entities/CorrectiveAction.cs`

## 约束说明
- 未修改后端代码。
- 未修改数据库和 Migration。
- 未新增登录/JWT/权限系统。
- 未开发整改验证真实页面。
- 未修改 AdminLayout 的整体结构。

## 验证结果
- 已执行前端构建命令：
  - `npm run build`
- 构建结果：成功。
- 构建过程中出现 chunk size 警告，但不影响构建通过。

## 备注
- 后续如需继续完善，可进一步补强：
  - 前端状态流转规则与后端状态机完全一致的可选项过滤
  - 表单校验规则精确对齐后端 DTO
  - 审计问题/整改措施下拉框改成远程搜索模式
