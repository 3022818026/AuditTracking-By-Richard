import request from '@/utils/request'

import type { DashboardSummary } from '@/types/dashboard'

export function getDashboardSummary(): Promise<DashboardSummary> {
  return request.get('/dashboard/summary')
}
