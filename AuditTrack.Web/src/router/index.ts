import { createRouter, createWebHistory } from 'vue-router'

import AuditPlanListView from '@/views/audit-plans/AuditPlanListView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/audit-plans',
    },
    {
      path: '/audit-plans',
      name: 'audit-plans',
      component: AuditPlanListView,
    },
  ],
})

export default router
