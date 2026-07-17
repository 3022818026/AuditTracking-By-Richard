<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'

import {
  getAuditPlans,
  getAuditPlanDetail,
  createAuditPlan,
  updateAuditPlan,
  deleteAuditPlan,
  getAuditPlanStatistics,
  getAuditPlanRiskStatistics,
  getAuditPlanRiskList,
  getRecycleBin,
  restoreAuditPlan,
  getAuditPlanLogs,
} from '@/api/audit-plans'

import type {
  AuditPlan,
  AuditPlanOperationLog,
  AuditPlanQuery,
  AuditPlanRiskQuery,
  AuditPlanRiskStatistics,
  AuditPlanStatistics,
  CreateAuditPlanData,
} from '@/types/audit-plan'
import type { FormInstance } from 'element-plus'

const loading = ref(false)
const total = ref(0)
const plans = ref<AuditPlan[]>([])
const statistics = ref<AuditPlanStatistics>({
  total: 0,
  draft: 0,
  inProgress: 0,
  completed: 0,
  closed: 0,
  cancelled: 0,
})

const riskStats = ref<AuditPlanRiskStatistics>({
  overdue: 0,
  dueWithinSevenDays: 0,
  completedThisMonth: 0,
})

const query = reactive<AuditPlanQuery>({
  keyword: '',
  status: '',
  auditType: '',
  startDate: undefined,
  endDate: undefined,
  page: 1,
  pageSize: 10,
})

type StatusCardKey =
  | 'all'
  | 'Draft'
  | 'InProgress'
  | 'Completed'
  | 'Closed'
  | 'Cancelled'
  | 'Overdue'
  | 'DueSoon'
  | 'CompletedThisMonth'

const selectedCard = ref<StatusCardKey>('all')
const currentRiskType = ref<'Overdue' | 'DueSoon' | ''>('')
const viewMode = ref<'list' | 'risk'>('list')

const formDialogVisible = ref(false)
const recycleDialogVisible = ref(false)
const logsDialogVisible = ref(false)
const formLoading = ref(false)
const recycleLoading = ref(false)
const logLoading = ref(false)
const deleteLoading = ref(false)
const formRef = ref<FormInstance>()
const recycleList = ref<AuditPlan[]>([])
const operationLogs = ref<AuditPlanOperationLog[]>([])
const logTitle = ref('操作日志')
const formMode = ref<'create' | 'edit'>('create')

const form = reactive({
  id: undefined as number | undefined,
  auditNo: '',
  title: '',
  auditType: '',
  plannedDate: '',
  auditee: '',
  auditor: '',
  status: 'Draft',
  result: '',
  remark: '',
})

const formRules = {
  auditNo: [
    { required: true, message: '请输入审计编号', trigger: 'blur' },
  ],
  title: [
    { required: true, message: '请输入审计标题', trigger: 'blur' },
  ],
  plannedDate: [
    { required: true, message: '请选择计划日期', trigger: 'change' },
  ],
  status: [
    { required: true, message: '请选择状态', trigger: 'change' },
  ],
}

const statusCards = computed<Array<{ key: StatusCardKey; label: string; count: number }>>(() => [
  { key: 'all', label: '全部', count: statistics.value.total },
  { key: 'Draft', label: '草稿', count: statistics.value.draft },
  { key: 'InProgress', label: '进行中', count: statistics.value.inProgress },
  { key: 'Completed', label: '已完成', count: statistics.value.completed },
  { key: 'Closed', label: '已关闭', count: statistics.value.closed },
  { key: 'Cancelled', label: '已取消', count: statistics.value.cancelled },
])

const riskCards = computed<Array<{ key: StatusCardKey; label: string; count: number }>>(() => [
  { key: 'Overdue', label: '逾期计划', count: riskStats.value.overdue },
  { key: 'DueSoon', label: '7天内到期', count: riskStats.value.dueWithinSevenDays },
  { key: 'CompletedThisMonth', label: '本月已完成', count: riskStats.value.completedThisMonth },
])

const tableEmptyText = computed(() => {
  return loading.value ? '正在加载...' : '暂无审计计划数据'
})

function formatDate(value: string | null) {
  if (!value) {
    return '-'
  }

  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  })
}

function formatStatus(status: string) {
  const statusMap: Record<string, string> = {
    Draft: '草稿',
    InProgress: '进行中',
    Completed: '已完成',
    Closed: '已关闭',
    Cancelled: '已取消',
  }

  return statusMap[status] ?? status
}

function getStatusType(status: string) {
  const typeMap: Record<string, 'primary' | 'success' | 'info' | 'warning' | 'danger'> = {
    Draft: 'info',
    InProgress: 'warning',
    Completed: 'success',
    Closed: 'primary',
    Cancelled: 'danger',
  }

  return typeMap[status] ?? 'info'
}

function formatOperationType(type: string) {
  const map: Record<string, string> = {
    Create: '新增',
    Update: '修改',
    Delete: '删除',
    Restore: '恢复',
  }

  return map[type] ?? type
}

function formatJson(value: string | null) {
  if (!value) {
    return '-'
  }

  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}

async function loadStatistics() {
  try {
    const result = await getAuditPlanStatistics()
    statistics.value = result
  } catch (error) {
    console.error(error)
    ElMessage.error('状态统计加载失败，请检查后端服务')
  }
}

async function loadRiskStatistics() {
  try {
    const result = await getAuditPlanRiskStatistics()
    riskStats.value = result
  } catch (error) {
    console.error(error)
    ElMessage.error('风险统计加载失败，请检查后端服务')
  }
}

async function loadPlans() {
  loading.value = true

  try {
    if (viewMode.value === 'risk' && currentRiskType.value) {
      const riskParams: AuditPlanRiskQuery = {
        type: currentRiskType.value,
        page: query.page,
        pageSize: query.pageSize,
      }
      const result = await getAuditPlanRiskList(riskParams)
      plans.value = result.items
      total.value = result.total
    } else {
      const result = await getAuditPlans(query)
      plans.value = result.items
      total.value = result.total
    }
  } catch (error: unknown) {
    console.error(error)
    plans.value = []
    total.value = 0
    ElMessage.error('列表加载失败，请确认后端服务已启动')
  } finally {
    loading.value = false
  }
}

async function refreshPageData() {
  await Promise.all([loadPlans(), loadStatistics(), loadRiskStatistics()])
}

function selectStatusCard(key: StatusCardKey) {
  selectedCard.value = key
  if (key === 'Overdue' || key === 'DueSoon') {
    viewMode.value = 'risk'
    currentRiskType.value = key
    query.status = ''
    query.page = 1
    loadPlans()
    return
  }

  viewMode.value = 'list'
  currentRiskType.value = ''
  query.page = 1
  query.status = key === 'all' || key === 'CompletedThisMonth' ? (key === 'CompletedThisMonth' ? 'Completed' : '') : key
  loadPlans()
}

function handleSearch() {
  viewMode.value = 'list'
  selectedCard.value = 'all'
  query.page = 1
  loadPlans()
}

function handleReset() {
  viewMode.value = 'list'
  selectedCard.value = 'all'
  query.keyword = ''
  query.status = ''
  query.auditType = ''
  query.startDate = undefined
  query.endDate = undefined
  query.page = 1
  loadPlans()
}

function handlePageChange(page: number) {
  query.page = page
  loadPlans()
}

function handlePageSizeChange(pageSize: number) {
  query.pageSize = pageSize
  query.page = 1
  loadPlans()
}

function resetForm() {
  form.id = undefined
  form.auditNo = ''
  form.title = ''
  form.auditType = ''
  form.plannedDate = ''
  form.auditee = ''
  form.auditor = ''
  form.status = 'Draft'
  form.result = ''
  form.remark = ''
}

function openCreateDialog() {
  formMode.value = 'create'
  resetForm()
  form.status = 'Draft'
  form.result = ''
  formDialogVisible.value = true
}

async function openEditDialog(row: AuditPlan) {
  formMode.value = 'edit'
  formDialogVisible.value = true
  formLoading.value = true

  try {
    const result = await getAuditPlanDetail(row.id)
    const plan = result.plan
    form.id = plan.id
    form.auditNo = plan.auditNo
    form.title = plan.title
    form.auditType = plan.auditType ?? ''
    form.plannedDate = plan.plannedDate
    form.auditee = plan.auditee ?? ''
    form.auditor = plan.auditor ?? ''
    form.status = plan.status
    form.result = plan.result ?? ''
    form.remark = plan.remark ?? ''
  } catch (error) {
    console.error(error)
    ElMessage.error('审计计划详情加载失败')
    formDialogVisible.value = false
  } finally {
    formLoading.value = false
  }
}

const statusOptions = computed(() => {
  const current = form.status
  if (formMode.value === 'create') {
    return [
      { label: 'Draft', value: 'Draft' },
      { label: 'InProgress', value: 'InProgress' },
      { label: 'Completed', value: 'Completed' },
      { label: 'Closed', value: 'Closed' },
      { label: 'Cancelled', value: 'Cancelled' },
    ]
  }

  if (current === 'Draft') {
    return [
      { label: 'Draft', value: 'Draft' },
      { label: 'InProgress', value: 'InProgress' },
      { label: 'Cancelled', value: 'Cancelled' },
    ]
  }

  if (current === 'InProgress') {
    return [
      { label: 'InProgress', value: 'InProgress' },
      { label: 'Completed', value: 'Completed' },
      { label: 'Cancelled', value: 'Cancelled' },
    ]
  }

  if (current === 'Completed') {
    return [
      { label: 'Completed', value: 'Completed' },
      { label: 'Closed', value: 'Closed' },
    ]
  }

  if (current === 'Closed') {
    return [{ label: 'Closed', value: 'Closed' }]
  }

  if (current === 'Cancelled') {
    return [{ label: 'Cancelled', value: 'Cancelled' }]
  }

  return [
    { label: 'Draft', value: 'Draft' },
    { label: 'InProgress', value: 'InProgress' },
    { label: 'Completed', value: 'Completed' },
    { label: 'Closed', value: 'Closed' },
    { label: 'Cancelled', value: 'Cancelled' },
  ]
})

const canChangeStatus = computed(() => {
  return !['Closed', 'Cancelled'].includes(form.status)
})

async function submitForm() {
  if (!formRef.value) {
    return
  }

  await formRef.value.validate(async (valid) => {
    if (!valid) {
      return
    }

    formLoading.value = true

    try {
      if (formMode.value === 'create') {
        const payload: CreateAuditPlanData = {
          auditNo: form.auditNo,
          title: form.title,
          auditType: form.auditType || undefined,
          plannedDate: form.plannedDate,
          auditee: form.auditee || undefined,
          auditor: form.auditor || undefined,
          remark: form.remark || undefined,
        }
        await createAuditPlan(payload)
        ElMessage.success('审计计划新增成功')
      } else {
        await updateAuditPlan(form.id ?? 0, {
          title: form.title,
          auditType: form.auditType || undefined,
          plannedDate: form.plannedDate,
          auditee: form.auditee || undefined,
          auditor: form.auditor || undefined,
          status: form.status,
          result: form.result || undefined,
          remark: form.remark || undefined,
        })
        ElMessage.success('审计计划保存成功')
      }
      formDialogVisible.value = false
      resetForm()
      refreshPageData()
    } catch (error: unknown) {
      console.error(error)
      ElMessage.error('保存审计计划失败，请稍后重试')
    } finally {
      formLoading.value = false
    }
  })
}

async function handleDelete(row: AuditPlan) {
  try {
    await ElMessageBox.confirm('确定要删除该审计计划吗？', '删除确认', {
      type: 'warning',
    })
    deleteLoading.value = true
    await deleteAuditPlan(row.id)
    ElMessage.success('审计计划删除成功')
    refreshPageData()
  } catch (error: unknown) {
    if (error !== 'cancel') {
      console.error(error)
      ElMessage.error('删除审计计划失败')
    }
  } finally {
    deleteLoading.value = false
  }
}

async function openRecycleDialog() {
  recycleDialogVisible.value = true
  await loadRecycleBin()
}

async function loadRecycleBin() {
  recycleLoading.value = true

  try {
    recycleList.value = await getRecycleBin()
  } catch (error) {
    console.error(error)
    ElMessage.error('回收站加载失败，请稍后重试')
  } finally {
    recycleLoading.value = false
  }
}

async function handleRestore(row: AuditPlan) {
  try {
    await restoreAuditPlan(row.id)
    ElMessage.success('审计计划恢复成功')
    await loadRecycleBin()
    refreshPageData()
  } catch (error) {
    console.error(error)
    ElMessage.error('恢复失败，请稍后重试')
  }
}

async function openLogsDialog(row: AuditPlan) {
  logsDialogVisible.value = true
  logTitle.value = `审计计划日志：${row.auditNo}`
  logLoading.value = true

  try {
    operationLogs.value = await getAuditPlanLogs(row.id)
  } catch (error) {
    console.error(error)
    ElMessage.error('日志加载失败，请稍后重试')
    operationLogs.value = []
  } finally {
    logLoading.value = false
  }
}

onMounted(() => {
  refreshPageData()
})
</script>

<template>
  <div class="page-shell">
    <div class="page-header">
      <div>
        <p class="eyebrow">AuditTrack • 审计中心</p>
        <h2>审计计划管理</h2>
        <p class="sub-title">查看、筛选和跟踪审计计划执行进度</p>
      </div>
      <div class="header-actions">
        <el-button type="primary" @click="openCreateDialog">新增审计计划</el-button>
        <el-button @click="openRecycleDialog">回收站</el-button>
      </div>
    </div>

    <div class="status-card-row">
      <div
        v-for="card in statusCards"
        :key="card.key"
        class="status-card"
        :class="{ active: selectedCard === card.key }"
        @click="selectStatusCard(card.key)"
      >
        <p>{{ card.label }}</p>
        <strong>{{ card.count }}</strong>
      </div>
    </div>

    <div class="risk-card-row">
      <div
        v-for="card in riskCards"
        :key="card.key"
        class="risk-card"
        :class="{ active: selectedCard === card.key }"
        @click="selectStatusCard(card.key)"
      >
        <p>{{ card.label }}</p>
        <strong>{{ card.count }}</strong>
      </div>
    </div>

    <el-card shadow="never" class="filter-card">
      <el-form ref="formRef" :inline="true" :model="query" class="search-form">
        <el-form-item label="关键词">
          <el-input
            v-model="query.keyword"
            placeholder="编号、标题或被审计对象"
            clearable
            style="width: 240px"
            @keyup.enter="handleSearch"
          />
        </el-form-item>

        <el-form-item label="状态">
          <el-select v-model="query.status" placeholder="全部状态" clearable style="width: 160px">
            <el-option label="草稿" value="Draft" />
            <el-option label="进行中" value="InProgress" />
            <el-option label="已完成" value="Completed" />
            <el-option label="已关闭" value="Closed" />
            <el-option label="已取消" value="Cancelled" />
          </el-select>
        </el-form-item>

        <el-form-item label="审计类型">
          <el-select
            v-model="query.auditType"
            placeholder="全部类型"
            clearable
            allow-create
            filterable
            style="width: 180px"
          >
            <el-option label="内部审计" value="内部审计" />
            <el-option label="外部审计" value="外部审计" />
            <el-option label="供应商审计" value="供应商审计" />
            <el-option label="过程审计" value="过程审计" />
          </el-select>
        </el-form-item>

        <el-form-item>
          <el-button type="primary" @click="handleSearch">查询</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-card shadow="never" class="table-card">
      <div class="table-toolbar">
        <span class="table-title">审计计划列表</span>
        <span class="table-tip">支持关键词、状态和类型联动筛选</span>
      </div>

      <div class="table-responsive">
        <el-table v-loading="loading" :data="plans" :empty-text="tableEmptyText" border stripe style="min-width:1530px">
          <el-table-column prop="auditNo" label="审计编号" width="130" fixed="left" />
          <el-table-column prop="title" label="审计标题" min-width="220" show-overflow-tooltip />
          <el-table-column prop="auditType" label="审计类型" width="130">
          <template #default="{ row }">
            {{ row.auditType || '-' }}
          </template>
        </el-table-column>
          <el-table-column prop="auditee" label="被审计对象" min-width="180" show-overflow-tooltip>
          <template #default="{ row }">
            {{ row.auditee || '-' }}
          </template>
        </el-table-column>
          <el-table-column prop="auditor" label="审计人员" width="140">
          <template #default="{ row }">
            {{ row.auditor || '-' }}
          </template>
        </el-table-column>
          <el-table-column prop="status" label="状态" width="110">
          <template #default="{ row }">
            <el-tag :type="getStatusType(row.status)">{{ formatStatus(row.status) }}</el-tag>
          </template>
        </el-table-column>
          <el-table-column label="计划日期" width="140">
          <template #default="{ row }">
            {{ formatDate(row.plannedDate) }}
          </template>
        </el-table-column>
          <el-table-column prop="result" label="审计结果" min-width="160" show-overflow-tooltip>
          <template #default="{ row }">
            {{ row.result || '-' }}
          </template>
        </el-table-column>
          <el-table-column prop="createdAt" label="创建时间" width="170">
            <template #default="{ row }">
              {{ formatDate(row.createdAt) }}
            </template>
          </el-table-column>
          <el-table-column prop="remark" label="备注" min-width="180" show-overflow-tooltip>
            <template #default="{ row }">
              {{ row.remark || '-' }}
            </template>
          </el-table-column>
          <el-table-column label="操作" width="230" fixed="right" header-align="center">
            <template #default="{ row }">
              <div class="action-buttons">
                <el-button type="text" @click="openEditDialog(row)">编辑</el-button>
                <el-button type="text" @click="handleDelete(row)">删除</el-button>
                <el-button type="text" @click="openLogsDialog(row)">日志</el-button>
              </div>
            </template>
          </el-table-column>
        </el-table>
      </div>

      <div class="pagination-wrapper">
        <el-pagination
          v-model:current-page="query.page"
          v-model:page-size="query.pageSize"
          background
          layout="total, sizes, prev, pager, next, jumper"
          :page-sizes="[10, 20, 50, 100]"
          :total="total"
          @current-change="handlePageChange"
          @size-change="handlePageSizeChange"
        />
      </div>
    </el-card>

    <el-dialog :title="formMode === 'create' ? '新增审计计划' : '编辑审计计划'" v-model="formDialogVisible" width="700px">
      <el-form ref="formRef" :model="form" :rules="formRules" label-width="120px">
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="审计编号" prop="auditNo">
              <el-input v-model="form.auditNo" :disabled="formMode === 'edit'" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="审计标题" prop="title">
              <el-input v-model="form.title" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="审计类型">
              <el-input v-model="form.auditType" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="计划日期" prop="plannedDate">
              <el-date-picker
                v-model="form.plannedDate"
                type="datetime"
                placeholder="选择计划日期"
                value-format="YYYY-MM-DDTHH:mm:ss"
                format="yyyy-MM-dd HH:mm"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="被审计对象">
              <el-input v-model="form.auditee" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="审计人员">
              <el-input v-model="form.auditor" />
            </el-form-item>
          </el-col>
          <el-col :span="12" v-if="formMode === 'edit'">
            <el-form-item label="状态" prop="status">
              <el-select v-model="form.status" :disabled="!canChangeStatus" style="width: 100%">
                <el-option v-for="option in statusOptions" :key="option.value" :label="formatStatus(option.value)" :value="option.value" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12" v-if="formMode === 'edit'">
            <el-form-item label="审计结果">
              <el-input v-model="form.result" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="备注">
              <el-input type="textarea" v-model="form.remark" rows="3" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <template #footer>
        <el-button @click="formDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="formLoading" @click="submitForm">保存</el-button>
      </template>
    </el-dialog>

    <el-dialog title="回收站" v-model="recycleDialogVisible" width="900px">
      <el-table v-loading="recycleLoading" :data="recycleList" border stripe>
        <el-table-column prop="auditNo" label="审计编号" width="150" />
        <el-table-column prop="title" label="审计标题" min-width="240" show-overflow-tooltip />
        <el-table-column prop="deletedAt" label="删除时间" width="180">
          <template #default="{ row }">
            {{ formatDate(row.deletedAt || '') }}
          </template>
        </el-table-column>
        <el-table-column prop="auditor" label="审计人员" width="140" />
        <el-table-column label="操作" width="130">
          <template #default="{ row }">
            <el-button type="text" @click="handleRestore(row)">恢复</el-button>
          </template>
        </el-table-column>
      </el-table>
      <template #footer>
        <el-button @click="recycleDialogVisible = false">关闭</el-button>
      </template>
    </el-dialog>

    <el-dialog :title="logTitle" v-model="logsDialogVisible" width="900px">
      <el-table v-loading="logLoading" :data="operationLogs" border stripe>
        <el-table-column label="操作类型" width="120">
          <template #default="{ row }">
            {{ formatOperationType(row.operationType) }}
          </template>
        </el-table-column>
        <el-table-column prop="operator" label="操作人" width="120" />
        <el-table-column prop="createdAt" label="操作时间" width="180" />
        <el-table-column label="修改前" min-width="260">
          <template #default="{ row }">
            <pre class="log-data">{{ formatJson(row.beforeData) }}</pre>
          </template>
        </el-table-column>
        <el-table-column label="修改后" min-width="260">
          <template #default="{ row }">
            <pre class="log-data">{{ formatJson(row.afterData) }}</pre>
          </template>
        </el-table-column>
      </el-table>
      <template #footer>
        <el-button @click="logsDialogVisible = false">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.page-shell {
  min-height: 100vh;
  padding: 24px;
  background: linear-gradient(135deg, #faf7ff 0%, #f5f2ff 100%);
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
  padding: 24px 28px;
  border-radius: 20px;
  background: linear-gradient(135deg, #ffffff 0%, #f8f2ff 100%);
  border: 1px solid #efe4ff;
  box-shadow: 0 10px 30px rgba(116, 78, 199, 0.08);
}

.eyebrow {
  margin: 0 0 6px;
  color: #8a5cf6;
  font-size: 12px;
  letter-spacing: 0.24em;
  text-transform: uppercase;
  font-weight: 700;
}

.page-header h2 {
  margin: 0;
  color: #34224f;
  font-size: 24px;
}

.sub-title {
  margin: 6px 0 0;
  color: #7c7390;
  font-size: 14px;
}

.header-actions {
  display: flex;
  gap: 10px;
}

.status-card-row,
.risk-card-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
  gap: 16px;
  margin-bottom: 16px;
}

.status-card,
.risk-card {
  padding: 18px 20px;
  border-radius: 18px;
  background: #ffffff;
  border: 1px solid transparent;
  box-shadow: 0 8px 24px rgba(116, 78, 199, 0.06);
  cursor: pointer;
  transition: all 0.2s ease;
}

.status-card.active,
.risk-card.active {
  border-color: #8a5cf6;
  background: linear-gradient(135deg, #f5f2ff 0%, #eef0ff 100%);
}

.status-card p,
.risk-card p {
  margin: 0 0 10px;
  color: #7c7390;
  font-size: 14px;
}

.status-card strong,
.risk-card strong {
  font-size: 24px;
  color: #34224f;
}

.filter-card,
.table-card {
  border: none;
  border-radius: 18px;
  background: #ffffff;
  box-shadow: 0 10px 28px rgba(116, 78, 199, 0.06);
}

.search-form {
  margin-bottom: -16px;
}

.table-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.table-title {
  font-size: 15px;
  font-weight: 700;
  color: #34224f;
}

.table-tip {
  font-size: 12px;
  color: #8b82a1;
}

.pagination-wrapper {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
}

.table-responsive {
  overflow-x: auto;
}

.table-responsive .el-table {
  /* ensure table respects min-width set on table element */
}

.action-buttons {
  display: flex;
  gap: 8px;
  align-items: center;
  white-space: nowrap;
}

.log-data {
  white-space: pre-wrap;
  word-break: break-word;
  font-size: 12px;
  color: #333;
  margin: 0;
}

@media (max-width: 768px) {
  .page-shell {
    padding: 12px;
  }

  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }

  .header-actions {
    width: 100%;
    justify-content: flex-start;
    flex-wrap: wrap;
  }

  .pagination-wrapper {
    justify-content: flex-start;
    overflow-x: auto;
  }
}
</style>
