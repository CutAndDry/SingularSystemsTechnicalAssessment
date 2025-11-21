<template>
  <div class="container mt-4">
    <h2>Sales</h2>

    <!-- Add New Sale -->
    <form @submit.prevent="submitSale" class="mb-3">
      <div class="mb-2">
        <label>Product Id:</label>
        <input v-model.number="newSale.productId" type="number" required class="form-control" />
      </div>
      <div class="mb-2">
        <label>Quantity:</label>
        <input v-model.number="newSale.quantity" type="number" required class="form-control" />
      </div>
      <div class="mb-2">
        <label>Unit Price (optional):</label>
        <input v-model.number="newSale.unitPrice" type="number" step="0.01" class="form-control" />
      </div>
      <button class="btn btn-primary">Add Sale</button>
    </form>

    <!-- Sales Table -->
    <table class="table table-bordered">
      <thead>
        <tr>
          <th>Id</th>
          <th>Product Name</th>
          <th>Quantity</th>
          <th>Unit Price</th>
          <th>Sale Date</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="sale in sales" :key="sale.id">
          <td>{{ sale.id }}</td>
          <td>{{ sale.productName }}</td>
          <td>{{ sale.quantity }}</td>
          <td>{{ sale.unitPrice }}</td>
          <td>{{ new Date(sale.saleDate).toLocaleString() }}</td>
        </tr>
      </tbody>
    </table>

    <!-- Pagination Controls -->
    <nav>
      <ul class="pagination">
        <li class="page-item" :class="{disabled: !pageInfo.hasPreviousPage}">
          <button class="page-link" @click="prevPage">Previous</button>
        </li>
        <li class="page-item disabled">
          <span class="page-link">{{ pageInfo.pageNumber }} / {{ pageInfo.totalPages }}</span>
        </li>
        <li class="page-item" :class="{disabled: !pageInfo.hasNextPage}">
          <button class="page-link" @click="nextPage">Next</button>
        </li>
      </ul>
    </nav>
  </div>
</template>

<script>
  import { getSales, addSale } from "../Services/salesService";
  export default {
    name: "Sales",
    data() {
      return {
        sales: [],
        pageInfo: {},
        pageNumber: 1,
        pageSize: 10,
        newSale: {
          productId: null,
          quantity: 1,
          unitPrice: null,
        }
      };
    },
    methods: {
      async fetchSales() {
        try {
          const res = await getSales(this.pageNumber, this.pageSize);
          this.sales = res.data.items;
          this.pageInfo = res.data;
        } catch (err) {
          console.error("Error fetching sales:", err);
        }
      },
      async submitSale() {
        try {
          await addSale(this.newSale);
          this.newSale = { productId: null, quantity: 1, unitPrice: null };
          this.fetchSales();
        } catch (err) {
          console.error("Error adding sale:", err);
        }
      },
      nextPage() {
        if (this.pageInfo.hasNextPage) {
          this.pageNumber++;
          this.fetchSales();
        }
      },
      prevPage() {
        if (this.pageInfo.hasPreviousPage) {
          this.pageNumber--;
          this.fetchSales();
        }
      }
    },
    mounted() {
      this.fetchSales();
    }
  };
</script>

<style scoped>
  .container {
    max-width: 900px;
  }
</style>
