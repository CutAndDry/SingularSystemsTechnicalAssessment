<template>
  <div class="dashboard-page dark">
        <header class="dashboard-header">
          <h1>Dashboard</h1>
        </header>

    <section class="cards">
      <!-- Bar Chart: Sales by Product -->
      <div class="card">
        <h3>Sales Quantity by Product</h3>
        <div class="chart-container">
          <Bar v-if="barChartData" :data="barChartData" :options="chartOptions" />
          <div v-else class="loading">Loading...</div>
        </div>
      </div>

      <!-- Line Chart: Sales Over Time -->
      <div class="card">
        <h3>Sales Trend (Last 7 Days)</h3>
        <div class="chart-container">
          <Line v-if="lineChartData" :data="lineChartData" :options="chartOptions" />
          <div v-else class="loading">Loading...</div>
        </div>
      </div>

      <!-- Pie Chart: Revenue Distribution -->
      <div class="card">
        <h3>Revenue Share by Product</h3>
        <div class="chart-container">
          <Pie v-if="pieChartData" :data="pieChartData" :options="pieOptions" />
          <div v-else class="loading">Loading...</div>
        </div>
      </div>

      <!-- Doughnut Chart: Product Sales Ratio -->
      <div class="card">
        <h3>Sales Volume Distribution</h3>
        <div class="chart-container">
          <Doughnut v-if="doughnutChartData" :data="doughnutChartData" :options="doughnutOptions" />
          <div v-else class="loading">Loading...</div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
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

// Dashboard is always dark in this build
const isDark = true;

const numberFormatter = new Intl.NumberFormat('en-US');

// dynamic chart options depending on theme
const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'top',
      labels: {
        color: '#E6EEF8',
        font: { weight: '600' }
      }
    },
    tooltip: {
      enabled: true,
      mode: 'index',
      intersect: false,
      callbacks: {
        label: function(context) {
          const v = context.parsed.y ?? context.parsed ?? 0;
          return `${context.dataset.label}: ${numberFormatter.format(v)}`;
        }
      }
    }
  },
  layout: { padding: { top: 8, right: 8, bottom: 8, left: 8 } },
  scales: {
    x: { ticks: { color: '#E6EEF8', maxRotation: 45, minRotation: 0, autoSkip: true }, grid: { display: false } },
    y: { beginAtZero: true, ticks: { color: '#E6EEF8', callback: (v) => numberFormatter.format(v) }, grid: { color: 'rgba(230,238,248,0.06)' } }
  },
  animation: { duration: 600, easing: 'easeOutQuart' }
};

const pieOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { position: 'right', labels: { color: '#E6EEF8' } },
    tooltip: { callbacks: { label: (ctx) => `${ctx.label}: $${numberFormatter.format(ctx.parsed ?? 0)}` } }
  },
  animation: { duration: 600 }
};

const doughnutOptions = {
  responsive: true,
  maintainAspectRatio: false,
  cutout: '50%',
  plugins: {
    legend: { position: 'bottom', labels: { color: '#E6EEF8' } },
    tooltip: { callbacks: { label: (ctx) => `${ctx.label}: ${numberFormatter.format(ctx.parsed ?? 0)}` } }
  },
  animation: { duration: 600 }
};

const colors = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899', '#14b8a6'];

// Utility: slightly darken/lighten a hex color by percent (-100..100)
function shadeColor(hex, percent) {
  try {
    const normalized = hex.replace('#', '');
    const num = parseInt(normalized, 16);
    let r = (num >> 16) & 0xff;
    let g = (num >> 8) & 0xff;
    let b = num & 0xff;

    r = Math.min(255, Math.max(0, Math.round(r * (100 + percent) / 100)));
    g = Math.min(255, Math.max(0, Math.round(g * (100 + percent) / 100)));
    b = Math.min(255, Math.max(0, Math.round(b * (100 + percent) / 100)));

    return '#' + ((1 << 24) + (r << 16) + (g << 8) + b).toString(16).slice(1);
  } catch (e) {
    return hex;
  }
}

const fetchAndBuildCharts = async () => {
  try {
    const productsRes = await getAllProducts(1, 100);
    const salesRes = await getSales(1, 100, null, null, null);

    const products = productsRes.data.items || [];
    const sales = salesRes.data.items || [];

    // --- BAR CHART ---
    const productNames = products.map(p => p.name);
    const salesQuantities = products.map(prod => {
      return sales.filter(s => s.productName === prod.name).reduce((sum, s) => sum + (s.saleQty || 0), 0);
    });

    barChartData.value = {
      labels: productNames,
      datasets: [
        {
          label: 'Quantity Sold',
          data: salesQuantities,
          backgroundColor: colors.slice(0, productNames.length),
          borderColor: colors.map(c => shadeColor(c, -8)).slice(0, productNames.length),
          borderWidth: 1,
          borderRadius: 8,
          maxBarThickness: 48
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
      return sales.filter(s => new Date(s.saleDate).toISOString().slice(0, 10) === dateKey).reduce((sum, s) => sum + (s.saleQty || 0), 0);
    });

    lineChartData.value = {
      labels: dateLabels,
      datasets: [
        {
          label: 'Daily Sales Quantity',
          data: dailySalesData,
          borderColor: '#2c3e50',
          backgroundColor: 'rgba(44, 62, 80, 0.08)',
          borderWidth: 2,
          tension: 0.36,
          pointRadius: 4,
          pointBackgroundColor: '#fff',
          pointBorderColor: '#2c3e50',
          pointBorderWidth: 2,
          fill: true
        }
      ]
    };

    // --- PIE CHART ---
    const revenueByProduct = products.map(prod => {
      const productSales = sales.filter(s => s.productName === prod.name);
      return productSales.reduce((sum, s) => sum + ((s.saleQty || 0) * (s.salePrice || 0)), 0);
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
          borderWidth: 2,
          hoverOffset: 8
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
          borderWidth: 2,
          hoverOffset: 6
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
  position: absolute;
  top: 0;
  bottom: 0;
  left: var(--sidebar-width, 220px);
  right: 0;
  padding: 1.5rem;
  background: var(--bg, #f5f5f5);
  color: var(--text, #1f2937);
  overflow: auto;
}

.dashboard-page.dark {
  --bg: #0b1220;
  --card-bg: #071024;
  --text: #e6eef8;
  --muted: #9aa9bf;
  --grid: rgba(230,238,248,0.06);
}

.dashboard-page {
  --bg: #f5f5f5;
  --card-bg: #ffffff;
  --text: #1f2937;
  --muted: #475569;
  --grid: rgba(15,23,42,0.06);
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

/* Make the cards area fill available vertical space and stretch cards */
.cards {
  height: calc(100vh - 140px); /* account for header and padding */
  grid-auto-rows: 1fr;
  align-items: stretch;
}

.card {
  background: var(--card-bg);
  border-radius: 8px;
  padding: 1.5rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  display: flex;
  flex-direction: column;
  min-height: 0; /* allow flex children to control height */
}

.chart-container {
  flex: 1 1 auto;
  display: flex;
  align-items: center;
  height: 100%;
}

.card canvas {
  width: 100% !important;
  height: 100% !important;
}

.card h3 {
  margin: 0 0 1rem 0;
  font-size: 1.1rem;
  color: var(--text);
  font-weight: 600;
}

.loading {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 250px;
  color: var(--muted);
  font-weight: 600;
}

@media (max-width: 1200px) {
  .cards {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 900px) {
  .dashboard-page {
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

  /* Header actions */
  /* header-actions and theme toggle removed - dashboard is always dark */
}
</style>
