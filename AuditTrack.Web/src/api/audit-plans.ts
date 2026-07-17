import request from '@/utils/request'

import type {
  AuditPlan,
  AuditPlanPagedResult,
  AuditPlanQuery,
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

export function createAuditPlan(data: CreateAuditPlanData): Promise<AuditPlan> {
  return request.post('/audit-plans', data)
}

export function updateAuditPlan(id: number, data: UpdateAuditPlanData): Promise<AuditPlan> {
  return request.put(`/audit-plans/${id}`, data)
}

export function deleteAuditPlan(id: number): Promise<{ message: string }> {
  return request.delete(`/audit-plans/${id}`)
}
