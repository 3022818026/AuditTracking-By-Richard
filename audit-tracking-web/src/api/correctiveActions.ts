import request from '@/utils/request'
import type {
  CorrectiveAction,
  CorrectiveActionDetail,
  CorrectiveActionQuery,
  CreateCorrectiveActionRequest,
  UpdateCorrectiveActionRequest,
  ChangeCorrectiveActionStatusRequest,
  CorrectiveActionOperationLog,
  PagedResult,
  AuditIssueOption,
} from '@/types/correctiveAction'

export function getCorrectiveActions(params: CorrectiveActionQuery): Promise<PagedResult<CorrectiveAction>> {
  return request.get('/corrective-actions', { params })
}

export function getCorrectiveActionById(id: number): Promise<CorrectiveActionDetail> {
  return request.get(`/corrective-actions/${id}`)
}

export function createCorrectiveAction(data: CreateCorrectiveActionRequest): Promise<CorrectiveAction> {
  return request.post('/corrective-actions', data)
}

export function updateCorrectiveAction(id: number, data: UpdateCorrectiveActionRequest): Promise<CorrectiveAction> {
  return request.put(`/corrective-actions/${id}`, data)
}

export function changeCorrectiveActionStatus(id: number, data: ChangeCorrectiveActionStatusRequest): Promise<CorrectiveAction> {
  return request.put(`/corrective-actions/${id}/status`, data)
}

export function deleteCorrectiveAction(id: number): Promise<{ message: string }> {
  return request.delete(`/corrective-actions/${id}`)
}

export function getCorrectiveActionRecycleBin(): Promise<CorrectiveAction[]> {
  return request.get('/corrective-actions/recycle-bin')
}

export function restoreCorrectiveAction(id: number): Promise<CorrectiveAction> {
  return request.put(`/corrective-actions/${id}/restore`)
}

export function getCorrectiveActionLogs(id: number): Promise<CorrectiveActionOperationLog[]> {
  return request.get(`/corrective-actions/${id}/logs`)
}

// helper to fetch audit issues for select (IssueNo + Title)
import { getAuditIssues } from '@/api/auditIssues'

export async function getAuditIssueOptions(): Promise<AuditIssueOption[]> {
  const res = await getAuditIssues({ page: 1, pageSize: 1000 })
  return res.items.map((i: any) => ({ id: i.id, issueNo: i.issueNo, title: i.title }))
}
