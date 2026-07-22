import { createRouter, createWebHistory } from 'vue-router'

import AdminLayout from '@/layouts/AdminLayout.vue'
import LoginView from '@/views/LoginView.vue'
import DashboardView from '@/views/DashboardView.vue'
import AuditPlanListView from '@/views/audit-plans/AuditPlanListView.vue'
import AuditIssuesView from '@/views/AuditIssuesView.vue'
import CorrectiveActionsView from '@/views/CorrectiveActionsView.vue'
import RectificationVerificationsView from '@/views/RectificationVerificationsView.vue'
import { pinia } from '@/stores'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: LoginView,
    },
    {
      path: '/',
      component: AdminLayout,
      meta: { requiresAuth: true },
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

router.beforeEach(async (to) => {
  const authStore = useAuthStore(pinia)
  await authStore.initializeAuth()

  if (to.name === 'login' && authStore.isAuthenticated) {
    return { name: 'dashboard' }
  }

  const authGuardDisabled =
    import.meta.env.DEV && import.meta.env.VITE_DISABLE_AUTH_GUARD === 'true'

  if (to.meta.requiresAuth && !authGuardDisabled && !authStore.isAuthenticated) {
    return {
      name: 'login',
      query: { redirect: to.fullPath },
    }
  }

  return true
})

export default router
