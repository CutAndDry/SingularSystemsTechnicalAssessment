import { createRouter, createWebHistory } from 'vue-router';
import Main from '../views/Main.vue';
import Products from '../views/Products.vue';
import Sales from '../views/Sales.vue';

const routes = [
  { path: '/', component: Main },         // Startup page
  { path: '/products', component: Products },
  { path: '/sales', component: Sales },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
