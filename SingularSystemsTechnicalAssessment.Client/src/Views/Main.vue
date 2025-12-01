<template>
  <div class="dashboard-page dark">
    <header class="dashboard-header">
      <div class="header-content">
        <h1>Dashboard</h1>
        <p class="header-subtitle">Real-time Sales Analytics</p>
      </div>
    </header>

    <!-- STATS CARDS -->
    <section class="stats-section">
      <div class="stat-card" v-for="(stat, idx) in stats" :key="idx" :style="{ animationDelay: `${idx * 0.1}s` }">
        <div class="stat-icon">{{ stat.icon }}</div>
        <div class="stat-content">
          <p class="stat-label">{{ stat.label }}</p>
          <p class="stat-value">{{ stat.value }}</p>
        </div>
        <div class="stat-decoration"></div>
      </div>
    </section>

    <!-- CHARTS SECTION -->
    <section class="cards">
      <!-- Bar Chart: Sales by Product -->
      <div class="card" v-for="chart in ['bar', 'line', 'pie', 'doughnut']" :key="chart">
        <div class="card-header">
          <h3>{{ chartTitles[chart] }}</h3>
          <div class="card-badge">{{ chartBadges[chart] }}</div>
        </div>
        <div class="chart-container">
          <Bar v-if="chart === 'bar' && barChartData" :data="barChartData" :options="chartOptions" />
          <Line v-else-if="chart === 'line' && lineChartData" :data="lineChartData" :options="chartOptions" />
          <Pie v-else-if="chart === 'pie' && pieChartData" :data="pieChartData" :options="pieOptions" />
          <Doughnut v-else-if="chart === 'doughnut' && doughnutChartData" :data="doughnutChartData" :options="doughnutOptions" />
          <div v-else class="loading">
            <div class="spinner"></div>
            <span>Loading...</span>
          </div>
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
const stats = ref([
  { label: 'Total Sales', value: '0', icon: '📊', color: '#6495ff' },
  { label: 'Total Revenue', value: '$0', icon: '💰', color: '#2ecc71' },
  { label: 'Active Products', value: '0', icon: '📦', color: '#f39c12' },
  { label: 'Transactions', value: '0', icon: '🔄', color: '#e74c3c' }
]);

const chartTitles = {
  bar: 'Sales Quantity by Product',
  line: 'Sales Trend (Last 7 Days)',
  pie: 'Revenue Share by Product',
  doughnut: 'Sales Volume Distribution'
};

const chartBadges = {
  bar: 'Quantity',
  line: '7-Day',
  pie: 'Revenue',
  doughnut: 'Volume'
};

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
        font: { weight: '600' },
        padding: 15
      }
    },
    tooltip: {
      enabled: true,
      mode: 'index',
      intersect: false,
      backgroundColor: 'rgba(0, 0, 0, 0.8)',
      titleColor: '#64d5ff',
      bodyColor: '#a8d5ff',
      borderColor: 'rgba(100, 150, 255, 0.3)',
      borderWidth: 1,
      cornerRadius: 6,
      padding: 12,
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
  animation: { duration: 800, easing: 'easeOutQuart' }
};

const pieOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { position: 'right', labels: { color: '#E6EEF8', padding: 15 } },
    tooltip: {
      backgroundColor: 'rgba(0, 0, 0, 0.8)',
      titleColor: '#64d5ff',
      bodyColor: '#a8d5ff',
      borderColor: 'rgba(100, 150, 255, 0.3)',
      borderWidth: 1,
      callbacks: { label: (ctx) => `${ctx.label}: $${numberFormatter.format(ctx.parsed ?? 0)}` }
    }
  },
  animation: { duration: 800 }
};

const doughnutOptions = {
  responsive: true,
  maintainAspectRatio: false,
  cutout: '50%',
  plugins: {
    legend: { position: 'bottom', labels: { color: '#E6EEF8', padding: 15 } },
    tooltip: {
      backgroundColor: 'rgba(0, 0, 0, 0.8)',
      titleColor: '#64d5ff',
      bodyColor: '#a8d5ff',
      borderColor: 'rgba(100, 150, 255, 0.3)',
      borderWidth: 1,
      callbacks: { label: (ctx) => `${ctx.label}: ${numberFormatter.format(ctx.parsed ?? 0)}` }
    }
  },
  animation: { duration: 800 }
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

// Animated counter function
const animateValue = (start, end, duration, callback) => {
  const startTime = Date.now();
  const animate = () => {
    const now = Date.now();
    const progress = Math.min((now - startTime) / duration, 1);
    const value = Math.floor(start + (end - start) * progress);
    callback(value);
    if (progress < 1) {
      requestAnimationFrame(animate);
    }
  };
  animate();
};

const fetchAndBuildCharts = async () => {
  try {
    const productsRes = await getAllProducts(1, 100);
    const salesRes = await getSales(1, 100, null, null, null);

    const productsData = productsRes.data || {};
    const salesData = salesRes.data || {};

    // handle both camelCase and PascalCase from API
    const products = productsData.items || productsData.Items || productsData || [];
    const sales = salesData.items || salesData.Items || salesData || [];

    // Calculate stats
    const totalSales = sales.reduce((sum, s) => sum + (s.saleQty || 0), 0);
    const totalRevenue = sales.reduce((sum, s) => sum + ((s.saleQty || 0) * (s.salePrice || 0)), 0);
    const activeProducts = products.length;
    const transactions = sales.length;

    // Animate stats
    animateValue(0, totalSales, 1200, (val) => {
      stats.value[0].value = numberFormatter.format(val);
    });
    animateValue(0, totalRevenue, 1200, (val) => {
      stats.value[1].value = '$' + numberFormatter.format(Math.floor(val));
    });
    stats.value[2].value = activeProducts;
    stats.value[3].value = transactions;

    // --- BAR CHART ---
    const productNames = products.map(p => p.description || '');
    const salesQuantities = products.map(prod => {
      return sales
        .filter(s => (s.productName || '') === (prod.description || ''))
        .reduce((sum, s) => sum + (s.saleQty || 0), 0);
    });

    barChartData.value = {
      labels: productNames,
      datasets: [
        {
          label: 'Quantity Sold',
          data: salesQuantities,
          backgroundColor: colors.slice(0, productNames.length),
          borderColor: colors.map(c => shadeColor(c, -8)).slice(0, productNames.length),
          borderWidth: 2,
          borderRadius: 8,
          maxBarThickness: 48,
          hoverBackgroundColor: colors.map(c => shadeColor(c, 15)).slice(0, productNames.length)
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
      return sales
        .filter(s => {
          const sd = new Date(s.saleDate ?? s.SaleDate ?? '');
          return !isNaN(sd.getTime()) && sd.toISOString().slice(0, 10) === dateKey;
        })
        .reduce((sum, s) => sum + (s.saleQty ?? s.SaleQty ?? 0), 0);
    });

    lineChartData.value = {
      labels: dateLabels,
      datasets: [
        {
          label: 'Daily Sales Quantity',
          data: dailySalesData,
          borderColor: '#6495ff',
          backgroundColor: 'rgba(100, 149, 255, 0.15)',
          borderWidth: 3,
          tension: 0.4,
          pointRadius: 5,
          pointBackgroundColor: '#6495ff',
          pointBorderColor: '#fff',
          pointBorderWidth: 2,
          pointHoverRadius: 7,
          pointHoverBackgroundColor: '#64d5ff',
          fill: true
        }
      ]
    };

    // --- PIE CHART ---
    const revenueByProduct = products.map(prod => {
      const productSales = sales.filter(s => (s.productName || '') === (prod.description || ''));
      return productSales.reduce((sum, s) => sum + ((s.saleQty || 0) * (s.salePrice || 0)), 0);
    });

    const nonZeroProducts = products.filter((_, i) => revenueByProduct[i] > 0);
    const nonZeroRevenue = revenueByProduct.filter(r => r > 0);

    pieChartData.value = {
      labels: nonZeroProducts.map(p => p.description || ''),
      datasets: [
        {
          data: nonZeroRevenue,
          backgroundColor: colors.slice(0, nonZeroProducts.length),
          borderColor: '#fff',
          borderWidth: 2,
          hoverOffset: 10
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
          hoverOffset: 8
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
  background: linear-gradient(135deg, #0b1220 0%, #0f1a2e 100%);
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

.dashboard-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 2rem;
  animation: slideInLeft 0.6s ease-out;
}

.header-content h1 {
  font-size: 2.5rem;
  margin: 0;
  color: #a8d5ff;
  font-weight: 700;
  letter-spacing: -0.5px;
}

.header-subtitle {
  color: #6495ff;
  font-size: 0.95rem;
  margin: 0.25rem 0 0 0;
  font-weight: 500;
  letter-spacing: 0.3px;
}

/* STATS CARDS */
.stats-section {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin-bottom: 2rem;
  animation: fadeInUp 0.7s ease-out 0.1s both;
}

.stat-card {
  background: linear-gradient(135deg, rgba(100, 150, 255, 0.1), rgba(100, 120, 200, 0.05));
  backdrop-filter: blur(15px);
  border: 1px solid rgba(100, 150, 255, 0.2);
  border-radius: 10px;
  padding: 1.5rem;
  position: relative;
  overflow: hidden;
  transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  animation: popIn 0.5s cubic-bezier(0.36, 0, 0.66, 1) forwards;
}

.stat-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.1), transparent);
  transition: left 0.5s ease;
}

.stat-card:hover {
  transform: translateY(-8px);
  border-color: rgba(100, 180, 255, 0.5);
  box-shadow: 0 12px 30px rgba(100, 150, 255, 0.25);
  background: linear-gradient(135deg, rgba(100, 150, 255, 0.15), rgba(100, 120, 200, 0.08));
}

.stat-card:hover::before {
  left: 100%;
}

.stat-icon {
  font-size: 2rem;
  margin-bottom: 0.5rem;
  animation: floatingCards 3s ease-in-out infinite;
}

.stat-label {
  color: #6495ff;
  font-size: 0.8rem;
  margin: 0;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.stat-value {
  color: #a8d5ff;
  font-size: 1.5rem;
  margin: 0.5rem 0 0 0;
  font-weight: 700;
  letter-spacing: -0.3px;
}

.stat-decoration {
  position: absolute;
  width: 60px;
  height: 60px;
  border-radius: 50%;
  background: rgba(100, 150, 255, 0.1);
  top: -20px;
  right: -20px;
}

/* CHARTS SECTION */
.cards {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1.5rem;
  margin-bottom: 1rem;
}

.card {
  background: linear-gradient(135deg, rgba(30, 50, 80, 0.4), rgba(20, 40, 70, 0.2));
  backdrop-filter: blur(20px);
  border: 1px solid rgba(100, 150, 255, 0.2);
  border-radius: 12px;
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  animation: floatingCards 6s ease-in-out infinite;
  transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
  min-height: 0;
}

.card:nth-child(1) {
  animation-delay: 0s;
}

.card:nth-child(2) {
  animation-delay: 0.2s;
}

.card:nth-child(3) {
  animation-delay: 0.4s;
}

.card:nth-child(4) {
  animation-delay: 0.6s;
}

.card:hover {
  transform: translateY(-15px);
  border-color: rgba(100, 180, 255, 0.6);
  box-shadow: 0 20px 50px rgba(100, 150, 255, 0.3), 0 0 30px rgba(100, 150, 255, 0.2);
  background: linear-gradient(135deg, rgba(40, 70, 120, 0.5), rgba(30, 50, 100, 0.3));
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.card h3 {
  margin: 0;
  font-size: 1.1rem;
  color: #a8d5ff;
  font-weight: 600;
  text-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
}

.card-badge {
  background: rgba(100, 150, 255, 0.2);
  color: #6495ff;
  padding: 0.35rem 0.75rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.chart-container {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 300px;
  animation: fadeInUp 0.8s ease-out 0.3s both;
}

.chart-container canvas {
  width: 100% !important;
  height: 100% !important;
}

.loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  gap: 1rem;
  color: #6495ff;
  font-weight: 600;
}

.spinner {
  width: 50px;
  height: 50px;
  border: 4px solid rgba(100, 150, 255, 0.2);
  border-top: 4px solid #6495ff;
  border-radius: 50%;
  animation: spin 1.2s linear infinite;
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
    font-size: 1.8rem;
  }

  .cards {
    gap: 1rem;
  }

  .card {
    padding: 1rem;
  }

  .stats-section {
    grid-template-columns: repeat(2, 1fr);
  }
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}
</style>
