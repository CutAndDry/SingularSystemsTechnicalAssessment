<template>
  <div class="container mt-4">
    <h2>Sales</h2>

    <!-- FILTERS -->
    <div class="filters mb-4 p-3 border rounded">
      <h5>Filters</h5>

      <div class="row">
        <div class="col-md-3 mb-2">
          <label>Product ID</label>
          <input v-model.number="filters.productId" type="number" class="form-control" placeholder="Product ID" />
        </div>

        <div class="col-md-3 mb-2">
          <label>Start Date</label>
          <input v-model="filters.startDate" type="date" class="form-control" />
        </div>

        <div class="col-md-3 mb-2">
          <label>End Date</label>
          <input v-model="filters.endDate" type="date" class="form-control" />
        </div>

        <div class="col-md-3 d-flex align-items-end mb-2">
          <button class="btn btn-primary me-2" @click="applyFilters">Apply</button>
          <button class="btn btn-secondary" @click="resetFilters">Reset</button>
        </div>
      </div>
    </div>

    <button class="btn btn-primary mb-3" @click="showModal = true">
      Add New Sale
    </button>


    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-content">
        <h4>Add Sale</h4>

        <form @submit.prevent="submitSale">
          <div class="mb-2">
            <label>Product ID:</label>
            <input v-model.number="newSale.productId" type="number" required class="form-control" />
          </div>

          <div class="mb-2">
            <label>Quantity:</label>
            <input v-model.number="newSale.quantity" type="number" min="1" required class="form-control" />
          </div>

          <div class="mb-2">
            <label>Unit Price (optional):</label>
            <input v-model.number="newSale.unitPrice" type="number" step="0.01" class="form-control" />
          </div>

          <div class="modal-actions">
            <button type="submit" class="btn btn-success">Add</button>
            <button type="button" class="btn btn-secondary" @click="closeModal">Cancel</button>
          </div>
        </form>
      </div>
    </div>


    <table class="sale-table table-bordered">
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
          <td>{{ sale.unitPrice.toFixed(2) }}</td>
          <td>{{ new Date(sale.saleDate).toLocaleString() }}</td>
        </tr>
      </tbody>
    </table>

    <!-- PAGINATION -->
    <nav class="pagination-container">
      <button class="pagination-btn"
              :disabled="!pageInfo.hasPreviousPage"
              @click="prevPage">
        ‹ Previous
      </button>

      <span class="pagination-info">
        Page {{ pageInfo.pageNumber }} of {{ pageInfo.totalPages }}
      </span>

      <button class="pagination-btn"
              :disabled="!pageInfo.hasNextPage"
              @click="nextPage">
        Next ›
      </button>
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

        showModal: false,

        filters: {
          productId: null,
          startDate: null,
          endDate: null
        },

        newSale: {
          productId: null,
          quantity: 1,
          unitPrice: null
        }
      };
    },

    methods: {
      async fetchSales() {
        try {
          const res = await getSales(
            this.pageNumber,
            this.pageSize,
            this.filters.productId,
            this.filters.startDate,
            this.filters.endDate
          );

          this.sales = res.data.items;
          this.pageInfo = res.data;
        } catch (err) {
          console.error("Error fetching sales:", err);
        }
      },

      applyFilters() {
        this.pageNumber = 1;
        this.fetchSales();
      },

      resetFilters() {
        this.filters = {
          productId: null,
          startDate: null,
          endDate: null
        };

        this.pageNumber = 1;
        this.fetchSales();
      },

      async submitSale() {
        try {
          await addSale(this.newSale);
          this.newSale = { productId: null, quantity: 1, unitPrice: null };
          this.showModal = false;
          this.fetchSales();
        } catch (err) {
          console.error("Error adding sale:", err);
        }
      },

      closeModal() {
        this.showModal = false;
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

  .filters {
    background: #f8f9fa;
  }

  .sale-table {
    width: 100%;
    border-collapse: collapse;
    margin-bottom: 1rem;
  }

    .sale-table th,
    .sale-table td {
      padding: 0.5rem;
      border: 1px solid #ddd;
    }

  .modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0,0,0,0.6);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
  }

  .modal-content {
    background: white;
    padding: 20px;
    width: 400px;
    border-radius: 6px;
    box-shadow: 0 2px 10px rgba(0,0,0,0.3);
  }

  .modal-actions {
    margin-top: 15px;
    display: flex;
    justify-content: space-between;
  }

  .pagination-container {
    display: flex;
    justify-content: center;
    align-items: center;
    margin-top: 20px;
    gap: 12px;
  }

  .pagination-btn {
    background-color: #3498db;
    color: white;
    border: none;
    padding: 8px 16px;
    border-radius: 6px;
    font-weight: 600;
    cursor: pointer;
    transition: background 0.2s;
  }

    .pagination-btn:disabled {
      background-color: #bdc3c7;
      cursor: not-allowed;
    }

    .pagination-btn:not(:disabled):hover {
      background-color: #2980b9;
    }

  .pagination-info {
    font-weight: 600;
    font-size: 14px;
  }
</style>
