async function fetchJson(url){
  const res = await fetch(url, {cache:'no-store'});
  if(!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

function formatDate(iso){
  try{return new Date(iso).toLocaleString();}catch{return iso}
}

function statusClass(isHealthy){
  if (isHealthy === true) return 'green';
  if (isHealthy === false) return 'red';
  return 'yellow';
}

async function loadServices(){
  const tbody = document.querySelector('#servicesTable tbody');
  tbody.innerHTML = '<tr><td colspan="5">Loading…</td></tr>';
  try{
    const data = await fetchJson('/api/health/services');
    if(!Array.isArray(data) || data.length===0){
      tbody.innerHTML = '<tr><td colspan="5">No services found</td></tr>';
      return;
    }
    tbody.innerHTML = '';
    data.forEach(s => {
      const tr = document.createElement('tr');
      const svc = document.createElement('td'); svc.textContent = s.serviceName;
      const status = document.createElement('td');
      const pill = document.createElement('span');
      pill.className = 'status-pill ' + statusClass(s.isHealthy);
      pill.textContent = s.isHealthy ? 'Healthy' : 'Unhealthy';
      status.appendChild(pill);

      const last = document.createElement('td'); last.textContent = formatDate(s.lastChecked);
      const latency = document.createElement('td'); latency.textContent = s.responseTimeMs ?? '-';
      const err = document.createElement('td'); err.textContent = s.statusMessage ?? '';

      tr.appendChild(svc);
      tr.appendChild(status);
      tr.appendChild(last);
      tr.appendChild(latency);
      tr.appendChild(err);

      tr.style.cursor = 'pointer';
      tr.addEventListener('click', ()=> showHistory(s.serviceName));

      tbody.appendChild(tr);
    });
  }catch(e){
    tbody.innerHTML = `<tr><td colspan="5">Error loading services: ${e.message}</td></tr>`;
  }
}

async function showHistory(serviceName){
  document.getElementById('services').hidden = true;
  const section = document.getElementById('historySection');
  section.hidden = false;
  document.getElementById('historyTitle').textContent = `History — ${serviceName}`;
  const tbody = document.querySelector('#historyTable tbody');
  tbody.innerHTML = '<tr><td colspan="4">Loading…</td></tr>';
  try{
    const data = await fetchJson(`/api/health/services/${encodeURIComponent(serviceName)}/history`);
    tbody.innerHTML = '';
    if(!Array.isArray(data) || data.length===0){
      tbody.innerHTML = '<tr><td colspan="4">No history</td></tr>';
      return;
    }
    data.forEach(h =>{
      const tr = document.createElement('tr');
      const when = document.createElement('td'); when.textContent = formatDate(h.checkedAt);
      const status = document.createElement('td');
      const pill = document.createElement('span'); pill.className = 'status-pill ' + statusClass(h.isHealthy);
      pill.textContent = h.isHealthy ? 'Healthy' : 'Unhealthy';
      status.appendChild(pill);
      const lat = document.createElement('td'); lat.textContent = h.responseTimeMs ?? '-';
      const err = document.createElement('td'); err.textContent = h.statusMessage ?? '';
      tr.appendChild(when); tr.appendChild(status); tr.appendChild(lat); tr.appendChild(err);
      tbody.appendChild(tr);
    });
  }catch(e){
    tbody.innerHTML = `<tr><td colspan="4">Error loading history: ${e.message}</td></tr>`;
  }
}

document.getElementById('backBtn').addEventListener('click', ()=>{
  document.getElementById('historySection').hidden = true;
  document.getElementById('services').hidden = false;
});

// initial load
loadServices();
// refresh every 20s
setInterval(loadServices, 20000);
