<template>
  <el-container style="height:100vh">
    <el-header style="background:#409EFF;color:white;display:flex;align-items:center">
      <div style="font-weight:600;font-size:18px;margin-left:12px">审计跟踪管理系统</div>
    </el-header>

    <el-container>
      <el-aside width="220px" style="background:#ffffff;padding-top:12px">
        <el-menu default-active="/" router>
          <el-menu-item index="/">
            <span>仪表盘</span>
          </el-menu-item>
          <el-menu-item index="/audit-plans">审计计划</el-menu-item>
          <el-menu-item index="/audit-issues">审计问题</el-menu-item>
          <el-menu-item index="/corrective-actions">整改措施</el-menu-item>
          <el-menu-item index="/rectification-verifications">整改验证</el-menu-item>
        </el-menu>
      </el-aside>

      <el-main style="padding:16px">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup lang="ts">
// Layout relies on Element Plus components via global registration
</script>

<style scoped>
.el-menu-demo {
  border-right: none;
}
</style>
