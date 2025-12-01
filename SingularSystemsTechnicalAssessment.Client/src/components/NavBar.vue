<!-- src/components/NavBar.vue -->
<template>
  <!-- Desktop sidebar -->
  <aside
    v-if="!isMobile"
    class="sidebar"
    :class="{ collapsed: isCollapsed }"
  >
    <div class="brand">
      <router-link to="/" class="brand-link">MyStore</router-link>
      <button class="toggle" @click="onToggleClick" aria-label="Toggle menu">☰</button>
    </div>

    <nav class="nav">
      <ul>
        <li><router-link to="/" exact>Dashboard</router-link></li>
        <li><router-link to="/products">Products</router-link></li>
        <li><router-link to="/sales">Sales</router-link></li>
      </ul>
    </nav>
  </aside>

  <!-- Mobile topbar -->
  <header v-else class="mobile-topbar">
    <div class="mobile-topbar-inner">
      <router-link to="/" class="brand-link">MyStore</router-link>
      <button class="toggle" @click="isMobileOpen = !isMobileOpen" aria-label="Open menu">☰</button>
    </div>

    <teleport to="body">
      <div v-show="isMobileOpen" class="mobile-menu">
        <nav class="nav">
          <ul>
            <li><router-link to="/">Dashboard</router-link></li>
            <li><router-link to="/products">Products</router-link></li>
            <li><router-link to="/sales">Sales</router-link></li>
          </ul>
        </nav>
      </div>
      <!-- Backdrop to close mobile menu when tapping outside -->
      <div v-if="isMobileOpen" class="sidebar-backdrop" @click="isMobileOpen = false"></div>
    </teleport>
  </header>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount, watch } from 'vue';
import { useRouter } from 'vue-router';

// collapsed (desktop) and mobile open state
const isCollapsed = ref(false);
const isMobileOpen = ref(false);
const isMobile = ref(false);
const router = useRouter();

// Keep track of whether menu should persist across navigation.
const mobileMenuPersistent = true;

function updateIsMobile() {
  isMobile.value = window.innerWidth <= 900;
}

function onToggleClick(e) {
  if (isMobile.value) {
    isMobileOpen.value = !isMobileOpen.value;
  } else {
    isCollapsed.value = !isCollapsed.value;
  }
}

function closeMobile() {
  isMobileOpen.value = false;
}

function closeOnBackdrop(e) {
  // clicking on aside self should not close; backdrop handles it
}

// keep CSS variable --sidebar-width in sync with state so layout shifts
function applySidebarWidth() {
  const root = document.documentElement;
  if (isMobile.value) {
    // on mobile, layout should not reserve sidebar width
    root.style.setProperty('--sidebar-width', '0px');
  } else {
    root.style.setProperty('--sidebar-width', isCollapsed.value ? '114px' : '220px');
  }
}

onMounted(() => {
  updateIsMobile();
  applySidebarWidth();
  window.addEventListener('resize', onResize);
  // restore persisted mobile menu state (helps if NavBar is remounted)
  try {
    const persisted = localStorage.getItem('mobileMenuOpen');
    if (persisted === '1') {
      isMobileOpen.value = true;
      document.body.classList.add('mobile-menu-open');
    }
  } catch (e) {}

  // keep menu visibility consistent across route changes
  if (mobileMenuPersistent) {
    router.afterEach(() => {
      // restore state from storage on route change in case component state reset
      try {
        const persisted = localStorage.getItem('mobileMenuOpen');
        if (persisted === '1') {
          isMobileOpen.value = true;
          document.body.classList.add('mobile-menu-open');
        } else {
          isMobileOpen.value = false;
          document.body.classList.remove('mobile-menu-open');
        }
      } catch (e) {
        // fallback: re-apply in-memory state
        if (isMobileOpen.value) document.body.classList.add('mobile-menu-open');
        else document.body.classList.remove('mobile-menu-open');
      }
    });
  }
});

onBeforeUnmount(() => {
  window.removeEventListener('resize', onResize);
  document.body.classList.remove('mobile-menu-open');
});

function onResize() {
  const prev = isMobile.value;
  updateIsMobile();
  if (prev !== isMobile.value) {
    // closing mobile open when switching modes
    isMobileOpen.value = false;
  }
  applySidebarWidth();
}

// react to state changes
watch([isCollapsed, isMobile], () => applySidebarWidth());
// watch mobile menu open state to set a global body class so teleported menu stays visible
watch(isMobileOpen, (v) => {
  if (v) document.body.classList.add('mobile-menu-open');
  else document.body.classList.remove('mobile-menu-open');
  try {
    localStorage.setItem('mobileMenuOpen', v ? '1' : '0');
  } catch (e) {}
});
</script>



<style scoped>
/* Sidebar left layout */
.sidebar {
  width: 220px;
  min-width: 64px;
  background: linear-gradient(180deg, #2c3e50 0%, #1a252f 100%);
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
  transition: width 0.3s cubic-bezier(0.4, 0, 0.2, 1), transform 0.3s ease;
  z-index: 40;
  box-shadow: 4px 0 15px rgba(0, 0, 0, 0.3);
  overflow-y: auto;
  overflow-x: hidden;
}

.sidebar::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 1px;
  background: linear-gradient(90deg, transparent, rgba(100, 150, 255, 0.3), transparent);
}

.sidebar.collapsed {
  width: 114px;
}

/* Mobile behavior: slide in/out */
.sidebar.mobile {
  transform: translateX(-100%);
  width: 220px;
}
.sidebar.mobile.open {
  transform: translateX(0);
}

/* Backdrop covers the app when mobile menu is open */
.sidebar-backdrop {
  position: fixed;
  inset: 0 0 0 0;
  background: rgba(0,0,0,0.5);
  z-index: 10000;
  animation: fadeIn 0.3s ease-out;
  backdrop-filter: blur(3px);
}

.brand {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  padding: 1rem 0.5rem;
  border-bottom: 1px solid rgba(100, 150, 255, 0.2);
  border-radius: 8px;
  margin-bottom: 0.5rem;
  animation: slideInLeft 0.5s ease-out;
  background: rgba(100, 150, 255, 0.05);
}

.brand-link {
  color: #64d5ff;
  font-weight: 700;
  text-decoration: none;
  font-size: 1.1rem;
  letter-spacing: -0.3px;
  transition: all 0.3s ease;
}

.brand-link:hover {
  color: #a8d5ff;
  text-shadow: 0 0 10px rgba(100, 150, 255, 0.5);
}

.toggle {
  background: rgba(100, 150, 255, 0.1);
  color: #6495ff;
  border: 1px solid rgba(100, 150, 255, 0.2);
  font-size: 18px;
  cursor: pointer;
  border-radius: 6px;
  padding: 0.35rem 0.5rem;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
}

.toggle:hover {
  background: rgba(100, 150, 255, 0.2);
  border-color: rgba(100, 180, 255, 0.4);
  color: #a8d5ff;
  transform: scale(1.1);
}

/* Mobile topbar */
.mobile-topbar {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  height: 56px;
  background: linear-gradient(90deg, #2c3e50 0%, #1a252f 100%);
  display: flex;
  align-items: center;
  z-index: 10002;
  padding: 0 1rem;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.3);
  animation: slideInDown 0.4s ease-out;
}

.mobile-topbar::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 1px;
  background: linear-gradient(90deg, transparent, rgba(100, 150, 255, 0.3), transparent);
}

.mobile-topbar-inner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
}

.mobile-menu {
  position: fixed;
  top: 56px;
  left: 0;
  right: 0;
  background: linear-gradient(180deg, #2c3e50 0%, #1a252f 100%);
  z-index: 10001;
  padding: 1rem;
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.4);
  animation: slideInDown 0.3s ease-out;
}

.mobile-menu .nav ul { 
  flex-direction: column;
  gap: 0.75rem;
}

.mobile-menu .nav a { 
  padding: 0.75rem 0.75rem;
  border-radius: 6px;
}

/* When mobile topbar present, ensure body content is pushed below */
:root {
  --mobile-topbar-height: 56px;
}

/* nav */
.nav ul {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.nav li {
  animation: slideInLeft 0.5s ease-out forwards;
}

.nav li:nth-child(1) { animation-delay: 0.1s; }
.nav li:nth-child(2) { animation-delay: 0.15s; }
.nav li:nth-child(3) { animation-delay: 0.2s; }

.nav a {
  display: block;
  color: #a8d5ff;
  text-decoration: none;
  padding: 0.75rem 0.75rem;
  border-radius: 8px;
  font-weight: 600;
  font-size: 0.95rem;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
  overflow: hidden;
  letter-spacing: 0.3px;
}

.nav a::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: rgba(100, 150, 255, 0.15);
  transition: left 0.3s ease;
  z-index: -1;
  border-radius: 8px;
}

.nav a:hover {
  color: #64d5ff;
  transform: translateX(4px);
  padding-left: 1rem;
}

.nav a:hover::before {
  left: 0;
}

.nav a.router-link-active {
  background: linear-gradient(135deg, rgba(100, 150, 255, 0.25), rgba(100, 120, 200, 0.1));
  color: #64d5ff;
  border-left: 3px solid #6495ff;
  padding-left: 0.5rem;
  box-shadow: inset 0 0 10px rgba(100, 150, 255, 0.1);
}

.nav a.router-link-active::before {
  display: none;
}

/* push content right by sidebar width */
:root {
  --sidebar-width: 220px;
}

@keyframes slideInDown {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes slideInLeft {
  from {
    opacity: 0;
    transform: translateX(-20px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}
</style>
