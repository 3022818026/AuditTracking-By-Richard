export interface CorrectiveAction {
  id: number
  auditIssueId: number
  actionNo: string
  actionDescription: string
  responsibleDepartment?: string | null
  responsiblePerson?: string | null
  plannedCompletionDate?: string | null
  actualCompletionDate?: string | null
  completionDescription?: string | null
  status: 'Draft' | 'Submitted' | 'Approved' | 'Rejected' | 'Completed'
  submittedAt?: string | null
  approvedAt?: string | null
  completedAt?: string | null
  createdAt: string
  createdBy: string
  updatedAt?: string | null
  updatedBy?: string | null
}

export interface CorrectiveActionListItem extends CorrectiveAction {}

export interface CorrectiveActionDetail extends CorrectiveAction {
  issueNo?: string
  issueTitle?: string
  auditPlanId?: number
}

export interface CorrectiveActionQuery {
  keyword?: string
  auditIssueId?: number | null
  status?: string
  responsibleDepartment?: string
  responsiblePerson?: string
  plannedDateStart?: string | null
  plannedDateEnd?: string | null
  isOverdue?: boolean | null
  page: number
  pageSize: number
}

export interface CreateCorrectiveActionRequest {
  auditIssueId: number
  actionNo: string
  actionDescription: string
  responsibleDepartment?: string
  responsiblePerson?: string
  plannedCompletionDate?: string | null
  completionDescription?: string
}

export interface UpdateCorrectiveActionRequest {
  actionDescription: string
  responsibleDepartment?: string
  responsiblePerson?: string
  plannedCompletionDate?: string | null
  completionDescription?: string
}

export interface ChangeCorrectiveActionStatusRequest {
  status: 'Draft' | 'Submitted' | 'Approved' | 'Rejected' | 'Completed'
  completionDescription?: string
  remark?: string
}

export interface CorrectiveActionOperationLog {
  id: number
  correctiveActionId: number
  actionNo: string
  operationType: string
  beforeData?: string | null
  afterData?: string | null
  operator: string
  remark?: string | null
  createdAt: string
}

export interface AuditIssueOption {
  id: number
  issueNo: string
  title: string
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  total: number
  totalPages: number
}
