export interface RecentAuditPlan {
  id: number
  auditNo: string
  title: string
  status: string
  plannedDate: string | null
  auditee: string | null
  auditor: string | null
  createdAt: string | null
}

export interface RecentAuditIssue {
  id: number
  issueNo: string
  title: string
  severity: string
  status: string
  dueDate: string | null
  responsibleDepartment: string | null
  responsiblePerson: string | null
  auditPlanId: number
  createdAt: string | null
}

export interface DashboardSummary {
  auditPlanTotal: number
  auditPlanDraft: number
  auditPlanInProgress: number
  auditPlanCompleted: number
  auditPlanClosed: number
  auditPlanCancelled: number
  auditPlanOverdue: number

  auditIssueTotal: number
  auditIssueOpen: number
  auditIssueRectifying: number
  auditIssuePendingVerification: number
  auditIssueClosed: number
  auditIssueRejected: number
  auditIssueOverdue: number

  correctiveActionTotal: number
  correctiveActionDraft: number
  correctiveActionSubmitted: number
  correctiveActionApproved: number
  correctiveActionRejected: number
  correctiveActionCompleted: number
  correctiveActionOverdue: number

  rectificationVerificationTotal: number
  rectificationVerificationPassed: number
  rectificationVerificationFailed: number
  rectificationVerificationNeedMoreEvidence: number

  correctiveActionCompletionRate: number
  recentAuditPlans: RecentAuditPlan[]
  recentAuditIssues: RecentAuditIssue[]
}
