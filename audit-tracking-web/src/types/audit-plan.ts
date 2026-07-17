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
  deletedAt?: string | null
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

export interface AuditPlanRiskQuery {
  type: 'Overdue' | 'DueSoon'
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

export interface AuditPlanStatistics {
  total: number
  draft: number
  inProgress: number
  completed: number
  closed: number
  cancelled: number
}

export interface AuditPlanRiskStatistics {
  overdue: number
  dueWithinSevenDays: number
  completedThisMonth: number
}

export interface AuditPlanOperationLog {
  id: number
  auditPlanId: number
  auditNo: string
  operationType: string
  beforeData: string | null
  afterData: string | null
  operator: string
  createdAt: string
}

export interface AuditPlanDetail {
  plan: AuditPlan
  logs: AuditPlanOperationLog[]
}
