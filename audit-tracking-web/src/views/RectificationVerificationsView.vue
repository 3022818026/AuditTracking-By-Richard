<script setup lang="ts">
import { nextTick, onMounted, reactive, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'

import {
  createRectificationVerification,
  deleteRectificationVerification,
  getAuditIssueOptions,
  getCorrectiveActionOptions,
  getRectificationVerificationById,
  getRectificationVerificationLogs,
  getRectificationVerificationRecycleBin,
  getRectificationVerifications,
  restoreRectificationVerification,
  updateRectificationVerification,
} from '@/api/rectificationVerifications'

import type {
  AuditIssueOption,
  CorrectiveActionOption,
  CreateRectificationVerificationRequest,
  RectificationVerification,
  RectificationVerificationDetail,
  RectificationVerificationListItem,
  RectificationVerificationOperationLog,
  RectificationVerificationQuery,
  UpdateRectificationVerificationRequest,
  VerificationResult,
} from '@/types/rectificationVerification'

interface VerificationForm {
  id?: number
  auditIssueId?: number
  correctiveActionId?: number
  issueLabel: string
  actionLabel: string
  verificationNo: string
  verificationResult: VerificationResult | ''
  verificationComment: string
  verifier: string
  verifiedAt: string | null
}

const verificationResultLabels: Record<VerificationResult, string> = {
  Passed: '通过',
  Failed: '不通过',
  NeedMoreEvidence: '需补充材料',
}

const loading = ref(false)
const total = ref(0)
const records = ref<RectificationVerificationListItem[]>([])
const issueOptions = ref<AuditIssueOption[]>([])
const allActionOptions = ref<CorrectiveActionOption[]>([])
const queryActionOptions = ref<CorrectiveActionOption[]>([])
const formActionOptions = ref<CorrectiveActionOption[]>([])
const queryActionsLoading = ref(false)
const formActionsLoading = ref(false)

const query = reactive<RectificationVerificationQuery>({
  keyword: '',
  auditIssueId: null,
  correctiveActionId: null,
  verificationResult: '',
  isPassed: null,
  verifiedDateStart: null,
  verifiedDateEnd: null,
  page: 1,
  pageSize: 10,
})

const formDialogVisible = ref(false)
const formMode = ref<'create' | 'edit'>('create')
const formLoading = ref(false)
const formRef = ref<FormInstance>()
const form = reactive<VerificationForm>({
  id: undefined,
  auditIssueId: undefined,
  correctiveActionId: undefined,
  issueLabel: '',
  actionLabel: '',
  verificationNo: '',
  verificationResult: '',
  verificationComment: '',
  verifier: '',
  verifiedAt: null,
})

const formRules: FormRules = {
  auditIssueId: [{ required: true, message: '请选择审计问题', trigger: 'change' }],
  correctiveActionId: [{ required: true, message: '请选择已完成的整改措施', trigger: 'change' }],
  verificationNo: [
    { required: true, message: '请输入验证编号', trigger: 'blur' },
    { max: 50, message: '验证编号不能超过50个字符', trigger: 'blur' },
    { pattern: /^\S+$/, message: '验证编号中不能包含空格', trigger: 'blur' },
  ],
  verificationResult: [{ required: true, message: '请选择验证结果', trigger: 'change' }],
  verificationComment: [
    { required: true, message: '请输入验证意见', trigger: 'blur' },
    { max: 4000, message: '验证意见不能超过4000个字符', trigger: 'blur' },
  ],
  verifier: [
    { required: true, message: '请输入验证人', trigger: 'blur' },
    { max: 100, message: '验证人不能超过100个字符', trigger: 'blur' },
  ],
}

const detailVisible = ref(false)
const detailLoading = ref(false)
const detail = ref<RectificationVerificationDetail | null>(null)

const recycleVisible = ref(false)
const recycleLoading = ref(false)
const recycleList = ref<RectificationVerification[]>([])

const logsVisible = ref(false)
const logsLoading = ref(false)
const operationLogs = ref<RectificationVerificationOperationLog[]>([])

function getErrorMessage(error: unknown, fallback: string) {
  return error instanceof Error && error.message ? error.message : fallback
}

function formatDate(value?: string | null) {
  if (!value) return '-'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  })
}

function getResultLabel(result: VerificationResult) {
  return verificationResultLabels[result] ?? result
}

function getResultTagType(result: VerificationResult) {
  if (result === 'Passed') return 'success'
  if (result === 'Failed') return 'danger'
  return 'warning'
}

function getIssueLabel(id: number) {
  const option = issueOptions.value.find((item) => item.id === id)
  return option ? `${option.issueNo} ${option.title}` : `审计问题 #${id}`
}

function getActionLabel(id: number) {
  const option = allActionOptions.value.find((item) => item.id === id)
  return option ? `${option.actionNo} ${option.actionDescription}` : `整改措施 #${id}`
}

function formatJson(value: string | null) {
  if (!value) return '-'
  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}

async function loadOptions() {
  try {
    const [issues, actions] = await Promise.all([
      getAuditIssueOptions(),
      getCorrectiveActionOptions(),
    ])
    issueOptions.value = issues
    allActionOptions.value = actions
    queryActionOptions.value = actions
  } catch (error) {
    console.error(error)
    ElMessage.error(getErrorMessage(error, '下拉选项加载失败'))
  }
}

async function loadRecords() {
  loading.value = true
  try {
    const result = await getRectificationVerifications(query)
    records.value = result.items
    total.value = result.total
  } catch (error) {
    console.error(error)
    records.value = []
    total.value = 0
    ElMessage.error(getErrorMessage(error, '整改验证列表加载失败'))
  } finally {
    loading.value = false
  }
}

async function loadQueryActions(auditIssueId?: number | null) {
  queryActionsLoading.value = true
  try {
    queryActionOptions.value = await getCorrectiveActionOptions(auditIssueId ?? undefined)
  } catch (error) {
    console.error(error)
    queryActionOptions.value = []
    ElMessage.error(getErrorMessage(error, '整改措施选项加载失败'))
  } finally {
    queryActionsLoading.value = false
  }
}

async function handleQueryIssueChange(value?: number) {
  query.correctiveActionId = null
  await loadQueryActions(value)
}

function handleSearch() {
  query.page = 1
  loadRecords()
}

async function handleReset() {
  Object.assign(query, {
    keyword: '',
    auditIssueId: null,
    correctiveActionId: null,
    verificationResult: '',
    isPassed: null,
    verifiedDateStart: null,
    verifiedDateEnd: null,
    page: 1,
  })
  await Promise.all([loadQueryActions(), loadRecords()])
}

function handlePageChange(page: number) {
  query.page = page
  loadRecords()
}

function handlePageSizeChange(pageSize: number) {
  query.pageSize = pageSize
  query.page = 1
  loadRecords()
}

async function openCreate() {
  formMode.value = 'create'
  Object.assign(form, {
    id: undefined,
    auditIssueId: undefined,
    correctiveActionId: undefined,
    issueLabel: '',
    actionLabel: '',
    verificationNo: '',
    verificationResult: '',
    verificationComment: '',
    verifier: '',
    verifiedAt: null,
  })
  formActionOptions.value = []
  formDialogVisible.value = true
  await nextTick()
  formRef.value?.clearValidate()
}

async function handleFormIssueChange(value?: number) {
  form.correctiveActionId = undefined
  formActionOptions.value = []
  if (!value) return

  formActionsLoading.value = true
  try {
    formActionOptions.value = await getCorrectiveActionOptions(value, true)
  } catch (error) {
    console.error(error)
    ElMessage.error(getErrorMessage(error, '已完成整改措施加载失败'))
  } finally {
    formActionsLoading.value = false
  }
}

function handleFormActionChange(value?: number) {
  if (!value) return
  const selected = formActionOptions.value.find((item) => item.id === value)
  if (selected) form.auditIssueId = selected.auditIssueId
}

async function openEdit(row: RectificationVerificationListItem) {
  formMode.value = 'edit'
  formDialogVisible.value = true
  formLoading.value = true
  try {
    const latest = await getRectificationVerificationById(row.id)
    Object.assign(form, {
      id: latest.id,
      auditIssueId: latest.auditIssueId,
      correctiveActionId: latest.correctiveActionId,
      issueLabel: `${latest.issueNo} ${latest.issueTitle}`,
      actionLabel: `${latest.actionNo} ${latest.actionDescription}`,
      verificationNo: latest.verificationNo,
      verificationResult: latest.verificationResult,
      verificationComment: latest.verificationComment,
      verifier: latest.verifier,
      verifiedAt: latest.verifiedAt,
    })
    await nextTick()
    formRef.value?.clearValidate()
  } catch (error) {
    console.error(error)
    formDialogVisible.value = false
    ElMessage.error(getErrorMessage(error, '整改验证详情加载失败'))
  } finally {
    formLoading.value = false
  }
}

async function submitForm() {
  if (formLoading.value || !formRef.value) return

  try {
    await formRef.value.validate()
  } catch {
    return
  }

  formLoading.value = true
  try {
    if (formMode.value === 'create') {
      const payload: CreateRectificationVerificationRequest = {
        auditIssueId: form.auditIssueId!,
        correctiveActionId: form.correctiveActionId!,
        verificationNo: form.verificationNo,
        verificationResult: form.verificationResult as VerificationResult,
        verificationComment: form.verificationComment,
        verifier: form.verifier,
        verifiedAt: form.verifiedAt,
      }
      await createRectificationVerification(payload)
      ElMessage.success('整改验证记录创建成功')
    } else {
      const payload: UpdateRectificationVerificationRequest = {
        verificationResult: form.verificationResult as VerificationResult,
        verificationComment: form.verificationComment,
        verifier: form.verifier,
        verifiedAt: form.verifiedAt,
      }
      await updateRectificationVerification(form.id!, payload)
      ElMessage.success('整改验证记录修改成功')
    }
    formDialogVisible.value = false
    await loadRecords()
  } catch (error) {
    console.error(error)
    ElMessage.error(getErrorMessage(error, '整改验证记录保存失败'))
  } finally {
    formLoading.value = false
  }
}

async function openDetail(row: RectificationVerificationListItem) {
  detailVisible.value = true
  detailLoading.value = true
  detail.value = null
  try {
    detail.value = await getRectificationVerificationById(row.id)
  } catch (error) {
    console.error(error)
    detailVisible.value = false
    ElMessage.error(getErrorMessage(error, '整改验证详情加载失败'))
  } finally {
    detailLoading.value = false
  }
}

async function handleDelete(row: RectificationVerificationListItem) {
  try {
    await ElMessageBox.confirm(`确定删除整改验证“${row.verificationNo}”吗？`, '删除确认', {
      type: 'warning',
    })
    await deleteRectificationVerification(row.id)
    ElMessage.success('整改验证记录删除成功')
    await loadRecords()
  } catch (error) {
    if (error === 'cancel' || error === 'close') return
    console.error(error)
    ElMessage.error(getErrorMessage(error, '整改验证记录删除失败'))
  }
}

async function openRecycle() {
  recycleVisible.value = true
  recycleLoading.value = true
  try {
    recycleList.value = await getRectificationVerificationRecycleBin()
  } catch (error) {
    console.error(error)
    recycleList.value = []
    ElMessage.error(getErrorMessage(error, '回收站加载失败'))
  } finally {
    recycleLoading.value = false
  }
}

async function handleRestore(row: RectificationVerification) {
  try {
    await ElMessageBox.confirm(`确定恢复整改验证“${row.verificationNo}”吗？`, '恢复确认')
    await restoreRectificationVerification(row.id)
    ElMessage.success('整改验证记录恢复成功')
    await Promise.all([loadRecords(), openRecycle()])
  } catch (error) {
    if (error === 'cancel' || error === 'close') return
    console.error(error)
    ElMessage.error(getErrorMessage(error, '整改验证记录恢复失败'))
  }
}

async function openLogs(row: RectificationVerificationListItem) {
  logsVisible.value = true
  logsLoading.value = true
  operationLogs.value = []
  try {
    operationLogs.value = await getRectificationVerificationLogs(row.id)
  } catch (error) {
    console.error(error)
    ElMessage.error(getErrorMessage(error, '操作日志加载失败'))
  } finally {
    logsLoading.value = false
  }
}

onMounted(async () => {
  await Promise.all([loadOptions(), loadRecords()])
})
</script>

<template>
  <div class="page-shell">
    <div class="page-header">
      <div>
        <p class="eyebrow">AuditTrack · 审计中心</p>
        <h2>整改验证管理</h2>
      </div>
      <div class="header-actions">
        <el-button type="primary" @click="openCreate">新增整改验证</el-button>
        <el-button @click="openRecycle">回收站</el-button>
      </div>
    </div>

    <el-card shadow="never" class="filter-card">
      <el-form :inline="true" class="search-form">
        <el-form-item label="关键词">
          <el-input
            v-model="query.keyword"
            clearable
            placeholder="验证编号、意见或验证人"
            style="width: 230px"
            @keyup.enter="handleSearch"
          />
        </el-form-item>
        <el-form-item label="审计问题">
          <el-select
            v-model="query.auditIssueId"
            clearable
            filterable
            placeholder="请选择审计问题"
            style="width: 280px"
            @change="handleQueryIssueChange"
          >
            <el-option
              v-for="item in issueOptions"
              :key="item.id"
              :label="`${item.issueNo} ${item.title}`"
              :value="item.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="整改措施">
          <el-select
            v-model="query.correctiveActionId"
            clearable
            filterable
            :loading="queryActionsLoading"
            placeholder="请选择整改措施"
            style="width: 300px"
          >
            <el-option
              v-for="item in queryActionOptions"
              :key="item.id"
              :label="`${item.actionNo} ${item.actionDescription}`"
              :value="item.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="验证结果">
          <el-select v-model="query.verificationResult" clearable placeholder="全部" style="width: 150px">
            <el-option label="通过" value="Passed" />
            <el-option label="不通过" value="Failed" />
            <el-option label="需补充材料" value="NeedMoreEvidence" />
          </el-select>
        </el-form-item>
        <el-form-item label="是否通过">
          <el-select v-model="query.isPassed" clearable placeholder="全部" style="width: 120px">
            <el-option label="是" :value="true" />
            <el-option label="否" :value="false" />
          </el-select>
        </el-form-item>
        <el-form-item label="验证日期">
          <div class="date-range">
            <el-date-picker
              v-model="query.verifiedDateStart"
              type="date"
              value-format="YYYY-MM-DD"
              placeholder="开始日期"
              style="width: 145px"
            />
            <span>至</span>
            <el-date-picker
              v-model="query.verifiedDateEnd"
              type="date"
              value-format="YYYY-MM-DD"
              placeholder="结束日期"
              style="width: 145px"
            />
          </div>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">查询</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-card shadow="never" class="table-card">
      <div class="table-responsive">
        <el-table v-loading="loading" :data="records" border stripe style="min-width: 1250px">
          <el-table-column prop="verificationNo" label="验证编号" width="150" fixed="left" />
          <el-table-column label="审计问题" min-width="230" show-overflow-tooltip>
            <template #default="{ row }">{{ getIssueLabel(row.auditIssueId) }}</template>
          </el-table-column>
          <el-table-column label="整改措施" min-width="260" show-overflow-tooltip>
            <template #default="{ row }">{{ getActionLabel(row.correctiveActionId) }}</template>
          </el-table-column>
          <el-table-column label="验证结果" width="130">
            <template #default="{ row }">
              <el-tag :type="getResultTagType(row.verificationResult)">
                {{ getResultLabel(row.verificationResult) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="verificationComment" label="验证意见" min-width="220" show-overflow-tooltip />
          <el-table-column prop="verifier" label="验证人" width="130" show-overflow-tooltip />
          <el-table-column label="验证时间" width="170">
            <template #default="{ row }">{{ formatDate(row.verifiedAt) }}</template>
          </el-table-column>
          <el-table-column label="是否通过" width="100">
            <template #default="{ row }">
              <el-tag :type="row.isPassed ? 'success' : 'danger'">{{ row.isPassed ? '是' : '否' }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column label="创建时间" width="170">
            <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
          </el-table-column>
          <el-table-column label="操作" width="210" fixed="right">
            <template #default="{ row }">
              <div class="row-actions">
                <el-button type="text" @click="openDetail(row)">详情</el-button>
                <el-button type="text" @click="openEdit(row)">编辑</el-button>
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
          :page-sizes="[10, 20, 50, 100]"
          :total="total"
          @current-change="handlePageChange"
          @size-change="handlePageSizeChange"
        />
      </div>
    </el-card>

    <el-dialog
      v-model="formDialogVisible"
      :title="formMode === 'create' ? '新增整改验证' : '编辑整改验证'"
      width="820px"
      destroy-on-close
    >
      <el-form ref="formRef" v-loading="formLoading" :model="form" :rules="formRules" label-width="120px">
        <el-alert
          title="通过：审计问题将自动关闭；不通过或需补充材料：审计问题将返回整改中。"
          type="info"
          :closable="false"
          show-icon
          class="linkage-tip"
        />
        <el-row :gutter="20">
          <template v-if="formMode === 'create'">
            <el-col :span="12">
              <el-form-item label="审计问题" prop="auditIssueId">
                <el-select
                  v-model="form.auditIssueId"
                  filterable
                  placeholder="请选择审计问题"
                  style="width: 100%"
                  @change="handleFormIssueChange"
                >
                  <el-option
                    v-for="item in issueOptions"
                    :key="item.id"
                    :label="`${item.issueNo} ${item.title}`"
                    :value="item.id"
                  />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="整改措施" prop="correctiveActionId">
                <el-select
                  v-model="form.correctiveActionId"
                  filterable
                  :disabled="!form.auditIssueId"
                  :loading="formActionsLoading"
                  placeholder="请选择已完成的整改措施"
                  style="width: 100%"
                  @change="handleFormActionChange"
                >
                  <el-option
                    v-for="item in formActionOptions"
                    :key="item.id"
                    :label="`${item.actionNo} ${item.actionDescription}`"
                    :value="item.id"
                  />
                </el-select>
              </el-form-item>
            </el-col>
          </template>
          <template v-else>
            <el-col :span="12">
              <el-form-item label="审计问题"><el-input v-model="form.issueLabel" disabled /></el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="整改措施"><el-input v-model="form.actionLabel" disabled /></el-form-item>
            </el-col>
          </template>
          <el-col :span="12">
            <el-form-item label="验证编号" prop="verificationNo">
              <el-input v-model="form.verificationNo" :disabled="formMode === 'edit'" maxlength="50" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="验证结果" prop="verificationResult">
              <el-select v-model="form.verificationResult" placeholder="请选择验证结果" style="width: 100%">
                <el-option label="通过" value="Passed" />
                <el-option label="不通过" value="Failed" />
                <el-option label="需补充材料" value="NeedMoreEvidence" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="验证人" prop="verifier">
              <el-input v-model="form.verifier" maxlength="100" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="验证时间" prop="verifiedAt">
              <el-date-picker
                v-model="form.verifiedAt"
                type="datetime"
                value-format="YYYY-MM-DDTHH:mm:ss"
                placeholder="未选择时由后端取当前时间"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="验证意见" prop="verificationComment">
              <el-input
                v-model="form.verificationComment"
                type="textarea"
                :rows="5"
                maxlength="4000"
                show-word-limit
              />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <template #footer>
        <el-button @click="formDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="formLoading" @click="submitForm">保存</el-button>
      </template>
    </el-dialog>

    <el-drawer v-model="detailVisible" title="整改验证详情" size="55%">
      <div v-loading="detailLoading">
        <el-descriptions v-if="detail" :column="2" border>
          <el-descriptions-item label="验证编号">{{ detail.verificationNo }}</el-descriptions-item>
          <el-descriptions-item label="验证结果">
            <el-tag :type="getResultTagType(detail.verificationResult)">
              {{ getResultLabel(detail.verificationResult) }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="审计问题">{{ detail.issueNo }} {{ detail.issueTitle }}</el-descriptions-item>
          <el-descriptions-item label="审计计划 ID">{{ detail.auditPlanId }}</el-descriptions-item>
          <el-descriptions-item label="整改措施" :span="2">
            {{ detail.actionNo }} {{ detail.actionDescription }}
          </el-descriptions-item>
          <el-descriptions-item label="验证人">{{ detail.verifier || '-' }}</el-descriptions-item>
          <el-descriptions-item label="验证时间">{{ formatDate(detail.verifiedAt) }}</el-descriptions-item>
          <el-descriptions-item label="是否通过">{{ detail.isPassed ? '是' : '否' }}</el-descriptions-item>
          <el-descriptions-item label="创建人">{{ detail.createdBy || '-' }}</el-descriptions-item>
          <el-descriptions-item label="创建时间">{{ formatDate(detail.createdAt) }}</el-descriptions-item>
          <el-descriptions-item label="更新人">{{ detail.updatedBy || '-' }}</el-descriptions-item>
          <el-descriptions-item label="更新时间">{{ formatDate(detail.updatedAt) }}</el-descriptions-item>
          <el-descriptions-item label="验证意见" :span="2">
            <div class="detail-text">{{ detail.verificationComment || '-' }}</div>
          </el-descriptions-item>
        </el-descriptions>
      </div>
    </el-drawer>

    <el-dialog v-model="recycleVisible" title="整改验证回收站" width="1000px">
      <div class="table-responsive">
        <el-table v-loading="recycleLoading" :data="recycleList" border stripe style="min-width: 900px">
          <el-table-column prop="verificationNo" label="验证编号" width="150" />
          <el-table-column label="验证结果" width="130">
            <template #default="{ row }">{{ getResultLabel(row.verificationResult) }}</template>
          </el-table-column>
          <el-table-column prop="verifier" label="验证人" width="130" />
          <el-table-column label="验证时间" width="180">
            <template #default="{ row }">{{ formatDate(row.verifiedAt) }}</template>
          </el-table-column>
          <el-table-column label="删除时间" width="180">
            <template #default="{ row }">{{ formatDate(row.deletedAt) }}</template>
          </el-table-column>
          <el-table-column prop="deletedBy" label="删除人" min-width="140">
            <template #default="{ row }">{{ row.deletedBy || '-' }}</template>
          </el-table-column>
          <el-table-column label="操作" width="100" fixed="right">
            <template #default="{ row }">
              <el-button type="text" @click="handleRestore(row)">恢复</el-button>
            </template>
          </el-table-column>
        </el-table>
      </div>
      <template #footer><el-button @click="recycleVisible = false">关闭</el-button></template>
    </el-dialog>

    <el-dialog v-model="logsVisible" title="整改验证操作日志" width="1100px">
      <div class="table-responsive">
        <el-table v-loading="logsLoading" :data="operationLogs" border stripe style="min-width: 1050px">
          <el-table-column prop="operationType" label="操作类型" width="120" />
          <el-table-column prop="operator" label="操作人" width="130" />
          <el-table-column prop="remark" label="备注" min-width="170">
            <template #default="{ row }">{{ row.remark || '-' }}</template>
          </el-table-column>
          <el-table-column label="操作时间" width="180">
            <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
          </el-table-column>
          <el-table-column label="操作前" min-width="300">
            <template #default="{ row }"><pre class="json-cell">{{ formatJson(row.beforeData) }}</pre></template>
          </el-table-column>
          <el-table-column label="操作后" min-width="300">
            <template #default="{ row }"><pre class="json-cell">{{ formatJson(row.afterData) }}</pre></template>
          </el-table-column>
        </el-table>
      </div>
      <template #footer><el-button @click="logsVisible = false">关闭</el-button></template>
    </el-dialog>
  </div>
</template>

<style scoped>
.page-shell { display: flex; flex-direction: column; gap: 16px; }
.page-header { display: flex; align-items: center; justify-content: space-between; gap: 16px; }
.page-header h2 { margin: 4px 0 0; }
.eyebrow { margin: 0; color: #909399; font-size: 13px; }
.header-actions { display: flex; gap: 8px; }
.filter-card, .table-card { border-radius: 10px; }
.search-form { margin-bottom: -18px; }
.date-range { display: flex; align-items: center; gap: 8px; }
.table-responsive { width: 100%; overflow-x: auto; }
.row-actions { display: flex; gap: 8px; white-space: nowrap; }
.pagination-wrapper { display: flex; justify-content: flex-end; margin-top: 16px; }
.linkage-tip { margin-bottom: 20px; }
.detail-text { white-space: pre-wrap; line-height: 1.7; }
.json-cell {
  max-height: 260px;
  margin: 0;
  overflow: auto;
  white-space: pre-wrap;
  overflow-wrap: anywhere;
  font-family: Consolas, monospace;
  font-size: 12px;
}
</style>
