<template>
  <div class="dashboard-page">
    <header class="dashboard-header">
      <h1>Dashboard</h1>
    </header>

    <section class="cards">
      <!-- Bar Chart: Sales by Product -->
      <div class="card">
        <h3>Sales Quantity by Product</h3>
        <Bar v-if="barChartData" :data="barChartData" :options="chartOptions" />
        <div v-else class="loading">Loading...</div>
      </div>

      <!-- Line Chart: Sales Over Time -->
      <div class="card">
        <h3>Sales Trend (Last 7 Days)</h3>
        <Line v-if="lineChartData" :data="lineChartData" :options="chartOptions" />
        <div v-else class="loading">Loading...</div>
      </div>

      <!-- Pie Chart: Revenue Distribution -->
      <div class="card">
        <h3>Revenue Share by Product</h3>
        <Pie v-if="pieChartData" :data="pieChartData" :options="pieOptions" />
        <div v-else class="loading">Loading...</div>
      </div>

      <!-- Doughnut Chart: Product Sales Ratio -->
      <div class="card">
        <h3>Sales Volume Distribution</h3>
        <Doughnut v-if="doughnutChartData" :data="doughnutChartData" :options="doughnutOptions" />
        <div v-else class="loading">Loading...</div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { Bar, Line, Pie, Doughnut } from 'vue-chartjs';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler
} from 'chart.js';
import { getAllProducts } from '../Services/productService';
import { getSales } from '../Services/salesService';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler
);

const barChartData = ref(null);
const lineChartData = ref(null);
const pieChartData = ref(null);
const doughnutChartData = ref(null);

const chartOptions = {
  responsive: true,
  maintainAspectRatio: true,
  plugins: {
    legend: {
      position: 'top'
    }
  },
  scales: {
    y: {
      beginAtZero: true
    }
  }
};

const pieOptions = {
  responsive: true,
  maintainAspectRatio: true,
  plugins: {
    legend: {
      position: 'right'
    }
  }
};

const doughnutOptions = {
  responsive: true,
  maintainAspectRatio: true,
  plugins: {
    legend: {
      position: 'bottom'
    }
  }
};

const colors = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899', '#14b8a6'];

const fetchAndBuildCharts = async () => {
  try {
    const productsRes = await getAllProducts(1, 100);
    const salesRes = await getSales(1, 100, null, null, null);

    const products = productsRes.data.items || [];
    const sales = salesRes.data.items || [];

    // --- BAR CHART ---
    const productNames = products.map(p => p.name);
    const salesQuantities = products.map(prod => {
      return sales.filter(s => s.productName === prod.name).reduce((sum, s) => sum + (s.quantity || 0), 0);
    });

    barChartData.value = {
      labels: productNames,
      datasets: [
        {
          label: 'Quantity Sold',
          data: salesQuantities,
          backgroundColor: colors.slice(0, productNames.length),
          borderColor: colors.slice(0, productNames.length),
          borderWidth: 1
        }
      ]
    };

    // --- LINE CHART ---
    const days = 7;
    const now = new Date();
    const dateLabels = Array.from({ length: days }).map((_, i) => {
      const d = new Date(now);
      d.setDate(now.getDate() - (days - 1 - i));
      return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    });

    const dailySalesData = Array.from({ length: days }).map((_, i) => {
      const d = new Date(now);
      d.setDate(now.getDate() - (days - 1 - i));
      const dateKey = d.toISOString().slice(0, 10);
      return sales.filter(s => new Date(s.saleDate).toISOString().slice(0, 10) === dateKey).reduce((sum, s) => sum + (s.quantity || 0), 0);
    });

    lineChartData.value = {
      labels: dateLabels,
      datasets: [
        {
          label: 'Daily Sales Quantity',
          data: dailySalesData,
          borderColor: '#2c3e50',
          backgroundColor: 'rgba(44, 62, 80, 0.1)',
          borderWidth: 2,
          tension: 0.4
        }
      ]
    };

    // --- PIE CHART ---
    const revenueByProduct = products.map(prod => {
      const productSales = sales.filter(s => s.productName === prod.name);
      return productSales.reduce((sum, s) => sum + ((s.quantity || 0) * (s.unitPrice || 0)), 0);
    });

    const nonZeroProducts = products.filter((_, i) => revenueByProduct[i] > 0);
    const nonZeroRevenue = revenueByProduct.filter(r => r > 0);

    pieChartData.value = {
      labels: nonZeroProducts.map(p => p.name),
      datasets: [
        {
          data: nonZeroRevenue,
          backgroundColor: colors.slice(0, nonZeroProducts.length),
          borderColor: '#fff',
          borderWidth: 2
        }
      ]
    };

    // --- DOUGHNUT CHART ---
    doughnutChartData.value = {
      labels: productNames,
      datasets: [
        {
          data: salesQuantities,
          backgroundColor: colors.slice(0, productNames.length),
          borderColor: '#fff',
          borderWidth: 2
        }
      ]
    };
  } catch (err) {
    console.error('Error fetching chart data:', err);
  }
};

onMounted(fetchAndBuildCharts);
</script>

<style scoped>
.dashboard-page {
  margin-left: var(--sidebar-width, 220px);
  padding: 1.5rem;
  min-height: 100vh;
  background: var(--color-background, #f5f5f5);
  transition: margin-left 0.2s ease;
}

.dashboard-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1.5rem;
}

.dashboard-header h1 {
  font-size: 2rem;
  margin: 0;
  color: var(--color-heading, #2c3e50);
}

.cards {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1.5rem;
}

.card {
  background: white;
  border-radius: 8px;
  padding: 1.5rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  min-height: 300px;
}

.card h3 {
  margin: 0 0 1rem 0;
  font-size: 1.1rem;
  color: #2c3e50;
  font-weight: 600;
}

.loading {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 250px;
  color: #7f8c8d;
  font-weight: 600;
}

@media (max-width: 1200px) {
  .cards {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 900px) {
  .dashboard-page {
    margin-left: 64px;
    padding: 1rem;
  }

  .dashboard-header h1 {
    font-size: 1.5rem;
  }

  .cards {
    gap: 1rem;
  }

  .card {
    padding: 1rem;
    min-height: 250px;
  }
}
</style>
