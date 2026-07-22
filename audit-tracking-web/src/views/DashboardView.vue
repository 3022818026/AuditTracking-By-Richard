<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'

import { getDashboardSummary } from '@/api/dashboard'
import type { DashboardSummary } from '@/types/dashboard'

type TagType = 'primary' | 'success' | 'warning' | 'info' | 'danger'

interface StatusItem {
  key: string
  label: string
  value: number
  tagType: TagType
}

interface StatusGroup {
  title: string
  total: number
  items: StatusItem[]
}

const router = useRouter()
const loading = ref(false)
const errorMessage = ref('')
const summary = ref<DashboardSummary | null>(null)

const auditPlanStatusLabels: Record<string, string> = {
  Draft: '草稿',
  InProgress: '进行中',
  Completed: '已完成',
  Closed: '已关闭',
  Cancelled: '已取消',
}

const auditIssueStatusLabels: Record<string, string> = {
  Open: '待处理',
  Rectifying: '整改中',
  PendingVerification: '待验证',
  Closed: '已关闭',
  Rejected: '已驳回',
}

const severityLabels: Record<string, string> = {
  Low: '低',
  Medium: '中',
  High: '高',
  Critical: '严重',
}

const statCards = computed(() => {
  const data = summary.value
  return [
    { title: '审计计划总数', value: data?.auditPlanTotal ?? 0, description: '当前有效审计计划', tone: 'normal' },
    { title: '进行中计划', value: data?.auditPlanInProgress ?? 0, description: '正在执行的审计计划', tone: 'normal' },
    { title: '逾期计划', value: data?.auditPlanOverdue ?? 0, description: '已超过计划日期', tone: 'warning' },
    { title: '审计问题总数', value: data?.auditIssueTotal ?? 0, description: '当前有效审计问题', tone: 'normal' },
    { title: '整改中问题', value: data?.auditIssueRectifying ?? 0, description: '正在进行问题整改', tone: 'normal' },
    { title: '待验证问题', value: data?.auditIssuePendingVerification ?? 0, description: '等待整改结果验证', tone: 'normal' },
    { title: '逾期问题', value: data?.auditIssueOverdue ?? 0, description: '已超过整改期限', tone: 'warning' },
    { title: '整改措施总数', value: data?.correctiveActionTotal ?? 0, description: '当前有效整改措施', tone: 'normal' },
    { title: '已完成整改措施', value: data?.correctiveActionCompleted ?? 0, description: '状态为已完成', tone: 'success' },
    { title: '逾期整改措施', value: data?.correctiveActionOverdue ?? 0, description: '未完成且超过计划日期', tone: 'warning' },
    { title: '整改验证总数', value: data?.rectificationVerificationTotal ?? 0, description: '当前有效验证记录', tone: 'normal' },
    { title: '验证通过数', value: data?.rectificationVerificationPassed ?? 0, description: '验证结果为通过', tone: 'success' },
  ]
})

const statusGroups = computed<StatusGroup[]>(() => {
  const data = summary.value
  return [
    {
      title: '审计计划状态',
      total: data?.auditPlanTotal ?? 0,
      items: [
        { key: 'Draft', label: '草稿', value: data?.auditPlanDraft ?? 0, tagType: 'info' },
        { key: 'InProgress', label: '进行中', value: data?.auditPlanInProgress ?? 0, tagType: 'primary' },
        { key: 'Completed', label: '已完成', value: data?.auditPlanCompleted ?? 0, tagType: 'success' },
        { key: 'Closed', label: '已关闭', value: data?.auditPlanClosed ?? 0, tagType: 'info' },
        { key: 'Cancelled', label: '已取消', value: data?.auditPlanCancelled ?? 0, tagType: 'danger' },
      ],
    },
    {
      title: '审计问题状态',
      total: data?.auditIssueTotal ?? 0,
      items: [
        { key: 'Open', label: '待处理', value: data?.auditIssueOpen ?? 0, tagType: 'info' },
        { key: 'Rectifying', label: '整改中', value: data?.auditIssueRectifying ?? 0, tagType: 'primary' },
        { key: 'PendingVerification', label: '待验证', value: data?.auditIssuePendingVerification ?? 0, tagType: 'warning' },
        { key: 'Closed', label: '已关闭', value: data?.auditIssueClosed ?? 0, tagType: 'success' },
        { key: 'Rejected', label: '已驳回', value: data?.auditIssueRejected ?? 0, tagType: 'danger' },
      ],
    },
    {
      title: '整改措施状态',
      total: data?.correctiveActionTotal ?? 0,
      items: [
        { key: 'Draft', label: '草稿', value: data?.correctiveActionDraft ?? 0, tagType: 'info' },
        { key: 'Submitted', label: '已提交', value: data?.correctiveActionSubmitted ?? 0, tagType: 'primary' },
        { key: 'Approved', label: '已批准', value: data?.correctiveActionApproved ?? 0, tagType: 'warning' },
        { key: 'Rejected', label: '已驳回', value: data?.correctiveActionRejected ?? 0, tagType: 'danger' },
        { key: 'Completed', label: '已完成', value: data?.correctiveActionCompleted ?? 0, tagType: 'success' },
      ],
    },
    {
      title: '整改验证结果',
      total: data?.rectificationVerificationTotal ?? 0,
      items: [
        { key: 'Passed', label: '通过', value: data?.rectificationVerificationPassed ?? 0, tagType: 'success' },
        { key: 'Failed', label: '不通过', value: data?.rectificationVerificationFailed ?? 0, tagType: 'danger' },
        {
          key: 'NeedMoreEvidence',
          label: '需补充材料',
          value: data?.rectificationVerificationNeedMoreEvidence ?? 0,
          tagType: 'warning',
        },
      ],
    },
  ]
})

const correctiveActionCompletionRate = computed(
  () => summary.value?.correctiveActionCompletionRate ?? 0,
)
const recentAuditPlans = computed(() => summary.value?.recentAuditPlans ?? [])
const recentAuditIssues = computed(() => summary.value?.recentAuditIssues ?? [])

function formatDate(value: string | null) {
  if (!value) return '-'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  })
}

function getStatusPercentage(value: number, total: number) {
  if (total === 0) return 0
  return Number(((value / total) * 100).toFixed(1))
}

function formatCompletionRate(percentage: number) {
  return `${percentage.toFixed(2)}%`
}

function getPlanStatusLabel(status: string) {
  return auditPlanStatusLabels[status] ?? status
}

function getIssueStatusLabel(status: string) {
  return auditIssueStatusLabels[status] ?? status
}

function getPlanStatusType(status: string): TagType {
  if (status === 'Completed' || status === 'Closed') return 'success'
  if (status === 'InProgress') return 'primary'
  if (status === 'Cancelled') return 'danger'
  return 'info'
}

function getIssueStatusType(status: string): TagType {
  if (status === 'Closed') return 'success'
  if (status === 'Rejected') return 'danger'
  if (status === 'PendingVerification') return 'warning'
  if (status === 'Rectifying') return 'primary'
  return 'info'
}

function getSeverityLabel(severity: string) {
  return severityLabels[severity] ?? severity
}

function getSeverityType(severity: string): TagType {
  if (severity === 'Critical') return 'danger'
  if (severity === 'High') return 'warning'
  if (severity === 'Low') return 'info'
  return 'primary'
}

async function loadDashboard() {
  if (loading.value) return
  loading.value = true
  errorMessage.value = ''
  try {
    summary.value = await getDashboardSummary()
  } catch (error) {
    console.error(error)
    summary.value = null
    errorMessage.value = error instanceof Error && error.message
      ? error.message
      : '仪表盘统计加载失败'
    ElMessage.error(errorMessage.value)
  } finally {
    loading.value = false
  }
}

function viewAll(path: '/audit-plans' | '/audit-issues') {
  router.push(path)
}

onMounted(() => {
  loadDashboard()
})
</script>

<template>
  <div v-loading="loading" class="dashboard-page">
    <header class="dashboard-header">
      <div>
        <h2>审计跟踪管理系统</h2>
        <p>审计计划、问题整改与验证执行情况总览</p>
      </div>
      <el-button :loading="loading" @click="loadDashboard">重新加载</el-button>
    </header>

    <el-empty v-if="!loading && !summary" :description="errorMessage || '暂无统计数据'">
      <el-button type="primary" @click="loadDashboard">重新加载</el-button>
    </el-empty>

    <template v-else-if="summary">
      <section class="stat-grid" aria-label="核心统计">
        <el-card
          v-for="card in statCards"
          :key="card.title"
          shadow="hover"
          class="stat-card"
          :class="`stat-card--${card.tone}`"
        >
          <div class="stat-title">{{ card.title }}</div>
          <div class="stat-value">{{ card.value }}</div>
          <div class="stat-description">{{ card.description }}</div>
        </el-card>
      </section>

      <section class="overview-grid">
        <el-card shadow="never" class="completion-card">
          <template #header>
            <div class="section-heading">
              <div>
                <h3>整改措施完成率</h3>
                <p>完成率由后端汇总结果直接提供</p>
              </div>
              <strong>{{ correctiveActionCompletionRate.toFixed(2) }}%</strong>
            </div>
          </template>
          <el-progress
            :percentage="correctiveActionCompletionRate"
            :stroke-width="14"
            :format="formatCompletionRate"
          />
          <div class="completion-summary">
            已完成 {{ summary.correctiveActionCompleted }} 项，共 {{ summary.correctiveActionTotal }} 项
          </div>
        </el-card>

        <el-card shadow="never" class="risk-card">
          <template #header><h3>逾期提醒</h3></template>
          <div class="risk-list">
            <div><span>审计计划</span><strong>{{ summary.auditPlanOverdue }}</strong></div>
            <div><span>审计问题</span><strong>{{ summary.auditIssueOverdue }}</strong></div>
            <div><span>整改措施</span><strong>{{ summary.correctiveActionOverdue }}</strong></div>
          </div>
        </el-card>
      </section>

      <section>
        <div class="section-title">
          <div>
            <h3>状态分布</h3>
            <p>各业务模块当前状态数量及占比</p>
          </div>
        </div>
        <div class="distribution-grid">
          <el-card v-for="group in statusGroups" :key="group.title" shadow="never" class="distribution-card">
            <template #header>
              <div class="distribution-header">
                <h4>{{ group.title }}</h4>
                <span>共 {{ group.total }} 条</span>
              </div>
            </template>
            <div class="distribution-list">
              <div v-for="item in group.items" :key="item.key" class="distribution-item">
                <div class="distribution-meta">
                  <el-tag :type="item.tagType" effect="plain">{{ item.label }}</el-tag>
                  <span>{{ item.value }}（{{ getStatusPercentage(item.value, group.total) }}%）</span>
                </div>
                <el-progress
                  :percentage="getStatusPercentage(item.value, group.total)"
                  :stroke-width="7"
                  :show-text="false"
                />
              </div>
            </div>
          </el-card>
        </div>
      </section>

      <section class="recent-section">
        <el-card shadow="never" class="table-card">
          <template #header>
            <div class="table-header">
              <div>
                <h3>最近审计计划</h3>
                <p>按创建时间显示最近 5 条记录</p>
              </div>
              <el-button type="primary" link @click="viewAll('/audit-plans')">查看全部</el-button>
            </div>
          </template>
          <div class="table-responsive">
            <el-table :data="recentAuditPlans" stripe style="min-width: 920px">
              <el-table-column prop="auditNo" label="计划编号" width="140" />
              <el-table-column prop="title" label="标题" min-width="190" show-overflow-tooltip />
              <el-table-column label="状态" width="110">
                <template #default="{ row }">
                  <el-tag :type="getPlanStatusType(row.status)">{{ getPlanStatusLabel(row.status) }}</el-tag>
                </template>
              </el-table-column>
              <el-table-column label="计划日期" width="130">
                <template #default="{ row }">{{ formatDate(row.plannedDate) }}</template>
              </el-table-column>
              <el-table-column prop="auditee" label="被审计对象" min-width="140" show-overflow-tooltip>
                <template #default="{ row }">{{ row.auditee || '-' }}</template>
              </el-table-column>
              <el-table-column prop="auditor" label="审计人员" min-width="140" show-overflow-tooltip>
                <template #default="{ row }">{{ row.auditor || '-' }}</template>
              </el-table-column>
              <el-table-column label="创建时间" width="130">
                <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
              </el-table-column>
            </el-table>
          </div>
        </el-card>

        <el-card shadow="never" class="table-card">
          <template #header>
            <div class="table-header">
              <div>
                <h3>最近审计问题</h3>
                <p>按创建时间显示最近 5 条记录</p>
              </div>
              <el-button type="primary" link @click="viewAll('/audit-issues')">查看全部</el-button>
            </div>
          </template>
          <div class="table-responsive">
            <el-table :data="recentAuditIssues" stripe style="min-width: 1080px">
              <el-table-column prop="issueNo" label="问题编号" width="140" />
              <el-table-column prop="title" label="标题" min-width="190" show-overflow-tooltip />
              <el-table-column label="严重程度" width="110">
                <template #default="{ row }">
                  <el-tag :type="getSeverityType(row.severity)">{{ getSeverityLabel(row.severity) }}</el-tag>
                </template>
              </el-table-column>
              <el-table-column label="状态" width="110">
                <template #default="{ row }">
                  <el-tag :type="getIssueStatusType(row.status)">{{ getIssueStatusLabel(row.status) }}</el-tag>
                </template>
              </el-table-column>
              <el-table-column label="到期日期" width="130">
                <template #default="{ row }">{{ formatDate(row.dueDate) }}</template>
              </el-table-column>
              <el-table-column prop="responsibleDepartment" label="责任部门" min-width="140" show-overflow-tooltip>
                <template #default="{ row }">{{ row.responsibleDepartment || '-' }}</template>
              </el-table-column>
              <el-table-column prop="responsiblePerson" label="责任人" min-width="120" show-overflow-tooltip>
                <template #default="{ row }">{{ row.responsiblePerson || '-' }}</template>
              </el-table-column>
              <el-table-column label="创建时间" width="130">
                <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
              </el-table-column>
            </el-table>
          </div>
        </el-card>
      </section>
    </template>
  </div>
</template>

<style scoped>
.dashboard-page {
  min-height: 320px;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.dashboard-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.dashboard-header h2,
.section-title h3,
.section-heading h3,
.table-header h3,
.distribution-header h4,
.risk-card h3 {
  margin: 0;
}

.dashboard-header p,
.section-title p,
.section-heading p,
.table-header p {
  margin: 6px 0 0;
  color: #909399;
  font-size: 13px;
}

.stat-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(185px, 1fr));
  gap: 14px;
}

.stat-card {
  position: relative;
  border: 0;
  border-radius: 10px;
  overflow: hidden;
}

.stat-card::before {
  content: '';
  position: absolute;
  inset: 0 auto 0 0;
  width: 3px;
  background: #a8abb2;
}

.stat-card--warning::before { background: #e6a23c; }
.stat-card--success::before { background: #67c23a; }
.stat-card--normal::before { background: #409eff; }
.stat-title { color: #606266; font-size: 14px; }
.stat-value { margin: 12px 0 8px; color: #303133; font-size: 30px; font-weight: 700; line-height: 1; }
.stat-description { color: #909399; font-size: 12px; }

.overview-grid {
  display: grid;
  grid-template-columns: minmax(0, 2fr) minmax(240px, 1fr);
  gap: 16px;
}

.completion-card,
.risk-card,
.distribution-card,
.table-card {
  border-radius: 10px;
}

.section-heading,
.distribution-header,
.table-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.section-heading strong { color: #409eff; font-size: 24px; }
.completion-summary { margin-top: 14px; color: #606266; font-size: 13px; }
.risk-list { display: grid; gap: 14px; }
.risk-list > div { display: flex; align-items: center; justify-content: space-between; color: #606266; }
.risk-list strong { color: #e6a23c; font-size: 20px; }

.section-title { margin-bottom: 12px; }
.distribution-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
}

.distribution-header span { color: #909399; font-size: 13px; }
.distribution-list { display: grid; gap: 16px; }
.distribution-meta { display: flex; align-items: center; justify-content: space-between; margin-bottom: 7px; }
.distribution-meta span { color: #606266; font-size: 13px; }
.recent-section { display: grid; gap: 16px; }
.table-responsive { width: 100%; overflow-x: auto; }

@media (max-width: 1050px) {
  .overview-grid,
  .distribution-grid { grid-template-columns: 1fr; }
}

@media (max-width: 640px) {
  .dashboard-header,
  .section-heading,
  .table-header { align-items: flex-start; flex-direction: column; }
  .stat-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}
</style>
