<template>
  <NavBar />
  <router-view />   
</template>

<script>
  import NavBar from './components/NavBar.vue';

  export default {
    components: {
      NavBar
    }
  };
</script>

<style>

  #app {
    display: flex;
    flex-direction: column;
    min-height: 100vh;
  }

  router-view {
    flex: 1;
  }
</style>
