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
  transition: width 0.25s ease, transform 0.25s ease;
  z-index: 40;
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
  background: rgba(0,0,0,0.4);
  z-index: 10000;
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

/* Mobile topbar */
.mobile-topbar {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  height: 56px;
  background: var(--vt-c-indigo);
  display: flex;
  align-items: center;
  z-index: 10002;
  padding: 0 1rem;
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
  background: var(--vt-c-indigo);
  z-index: 10001;
  padding: 0.5rem 1rem 1rem 1rem;
  box-shadow: 0 8px 20px rgba(0,0,0,0.4);
}
.mobile-menu .nav ul { flex-direction: column; gap: 0.5rem }
.mobile-menu .nav a { padding: 0.75rem 0.5rem }

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
