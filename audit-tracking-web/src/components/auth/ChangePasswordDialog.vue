<script setup lang="ts">
import { computed, nextTick, reactive, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'

import { changePassword } from '@/api/auth'
import { useAuthStore } from '@/stores/auth'

interface ChangePasswordForm {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}

const props = defineProps<{ modelValue: boolean }>()
const emit = defineEmits<{ 'update:modelValue': [value: boolean] }>()

const router = useRouter()
const authStore = useAuthStore()
const formRef = ref<FormInstance>()
const submitLoading = ref(false)
const form = reactive<ChangePasswordForm>({
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
})

const dialogVisible = computed({
  get: () => props.modelValue,
  set: (value: boolean) => emit('update:modelValue', value),
})

const validateNewPassword = (
  _rule: unknown,
  value: string,
  callback: (error?: Error) => void,
) => {
  if (value && value === form.currentPassword) {
    callback(new Error('新密码不能与当前密码相同'))
    return
  }
  callback()
}

const validateConfirmPassword = (
  _rule: unknown,
  value: string,
  callback: (error?: Error) => void,
) => {
  if (value && value !== form.newPassword) {
    callback(new Error('两次输入的新密码不一致'))
    return
  }
  callback()
}

const rules: FormRules<ChangePasswordForm> = {
  currentPassword: [
    { required: true, message: '请输入当前密码', trigger: 'blur' },
    { min: 6, max: 100, message: '当前密码长度应为6到100个字符', trigger: 'blur' },
  ],
  newPassword: [
    { required: true, message: '请输入新密码', trigger: 'blur' },
    { min: 6, max: 100, message: '新密码长度应为6到100个字符', trigger: 'blur' },
    { validator: validateNewPassword, trigger: 'blur' },
  ],
  confirmPassword: [
    { required: true, message: '请再次输入新密码', trigger: 'blur' },
    { validator: validateConfirmPassword, trigger: 'blur' },
  ],
}

function resetForm() {
  Object.assign(form, {
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  })
  formRef.value?.clearValidate()
}

function getErrorMessage(error: unknown) {
  if (!(error instanceof Error) || !error.message) return '密码修改失败，请稍后重试'
  if (error.message === 'Network Error' || error.message.includes('ERR_NETWORK')) {
    return '无法连接认证服务，请稍后重试'
  }
  return error.message
}

async function submitChangePassword() {
  if (!formRef.value || submitLoading.value) return

  try {
    await formRef.value.validate()
  } catch {
    return
  }

  submitLoading.value = true
  try {
    await changePassword({
      currentPassword: form.currentPassword,
      newPassword: form.newPassword,
    })
    ElMessage.success('密码修改成功，请重新登录')
    dialogVisible.value = false
    authStore.logout()
    await router.replace('/login')
  } catch (error) {
    ElMessage.error(getErrorMessage(error))
  } finally {
    submitLoading.value = false
  }
}

watch(
  () => props.modelValue,
  async (visible) => {
    if (!visible) return
    resetForm()
    await nextTick()
    formRef.value?.clearValidate()
  },
)
</script>

<template>
  <el-dialog
    v-model="dialogVisible"
    title="修改密码"
    width="520px"
    destroy-on-close
    @closed="resetForm"
  >
    <el-form
      ref="formRef"
      :model="form"
      :rules="rules"
      label-width="100px"
      @keyup.enter="submitChangePassword"
    >
      <el-form-item label="当前密码" prop="currentPassword">
        <el-input
          v-model="form.currentPassword"
          type="password"
          maxlength="100"
          autocomplete="current-password"
          show-password
        />
      </el-form-item>
      <el-form-item label="新密码" prop="newPassword">
        <el-input
          v-model="form.newPassword"
          type="password"
          maxlength="100"
          autocomplete="new-password"
          show-password
        />
      </el-form-item>
      <el-form-item label="确认新密码" prop="confirmPassword">
        <el-input
          v-model="form.confirmPassword"
          type="password"
          maxlength="100"
          autocomplete="new-password"
          show-password
        />
      </el-form-item>
    </el-form>

    <template #footer>
      <el-button :disabled="submitLoading" @click="dialogVisible = false">取消</el-button>
      <el-button type="primary" :loading="submitLoading" @click="submitChangePassword">
        确认修改
      </el-button>
    </template>
  </el-dialog>
</template>

<style scoped>
@media (max-width: 600px) {
  :deep(.el-dialog) { width: calc(100% - 28px) !important; }
}
</style>
