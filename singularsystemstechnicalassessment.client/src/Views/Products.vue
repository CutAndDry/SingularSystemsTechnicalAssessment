<template>
  <div class="container">
    <h1>Products</h1>

    <!-- Add Product Form -->
    <form @submit.prevent="addProduct" class="add-product-form">
      <input v-model="newProduct.name" type="text" placeholder="Product Name" required />
      <input v-model="newProduct.description" type="text" placeholder="Description" />
      <input v-model.number="newProduct.price" type="number" placeholder="Price" required />
      <button type="submit">Add Product</button>
    </form>

    <!-- Products Table -->
    <table class="products-table">
      <thead>
        <tr>
          <th>ID</th>
          <th>Name</th>
          <th>Description</th>
          <th>Price</th>
          <th>Total Sales</th>
          <th>Total Revenue</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="product in products" :key="product.id">
          <td>{{ product.id }}</td>
          <td>{{ product.name }}</td>
          <td>{{ product.description || '-' }}</td>
          <td>{{ product.price.toFixed(2) }}</td>
          <td>{{ product.totalSales }}</td>
          <td>{{ product.totalRevenue.toFixed(2) }}</td>
        </tr>
      </tbody>
    </table>

    <!-- Pagination Controls -->
    <div class="pagination">
      <button @click="prevPage" :disabled="pageNumber <= 1">Previous</button>
      <span>Page {{ pageNumber }} of {{ totalPages }}</span>
      <button @click="nextPage" :disabled="pageNumber >= totalPages">Next</button>
    </div>
  </div>
</template>

<script setup>
  import { ref, onMounted } from 'vue';
  import { getAllProducts, createProduct } from '../services/productService.js';

  const products = ref([]);
  const pageNumber = ref(1);
  const pageSize = ref(10);
  const totalPages = ref(1);

  const newProduct = ref({
    name: '',
    description: '',
    price: 0
  });

  const fetchProducts = async () => {
    try {
      const response = await getAllProducts(pageNumber.value, pageSize.value);
      products.value = response.data.items;
      totalPages.value = response.data.totalPages;
    } catch (error) {
      console.error('Error fetching products:', error);
    }
  };

  const addProduct = async () => {
    try {
      await createProduct(newProduct.value);
      // Clear form
      newProduct.value = { name: '', description: '', price: 0 };
      // Refresh table
      fetchProducts();
    } catch (error) {
      console.error('Error adding product:', error);
    }
  };

  const nextPage = () => {
    if (pageNumber.value < totalPages.value) {
      pageNumber.value++;
      fetchProducts();
    }
  };

  const prevPage = () => {
    if (pageNumber.value > 1) {
      pageNumber.value--;
      fetchProducts();
    }
  };

  onMounted(fetchProducts);
</script>

<style scoped>
  .container {
    max-width: 900px;
    margin: 2rem auto;
    font-family: Arial, sans-serif;
  }

  .add-product-form {
    display: flex;
    gap: 0.5rem;
    margin-bottom: 1rem;
  }
</style>
