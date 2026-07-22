import { createApp } from 'vue'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'

import App from './App.vue'
import router from './router'
import { pinia } from '@/stores'
import { useAuthStore } from '@/stores/auth'
import { AUTH_UNAUTHORIZED_EVENT } from '@/utils/auth'

const app = createApp(App)

app.use(pinia)

const authStore = useAuthStore(pinia)
authStore.initializeAuth()

window.addEventListener(AUTH_UNAUTHORIZED_EVENT, () => {
  const currentRoute = router.currentRoute.value
  authStore.clearAuth()
  if (currentRoute.path !== '/login') {
    router.replace({
      name: 'login',
      query: { redirect: currentRoute.fullPath },
    })
  }
})

app.use(router)
app.use(ElementPlus)

app.mount('#app')
