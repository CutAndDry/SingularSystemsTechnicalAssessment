import { createRouter, createWebHistory } from 'vue-router';
import Main from '../Views/Main.vue';
import Products from '../Views/Products.vue';
import Sales from '../Views/Sales.vue';

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
