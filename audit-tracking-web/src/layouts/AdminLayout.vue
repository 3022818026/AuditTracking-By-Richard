<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'

import { useAuthStore } from '@/stores/auth'
import ChangePasswordDialog from '@/components/auth/ChangePasswordDialog.vue'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const changePasswordVisible = ref(false)

const roleLabel = computed(() => {
  if (authStore.currentUser?.role === 'Admin') return '管理员'
  if (authStore.currentUser?.role === 'User') return '普通用户'
  return authStore.currentUser?.role || '-'
})

async function handleLogout() {
  try {
    await ElMessageBox.confirm('确定退出当前登录吗？', '退出登录', {
      confirmButtonText: '退出',
      cancelButtonText: '取消',
      type: 'warning',
    })
    authStore.logout()
    ElMessage.success('已退出登录')
    await router.replace('/login')
  } catch (error) {
    if (error !== 'cancel' && error !== 'close') throw error
  }
}

function handleUserCommand(command: string) {
  if (command === 'change-password') {
    changePasswordVisible.value = true
    return
  }

  if (command === 'logout') handleLogout()
}
</script>

<template>
  <el-container class="app-shell">
    <el-header class="top-header">
      <div class="brand-area">
        <span>Design By</span>
        <strong>Richard_Xia</strong>
      </div>

      <div class="header-main">
        <div class="system-name">审计跟踪管理系统</div>
        <div class="user-area">
          <el-dropdown trigger="click" @command="handleUserCommand">
            <button type="button" class="user-trigger">
              <span class="user-copy">
                <strong>{{ authStore.currentUser?.displayName || '-' }}</strong>
                <span>{{ authStore.currentUser?.userName || '-' }} · {{ roleLabel }}</span>
              </span>
              <span class="menu-caret">⌄</span>
            </button>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item command="change-password">修改密码</el-dropdown-item>
                <el-dropdown-item command="logout" divided>退出登录</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </div>
    </el-header>

    <el-container class="content-shell">
      <el-aside width="220px" class="side-panel">
        <el-menu :default-active="route.path" router class="side-menu">
          <el-menu-item index="/">仪表盘</el-menu-item>
          <el-menu-item index="/audit-plans">审计计划</el-menu-item>
          <el-menu-item index="/audit-issues">审计问题</el-menu-item>
          <el-menu-item index="/corrective-actions">整改措施</el-menu-item>
          <el-menu-item index="/rectification-verifications">整改验证</el-menu-item>
        </el-menu>
      </el-aside>

      <el-main class="main-content">
        <router-view />
      </el-main>
    </el-container>

    <ChangePasswordDialog v-model="changePasswordVisible" />
  </el-container>
</template>

<style scoped>
.app-shell { height: 100vh; background: #f5f7fa; }
.top-header {
  height: 66px;
  display: flex;
  align-items: stretch;
  padding: 0;
  background: #409eff;
  color: #fff;
}
.brand-area {
  box-sizing: border-box;
  width: 220px;
  flex: 0 0 220px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  padding: 0 24px;
  background: rgba(28, 91, 158, 0.22);
}
.brand-area span { font-size: 12px; font-weight: 500; opacity: 0.65; letter-spacing: 0.5px; }
.brand-area strong { margin-top: 2px; font-size: 19px; font-weight: 700; letter-spacing: 0.3px; }
.header-main {
  min-width: 0;
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 24px;
  padding: 0 20px;
}
.system-name { font-size: 18px; font-weight: 600; }
.user-area { display: flex; align-items: center; gap: 14px; }
.user-trigger {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 7px 10px;
  border: 0;
  border-radius: 8px;
  color: #fff;
  background: transparent;
  cursor: pointer;
}
.user-trigger:hover { background: rgba(255, 255, 255, 0.12); }
.user-copy { display: flex; flex-direction: column; align-items: flex-end; line-height: 1.35; }
.user-copy strong { font-size: 14px; }
.user-copy span { font-size: 12px; opacity: 0.78; }
.menu-caret { font-size: 16px; opacity: 0.85; }
.content-shell { min-height: 0; }
.side-panel { background: #fff; border-right: 1px solid #ebeef5; }
.side-menu { height: 100%; padding-top: 12px; border-right: 0; }
.main-content { padding: 16px; overflow: auto; }

@media (max-width: 720px) {
  .brand-area { width: 170px; flex-basis: 170px; padding: 0 16px; }
  .system-name { display: none; }
  .user-copy span { display: none; }
}
</style>
