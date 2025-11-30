<template>
  <div class="sales-page">
    <header class="page-header">
      <h1>Sales</h1>
    </header>

    <!-- Filter Panel -->
    <div class="filter-panel">
      <div class="filter-row">
        <div class="filter-group">
          <label>Product ID</label>
          <input v-model.number="filters.productId" type="number" placeholder="Filter by Product ID" />
        </div>
        <div class="filter-group">
          <label>Start Date</label>
          <input v-model="filters.startDate" type="date" />
        </div>
        <div class="filter-group">
          <label>End Date</label>
          <input v-model="filters.endDate" type="date" />
        </div>
        <div class="filter-actions">
          <button class="btn btn-secondary" @click="applyFilters">Apply Filters</button>
          <button class="btn btn-outline" @click="resetFilters">Reset</button>
          <button class="btn btn-primary" @click="showAddModal = true">+ Add Sale</button>
        </div>
      </div>
    </div>

    <!-- Add Sale Modal -->
    <div v-if="showAddModal" class="modal-overlay" @click.self="closeAddModal">
      <div class="modal-card">
        <div class="modal-header">
          <h3>Add New Sale</h3>
          <button class="modal-close" @click="closeAddModal">&times;</button>
        </div>
        <form @submit.prevent="submitAddSale" class="modal-form">
          <div class="form-group">
            <label>Product ID *</label>
            <input v-model.number="newSale.productId" type="number" required placeholder="Enter Product ID" />
          </div>
          <div class="form-group">
            <label>Quantity *</label>
            <input v-model.number="newSale.quantity" type="number" min="1" required placeholder="Enter quantity" />
          </div>
          <div class="form-group">
            <label>Unit Price (optional)</label>
            <input v-model.number="newSale.unitPrice" type="number" step="0.01" placeholder="Enter unit price" />
          </div>
          <div class="modal-actions">
            <button type="submit" class="btn btn-success">Add Sale</button>
            <button type="button" class="btn btn-secondary" @click="closeAddModal">Cancel</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Sales Table -->
    <div class="table-container">
      <div class="table-wrapper">
        <table class="sales-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Product Name</th>
              <th>Quantity</th>
              <th>Unit Price</th>
              <th>Total</th>
              <th>Sale Date</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading" v-for="i in 8" :key="'skeleton-'+i" class="skeleton-row">
              <td colspan="6">&nbsp;</td>
            </tr>

            <tr v-for="sale in sales" :key="sale.id">
              <td>{{ sale.id }}</td>
              <td>{{ sale.productName }}</td>
              <td>{{ sale.quantity }}</td>
              <td>${{ (sale.unitPrice || 0).toFixed(2) }}</td>
              <td>${{ ((sale.quantity || 0) * (sale.unitPrice || 0)).toFixed(2) }}</td>
              <td>{{ new Date(sale.saleDate).toLocaleString() }}</td>
            </tr>

            <tr v-if="!loading && sales.length === 0">
              <td colspan="6" class="text-center">No sales found.</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Pagination -->
    <div class="pagination-container">
      <button class="pagination-btn" :disabled="!pageInfo.hasPreviousPage" @click="prevPage">‹ Previous</button>
      <span class="pagination-info">Page {{ pageInfo.pageNumber }} of {{ pageInfo.totalPages }}</span>
      <button class="pagination-btn" :disabled="!pageInfo.hasNextPage" @click="nextPage">Next ›</button>
    </div>
  </div>
</template>

<script>
import { getSales, getAllSales, addSale } from "../Services/salesService";

export default {
  name: "Sales",
  data() {
    return {
      sales: [],
      pageInfo: {
        pageNumber: 1,
        pageSize: 10,
        totalPages: 1,
        hasNextPage: false,
        hasPreviousPage: false
      },
      pageNumber: 1,
      pageSize: 10,
      showAddModal: false,
      loading: false,
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
      this.loading = true;
      try {
        const res = await getSales(
          this.pageNumber,
          this.pageSize,
          this.filters.productId || null,
          this.filters.startDate || null,
          this.filters.endDate || null
        );

        const data = res.data || {};
        const items = data.items || data.Items || [];
        const pageNumber = data.pageNumber ?? data.PageNumber ?? this.pageNumber;
        const pageSize = data.pageSize ?? data.PageSize ?? this.pageSize;
        const totalPages = data.totalPages ?? data.TotalPages ?? Math.max(1, Math.ceil((data.totalCount ?? items.length) / (pageSize || 10)));

        if (pageNumber === 1 && items.length === 0) {
          try {
            const fullRes = await getAllSales();
            const fullItems = (fullRes.data || []);
            this.sales = fullItems;
            this.pageInfo = {
              pageNumber: 1,
              pageSize: fullItems.length || pageSize,
              totalPages: 1,
              hasNextPage: false,
              hasPreviousPage: false
            };
            return;
          } catch (fallbackErr) {
            console.warn("Fallback full sales fetch failed", fallbackErr);
          }
        }

        this.sales = items;
        this.pageInfo = {
          pageNumber: pageNumber || 1,
          pageSize: pageSize || 10,
          totalPages: totalPages || 1,
          hasNextPage: (pageNumber ?? 1) < (totalPages || 1),
          hasPreviousPage: (pageNumber ?? 1) > 1
        };
      } catch (err) {
        console.error("Error fetching sales:", err);
      } finally {
        this.loading = false;
      }
    },

    applyFilters() {
      this.pageNumber = 1;
      this.fetchSales();
    },

    resetFilters() {
      this.filters = { productId: null, startDate: null, endDate: null };
      this.pageNumber = 1;
      this.fetchSales();
    },

    async submitAddSale() {
      try {
        await addSale(this.newSale);
        this.newSale = { productId: null, quantity: 1, unitPrice: null };
        this.showAddModal = false;
        this.fetchSales();
      } catch (err) {
        console.error("Error adding sale:", err);
      }
    },

    closeAddModal() {
      this.showAddModal = false;
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
.sales-page {
  margin-left: var(--sidebar-width, 220px);
  padding: 1.5rem;
  min-height: 100vh;
  background: var(--color-background, #f5f5f5);
  transition: margin-left 0.2s ease;
}

.page-header {
  margin-bottom: 1.5rem;
}

.page-header h1 {
  font-size: 2rem;
  margin: 0;
  color: var(--color-heading, #2c3e50);
}

/* Filter Panel */
.filter-panel {
  background: #e8eef2;
  border-radius: 8px;
  padding: 1rem;
  margin-bottom: 1.5rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.02);
}

.filter-row {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  align-items: flex-end;
}

.filter-group {
  flex: 1;
  min-width: 160px;
}

.filter-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 600;
  color: #2c3e50;
  font-size: 14px;
}

.filter-group input {
  width: 100%;
  padding: 0.6rem;
  border: 1px solid #bdc3c7;
  border-radius: 6px;
  font-size: 14px;
}

.filter-group input:focus {
  outline: none;
  border-color: #3498db;
  box-shadow: 0 0 0 3px rgba(52, 152, 219, 0.1);
}

.filter-actions {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.btn {
  padding: 0.6rem 1.2rem;
  border: none;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.btn-primary {
  background: #3498db;
  color: white;
}

.btn-primary:hover {
  background: #2980b9;
}

.btn-secondary {
  background: #95a5a6;
  color: white;
}

.btn-secondary:hover {
  background: #7f8c8d;
}

.btn-outline {
  background: transparent;
  color: #2c3e50;
  border: 1px solid #bdc3c7;
}

.btn-outline:hover {
  background: #f8f9fa;
}

.btn-success {
  background: #27ae60;
  color: white;
}

.btn-success:hover {
  background: #229954;
}

/* Modal */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-card {
  background: white;
  border-radius: 8px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
  max-width: 500px;
  width: 95%;
  animation: slideIn 0.2s ease;
}

@keyframes slideIn {
  from { transform: translateY(-20px); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid #e6e6e6;
}

.modal-header h3 {
  margin: 0;
  font-size: 1.3rem;
  color: #2c3e50;
}

.modal-close {
  background: transparent;
  border: none;
  font-size: 28px;
  cursor: pointer;
  color: #7f8c8d;
}

.modal-close:hover {
  color: #2c3e50;
}

.modal-form {
  padding: 1.5rem;
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 600;
  color: #2c3e50;
}

.form-group input {
  width: 100%;
  padding: 0.6rem;
  border: 1px solid #bdc3c7;
  border-radius: 6px;
  font-size: 14px;
}

.form-group input:focus {
  outline: none;
  border-color: #3498db;
  box-shadow: 0 0 0 3px rgba(52, 152, 219, 0.1);
}

.modal-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  margin-top: 1.5rem;
  padding-top: 1rem;
  border-top: 1px solid #e6e6e6;
}

/* Table */
.table-container {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  overflow: hidden;
  margin-bottom: 1.5rem;
}

.table-wrapper {
  overflow-x: auto;
}

.sales-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
}

.sales-table thead {
  background: #f8f9fa;
  border-bottom: 2px solid #e6e6e6;
}

.sales-table th {
  padding: 1rem;
  text-align: left;
  font-weight: 600;
  color: #2c3e50;
}

.sales-table td {
  padding: 0.8rem 1rem;
  border-bottom: 1px solid #e6e6e6;
}

.sales-table tbody tr:hover {
  background: #f8f9fa;
}

.skeleton-row td {
  background: linear-gradient(90deg, rgba(0,0,0,0.02) 25%, rgba(0,0,0,0.04) 37%, rgba(0,0,0,0.02) 63%);
  height: 40px;
  animation: shimmer 1.5s infinite;
}

@keyframes shimmer {
  0% { opacity: 1; }
  50% { opacity: 0.7; }
  100% { opacity: 1; }
}

.text-center {
  text-align: center;
  color: #7f8c8d;
}

/* Pagination */
.pagination-container {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 1rem;
}

.pagination-btn {
  padding: 0.6rem 1rem;
  background: #3498db;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  transition: background 0.2s ease;
}

.pagination-btn:hover:not(:disabled) {
  background: #2980b9;
}

.pagination-btn:disabled {
  background: #bdc3c7;
  cursor: not-allowed;
}

.pagination-info {
  font-weight: 600;
  color: #2c3e50;
}

/* Responsive */
@media (max-width: 900px) {
  .sales-page {
    margin-left: 64px;
    padding: 1rem;
  }

  .filter-row {
    flex-direction: column;
  }

  .filter-group {
    min-width: unset;
  }

  .filter-actions {
    width: 100%;
    justify-content: flex-start;
  }

  .sales-table {
    font-size: 12px;
  }

  .sales-table th,
  .sales-table td {
    padding: 0.6rem 0.4rem;
  }

  .page-header h1 {
    font-size: 1.5rem;
  }
}
</style>
