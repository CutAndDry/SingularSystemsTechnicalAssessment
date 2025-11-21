<template>
  <div class="container">
    <h1>Products</h1>

    <form @submit.prevent="addProduct" class="add-product-form">
      <p class="input-label">Product Name</p>
      <input v-model="newProduct.name" type="text" placeholder="Product Name" required />
      <p class="input-label">Description</p>
      <input v-model="newProduct.description" type="text" placeholder="Description" />
      <p class="input-label">Price</p>
      <input v-model.number="newProduct.price" type="number" placeholder="Price" required />
      <button type="submit">Add Product</button>
    </form>

    <table class="products-table">
      <thead>
        <tr>
          <th>ID</th>
          <th>Name</th>
          <th>Price</th>
        </tr>
      </thead>

      <tbody>
        <tr v-for="product in products" :key="product.id">
          <td>{{ product.id }}</td>
          <td>{{ product.name }}</td>
          <td>{{ product.price.toFixed(2) }}</td>
        </tr>
      </tbody>
    </table>

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
      const res = await getAllProducts(pageNumber.value, pageSize.value);

      products.value = res.data.items;
      totalPages.value = res.data.totalPages;
    } catch (err) {
      console.error('Error fetching products:', err);
    }
  };

  const addProduct = async () => {
    try {
      await createProduct(newProduct.value);

      newProduct.value = { name: '', description: '', price: 0 };

      fetchProducts();
    } catch (err) {
      console.error('Error adding product:', err);
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
    width:100%;
    gap: 0.5rem;
    margin-bottom: 1rem;
  }

  .products-table {
    width: 100%;
    border-collapse: collapse;
    margin-bottom: 1rem;
  }

    .products-table th,
    .products-table td {
      padding: 0.5rem;
      border: 1px solid #ddd;
    }

  .pagination {
    display: flex;
    justify-content: center;
    gap: 1rem;
  }
  .add-product-form input {
    padding: 8px;
    margin-bottom: 5px;
    width: 100%;
    border: 1px solid #ccc;
    border-radius: 4px;
  }
  .input-label {
    margin: 0;
    font-weight: bold;
    padding-top: 10px;
  }
</style>
