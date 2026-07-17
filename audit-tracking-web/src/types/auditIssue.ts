export interface AuditIssue {
  id: number
  auditPlanId: number
  issueNo: string
  title: string
  description: string
  issueType?: string | null
  severity: 'Low' | 'Medium' | 'High' | 'Critical'
  responsibleDepartment?: string | null
  responsiblePerson?: string | null
  dueDate?: string | null
  status: 'Open' | 'Rectifying' | 'PendingVerification' | 'Closed' | 'Rejected'
  closedAt?: string | null
  createdAt: string
  createdBy: string
  updatedAt?: string | null
  updatedBy?: string | null
}

export interface AuditIssueListItem extends AuditIssue {}

export interface AuditIssueDetail {
  id: number
  auditPlanId: number
  auditNo: string
  auditTitle: string
  issueNo: string
  title: string
  description: string
  issueType?: string | null
  severity: 'Low' | 'Medium' | 'High' | 'Critical'
  responsibleDepartment?: string | null
  responsiblePerson?: string | null
  dueDate?: string | null
  status: 'Open' | 'Rectifying' | 'PendingVerification' | 'Closed' | 'Rejected'
  closedAt?: string | null
  createdAt: string
  createdBy: string
  updatedAt?: string | null
  updatedBy?: string | null
}

export interface AuditIssueQuery {
  keyword?: string
  auditPlanId?: number | null
  status?: string
  severity?: string
  issueType?: string
  responsibleDepartment?: string
  responsiblePerson?: string
  dueDateStart?: string | null
  dueDateEnd?: string | null
  isOverdue?: boolean | null
  page: number
  pageSize: number
}

export interface CreateAuditIssueRequest {
  auditPlanId: number
  issueNo: string
  title: string
  description: string
  issueType?: string
  severity: 'Low' | 'Medium' | 'High' | 'Critical'
  responsibleDepartment?: string
  responsiblePerson?: string
  dueDate?: string | null
}

export interface UpdateAuditIssueRequest {
  title: string
  description: string
  issueType?: string
  severity: 'Low' | 'Medium' | 'High' | 'Critical'
  responsibleDepartment?: string
  responsiblePerson?: string
  dueDate?: string | null
}

export interface ChangeAuditIssueStatusRequest {
  status: 'Open' | 'Rectifying' | 'PendingVerification' | 'Closed' | 'Rejected'
  remark?: string
}

export interface AuditIssueOperationLog {
  id: number
  auditIssueId: number
  issueNo: string
  operationType: string
  beforeData?: string | null
  afterData?: string | null
  operator: string
  remark?: string | null
  createdAt: string
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  total: number
  totalPages: number
}

export interface AuditPlanOption {
  id: number
  auditNo: string
  title: string
}
