import request from '@/utils/request'
import { getAuditIssues } from '@/api/auditIssues'
import { getCorrectiveActions } from '@/api/correctiveActions'

import type {
  AuditIssueOption,
  CorrectiveActionOption,
  CreateRectificationVerificationRequest,
  PagedResult,
  RectificationVerification,
  RectificationVerificationDetail,
  RectificationVerificationListItem,
  RectificationVerificationOperationLog,
  RectificationVerificationQuery,
  UpdateRectificationVerificationRequest,
} from '@/types/rectificationVerification'

export function getRectificationVerifications(
  params: RectificationVerificationQuery,
): Promise<PagedResult<RectificationVerificationListItem>> {
  return request.get('/rectification-verifications', { params })
}

export function getRectificationVerificationById(
  id: number,
): Promise<RectificationVerificationDetail> {
  return request.get(`/rectification-verifications/${id}`)
}

export function createRectificationVerification(
  data: CreateRectificationVerificationRequest,
): Promise<RectificationVerification> {
  return request.post('/rectification-verifications', data)
}

export function updateRectificationVerification(
  id: number,
  data: UpdateRectificationVerificationRequest,
): Promise<RectificationVerification> {
  return request.put(`/rectification-verifications/${id}`, data)
}

export function deleteRectificationVerification(id: number): Promise<void> {
  return request.delete(`/rectification-verifications/${id}`)
}

export function getRectificationVerificationRecycleBin(): Promise<RectificationVerification[]> {
  return request.get('/rectification-verifications/recycle-bin')
}

export function restoreRectificationVerification(id: number): Promise<RectificationVerification> {
  return request.put(`/rectification-verifications/${id}/restore`)
}

export function getRectificationVerificationLogs(
  id: number,
): Promise<RectificationVerificationOperationLog[]> {
  return request.get(`/rectification-verifications/${id}/logs`)
}

export async function getAuditIssueOptions(): Promise<AuditIssueOption[]> {
  const result = await getAuditIssues({ page: 1, pageSize: 100 })
  return result.items.map(({ id, issueNo, title }) => ({ id, issueNo, title }))
}

export async function getCorrectiveActionOptions(
  auditIssueId?: number,
  completedOnly = false,
): Promise<CorrectiveActionOption[]> {
  const result = await getCorrectiveActions({
    auditIssueId: auditIssueId ?? null,
    status: completedOnly ? 'Completed' : '',
    page: 1,
    pageSize: 100,
  })

  return result.items.map(({ id, auditIssueId: issueId, actionNo, actionDescription, status }) => ({
    id,
    auditIssueId: issueId,
    actionNo,
    actionDescription,
    status,
  }))
}
