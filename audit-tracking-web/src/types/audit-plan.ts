export interface AuditPlan {
  id: number
  auditNo: string
  title: string
  auditType: string | null
  plannedDate: string
  auditee: string | null
  auditor: string | null
  status: string
  result: string | null
  remark: string | null
  createdAt: string
  updatedAt: string | null
}

export interface AuditPlanQuery {
  keyword?: string
  status?: string
  auditType?: string
  startDate?: string
  endDate?: string
  page: number
  pageSize: number
}

export interface AuditPlanPagedResult {
  items: AuditPlan[]
  page: number
  pageSize: number
  total: number
  totalPages: number
}

export interface CreateAuditPlanData {
  auditNo: string
  title: string
  auditType?: string
  plannedDate: string
  auditee?: string
  auditor?: string
  remark?: string
}

export interface UpdateAuditPlanData {
  title: string
  auditType?: string
  plannedDate: string
  auditee?: string
  auditor?: string
  status: string
  result?: string
  remark?: string
}
