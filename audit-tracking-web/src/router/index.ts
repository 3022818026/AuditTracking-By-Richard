import { createRouter, createWebHistory } from 'vue-router'

import AdminLayout from '@/layouts/AdminLayout.vue'
import DashboardView from '@/views/DashboardView.vue'
import AuditPlanListView from '@/views/audit-plans/AuditPlanListView.vue'
import AuditIssuesView from '@/views/AuditIssuesView.vue'
import CorrectiveActionsView from '@/views/CorrectiveActionsView.vue'
import RectificationVerificationsView from '@/views/RectificationVerificationsView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      component: AdminLayout,
      children: [
        { path: '', name: 'dashboard', component: DashboardView },
        { path: 'audit-plans', name: 'audit-plans', component: AuditPlanListView },
        { path: 'audit-issues', name: 'audit-issues', component: AuditIssuesView },
        { path: 'corrective-actions', name: 'corrective-actions', component: CorrectiveActionsView },
        { path: 'rectification-verifications', name: 'rectification-verifications', component: RectificationVerificationsView },
      ],
    },
  ],
})

export default router
