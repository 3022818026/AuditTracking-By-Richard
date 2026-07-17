import request from '@/utils/request'

import type {
  AuditIssue,
  AuditIssueDetail,
  AuditIssueQuery,
  CreateAuditIssueRequest,
  UpdateAuditIssueRequest,
  ChangeAuditIssueStatusRequest,
  AuditIssueOperationLog,
  PagedResult,
  AuditPlanOption,
} from '@/types/auditIssue'

export function getAuditIssues(params: AuditIssueQuery): Promise<PagedResult<AuditIssue>> {
  return request.get('/audit-issues', { params })
}

export function getAuditIssueById(id: number): Promise<AuditIssueDetail> {
  return request.get(`/audit-issues/${id}`)
}

export function createAuditIssue(data: CreateAuditIssueRequest): Promise<AuditIssue> {
  return request.post('/audit-issues', data)
}

export function updateAuditIssue(id: number, data: UpdateAuditIssueRequest): Promise<AuditIssue> {
  return request.put(`/audit-issues/${id}`, data)
}

export function changeAuditIssueStatus(id: number, data: ChangeAuditIssueStatusRequest): Promise<AuditIssue> {
  return request.put(`/audit-issues/${id}/status`, data)
}

export function deleteAuditIssue(id: number): Promise<{ message: string }> {
  return request.delete(`/audit-issues/${id}`)
}

export function getAuditIssueRecycleBin(): Promise<AuditIssue[]> {
  return request.get('/audit-issues/recycle-bin')
}

export function restoreAuditIssue(id: number): Promise<AuditIssue> {
  return request.put(`/audit-issues/${id}/restore`)
}

export function getAuditIssueLogs(id: number): Promise<AuditIssueOperationLog[]> {
  return request.get(`/audit-issues/${id}/logs`)
}

// helper to fetch audit plans for select (returns id, auditNo, title)
import { getAuditPlans } from '@/api/audit-plans'

export async function getAuditPlanOptions(): Promise<AuditPlanOption[]> {
  const res = await getAuditPlans({ page: 1, pageSize: 1000 })
  return res.items.map((p: any) => ({ id: p.id, auditNo: p.auditNo, title: p.title }))
}
