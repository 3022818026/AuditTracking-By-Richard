<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'

import { getAuditPlans } from '@/api/audit-plans'

import type { AuditPlan, AuditPlanQuery } from '@/types/audit-plan'

const loading = ref(false)
const plans = ref<AuditPlan[]>([])
const total = ref(0)

const query = reactive<AuditPlanQuery>({
  keyword: '',
  status: '',
  auditType: '',
  page: 1,
  pageSize: 10,
})

const tableEmptyText = computed(() => {
  return loading.value ? '正在加载...' : '暂无审计计划数据'
})

async function loadPlans() {
  loading.value = true

  try {
    const result = await getAuditPlans(query)

    plans.value = result.items
    total.value = result.total
  } catch (error) {
    console.error(error)

    plans.value = []
    total.value = 0

    ElMessage.error('审计计划加载失败，请确认后端服务已启动')
  } finally {
    loading.value = false
  }
}

function handleSearch() {
  query.page = 1
  loadPlans()
}

function handleReset() {
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

onMounted(() => {
  loadPlans()
})
</script>

<template>
  <div class="page-shell">
    <div class="page-header">
      <div>
        <p class="eyebrow">AuditTrack • 审计中心</p>
        <h2>审计计划管理</h2>
        <p class="sub-title">管理审计计划、执行时间和审核状态</p>
      </div>
      <div class="header-badge">实时同步</div>
    </div>

    <el-card shadow="never" class="filter-card">
      <el-form :inline="true" :model="query" class="search-form">
        <el-form-item label="关键词">
          <el-input v-model="query.keyword" placeholder="编号、标题或被审计对象" clearable style="width: 240px" @keyup.enter="handleSearch" />
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
          <el-select v-model="query.auditType" placeholder="全部类型" clearable allow-create filterable style="width: 180px">
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
      <el-table v-loading="loading" :data="plans" :empty-text="tableEmptyText" border stripe>
        <el-table-column prop="auditNo" label="审计编号" width="150" fixed="left" />
        <el-table-column prop="title" label="审计标题" min-width="220" show-overflow-tooltip />
        <el-table-column prop="auditType" label="审计类型" width="130">
          <template #default="{ row }">
            {{ row.auditType || '-' }}
          </template>
        </el-table-column>
        <el-table-column prop="auditee" label="被审计对象" width="160" show-overflow-tooltip>
          <template #default="{ row }">
            {{ row.auditee || '-' }}
          </template>
        </el-table-column>
        <el-table-column prop="auditor" label="审计人员" width="130">
          <template #default="{ row }">
            {{ row.auditor || '-' }}
          </template>
        </el-table-column>
        <el-table-column prop="status" label="状态" width="110">
          <template #default="{ row }">
            <el-tag :type="getStatusType(row.status)">{{ formatStatus(row.status) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="计划日期" width="180">
          <template #default="{ row }">
            {{ formatDate(row.plannedDate) }}
          </template>
        </el-table-column>
        <el-table-column prop="result" label="审计结果" width="130">
          <template #default="{ row }">
            {{ row.result || '-' }}
          </template>
        </el-table-column>
        <el-table-column prop="remark" label="备注" min-width="180" show-overflow-tooltip>
          <template #default="{ row }">
            {{ row.remark || '-' }}
          </template>
        </el-table-column>
      </el-table>

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
  </div>
</template>

<style scoped>
.page-shell {
  min-height: 100vh;
  padding: 24px;
  background: linear-gradient(135deg, #fff7fb 0%, #f5ecff 100%);
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
  padding: 24px 28px;
  border-radius: 24px;
  background: rgba(255, 255, 255, 0.85);
  box-shadow: 0 20px 45px rgba(173, 104, 255, 0.12);
  backdrop-filter: blur(10px);
}

.eyebrow {
  margin: 0 0 6px;
  color: #b25cd2;
  font-size: 12px;
  letter-spacing: 0.24em;
  text-transform: uppercase;
  font-weight: 700;
}

.page-header h2 {
  margin: 0;
  color: #4b2e6b;
  font-size: 24px;
}

.sub-title {
  margin: 6px 0 0;
  color: #8b7aa8;
  font-size: 14px;
}

.header-badge {
  padding: 8px 14px;
  border-radius: 999px;
  background: linear-gradient(90deg, #ff5fb2 0%, #8a5cf6 100%);
  color: white;
  font-size: 13px;
  font-weight: 600;
}

.filter-card,
.table-card {
  border: none;
  border-radius: 20px;
  background: rgba(255, 255, 255, 0.92);
  box-shadow: 0 16px 40px rgba(173, 104, 255, 0.1);
}

.search-form {
  margin-bottom: -16px;
}

.table-card {
  margin-top: 20px;
}

.pagination-wrapper {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
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

  .pagination-wrapper {
    justify-content: flex-start;
    overflow-x: auto;
  }
}
</style>
