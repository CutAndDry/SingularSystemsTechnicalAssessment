<template>
  <div id="app">
    <NavBar />
    <router-view />
  </div>
</template>

<script setup>
  import NavBar from '../components/NavBar.vue';
</script>

<style>
  /* Optional global styles */
  body {
    margin: 0;
    font-family: Avenir, Helvetica, Arial, sans-serif;
   
  }
  #app {
    width: 100%;
  }


</style>
