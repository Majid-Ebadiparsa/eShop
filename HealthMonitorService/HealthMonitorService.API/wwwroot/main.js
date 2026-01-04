// ===== State Management =====
let allServices = [];
let currentFilter = 'all';
let currentSearchTerm = '';
let latencyChart = null;
let statusChart = null;
let currentHistoryData = [];
let currentExecutionLogs = [];
let currentServiceName = '';

// ===== Utility Functions =====
async function fetchJson(url) {
  const res = await fetch(url, { cache: 'no-store' });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

function formatDate(iso) {
  try { return new Date(iso).toLocaleString(); } catch { return iso; }
}

function formatDateShort(iso) {
  try {
    const date = new Date(iso);
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  } catch { return iso; }
}

function statusClass(isHealthy) {
  if (isHealthy === true) return 'green';
  if (isHealthy === false) return 'red';
  return 'yellow';
}

// ===== Services Loading & Filtering =====
async function loadServices() {
  const tbody = document.querySelector('#servicesTable tbody');
  tbody.innerHTML = '<tr><td colspan="5">Loading…</td></tr>';
  try {
    allServices = await fetchJson('/api/health/services');
    if (!Array.isArray(allServices) || allServices.length === 0) {
      tbody.innerHTML = '<tr><td colspan="5">No services found</td></tr>';
      updateStats(allServices);
      return;
    }
    updateStats(allServices);
    renderFilteredServices();
  } catch (e) {
    tbody.innerHTML = `<tr><td colspan="5">Error loading services: ${e.message}</td></tr>`;
  }
}

function renderFilteredServices() {
  const tbody = document.querySelector('#servicesTable tbody');
  
  // Apply filters
  let filtered = allServices.filter(s => {
    // Status filter
    if (currentFilter === 'healthy' && !s.isHealthy) return false;
    if (currentFilter === 'unhealthy' && s.isHealthy) return false;
    
    // Search filter
    if (currentSearchTerm) {
      const term = currentSearchTerm.toLowerCase();
      return s.serviceName.toLowerCase().includes(term) ||
             (s.statusMessage && s.statusMessage.toLowerCase().includes(term));
    }
    
    return true;
  });

  if (filtered.length === 0) {
    tbody.innerHTML = '<tr><td colspan="5">No services match your filters</td></tr>';
    return;
  }

  tbody.innerHTML = '';
  filtered.forEach(s => {
    const tr = document.createElement('tr');
    
    const svc = document.createElement('td');
    svc.textContent = s.serviceName;
    
    const status = document.createElement('td');
    const pill = document.createElement('span');
    pill.className = 'status-pill ' + statusClass(s.isHealthy);
    pill.textContent = s.isHealthy ? 'Healthy' : 'Unhealthy';
    status.appendChild(pill);

    const last = document.createElement('td');
    last.textContent = formatDate(s.lastChecked);
    
    const latency = document.createElement('td');
    latency.textContent = s.responseTimeMs ?? '-';
    
    const err = document.createElement('td');
    err.textContent = s.statusMessage ?? '';
    err.classList.add('error-cell');

    tr.appendChild(svc);
    tr.appendChild(status);
    tr.appendChild(last);
    tr.appendChild(latency);
    tr.appendChild(err);

    tr.style.cursor = 'pointer';
    tr.addEventListener('click', () => showHistory(s.serviceName));

    tbody.appendChild(tr);
  });
}

function updateStats(services) {
  const total = services.length;
  const healthy = services.filter(s => s.isHealthy === true).length;
  const unhealthy = services.filter(s => s.isHealthy === false).length;
  const avgLatency = services.length > 0 
    ? Math.round(services.reduce((sum, s) => sum + (s.responseTimeMs || 0), 0) / services.length)
    : 0;

  document.getElementById('totalServices').textContent = total;
  document.getElementById('healthyServices').textContent = healthy;
  document.getElementById('unhealthyServices').textContent = unhealthy;
  document.getElementById('avgLatency').textContent = `${avgLatency} ms`;
}

// ===== History & Charts =====
async function showHistory(serviceName) {
  currentServiceName = serviceName;
  document.getElementById('services').hidden = true;
  const section = document.getElementById('historySection');
  section.hidden = false;
  document.getElementById('historyTitle').textContent = `${serviceName}`;
  
  const tbody = document.querySelector('#historyTable tbody');
  tbody.innerHTML = '<tr><td colspan="5">Loading…</td></tr>';
  
  try {
    const data = await fetchJson(`/api/health/services/${encodeURIComponent(serviceName)}/history`);
    currentHistoryData = data;
    
    if (!Array.isArray(data) || data.length === 0) {
      tbody.innerHTML = '<tr><td colspan="5">No history available</td></tr>';
      return;
    }

    // Render table
    tbody.innerHTML = '';
    data.forEach(h => {
      const tr = document.createElement('tr');
      
      const when = document.createElement('td');
      when.textContent = formatDate(h.checkedAt);
      
      const status = document.createElement('td');
      const pill = document.createElement('span');
      pill.className = 'status-pill ' + statusClass(h.isHealthy);
      pill.textContent = h.isHealthy ? 'Healthy' : 'Unhealthy';
      status.appendChild(pill);
      
      const lat = document.createElement('td');
      lat.textContent = h.responseTimeMs ?? '-';
      
      const errCode = document.createElement('td');
      errCode.textContent = h.errorCode ?? '-';
      
      const err = document.createElement('td');
      err.textContent = h.statusMessage ?? '';
      err.classList.add('error-cell');
      
      tr.appendChild(when);
      tr.appendChild(status);
      tr.appendChild(lat);
      tr.appendChild(errCode);
      tr.appendChild(err);
      
      tbody.appendChild(tr);
    });

    // Render charts
    renderCharts(data);

    // Load execution logs
    await loadExecutionLogs(serviceName);
  } catch (e) {
    tbody.innerHTML = `<tr><td colspan="5">Error loading history: ${e.message}</td></tr>`;
  }
}

async function loadExecutionLogs(serviceName) {
  const tbody = document.querySelector('#executionLogsTable tbody');
  tbody.innerHTML = '<tr><td colspan="6">Loading execution logs…</td></tr>';
  
  try {
    const logs = await fetchJson(`/api/health/execution-logs/${encodeURIComponent(serviceName)}?limit=100`);
    currentExecutionLogs = logs;
    
    if (!Array.isArray(logs) || logs.length === 0) {
      tbody.innerHTML = '<tr><td colspan="6">No execution logs available</td></tr>';
      return;
    }

    // Render execution logs table
    tbody.innerHTML = '';
    logs.forEach(log => {
      const tr = document.createElement('tr');
      
      const execTime = document.createElement('td');
      execTime.textContent = formatDate(log.executionStartedAt);
      
      const duration = document.createElement('td');
      duration.textContent = log.durationMs;
      
      const execSuccess = document.createElement('td');
      const execPill = document.createElement('span');
      execPill.className = 'status-pill ' + (log.executionSucceeded ? 'green' : 'red');
      execPill.textContent = log.executionSucceeded ? 'Success' : 'Failed';
      execSuccess.appendChild(execPill);
      
      const serviceHealth = document.createElement('td');
      if (log.serviceIsHealthy !== null && log.serviceIsHealthy !== undefined) {
        const healthPill = document.createElement('span');
        healthPill.className = 'status-pill ' + statusClass(log.serviceIsHealthy);
        healthPill.textContent = log.serviceIsHealthy ? 'Healthy' : 'Unhealthy';
        serviceHealth.appendChild(healthPill);
      } else {
        serviceHealth.textContent = 'N/A';
      }
      
      const httpStatus = document.createElement('td');
      httpStatus.textContent = log.httpStatusCode ?? '-';
      
      const error = document.createElement('td');
      error.textContent = log.errorMessage || '-';
      error.classList.add('error-cell');
      
      tr.appendChild(execTime);
      tr.appendChild(duration);
      tr.appendChild(execSuccess);
      tr.appendChild(serviceHealth);
      tr.appendChild(httpStatus);
      tr.appendChild(error);
      
      tbody.appendChild(tr);
    });
  } catch (e) {
    tbody.innerHTML = `<tr><td colspan="6">Error loading execution logs: ${e.message}</td></tr>`;
  }
}

function renderCharts(historyData) {
  // Take last 50 records for charts (or all if less)
  const chartData = historyData.slice(0, 50).reverse();
  
  // Prepare data
  const labels = chartData.map(h => formatDateShort(h.checkedAt));
  const latencies = chartData.map(h => h.responseTimeMs || 0);
  const statuses = chartData.map(h => h.isHealthy ? 1 : 0);

  // Destroy existing charts
  if (latencyChart) latencyChart.destroy();
  if (statusChart) statusChart.destroy();

  // Latency Chart
  const latencyCtx = document.getElementById('latencyChart').getContext('2d');
  latencyChart = new Chart(latencyCtx, {
    type: 'line',
    data: {
      labels: labels,
      datasets: [{
        label: 'Response Time (ms)',
        data: latencies,
        borderColor: '#06b6d4',
        backgroundColor: 'rgba(6, 182, 212, 0.1)',
        borderWidth: 2,
        fill: true,
        tension: 0.3,
        pointRadius: 3,
        pointHoverRadius: 5
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: true,
      plugins: {
        legend: { display: false },
        tooltip: {
          mode: 'index',
          intersect: false,
          backgroundColor: 'rgba(15, 23, 36, 0.95)',
          titleColor: '#e6eef6',
          bodyColor: '#94a3b8',
          borderColor: 'rgba(255, 255, 255, 0.1)',
          borderWidth: 1
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: { color: '#94a3b8' },
          grid: { color: 'rgba(255, 255, 255, 0.05)' }
        },
        x: {
          ticks: { 
            color: '#94a3b8',
            maxRotation: 45,
            minRotation: 45
          },
          grid: { display: false }
        }
      }
    }
  });

  // Status Chart
  const statusCtx = document.getElementById('statusChart').getContext('2d');
  statusChart = new Chart(statusCtx, {
    type: 'bar',
    data: {
      labels: labels,
      datasets: [{
        label: 'Health Status',
        data: statuses,
        backgroundColor: statuses.map(s => s === 1 
          ? 'rgba(22, 163, 74, 0.6)' 
          : 'rgba(220, 38, 38, 0.6)'),
        borderColor: statuses.map(s => s === 1 
          ? '#16a34a' 
          : '#dc2626'),
        borderWidth: 1
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: true,
      plugins: {
        legend: { display: false },
        tooltip: {
          callbacks: {
            label: function(context) {
              return context.parsed.y === 1 ? 'Healthy' : 'Unhealthy';
            }
          },
          backgroundColor: 'rgba(15, 23, 36, 0.95)',
          titleColor: '#e6eef6',
          bodyColor: '#94a3b8',
          borderColor: 'rgba(255, 255, 255, 0.1)',
          borderWidth: 1
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          max: 1,
          ticks: {
            stepSize: 1,
            callback: function(value) {
              return value === 1 ? 'Healthy' : 'Unhealthy';
            },
            color: '#94a3b8'
          },
          grid: { color: 'rgba(255, 255, 255, 0.05)' }
        },
        x: {
          ticks: { 
            color: '#94a3b8',
            maxRotation: 45,
            minRotation: 45
          },
          grid: { display: false }
        }
      }
    }
  });
}

// ===== CSV Export =====
function exportServicesToCSV() {
  if (allServices.length === 0) {
    alert('No data to export');
    return;
  }

  const headers = ['Service Name', 'Status', 'Last Checked', 'Latency (ms)', 'Error Message'];
  const rows = allServices.map(s => [
    s.serviceName,
    s.isHealthy ? 'Healthy' : 'Unhealthy',
    s.lastChecked,
    s.responseTimeMs || '',
    (s.statusMessage || '').replace(/"/g, '""') // Escape quotes
  ]);

  const csvContent = [
    headers.join(','),
    ...rows.map(row => row.map(cell => `"${cell}"`).join(','))
  ].join('\n');

  downloadCSV(csvContent, `health-monitor-services-${new Date().toISOString().split('T')[0]}.csv`);
}

function exportHistoryToCSV() {
  if (currentHistoryData.length === 0) {
    alert('No history data to export');
    return;
  }

  const headers = ['Checked At', 'Status', 'Latency (ms)', 'Error Code', 'Error Message'];
  const rows = currentHistoryData.map(h => [
    h.checkedAt,
    h.isHealthy ? 'Healthy' : 'Unhealthy',
    h.responseTimeMs || '',
    h.errorCode || '',
    (h.statusMessage || '').replace(/"/g, '""')
  ]);

  const csvContent = [
    headers.join(','),
    ...rows.map(row => row.map(cell => `"${cell}"`).join(','))
  ].join('\n');

  downloadCSV(csvContent, `health-history-${currentServiceName}-${new Date().toISOString().split('T')[0]}.csv`);
}

function exportExecutionLogsToCSV() {
  if (currentExecutionLogs.length === 0) {
    alert('No execution logs to export');
    return;
  }

  const headers = ['Execution Time', 'Duration (ms)', 'Execution Success', 'Service Healthy', 'HTTP Status', 'Response Time (ms)', 'Error Message', 'Error Code', 'Exception Type'];
  const rows = currentExecutionLogs.map(log => [
    log.executionStartedAt,
    log.durationMs || '',
    log.executionSucceeded ? 'Success' : 'Failed',
    log.serviceIsHealthy !== null && log.serviceIsHealthy !== undefined ? (log.serviceIsHealthy ? 'Healthy' : 'Unhealthy') : 'N/A',
    log.httpStatusCode || '',
    log.serviceResponseTimeMs || '',
    (log.errorMessage || '').replace(/"/g, '""'),
    log.errorCode || '',
    log.exceptionType || ''
  ]);

  const csvContent = [
    headers.join(','),
    ...rows.map(row => row.map(cell => `"${cell}"`).join(','))
  ].join('\n');

  downloadCSV(csvContent, `execution-logs-${currentServiceName}-${new Date().toISOString().split('T')[0]}.csv`);
}

function downloadCSV(content, filename) {
  const blob = new Blob([content], { type: 'text/csv;charset=utf-8;' });
  const link = document.createElement('a');
  const url = URL.createObjectURL(blob);
  link.setAttribute('href', url);
  link.setAttribute('download', filename);
  link.style.visibility = 'hidden';
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}

// ===== Event Listeners =====
document.getElementById('searchInput').addEventListener('input', (e) => {
  currentSearchTerm = e.target.value;
  renderFilteredServices();
});

document.querySelectorAll('.filter-btn').forEach(btn => {
  btn.addEventListener('click', (e) => {
    document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
    e.target.classList.add('active');
    currentFilter = e.target.dataset.filter;
    renderFilteredServices();
  });
});

document.getElementById('exportBtn').addEventListener('click', exportServicesToCSV);
document.getElementById('exportHistoryBtn').addEventListener('click', exportHistoryToCSV);
document.getElementById('exportExecutionLogsBtn').addEventListener('click', exportExecutionLogsToCSV);

document.getElementById('backBtn').addEventListener('click', () => {
  document.getElementById('historySection').hidden = true;
  document.getElementById('services').hidden = false;
  // Destroy charts when leaving history view
  if (latencyChart) latencyChart.destroy();
  if (statusChart) statusChart.destroy();
  latencyChart = null;
  statusChart = null;
});

// ===== Initial Load =====
loadServices();
setInterval(loadServices, 20000); // Refresh every 20s
