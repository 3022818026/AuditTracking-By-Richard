<script setup lang="ts">
import { reactive, ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'

import {
  getCorrectiveActions,
  getCorrectiveActionById,
  createCorrectiveAction,
  updateCorrectiveAction,
  changeCorrectiveActionStatus,
  deleteCorrectiveAction,
  getCorrectiveActionRecycleBin,
  restoreCorrectiveAction,
  getCorrectiveActionLogs,
  getAuditIssueOptions,
} from '@/api/correctiveActions'

import type {
  CorrectiveAction,
  CorrectiveActionDetail,
  CorrectiveActionQuery,
  CreateCorrectiveActionRequest,
  UpdateCorrectiveActionRequest,
  ChangeCorrectiveActionStatusRequest,
  CorrectiveActionOperationLog,
  AuditIssueOption,
} from '@/types/correctiveAction'

const loading = ref(false)
const total = ref(0)
const actions = ref<CorrectiveAction[]>([])
const issues = ref<AuditIssueOption[]>([])

const query = reactive<CorrectiveActionQuery>({
  keyword: '',
  auditIssueId: null,
  status: '',
  responsibleDepartment: '',
  responsiblePerson: '',
  plannedDateStart: undefined,
  plannedDateEnd: undefined,
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
  auditIssueId: undefined,
  actionNo: '',
  actionDescription: '',
  responsibleDepartment: '',
  responsiblePerson: '',
  plannedCompletionDate: undefined,
  completionDescription: '',
})

const detail = ref<CorrectiveActionDetail | null>(null)
const recycleList = ref<CorrectiveAction[]>([])
const operationLogs = ref<CorrectiveActionOperationLog[]>([])
const statusPayload = reactive<ChangeCorrectiveActionStatusRequest>({
  status: '' as ChangeCorrectiveActionStatusRequest['status'],
  completionDescription: undefined,
  remark: '',
})
const currentRow = ref<CorrectiveAction | null>(null)
const allowedActionStatuses = ref<ChangeCorrectiveActionStatusRequest['status'][]>([])

const correctiveActionTransitions: Record<string, ChangeCorrectiveActionStatusRequest['status'][]> = {
  Draft: ['Submitted'],
  Submitted: ['Approved', 'Rejected'],
  Rejected: ['Draft'],
  Approved: ['Completed'],
  Completed: [],
}

const correctiveActionStatusLabels: Record<string, string> = {
  Draft: '草稿',
  Submitted: '已提交',
  Approved: '已批准',
  Rejected: '已驳回',
  Completed: '已完成',
}

function getAllowedActionStatuses(status: string) {
  return correctiveActionTransitions[status] ?? []
}

function getActionStatusLabel(status: string) {
  return correctiveActionStatusLabels[status] ?? status
}

function formatDate(value?: string | null) {
  if (!value) return '-'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return value
  return d.toLocaleString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit' })
}

function getStatusType(s: string) {
  return s === 'Completed' ? 'success' : s === 'Rejected' ? 'danger' : s === 'Approved' ? 'primary' : 'info'
}

async function loadIssues() {
  try {
    issues.value = await getAuditIssueOptions()
  } catch (e) {
    console.error(e)
  }
}

async function loadActions() {
  loading.value = true
  try {
    const res = await getCorrectiveActions(query as CorrectiveActionQuery)
    actions.value = res.items
    total.value = res.total
  } catch (e) {
    console.error(e)
    ElMessage.error('整改措施列表加载失败')
    actions.value = []
    total.value = 0
  } finally {
    loading.value = false
  }
}

async function refresh() {
  query.page = 1
  await Promise.all([loadActions(), loadIssues()])
}

function handleSearch() {
  query.page = 1
  loadActions()
}

function handleReset() {
  query.keyword = ''
  query.auditIssueId = null
  query.status = ''
  query.responsibleDepartment = ''
  query.responsiblePerson = ''
  query.plannedDateStart = undefined
  query.plannedDateEnd = undefined
  query.isOverdue = null
  query.page = 1
  loadActions()
}

function handlePageChange(page: number) {
  query.page = page
  loadActions()
}

function handlePageSizeChange(size: number) {
  query.pageSize = size
  query.page = 1
  loadActions()
}

function openCreate() {
  formMode.value = 'create'
  Object.assign(form, {
    id: undefined,
    auditIssueId: undefined,
    actionNo: '',
    actionDescription: '',
    responsibleDepartment: '',
    responsiblePerson: '',
    plannedCompletionDate: undefined,
    completionDescription: '',
  })
  formDialogVisible.value = true
}

async function openEdit(row: CorrectiveAction) {
  formMode.value = 'edit'
  formDialogVisible.value = true
  formLoading.value = true
  try {
    const res = await getCorrectiveActionById(row.id)
    Object.assign(form, {
      id: res.id,
      auditIssueId: res.auditIssueId,
      actionNo: res.actionNo,
      actionDescription: res.actionDescription,
      responsibleDepartment: res.responsibleDepartment ?? '',
      responsiblePerson: res.responsiblePerson ?? '',
      plannedCompletionDate: res.plannedCompletionDate ?? undefined,
      completionDescription: res.completionDescription ?? '',
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
      const payload: CreateCorrectiveActionRequest = {
        auditIssueId: form.auditIssueId,
        actionNo: form.actionNo,
        actionDescription: form.actionDescription,
        responsibleDepartment: form.responsibleDepartment || undefined,
        responsiblePerson: form.responsiblePerson || undefined,
        plannedCompletionDate: form.plannedCompletionDate || undefined,
        completionDescription: form.completionDescription || undefined,
      }
      await createCorrectiveAction(payload)
      ElMessage.success('整改措施创建成功')
    } else {
      const payload: UpdateCorrectiveActionRequest = {
        actionDescription: form.actionDescription,
        responsibleDepartment: form.responsibleDepartment || undefined,
        responsiblePerson: form.responsiblePerson || undefined,
        plannedCompletionDate: form.plannedCompletionDate || undefined,
        completionDescription: form.completionDescription || undefined,
      }
      await updateCorrectiveAction(form.id, payload)
      ElMessage.success('整改措施保存成功')
    }
    formDialogVisible.value = false
    loadActions()
  } catch (e) {
    console.error(e)
    ElMessage.error('保存失败')
  } finally {
    formLoading.value = false
  }
}

async function openDetail(row: CorrectiveAction) {
  detailVisible.value = true
  try {
    detail.value = await getCorrectiveActionById(row.id)
  } catch (e) {
    console.error(e)
    ElMessage.error('详情加载失败')
    detailVisible.value = false
  }
}

async function handleDelete(row: CorrectiveAction) {
  try {
    await ElMessageBox.confirm('确定删除该整改措施？', '删除确认', { type: 'warning' })
    await deleteCorrectiveAction(row.id)
    ElMessage.success('删除成功')
    loadActions()
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
    recycleList.value = await getCorrectiveActionRecycleBin()
  } catch (e) {
    console.error(e)
    ElMessage.error('回收站加载失败')
    recycleList.value = []
  } finally {
    recycleLoading.value = false
  }
}

async function handleRestore(row: CorrectiveAction) {
  try {
    await ElMessageBox.confirm('确定恢复该整改措施？', '恢复确认')
    await restoreCorrectiveAction(row.id)
    ElMessage.success('恢复成功')
    loadActions()
    openRecycle()
  } catch (e) {
    if (e !== 'cancel') {
      console.error(e)
      ElMessage.error('恢复失败')
    }
  }
}

async function openLogs(row: CorrectiveAction) {
  logsDialogVisible.value = true
  logLoading.value = true
  try {
    operationLogs.value = await getCorrectiveActionLogs(row.id)
  } catch (e) {
    console.error(e)
    ElMessage.error('日志加载失败')
    operationLogs.value = []
  } finally {
    logLoading.value = false
  }
}

function openChangeStatus(row: CorrectiveAction) {
  currentRow.value = row
  allowedActionStatuses.value = getAllowedActionStatuses(row.status)
  statusPayload.status = '' as ChangeCorrectiveActionStatusRequest['status']
  statusPayload.completionDescription = undefined
  statusPayload.remark = ''
  statusDialogVisible.value = true
}

async function submitStatusChange() {
  if (!currentRow.value) return
  try {
    await changeCorrectiveActionStatus(currentRow.value.id, statusPayload)
    ElMessage.success('状态变更成功')
    statusDialogVisible.value = false
    loadActions()
  } catch (e) {
    console.error(e)
    ElMessage.error('状态变更失败')
  }
}

onMounted(() => {
  refresh()
})
</script>

<template>
  <div class="page-shell">
    <div class="page-header">
      <div>
        <p class="eyebrow">AuditTrack • 审计中心</p>
        <h2>整改措施管理</h2>
      </div>
      <div class="header-actions">
        <el-button type="primary" @click="openCreate">新增整改措施</el-button>
        <el-button @click="openRecycle">回收站</el-button>
      </div>
    </div>

    <el-card shadow="never" class="filter-card">
      <el-form :inline="true" class="search-form">
        <el-form-item label="关键词">
          <el-input v-model="query.keyword" placeholder="编号、描述或责任人" style="width:240px" />
        </el-form-item>
        <el-form-item label="审计问题">
          <el-select v-model="query.auditIssueId" placeholder="选择审计问题" clearable style="width:320px">
            <el-option v-for="p in issues" :key="p.id" :label="`${p.issueNo} ${p.title}`" :value="p.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="query.status" placeholder="全部" clearable style="width:160px">
            <el-option label="Draft" value="Draft" />
            <el-option label="Submitted" value="Submitted" />
            <el-option label="Approved" value="Approved" />
            <el-option label="Rejected" value="Rejected" />
            <el-option label="Completed" value="Completed" />
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
        <span class="table-title">整改措施列表</span>
      </div>

      <div class="table-responsive">
        <el-table v-loading="loading" :data="actions" border stripe style="min-width:1200px">
          <el-table-column prop="actionNo" label="措施编号" width="140" fixed="left" />
          <el-table-column prop="actionDescription" label="描述" min-width="220" show-overflow-tooltip />
          <el-table-column label="所属问题" min-width="220">
            <template #default="{ row }">
              <div>{{ row.auditIssueId }}</div>
            </template>
          </el-table-column>
          <el-table-column prop="responsibleDepartment" label="责任部门" min-width="160" show-overflow-tooltip />
          <el-table-column prop="responsiblePerson" label="责任人" width="140" />
          <el-table-column prop="plannedCompletionDate" label="计划完成" width="140">
            <template #default="{ row }">{{ formatDate(row.plannedCompletionDate) }}</template>
          </el-table-column>
          <el-table-column prop="actualCompletionDate" label="实际完成" width="140">
            <template #default="{ row }">{{ formatDate(row.actualCompletionDate) }}</template>
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
                <el-button
                  v-if="getAllowedActionStatuses(row.status).length > 0"
                  type="text"
                  @click="openChangeStatus(row)"
                >状态</el-button>
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
    <el-dialog :title="formMode === 'create' ? '新增整改措施' : '编辑整改措施'" v-model="formDialogVisible" width="800px">
      <el-form label-width="140px">
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="所属审计问题">
              <el-select v-model="form.auditIssueId" placeholder="选择审计问题" style="width:100%">
                <el-option v-for="p in issues" :key="p.id" :label="`${p.issueNo} ${p.title}`" :value="p.id" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="措施编号">
              <el-input v-model="form.actionNo" :disabled="formMode==='edit'" />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="措施内容">
              <el-input type="textarea" v-model="form.actionDescription" rows="4" />
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
          <el-col :span="12">
            <el-form-item label="计划完成日期">
              <el-date-picker v-model="form.plannedCompletionDate" type="date" placeholder="选择日期" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="完成说明">
              <el-input v-model="form.completionDescription" />
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
    <el-drawer v-model="detailVisible" title="整改措施详情" size="50%">
      <div v-if="detail">
        <el-descriptions title="基本信息" column="2">
          <el-descriptions-item label="措施编号">{{ detail.actionNo }}</el-descriptions-item>
          <el-descriptions-item label="所属问题">{{ detail.issueNo }} {{ detail.issueTitle }}</el-descriptions-item>
          <el-descriptions-item label="状态">{{ detail.status }}</el-descriptions-item>
          <el-descriptions-item label="责任部门">{{ detail.responsibleDepartment || '-' }}</el-descriptions-item>
          <el-descriptions-item label="责任人">{{ detail.responsiblePerson || '-' }}</el-descriptions-item>
          <el-descriptions-item label="计划完成日期">{{ formatDate(detail.plannedCompletionDate) }}</el-descriptions-item>
          <el-descriptions-item label="实际完成日期">{{ formatDate(detail.actualCompletionDate) }}</el-descriptions-item>
          <el-descriptions-item label="提交时间">{{ formatDate(detail.submittedAt) }}</el-descriptions-item>
          <el-descriptions-item label="批准时间">{{ formatDate(detail.approvedAt) }}</el-descriptions-item>
          <el-descriptions-item label="完成时间">{{ formatDate(detail.completedAt) }}</el-descriptions-item>
          <el-descriptions-item label="创建时间">{{ formatDate(detail.createdAt) }}</el-descriptions-item>
          <el-descriptions-item label="创建人">{{ detail.createdBy }}</el-descriptions-item>
        </el-descriptions>

        <el-divider />
        <h4>措施描述</h4>
        <div>{{ detail.actionDescription }}</div>
        <el-divider />
        <h4>完成说明</h4>
        <div>{{ detail.completionDescription || '-' }}</div>
      </div>
    </el-drawer>

    <!-- 状态变更 -->
    <el-dialog title="变更状态" v-model="statusDialogVisible">
      <div>
        <p>当前状态：{{ getActionStatusLabel(currentRow?.status ?? '') }}</p>
        <el-form>
          <el-form-item label="目标状态">
            <el-select v-model="statusPayload.status" placeholder="请选择目标状态" style="width:100%">
              <el-option
                v-for="status in allowedActionStatuses"
                :key="status"
                :label="getActionStatusLabel(status)"
                :value="status"
              />
            </el-select>
          </el-form-item>
          <el-form-item label="完成说明" v-if="statusPayload.status === 'Completed'">
            <el-input type="textarea" v-model="statusPayload.completionDescription" rows="3" />
          </el-form-item>
          <el-form-item label="备注">
            <el-input type="textarea" v-model="statusPayload.remark" rows="2" />
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
        <el-table-column prop="actionNo" label="措施编号" width="140" />
        <el-table-column prop="actionDescription" label="描述" min-width="220" show-overflow-tooltip />
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

<script lang="ts">
export default {
  methods: {
    tryFormat(val: any) {
      if (!val) return '-'
      try {
        return JSON.stringify(JSON.parse(val), null, 2)
      } catch {
        return val
      }
    },
  },
}
</script>

<style scoped>
.table-responsive { overflow-x: auto }
.pagination-wrapper { display:flex; justify-content:flex-end; margin-top:12px }
</style>
