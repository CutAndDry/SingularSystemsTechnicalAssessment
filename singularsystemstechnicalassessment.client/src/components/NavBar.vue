<!-- src/components/NavBar.vue -->
<template>
  <nav class="navbar">
    <div class="logo">
      <router-link to="/">MyStore</router-link>
    </div>
    <ul class="nav-links">
      <li><router-link to="/">Home</router-link></li>
      <li><router-link to="/products">Products</router-link></li>
      <li><router-link to="/sales">Sales</router-link></li>
    </ul>
  </nav>
</template>

<script setup>
</script>

<style scoped>
  .navbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.5rem 1rem;
    background-color: #2c3e50;
    color: white;
  }

    .navbar a {
      color: white;
      text-decoration: none;
      font-weight: bold;
    }

  .nav-links {
    list-style: none;
    display: flex;
    gap: 1rem;
  }

    .nav-links a.router-link-active {
      border-bottom: 2px solid white;
    }
</style>
