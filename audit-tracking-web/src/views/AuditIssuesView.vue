<script setup lang="ts">
import { reactive, ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'

import {
  getAuditIssues,
  getAuditIssueById,
  createAuditIssue,
  updateAuditIssue,
  changeAuditIssueStatus,
  deleteAuditIssue,
  getAuditIssueRecycleBin,
  restoreAuditIssue,
  getAuditIssueLogs,
  getAuditPlanOptions,
} from '@/api/auditIssues'

import type {
  AuditIssue,
  AuditIssueDetail,
  AuditIssueQuery,
  CreateAuditIssueRequest,
  UpdateAuditIssueRequest,
  ChangeAuditIssueStatusRequest,
  AuditIssueOperationLog,
  
  AuditPlanOption,
} from '@/types/auditIssue'

const loading = ref(false)
const total = ref(0)
const issues = ref<AuditIssue[]>([])
const plans = ref<AuditPlanOption[]>([])

const query = reactive<AuditIssueQuery>({
  keyword: '',
  auditPlanId: null,
  status: '',
  severity: '',
  issueType: '',
  responsibleDepartment: '',
  responsiblePerson: '',
  dueDateStart: undefined,
  dueDateEnd: undefined,
  isOverdue: null,
  page: 1,
  pageSize: 10,
})

const formDialogVisible = ref(false)
const detailVisible = ref(false)
const statusDialogVisible = ref(false)
const recycleDialogVisible = ref(false)
const logsDialogVisible = ref(false)
const formLoading = ref(false)
const recycleLoading = ref(false)
const logLoading = ref(false)

const formMode = ref<'create' | 'edit'>('create')

const form = reactive<any>({
  id: undefined,
  auditPlanId: undefined,
  issueNo: '',
  title: '',
  description: '',
  issueType: '',
  severity: 'Medium',
  responsibleDepartment: '',
  responsiblePerson: '',
  dueDate: undefined,
})

const detail = ref<AuditIssueDetail | null>(null)
const recycleList = ref<AuditIssue[]>([])
const operationLogs = ref<AuditIssueOperationLog[]>([])
const statusPayload = reactive<ChangeAuditIssueStatusRequest>({ status: 'Open', remark: '' })
const currentRow = ref<AuditIssue | null>(null)

function formatDate(value?: string | null) {
  if (!value) return '-'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return value
  return d.toLocaleString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit' })
}

function formatSeverity(s: string) {
  return s || '-'
}

function getSeverityType(s: string) {
  return s === 'Critical' ? 'danger' : s === 'High' ? 'warning' : s === 'Low' ? 'info' : 'success'
}

function getStatusType(s: string) {
  return s === 'Closed' ? 'success' : s === 'Rejected' ? 'danger' : s === 'warning' ? 'warning' : 'info'
}

async function loadPlans() {
  try {
    plans.value = await getAuditPlanOptions()
  } catch (e) {
    console.error(e)
  }
}

async function loadIssues() {
  loading.value = true
  try {
    const res = await getAuditIssues(query as AuditIssueQuery)
    issues.value = res.items
    total.value = res.total
  } catch (e) {
    console.error(e)
    ElMessage.error('审计问题列表加载失败')
    issues.value = []
    total.value = 0
  } finally {
    loading.value = false
  }
}

async function refresh() {
  query.page = 1
  await Promise.all([loadIssues(), loadPlans()])
}

function handleSearch() {
  query.page = 1
  loadIssues()
}

function handleReset() {
  query.keyword = ''
  query.auditPlanId = null
  query.status = ''
  query.severity = ''
  query.issueType = ''
  query.responsibleDepartment = ''
  query.responsiblePerson = ''
  query.dueDateStart = undefined
  query.dueDateEnd = undefined
  query.isOverdue = null
  query.page = 1
  loadIssues()
}

function handlePageChange(page: number) {
  query.page = page
  loadIssues()
}

function handlePageSizeChange(size: number) {
  query.pageSize = size
  query.page = 1
  loadIssues()
}

function openCreate() {
  formMode.value = 'create'
  Object.assign(form, {
    id: undefined,
    auditPlanId: undefined,
    issueNo: '',
    title: '',
    description: '',
    issueType: '',
    severity: 'Medium',
    responsibleDepartment: '',
    responsiblePerson: '',
    dueDate: undefined,
  })
  formDialogVisible.value = true
}

async function openEdit(row: AuditIssue) {
  formMode.value = 'edit'
  formDialogVisible.value = true
  formLoading.value = true
  try {
    const res = await getAuditIssueById(row.id)
    Object.assign(form, {
      id: res.id,
      auditPlanId: res.auditPlanId,
      issueNo: res.issueNo,
      title: res.title,
      description: res.description,
      issueType: res.issueType ?? '',
      severity: res.severity,
      responsibleDepartment: res.responsibleDepartment ?? '',
      responsiblePerson: res.responsiblePerson ?? '',
      dueDate: res.dueDate ?? undefined,
    })
  } catch (e) {
    console.error(e)
    ElMessage.error('加载详情失败')
    formDialogVisible.value = false
  } finally {
    formLoading.value = false
  }
}

async function submitForm() {
  formLoading.value = true
  try {
    if (formMode.value === 'create') {
      const payload: CreateAuditIssueRequest = {
        auditPlanId: form.auditPlanId,
        issueNo: form.issueNo,
        title: form.title,
        description: form.description,
        issueType: form.issueType || undefined,
        severity: form.severity,
        responsibleDepartment: form.responsibleDepartment || undefined,
        responsiblePerson: form.responsiblePerson || undefined,
        dueDate: form.dueDate || undefined,
      }
      await createAuditIssue(payload)
      ElMessage.success('审计问题创建成功')
    } else {
      const payload: UpdateAuditIssueRequest = {
        title: form.title,
        description: form.description,
        issueType: form.issueType || undefined,
        severity: form.severity,
        responsibleDepartment: form.responsibleDepartment || undefined,
        responsiblePerson: form.responsiblePerson || undefined,
        dueDate: form.dueDate || undefined,
      }
      await updateAuditIssue(form.id, payload)
      ElMessage.success('审计问题保存成功')
    }
    formDialogVisible.value = false
    loadIssues()
  } catch (e) {
    console.error(e)
    ElMessage.error('保存失败')
  } finally {
    formLoading.value = false
  }
}

async function openDetail(row: AuditIssue) {
  detailVisible.value = true
  try {
    detail.value = await getAuditIssueById(row.id)
  } catch (e) {
    console.error(e)
    ElMessage.error('详情加载失败')
    detailVisible.value = false
  }
}

async function handleDelete(row: AuditIssue) {
  try {
    await ElMessageBox.confirm('确定删除该问题？', '删除确认', { type: 'warning' })
    await deleteAuditIssue(row.id)
    ElMessage.success('删除成功')
    loadIssues()
  } catch (e) {
    if (e !== 'cancel') {
      console.error(e)
      ElMessage.error('删除失败')
    }
  }
}

async function openRecycle() {
  recycleDialogVisible.value = true
  recycleLoading.value = true
  try {
    recycleList.value = await getAuditIssueRecycleBin()
  } catch (e) {
    console.error(e)
    ElMessage.error('回收站加载失败')
    recycleList.value = []
  } finally {
    recycleLoading.value = false
  }
}

async function handleRestore(row: AuditIssue) {
  try {
    await ElMessageBox.confirm('确定恢复该问题？', '恢复确认')
    await restoreAuditIssue(row.id)
    ElMessage.success('恢复成功')
    loadIssues()
    openRecycle()
  } catch (e) {
    if (e !== 'cancel') {
      console.error(e)
      ElMessage.error('恢复失败')
    }
  }
}

async function openLogs(row: AuditIssue) {
  logsDialogVisible.value = true
  logLoading.value = true
  try {
    operationLogs.value = await getAuditIssueLogs(row.id)
  } catch (e) {
    console.error(e)
    ElMessage.error('日志加载失败')
    operationLogs.value = []
  } finally {
    logLoading.value = false
  }
}

function openChangeStatus(row: AuditIssue) {
  currentRow.value = row
  statusPayload.status = row.status as any
  statusPayload.remark = ''
  statusDialogVisible.value = true
}

async function submitStatusChange() {
  if (!currentRow.value) return
  try {
    await changeAuditIssueStatus(currentRow.value.id, statusPayload)
    ElMessage.success('状态变更成功')
    statusDialogVisible.value = false
    loadIssues()
  } catch (e) {
    console.error(e)
    ElMessage.error('状态变更失败')
  }
}

onMounted(() => {
  refresh()
})
function tryFormat(val: any) {
  if (!val) return '-'
  try {
    return JSON.stringify(JSON.parse(val), null, 2)
  } catch {
    return val
  }
}

</script>
<template>
  <div class="page-shell">
    <div class="page-header">
      <div>
        <p class="eyebrow">AuditTrack • 审计中心</p>
        <h2>审计问题管理</h2>
      </div>
      <div class="header-actions">
        <el-button type="primary" @click="openCreate">新增问题</el-button>
        <el-button @click="openRecycle">回收站</el-button>
      </div>
    </div>

    <el-card shadow="never" class="filter-card">
      <el-form :inline="true" class="search-form">
        <el-form-item label="关键词">
          <el-input v-model="query.keyword" placeholder="编号、标题、责任人、部门" style="width:240px" />
        </el-form-item>
        <el-form-item label="审计计划">
          <el-select v-model="query.auditPlanId" placeholder="选择审计计划" clearable style="width:320px">
            <el-option v-for="p in plans" :key="p.id" :label="`${p.auditNo} ${p.title}`" :value="p.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="query.status" placeholder="全部" clearable style="width:160px">
            <el-option label="Open" value="Open" />
            <el-option label="Rectifying" value="Rectifying" />
            <el-option label="PendingVerification" value="PendingVerification" />
            <el-option label="Closed" value="Closed" />
            <el-option label="Rejected" value="Rejected" />
          </el-select>
        </el-form-item>
        <el-form-item label="严重程度">
          <el-select v-model="query.severity" placeholder="全部" clearable style="width:140px">
            <el-option label="Low" value="Low" />
            <el-option label="Medium" value="Medium" />
            <el-option label="High" value="High" />
            <el-option label="Critical" value="Critical" />
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
        <span class="table-title">审计问题列表</span>
      </div>

      <div class="table-responsive">
        <el-table v-loading="loading" :data="issues" border stripe style="min-width:1200px">
          <el-table-column prop="issueNo" label="问题编号" width="140" fixed="left" />
          <el-table-column prop="title" label="标题" min-width="220" show-overflow-tooltip />
          <el-table-column label="审计计划" min-width="220">
            <template #default="{ row }">
              <div>{{ row.auditPlanId }}</div>
            </template>
          </el-table-column>
          <el-table-column prop="issueType" label="问题类型" width="140" />
          <el-table-column prop="severity" label="严重程度" width="120">
            <template #default="{ row }">
              <el-tag :type="getSeverityType(row.severity)">{{ formatSeverity(row.severity) }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="responsibleDepartment" label="责任部门" min-width="160" show-overflow-tooltip />
          <el-table-column prop="responsiblePerson" label="责任人" width="140" />
          <el-table-column prop="dueDate" label="到期日期" width="140">
            <template #default="{ row }">{{ formatDate(row.dueDate) }}</template>
          </el-table-column>
          <el-table-column prop="status" label="状态" width="140">
            <template #default="{ row }">
              <el-tag :type="getStatusType(row.status)">{{ row.status }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="createdAt" label="创建时间" width="170">
            <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
          </el-table-column>
          <el-table-column label="操作" width="260" fixed="right">
            <template #default="{ row }">
              <div style="display:flex;gap:8px;white-space:nowrap;">
                <el-button type="text" @click="openDetail(row)">详情</el-button>
                <el-button type="text" @click="openEdit(row)">编辑</el-button>
                <el-button type="text" @click="openChangeStatus(row)">状态</el-button>
                <el-button type="text" @click="handleDelete(row)">删除</el-button>
                <el-button type="text" @click="openLogs(row)">日志</el-button>
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
          :page-sizes="[10,20,50]"
          :total="total"
          @current-change="handlePageChange"
          @size-change="handlePageSizeChange"
        />
      </div>
    </el-card>

    <!-- 新增/编辑 -->
    <el-dialog :title="formMode === 'create' ? '新增问题' : '编辑问题'" v-model="formDialogVisible" width="800px">
      <el-form label-width="120px">
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="所属审计计划">
              <el-select v-model="form.auditPlanId" placeholder="选择审计计划" style="width:100%">
                <el-option v-for="p in plans" :key="p.id" :label="`${p.auditNo} ${p.title}`" :value="p.id" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="问题编号">
              <el-input v-model="form.issueNo" :disabled="formMode==='edit'" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="问题标题">
              <el-input v-model="form.title" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="问题描述">
              <el-input type="textarea" v-model="form.description" rows="4" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="问题类型">
              <el-input v-model="form.issueType" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="严重程度">
              <el-select v-model="form.severity" style="width:100%">
                <el-option label="Low" value="Low" />
                <el-option label="Medium" value="Medium" />
                <el-option label="High" value="High" />
                <el-option label="Critical" value="Critical" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="到期日期">
              <el-date-picker v-model="form.dueDate" type="date" placeholder="选择日期" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="责任部门">
              <el-input v-model="form.responsibleDepartment" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="责任人">
              <el-input v-model="form.responsiblePerson" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <template #footer>
        <el-button @click="formDialogVisible=false">取消</el-button>
        <el-button type="primary" :loading="formLoading" @click="submitForm">保存</el-button>
      </template>
    </el-dialog>

    <!-- 详情 -->
    <el-drawer v-model="detailVisible" title="问题详情" size="50%">
      <div v-if="detail">
        <el-descriptions title="基本信息" column="2">
          <el-descriptions-item label="问题编号">{{ detail.issueNo }}</el-descriptions-item>
          <el-descriptions-item label="标题">{{ detail.title }}</el-descriptions-item>
          <el-descriptions-item label="所属审计计划">{{ detail.auditNo }} {{ detail.auditTitle }}</el-descriptions-item>
          <el-descriptions-item label="状态">{{ detail.status }}</el-descriptions-item>
          <el-descriptions-item label="严重程度">{{ detail.severity }}</el-descriptions-item>
          <el-descriptions-item label="责任部门">{{ detail.responsibleDepartment || '-' }}</el-descriptions-item>
          <el-descriptions-item label="责任人">{{ detail.responsiblePerson || '-' }}</el-descriptions-item>
          <el-descriptions-item label="到期日期">{{ formatDate(detail.dueDate) }}</el-descriptions-item>
          <el-descriptions-item label="创建时间">{{ formatDate(detail.createdAt) }}</el-descriptions-item>
          <el-descriptions-item label="更新时间">{{ formatDate(detail.updatedAt) }}</el-descriptions-item>
          <el-descriptions-item label="关闭时间">{{ formatDate(detail.closedAt) }}</el-descriptions-item>
        </el-descriptions>

        <el-divider />
        <h4>问题描述</h4>
        <div>{{ detail.description }}</div>
      </div>
    </el-drawer>

    <!-- 状态变更 -->
    <el-dialog title="变更状态" v-model="statusDialogVisible">
      <div>
        <p>当前状态：{{ currentRow?.status }}</p>
        <el-form>
          <el-form-item label="目标状态">
            <el-select v-model="statusPayload.status" style="width:100%">
              <el-option label="Open" value="Open" />
              <el-option label="Rectifying" value="Rectifying" />
              <el-option label="PendingVerification" value="PendingVerification" />
              <el-option label="Closed" value="Closed" />
              <el-option label="Rejected" value="Rejected" />
            </el-select>
          </el-form-item>
          <el-form-item label="备注">
            <el-input type="textarea" v-model="statusPayload.remark" rows="3" />
          </el-form-item>
        </el-form>
      </div>
      <template #footer>
        <el-button @click="statusDialogVisible=false">取消</el-button>
        <el-button type="primary" @click="submitStatusChange">确认</el-button>
      </template>
    </el-dialog>

    <!-- 回收站 -->
    <el-dialog title="回收站" v-model="recycleDialogVisible" width="900px">
      <el-table v-loading="recycleLoading" :data="recycleList" border stripe>
        <el-table-column prop="issueNo" label="问题编号" width="140" />
        <el-table-column prop="title" label="标题" min-width="220" show-overflow-tooltip />
        <el-table-column prop="status" label="状态" width="140" />
        <el-table-column prop="deletedAt" label="删除时间" width="180">
          <template #default="{ row }">{{ formatDate((row as any).deletedAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="120">
          <template #default="{ row }">
            <el-button type="text" @click="handleRestore(row)">恢复</el-button>
          </template>
        </el-table-column>
      </el-table>
      <template #footer>
        <el-button @click="recycleDialogVisible=false">关闭</el-button>
      </template>
    </el-dialog>

    <!-- 日志 -->
    <el-dialog title="操作日志" v-model="logsDialogVisible" width="900px">
      <el-table v-loading="logLoading" :data="operationLogs" border stripe>
        <el-table-column prop="operationType" label="操作类型" width="140" />
        <el-table-column prop="operator" label="操作人" width="120" />
        <el-table-column prop="remark" label="备注" min-width="200" />
        <el-table-column prop="createdAt" label="时间" width="180" />
        <el-table-column label="修改前" min-width="260">
          <template #default="{ row }"><pre style="white-space:pre-wrap">{{ tryFormat(row.beforeData) }}</pre></template>
        </el-table-column>
        <el-table-column label="修改后" min-width="260">
          <template #default="{ row }"><pre style="white-space:pre-wrap">{{ tryFormat(row.afterData) }}</pre></template>
        </el-table-column>
      </el-table>
      <template #footer>
        <el-button @click="logsDialogVisible=false">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>



<style scoped>
.table-responsive { overflow-x: auto }
.pagination-wrapper { display:flex; justify-content:flex-end; margin-top:12px }
</style>

