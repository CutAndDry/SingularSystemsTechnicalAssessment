<!-- src/components/NavBar.vue -->
<template>
  <aside class="sidebar" :class="{ collapsed: isCollapsed }">
    <div class="brand">
      <router-link to="/" class="brand-link">MyStore</router-link>
      <button class="toggle" @click="isCollapsed = !isCollapsed" aria-label="Toggle menu">☰</button>
    </div>

    <nav class="nav">
      <ul>
        <li><router-link to="/" exact>Dashboard</router-link></li>
        <li><router-link to="/products">Products</router-link></li>
        <li><router-link to="/sales">Sales</router-link></li>
      </ul>
    </nav>
  </aside>
</template>

<script setup>
import { ref } from 'vue';
const isCollapsed = ref(false);
</script>

<style scoped>
/* Sidebar left layout */
.sidebar {
  width: 220px;
  min-width: 64px;
  background: var(--vt-c-indigo);
  color: #fff;
  height: 100vh;
  position: fixed;
  left: 0;
  top: 0;
  bottom: 0;
  padding: 1rem 0.75rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
  transition: width 0.25s ease;
  z-index: 30;
}

.sidebar.collapsed {
  width: 114px;
}

.brand {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  padding-bottom: 0.5rem;
  border-bottom: 1px solid rgba(255,255,255,0.06);
}

.brand-link {
  color: white;
  font-weight: 700;
  text-decoration: none;
}

.toggle {
  background: transparent;
  color: white;
  border: none;
  font-size: 18px;
  cursor: pointer;
}

/* nav */
.nav ul {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.nav a {
  display: block;
  color: #fff;
  text-decoration: none;
  padding: 0.55rem 0.5rem;
  border-radius: 6px;
  font-weight: 600;
}

.nav a.router-link-active {
  background: rgba(255,255,255,0.12);
}

/* push content right by sidebar width */
:root {
  --sidebar-width: 220px;
}
</style>
