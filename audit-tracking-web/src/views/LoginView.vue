<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'

import { useAuthStore } from '@/stores/auth'

interface LoginForm {
  userName: string
  password: string
}

const REMEMBERED_USER_NAME_KEY = 'audit_tracking_remembered_username'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const formRef = ref<FormInstance>()
const rememberUserName = ref(false)
const form = reactive<LoginForm>({
  userName: '',
  password: '',
})

const rules: FormRules<LoginForm> = {
  userName: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 6, max: 100, message: '密码长度应为6到100个字符', trigger: 'blur' },
  ],
}

function getRedirectTarget() {
  const redirect = route.query.redirect
  return typeof redirect === 'string' && redirect.startsWith('/') && !redirect.startsWith('//')
    ? redirect
    : '/'
}

function getLoginErrorMessage(error: unknown) {
  if (!(error instanceof Error) || !error.message) return '登录失败，请稍后重试'
  if (error.message === 'Network Error' || error.message.includes('ERR_NETWORK')) {
    return '无法连接认证服务，请确认后端服务已启动'
  }
  if (error.message.toLowerCase().includes('timeout')) {
    return '认证服务响应超时，请稍后重试'
  }
  return error.message
}

async function submitLogin() {
  if (!formRef.value || authStore.loginLoading) return

  try {
    await formRef.value.validate()
  } catch {
    return
  }

  try {
    await authStore.login({
      userName: form.userName.trim(),
      password: form.password,
    })

    if (rememberUserName.value) {
      localStorage.setItem(REMEMBERED_USER_NAME_KEY, form.userName.trim())
    } else {
      localStorage.removeItem(REMEMBERED_USER_NAME_KEY)
    }

    await router.replace(getRedirectTarget())
  } catch (error) {
    ElMessage.error(getLoginErrorMessage(error))
  }
}

onMounted(() => {
  if (authStore.isAuthenticated) {
    router.replace('/')
    return
  }

  const rememberedUserName = localStorage.getItem(REMEMBERED_USER_NAME_KEY)
  if (rememberedUserName) {
    form.userName = rememberedUserName
    rememberUserName.value = true
  }
})
</script>

<template>
  <main class="login-page">
    <section class="login-shell">
      <div class="brand-block">
        <span>Design By</span>
        <strong>Richard_Xia</strong>
      </div>

      <el-card shadow="never" class="login-card">
        <div class="login-heading">
          <h1>审计跟踪管理系统</h1>
          <p>审计计划、问题整改与验证全过程管理</p>
        </div>

        <el-form
          ref="formRef"
          :model="form"
          :rules="rules"
          label-position="top"
          size="large"
          @keyup.enter="submitLogin"
        >
          <el-form-item label="用户名" prop="userName">
            <el-input
              v-model="form.userName"
              autocomplete="username"
              maxlength="50"
              placeholder="请输入用户名"
            />
          </el-form-item>

          <el-form-item label="密码" prop="password">
            <el-input
              v-model="form.password"
              type="password"
              autocomplete="current-password"
              maxlength="100"
              placeholder="请输入密码"
              show-password
            />
          </el-form-item>

          <div class="login-options">
            <el-checkbox v-model="rememberUserName">记住用户名</el-checkbox>
          </div>

          <el-button
            type="primary"
            class="login-button"
            :loading="authStore.loginLoading"
            @click="submitLogin"
          >
            登录系统
          </el-button>
        </el-form>
      </el-card>
    </section>
  </main>
</template>

<style scoped>
.login-page {
  min-height: 100vh;
  display: grid;
  place-items: center;
  padding: 32px 20px;
  background:
    radial-gradient(circle at 18% 16%, rgba(64, 158, 255, 0.13), transparent 34%),
    linear-gradient(145deg, #f4f8fd 0%, #eaf1f9 100%);
}

.login-shell {
  width: min(100%, 440px);
}

.brand-block {
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-bottom: 18px;
  color: #35506d;
}

.brand-block span {
  font-size: 12px;
  font-weight: 500;
  opacity: 0.65;
  letter-spacing: 0.5px;
}

.brand-block strong {
  margin-top: 2px;
  font-size: 19px;
  font-weight: 700;
  letter-spacing: 0.3px;
}

.login-card {
  border: 0;
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.97);
  box-shadow: 0 18px 45px rgba(44, 75, 108, 0.12);
}

.login-card :deep(.el-card__body) {
  padding: 38px 40px 36px;
}

.login-heading {
  margin-bottom: 30px;
  text-align: center;
}

.login-heading h1 {
  margin: 0;
  color: #263b52;
  font-size: 25px;
  font-weight: 700;
}

.login-heading p {
  margin: 10px 0 0;
  color: #8492a3;
  font-size: 14px;
  line-height: 1.6;
}

.login-options {
  display: flex;
  justify-content: flex-start;
  margin: -4px 0 22px;
}

.login-button {
  width: 100%;
  font-weight: 600;
  letter-spacing: 1px;
}

@media (max-width: 520px) {
  .login-page { padding: 24px 14px; }
  .login-card :deep(.el-card__body) { padding: 30px 24px 28px; }
  .login-heading h1 { font-size: 22px; }
}
</style>
