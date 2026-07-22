export type VerificationResult = 'Passed' | 'Failed' | 'NeedMoreEvidence'

export interface RectificationVerification {
  id: number
  auditIssueId: number
  correctiveActionId: number
  verificationNo: string
  verificationResult: VerificationResult
  verificationComment: string
  verifier: string
  verifiedAt: string
  isPassed: boolean
  createdAt: string
  createdBy: string
  updatedAt: string | null
  updatedBy: string | null
  isDeleted?: boolean
  deletedAt?: string | null
  deletedBy?: string | null
}

export interface RectificationVerificationListItem extends RectificationVerification {}

export interface RectificationVerificationDetail extends RectificationVerification {
  issueNo: string
  issueTitle: string
  actionNo: string
  actionDescription: string
  auditPlanId: number
}

export interface RectificationVerificationQuery {
  keyword?: string
  auditIssueId?: number | null
  correctiveActionId?: number | null
  verificationResult?: VerificationResult | ''
  isPassed?: boolean | null
  verifiedDateStart?: string | null
  verifiedDateEnd?: string | null
  page: number
  pageSize: number
}

export interface CreateRectificationVerificationRequest {
  auditIssueId: number
  correctiveActionId: number
  verificationNo: string
  verificationResult: VerificationResult
  verificationComment: string
  verifier: string
  verifiedAt?: string | null
}

export interface UpdateRectificationVerificationRequest {
  verificationResult: VerificationResult
  verificationComment: string
  verifier: string
  verifiedAt?: string | null
}

export interface RectificationVerificationOperationLog {
  id: number
  rectificationVerificationId: number
  verificationNo: string
  operationType: string
  beforeData: string | null
  afterData: string | null
  operator: string
  remark: string | null
  createdAt: string
}

export interface AuditIssueOption {
  id: number
  issueNo: string
  title: string
}

export interface CorrectiveActionOption {
  id: number
  auditIssueId: number
  actionNo: string
  actionDescription: string
  status: string
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  total: number
  totalPages: number
}
