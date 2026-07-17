import request from '@/utils/request'

import type {
  AuditPlan,
  AuditPlanDetail,
  AuditPlanOperationLog,
  AuditPlanPagedResult,
  AuditPlanQuery,
  AuditPlanRiskQuery,
  AuditPlanRiskStatistics,
  AuditPlanStatistics,
  CreateAuditPlanData,
  UpdateAuditPlanData,
} from '@/types/audit-plan'

export function getAuditPlans(params: AuditPlanQuery): Promise<AuditPlanPagedResult> {
  return request.get('/audit-plans', {
    params,
  })
}

export function getAuditPlanById(id: number): Promise<AuditPlan> {
  return request.get(`/audit-plans/${id}`)
}

export function getAuditPlanDetail(id: number): Promise<AuditPlanDetail> {
  return request.get(`/audit-plans/${id}/detail`)
}

export function createAuditPlan(data: CreateAuditPlanData): Promise<AuditPlan> {
  return request.post('/audit-plans', data)
}

export function updateAuditPlan(id: number, data: UpdateAuditPlanData): Promise<AuditPlan> {
  return request.put(`/audit-plans/${id}`, data)
}

export function deleteAuditPlan(id: number): Promise<{ message: string }> {
  return request.delete(`/audit-plans/${id}`)
}

export function getAuditPlanStatistics(): Promise<AuditPlanStatistics> {
  return request.get('/audit-plans/statistics')
}

export function getAuditPlanRiskStatistics(): Promise<AuditPlanRiskStatistics> {
  return request.get('/audit-plans/risk-statistics')
}

export function getAuditPlanRiskList(
  params: AuditPlanRiskQuery,
): Promise<AuditPlanPagedResult> {
  return request.get('/audit-plans/risk-list', {
    params,
  })
}

export function getRecycleBin(): Promise<AuditPlan[]> {
  return request.get('/audit-plans/recycle-bin')
}

export function restoreAuditPlan(
  id: number,
): Promise<{ message: string; data: AuditPlan }> {
  return request.put(`/audit-plans/${id}/restore`)
}

export function getAuditPlanLogs(
  id: number,
): Promise<AuditPlanOperationLog[]> {
  return request.get(`/audit-plans/${id}/logs`)
}
