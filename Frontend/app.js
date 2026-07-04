// =============================================
//  Travel Calculator — app.js
//  يتوافق مع API الجديد (Scenarios + ScenarioItems)
// =============================================

const API_BASE = 'https://localhost:7275/api';

// ===== Device ID =====
function getDeviceId() {
    let id = localStorage.getItem('deviceId');
    if (!id) { id = crypto.randomUUID(); localStorage.setItem('deviceId', id); }
    return id;
}

// ===== DOM =====
const fromCitySelect     = document.getElementById('fromCity');
const toCitySelect       = document.getElementById('toCity');
const fuelTypeSelect     = document.getElementById('fuelType');
const daysPerWeekInput   = document.getElementById('daysPerWeek');
const weeksPerMonthInput = document.getElementById('weeksPerMonth');
const consumptionInput   = document.getElementById('consumption');
const passengerCountInput= document.getElementById('passengerCount');
const calculateBtn       = document.getElementById('calculateBtn');
const saveScenarioBtn    = document.getElementById('saveScenarioBtn');
const scenariosContainer = document.getElementById('scenariosContainer');
const scenariosCount     = document.getElementById('scenariosCount');
const loader             = document.getElementById('loader');
const resultsSection     = document.getElementById('results');
const routesContainer    = document.getElementById('routesContainer');
const carContainer       = document.getElementById('carContainer');
const housingContainer   = document.getElementById('housingContainer');

// Save Modal
const scenarioModal         = document.getElementById('scenarioModal');
const closeModalBtn         = document.getElementById('closeModalBtn');
const cancelModalBtn        = document.getElementById('cancelModalBtn');
const confirmSaveBtn        = document.getElementById('confirmSaveScenarioBtn');

// Detail Modal
const detailModal           = document.getElementById('scenarioDetailModal');
const closeDetailModalBtn   = document.getElementById('closeDetailModalBtn');
const detailScenarioName    = document.getElementById('detailScenarioName');
const detailScenarioType    = document.getElementById('detailScenarioType');
const scenarioItemsList     = document.getElementById('scenarioItemsList');
const noItemsMsg            = document.getElementById('noItemsMsg');
const addItemToScenarioBtn  = document.getElementById('addItemToScenarioBtn');

const specificItemContainer = document.getElementById('specificItemContainer');
const scSpecificItem        = document.getElementById('scSpecificItem');

// ===== App State =====
let lastCalculatedData = { routes: [], car: null, housing: [] };
let currentDetailScenarioId = null;

// =============================================
//  Toast Notifications
// =============================================
function showToast(message, type = 'success') {
    const toast = document.getElementById('toast');
    toast.className = `toast ${type} show`;
    toast.innerHTML = `<i class="fa-solid fa-${type === 'success' ? 'circle-check' : 'circle-xmark'}"></i> ${message}`;
    setTimeout(() => { toast.classList.remove('show'); }, 3000);
}

// =============================================
//  Init
// =============================================
document.addEventListener('DOMContentLoaded', async () => {
    await loadCities();
    await loadFuelTypes();
    await loadScenarios();
    setupTypeSelector();
});

// =============================================
//  API: Cities & Fuel
// =============================================
async function loadCities() {
    try {
        const res  = await fetch(`${API_BASE}/Cities/All`);
        if (!res.ok) throw new Error();
        const data = await res.json();
        let opts = '<option value="">اختر المدينة...</option>';
        data.forEach(c => opts += `<option value="${c.cityID}">${c.cityName}</option>`);
        fromCitySelect.innerHTML = opts;
        toCitySelect.innerHTML   = opts;
    } catch {
        fromCitySelect.innerHTML = '<option value="">خطأ في التحميل</option>';
        toCitySelect.innerHTML   = '<option value="">خطأ في التحميل</option>';
    }
}

async function loadFuelTypes() {
    try {
        const res  = await fetch(`${API_BASE}/Fuel/Prices`);
        if (!res.ok) throw new Error();
        const data = await res.json();
        fuelTypeSelect.innerHTML = data.map(f => `<option value="${f.fuelType}">${f.fuelType} (${f.pricePerLiter} ج.م)</option>`).join('');
    } catch {
        fuelTypeSelect.innerHTML = '<option value="">خطأ في التحميل</option>';
    }
}

// =============================================
//  Calculate
// =============================================
calculateBtn.addEventListener('click', async () => {
    const fromCityID    = fromCitySelect.value;
    const toCityID      = toCitySelect.value;
    const daysPerWeek   = daysPerWeekInput.value;
    const weeksPerMonth = weeksPerMonthInput.value;
    const fuelType      = fuelTypeSelect.value;
    const consumption   = consumptionInput.value;
    const passengers    = passengerCountInput.value || 1;

    if (!fromCityID || !toCityID) { showToast('اختر مدينة الانطلاق ومدينة الوجهة', 'error'); return; }
    if (fromCityID === toCityID)  { showToast('يجب أن تكون المدينتان مختلفتين', 'error'); return; }

    loader.classList.remove('hidden');
    resultsSection.classList.add('hidden');

    try {
        const [routesRes, carRes, housingRes] = await Promise.all([
            fetch(`${API_BASE}/Routes/Calculate?fromCityID=${fromCityID}&toCityID=${toCityID}&daysPerWeek=${daysPerWeek}&weeksPerMonth=${weeksPerMonth}`),
            fetch(`${API_BASE}/Fuel/CalculateSplit?fromCityID=${fromCityID}&toCityID=${toCityID}&fuelType=${fuelType}&consumptionPer100=${consumption}&daysPerWeek=${daysPerWeek}&weeksPerMonth=${weeksPerMonth}&passengerCount=${passengers}`),
            fetch(`${API_BASE}/Housing/Compare?fromCityID=${fromCityID}&toCityID=${toCityID}&daysPerWeek=${daysPerWeek}&weeksPerMonth=${weeksPerMonth}`)
        ]);

        lastCalculatedData.routes  = routesRes.ok  ? await routesRes.json()  : [];
        lastCalculatedData.car     = carRes.ok      ? await carRes.json()     : null;
        lastCalculatedData.housing = housingRes.ok  ? await housingRes.json() : [];

        displayRoutes(lastCalculatedData.routes);
        displayCarCost(lastCalculatedData.car);
        displayHousing(lastCalculatedData.housing);
        updateCostPreview();

        loader.classList.add('hidden');
        resultsSection.classList.remove('hidden');
    } catch {
        loader.classList.add('hidden');
        showToast('خطأ في الاتصال بالخادم. تأكد من تشغيل الـ API.', 'error');
    }
});

// =============================================
//  Display Functions
// =============================================
function displayRoutes(routes) {
    if (!routes.length) {
        routesContainer.innerHTML = '<p class="text-muted">لا توجد مواصلات عامة مسجلة بين هاتين المدينتين.</p>';
        return;
    }
    routesContainer.innerHTML = routes.map((r, i) => `
        <div class="item-card fade-in" style="animation-delay:${i * 0.07}s">
            <h3><i class="fa-solid fa-van-shuttle"></i> ${r.transportType}</h3>
            <div class="stat-row"><span>الوقت:</span><span class="stat-value">${r.durationMinutes} دقيقة</span></div>
            <div class="stat-row"><span>المسافة:</span><span class="stat-value">${r.distanceKm} كم</span></div>
            <div class="stat-row"><span>سعر التذكرة:</span><span class="stat-value">${r.oneWayPrice} ج.م</span></div>
            <div class="stat-row"><span>ذهاب وعودة يومياً:</span><span class="stat-value">${r.dailyRoundTrip} ج.م</span></div>
            <div class="stat-row total"><span>التكلفة الشهرية:</span><span class="stat-value">${r.monthlyCost} ج.م</span></div>
        </div>
    `).join('');
}

function displayCarCost(car) {
    if (!car) {
        carContainer.innerHTML = '<p class="text-muted">لا يمكن حساب تكلفة السيارة. تأكد من وجود مسافة مسجلة.</p>';
        return;
    }
    carContainer.innerHTML = `
        <div class="item-card" style="max-width: 500px;">
            <h3><i class="fa-solid fa-car"></i> تكلفة السيارة الخاصة</h3>
            <div class="stat-row"><span>المسافة (ذهاب وعودة):</span><span class="stat-value">${car.dailyRoundTripKm} كم</span></div>
            <div class="stat-row"><span>تكلفة الوقود يومياً:</span><span class="stat-value">${car.dailyFuelCost} ج.م</span></div>
            <div class="stat-row"><span>تكلفة الصيانة يومياً:</span><span class="stat-value">${car.dailyMaintenanceCost} ج.م</span></div>
            <div class="stat-row"><span>الإجمالي اليومي:</span><span class="stat-value">${car.dailyTotalCost} ج.م</span></div>
            <div class="stat-row total"><span>الإجمالي الشهري:</span><span class="stat-value">${car.monthlyTotalCost} ج.م</span></div>
            ${car.passengerCount > 1 ? `
            <div class="stat-row total" style="color:var(--primary); margin-top:8px; border-top:1px dashed rgba(255,255,255,0.1); padding-top:8px;">
                <span><i class="fa-solid fa-users"></i> نصيب الفرد (${car.passengerCount} ركاب):</span>
                <span class="stat-value">${car.monthlyCostPerPerson} ج.م</span>
            </div>` : ''}
        </div>
    `;
}

function displayHousing(list) {
    if (!list.length) {
        housingContainer.innerHTML = '<p class="text-muted">لا توجد خيارات سكن مسجلة في المدينة المطلوبة.</p>';
        return;
    }
    let html = list.map((h, i) => {
        const cheaper = h.difference < 0;
        return `
        <div class="item-card fade-in" style="animation-delay:${i * 0.07}s">
            <h3><i class="fa-solid fa-bed"></i> ${h.housingType}</h3>
            <div class="stat-row"><span>الإيجار الشهري:</span><span class="stat-value">${h.monthlyRent} ج.م</span></div>
            <div class="stat-row"><span>مواصلات شهرية:</span><span class="stat-value">${h.cheapestMonthlyCommute} ج.م</span></div>
            <div class="stat-row">
                <span>الفرق:</span>
                <span class="stat-value ${cheaper ? 'text-success' : 'text-danger'}">
                    ${cheaper ? 'أرخص' : 'أغلى'} بـ ${Math.abs(h.difference)} ج.م
                </span>
            </div>
            <div class="stat-row total" style="color:${cheaper ? 'var(--success)' : 'var(--warning)'};">
                <span><i class="fa-solid fa-circle-info"></i> التوصية:</span>
                <span style="font-size:0.9rem;">${h.recommendation}</span>
            </div>
        </div>`;
    }).join('');

    html += `
        <div style="grid-column:1/-1; margin-top:10px; padding:12px; background:rgba(255,255,255,0.04); border-radius:10px; font-size:0.88rem; color:#ddd; text-align:center; border:1px solid rgba(255,255,255,0.08);">
            <i class="fa-solid fa-circle-info" style="color:var(--secondary);"></i>
            <strong>ملاحظة:</strong> تكلفة المواصلات مبنية على تعريفة <strong>الميكروباص</strong> كخيار افتراضي.
        </div>`;
    housingContainer.innerHTML = html;
}

// =============================================
//  Type Selector (Radio Buttons in Modal)
// =============================================
function setupTypeSelector() {
    document.querySelectorAll('.type-option input').forEach(radio => {
        radio.addEventListener('change', () => {
            populateSpecificItems();
            updateCostPreview();
        });
    });
    scSpecificItem.addEventListener('change', updateCostPreview);
}

function getSelectedType() {
    const checked = document.querySelector('.type-option input:checked');
    return checked ? checked.value : 'Commute';
}

function populateSpecificItems() {
    const type = getSelectedType();
    scSpecificItem.innerHTML = '';
    
    if (type === 'Commute' && lastCalculatedData.routes.length > 0) {
        specificItemContainer.style.display = 'flex';
        // جعل الميكروباص الافتراضي
        let defaultIndex = 0;
        lastCalculatedData.routes.forEach((r, idx) => {
            if (r.transportType.includes('ميكروباص')) defaultIndex = idx;
        });

        lastCalculatedData.routes.forEach((r, idx) => {
            const isSelected = idx === defaultIndex ? 'selected' : '';
            scSpecificItem.innerHTML += `<option value="${idx}" ${isSelected}>${r.transportType} - ${r.monthlyCost} ج.م</option>`;
        });
    } else if (type === 'Housing' && lastCalculatedData.housing.length > 0) {
        specificItemContainer.style.display = 'flex';
        lastCalculatedData.housing.forEach((h, idx) => {
            scSpecificItem.innerHTML += `<option value="${idx}">${h.housingType} - ${h.monthlyRent} ج.م</option>`;
        });
    } else {
        specificItemContainer.style.display = 'none';
    }
}

function updateCostPreview() {
    const type    = getSelectedType();
    const preview = document.getElementById('costPreview');
    const text    = document.getElementById('costPreviewText');

    let cost = 0;
    const selectedIdx = parseInt(scSpecificItem.value) || 0;

    if (type === 'Commute' && lastCalculatedData.routes.length > 0) {
        cost = lastCalculatedData.routes[selectedIdx].monthlyCost;
    } else if (type === 'CarTrip' && lastCalculatedData.car) {
        cost = lastCalculatedData.car.passengerCount > 1
            ? lastCalculatedData.car.monthlyCostPerPerson
            : lastCalculatedData.car.monthlyTotalCost;
    } else if (type === 'Housing' && lastCalculatedData.housing.length > 0) {
        cost = lastCalculatedData.housing[selectedIdx].monthlyRent;
    }

    if (cost > 0) {
        preview.classList.add('has-cost');
        text.textContent = `التكلفة الشهرية المحسوبة: ${cost} ج.م`;
    } else {
        preview.classList.remove('has-cost');
        text.textContent = 'لا توجد تكلفة محسوبة لهذا النوع — احسب التكلفة أولاً';
    }
}

// =============================================
//  Scenarios — Load & Display
// =============================================
async function loadScenarios() {
    try {
        const res = await fetch(`${API_BASE}/Scenarios`, {
            headers: { 'X-Device-Id': getDeviceId() }
        });

        if (res.status === 404) {
            showEmptyScenarios(); return;
        }
        if (!res.ok) {
            scenariosContainer.innerHTML = '<p class="text-danger">فشل في تحميل السيناريوهات.</p>'; return;
        }

        const scenarios = await res.json();
        if (!scenarios.length) { showEmptyScenarios(); return; }

        scenariosCount.textContent = scenarios.length;
        scenariosContainer.innerHTML = scenarios.map(sc => renderScenarioCard(sc)).join('');
    } catch {
        scenariosContainer.innerHTML = '<p class="text-danger">خطأ في الاتصال بالخادم.</p>';
    }
}

function showEmptyScenarios() {
    scenariosCount.textContent = '0';
    scenariosContainer.innerHTML = `
        <div class="empty-state" style="grid-column:1/-1;">
            <i class="fa-solid fa-folder-open"></i>
            <p>لا توجد سيناريوهات محفوظة بعد.<br>احسب التكلفة واضغط «حفظ كسيناريو»</p>
        </div>`;
}

function renderScenarioCard(sc) {
    const typeLabel = sc.scenarioType === 'Commute' ? 'مواصلات'
                    : sc.scenarioType === 'Housing' ? 'سكن'
                    : 'سيارة';
    const typeCls   = sc.scenarioType === 'Housing' ? 'housing'
                    : sc.scenarioType === 'CarTrip'  ? 'cartrip' : '';
    const typeIcon  = sc.scenarioType === 'Commute'  ? 'fa-van-shuttle'
                    : sc.scenarioType === 'Housing'  ? 'fa-house' : 'fa-car';
    const date      = new Date(sc.createdAt).toLocaleDateString('ar-EG');

    return `
    <div class="scenario-card fade-in" id="sc-${sc.scenarioID}">
        <div class="scenario-card-header">
            <div>
                <div class="scenario-name">
                    <i class="fa-solid ${typeIcon}" style="color:var(--primary);"></i>
                    ${sc.scenarioName}
                </div>
                <div class="scenario-date">${date}</div>
            </div>
            <span class="scenario-type-badge ${typeCls}">${typeLabel}</span>
        </div>
        <div class="scenario-items-count">
            <i class="fa-solid fa-layer-group"></i>
            اضغط لعرض الخيارات المحفوظة
        </div>
        <div class="scenario-card-footer">
            <button class="btn-view" onclick="openDetailModal(${sc.scenarioID}, '${sc.scenarioName}', '${sc.scenarioType}')">
                <i class="fa-solid fa-eye"></i> عرض التفاصيل
            </button>
            <button class="btn-delete" onclick="deleteScenario(${sc.scenarioID}, event)">
                <i class="fa-solid fa-trash"></i>
            </button>
        </div>
    </div>`;
}

// =============================================
//  Delete Scenario
// =============================================
async function deleteScenario(id, event) {
    event.stopPropagation();
    if (!confirm('هل أنت متأكد من حذف هذا السيناريو؟')) return;

    try {
        const res = await fetch(`${API_BASE}/Scenarios/${id}`, {
            method: 'DELETE',
            headers: { 'X-Device-Id': getDeviceId() }
        });

        if (res.ok) {
            document.getElementById(`sc-${id}`)?.remove();
            const remaining = scenariosContainer.querySelectorAll('.scenario-card').length;
            scenariosCount.textContent = remaining;
            if (!remaining) showEmptyScenarios();
            showToast('تم حذف السيناريو بنجاح');
        } else {
            showToast('فشل في حذف السيناريو', 'error');
        }
    } catch {
        showToast('خطأ في الاتصال بالخادم', 'error');
    }
}

// =============================================
//  Save Scenario Modal
// =============================================
saveScenarioBtn.addEventListener('click', () => {
    const from = fromCitySelect.value;
    const to   = toCitySelect.value;
    if (!from || !to) { showToast('اختر المدن أولاً', 'error'); return; }
    if (!lastCalculatedData.routes.length && !lastCalculatedData.car && !lastCalculatedData.housing.length) {
        showToast('احسب التكلفة أولاً قبل الحفظ', 'error'); return;
    }
    populateSpecificItems();
    updateCostPreview();
    openModal(scenarioModal);
});

closeModalBtn.addEventListener('click', () => closeModal(scenarioModal));
cancelModalBtn.addEventListener('click', () => closeModal(scenarioModal));

// ===== Confirm Save =====
confirmSaveBtn.addEventListener('click', async () => {
    const name     = document.getElementById('scName').value.trim();
    const type     = getSelectedType();
    const itemName = document.getElementById('scItemName').value.trim();

    if (!name)     { showToast('أدخل اسم السيناريو', 'error'); return; }
    if (!itemName) { showToast('أدخل اسم الخيار', 'error'); return; }

    // حساب التكلفة بناءً على الخيار المحدد
    let cost = 0;
    const selectedIdx = parseInt(scSpecificItem.value) || 0;

    if (type === 'Commute' && lastCalculatedData.routes.length)
        cost = lastCalculatedData.routes[selectedIdx].monthlyCost;
    else if (type === 'CarTrip' && lastCalculatedData.car)
        cost = lastCalculatedData.car.passengerCount > 1
            ? lastCalculatedData.car.monthlyCostPerPerson
            : lastCalculatedData.car.monthlyTotalCost;
    else if (type === 'Housing' && lastCalculatedData.housing.length)
        cost = lastCalculatedData.housing[selectedIdx].monthlyRent;

    if (!cost) { showToast('لا توجد تكلفة محسوبة لهذا النوع', 'error'); return; }

    confirmSaveBtn.disabled = true;
    confirmSaveBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> جاري الحفظ...';

    try {
        // الخطوة 1: إنشاء السيناريو (الرأس)
        const scRes = await fetch(`${API_BASE}/Scenarios`, {
            method: 'POST',
            headers: { 'X-Device-Id': getDeviceId(), 'Content-Type': 'application/json' },
            body: JSON.stringify({ 
                scenarioID: 0,
                deviceID: getDeviceId(),
                scenarioName: name, 
                scenarioType: type,
                createdAt: new Date().toISOString()
            })
        });

        if (!scRes.ok) { 
            const errorText = await scRes.text();
            showToast(`فشل إنشاء السيناريو: ${errorText}`, 'error'); 
            return; 
        }
        const newSc = await scRes.json();
        const scenarioID = newSc.scenarioID;

        // الخطوة 2: إضافة الخيار (ScenarioItem)
        const itemPayload = buildItemPayload(type, itemName, scenarioID, cost);

        const itemRes = await fetch(`${API_BASE}/Scenarios/${scenarioID}/items`, {
            method: 'POST',
            headers: { 'X-Device-Id': getDeviceId(), 'Content-Type': 'application/json' },
            body: JSON.stringify(itemPayload)
        });

        if (!itemRes.ok) {
            const errText = await itemRes.text();
            showToast(`فشل إضافة الخيار: ${errText}`, 'error');
        } else {
            showToast('تم حفظ السيناريو بنجاح ✓');
        }

        closeModal(scenarioModal);
        document.getElementById('scName').value     = '';
        document.getElementById('scItemName').value = '';
        loadScenarios();

    } catch {
        showToast('خطأ في الاتصال بالخادم', 'error');
    } finally {
        confirmSaveBtn.disabled = false;
        confirmSaveBtn.innerHTML = '<i class="fa-solid fa-check"></i> حفظ';
    }
});

// بناء الـ payload حسب نوع الخيار
function buildItemPayload(type, itemName, scenarioID, cost) {
    const fromCityID    = parseInt(fromCitySelect.value);
    const toCityID      = parseInt(toCitySelect.value);
    const daysPerWeek   = parseInt(daysPerWeekInput.value) || 5;
    const weeksPerMonth = parseInt(weeksPerMonthInput.value) || 4;
    
    const selectedIdx = parseInt(scSpecificItem.value) || 0;

    const base = { scenarioID, itemName, itemType: type, monthlyCostSnapshot: cost };

    if (type === 'Commute') {
        const tTypeID = lastCalculatedData.routes[selectedIdx]?.transportTypeID ?? null;
        return { ...base, fromCityID, toCityID, transportTypeID: tTypeID, daysPerWeek, weeksPerMonth };
    }
    if (type === 'CarTrip') {
        return {
            ...base,
            fromCityID, toCityID,
            daysPerWeek, weeksPerMonth,
            fuelType:        fuelTypeSelect.value || null,
            consumptionPer100: parseFloat(consumptionInput.value) || 8,
            passengerCount:  parseInt(passengerCountInput.value) || 1
        };
    }
    if (type === 'Housing') {
        const hID = lastCalculatedData.housing[selectedIdx]?.housingID ?? null;
        return { ...base, housingID: hID };
    }
    return base;
}

// =============================================
//  Detail Modal — عرض Items السيناريو
// =============================================
async function openDetailModal(scenarioID, name, type) {
    currentDetailScenarioId = scenarioID;
    detailScenarioName.textContent = name;
    const typeLabel = type === 'Commute' ? 'مواصلات' : type === 'Housing' ? 'سكن' : 'سيارة';
    detailScenarioType.textContent = `نوع السيناريو: ${typeLabel}`;
    scenarioItemsList.innerHTML = '<p class="text-muted">جاري التحميل...</p>';
    noItemsMsg.classList.add('hidden');
    openModal(detailModal);
    await loadScenarioItems(scenarioID);
}

async function loadScenarioItems(scenarioID) {
    try {
        const res = await fetch(`${API_BASE}/Scenarios/${scenarioID}`, {
            headers: { 'X-Device-Id': getDeviceId() }
        });
        if (!res.ok) { scenarioItemsList.innerHTML = '<p class="text-danger">فشل في تحميل التفاصيل.</p>'; return; }

        const data = await res.json();
        renderScenarioItems(data.items || []);
    } catch {
        scenarioItemsList.innerHTML = '<p class="text-danger">خطأ في الاتصال.</p>';
    }
}

function renderScenarioItems(items) {
    if (!items.length) {
        scenarioItemsList.innerHTML = '';
        noItemsMsg.classList.remove('hidden');
        return;
    }

    noItemsMsg.classList.add('hidden');
    scenarioItemsList.innerHTML = items.map(item => {
        const typeIcon = item.itemType === 'Commute'  ? 'fa-van-shuttle'
                       : item.itemType === 'Housing'  ? 'fa-house' : 'fa-car';
        const subInfo  = item.itemType === 'Commute'  ? `${item.fromCityName || ''} ← ${item.toCityName || ''} · ${item.transportTypeName || ''}`
                       : item.itemType === 'Housing'  ? `${item.housingType || ''} — إيجار ${item.monthlyRent || ''} ج.م`
                       : `${item.fromCityName || ''} ← ${item.toCityName || ''} · ${item.fuelType || ''} · ${item.passengerCount || 1} ركاب`;
        const recBadge = item.isRecommended ? '<span class="rec-badge"><i class="fa-solid fa-star"></i> الأوفر</span>' : '';

        return `
        <div class="item-row ${item.isRecommended ? 'recommended' : ''}">
            <div class="item-row-info">
                <div class="item-row-name">
                    <i class="fa-solid ${typeIcon}" style="color:var(--primary);"></i>
                    ${item.itemName}
                    ${recBadge}
                </div>
                <div class="item-row-sub">${subInfo}</div>
            </div>
            <div class="item-row-cost">
                ${item.monthlyCostSnapshot ? item.monthlyCostSnapshot + ' ج.م' : '—'}
            </div>
            <button class="btn-delete-item" onclick="deleteScenarioItem(${item.itemID})">
                <i class="fa-solid fa-trash"></i>
            </button>
        </div>`;
    }).join('');
}

// Delete Item
async function deleteScenarioItem(itemID) {
    if (!confirm('هل تريد حذف هذا الخيار؟')) return;
    try {
        const res = await fetch(`${API_BASE}/Scenarios/${currentDetailScenarioId}/items/${itemID}`, {
            method: 'DELETE',
            headers: { 'X-Device-Id': getDeviceId() }
        });
        if (res.ok) {
            showToast('تم حذف الخيار بنجاح');
            await loadScenarioItems(currentDetailScenarioId);
        } else {
            showToast('فشل في حذف الخيار', 'error');
        }
    } catch {
        showToast('خطأ في الاتصال', 'error');
    }
}

// Add Item Button — يضيف خيار من البيانات المحسوبة الحالية
addItemToScenarioBtn.addEventListener('click', async () => {
    if (!lastCalculatedData.routes.length && !lastCalculatedData.car && !lastCalculatedData.housing.length) {
        showToast('احسب التكلفة أولاً لإضافة خيار', 'error'); return;
    }

    const itemName = prompt('اسم الخيار الجديد (مثال: قطر / سيارتي / سكن قريب):');
    if (!itemName?.trim()) return;

    // نسأل عن النوع
    const typeChoice = prompt('نوع الخيار:\n1 = Commute (مواصلات)\n2 = CarTrip (سيارة)\n3 = Housing (سكن)\nادخل 1 أو 2 أو 3:');
    const typeMap = { '1': 'Commute', '2': 'CarTrip', '3': 'Housing' };
    const type = typeMap[typeChoice?.trim()];
    if (!type) { showToast('اختيار غير صحيح', 'error'); return; }

    let cost = 0;
    if (type === 'Commute' && lastCalculatedData.routes.length) cost = lastCalculatedData.routes[0].monthlyCost;
    else if (type === 'CarTrip' && lastCalculatedData.car) cost = lastCalculatedData.car.monthlyCostPerPerson || lastCalculatedData.car.monthlyTotalCost;
    else if (type === 'Housing' && lastCalculatedData.housing.length) cost = lastCalculatedData.housing[0].monthlyRent;

    if (!cost) { showToast('لا توجد تكلفة محسوبة لهذا النوع', 'error'); return; }

    const payload = buildItemPayload(type, itemName.trim(), currentDetailScenarioId, cost);

    try {
        const res = await fetch(`${API_BASE}/Scenarios/${currentDetailScenarioId}/items`, {
            method: 'POST',
            headers: { 'X-Device-Id': getDeviceId(), 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        if (res.ok) {
            showToast('تم إضافة الخيار بنجاح');
            await loadScenarioItems(currentDetailScenarioId);
        } else {
            showToast('فشل في إضافة الخيار. تأكد من الحقول المطلوبة.', 'error');
        }
    } catch {
        showToast('خطأ في الاتصال', 'error');
    }
});

closeDetailModalBtn.addEventListener('click', () => closeModal(detailModal));

// =============================================
//  Modal Helpers
// =============================================
function openModal(modal)  { modal.classList.add('active'); }
function closeModal(modal) { modal.classList.remove('active'); }

// إغلاق عند الضغط خارج الـ Modal
[scenarioModal, detailModal].forEach(modal => {
    modal.addEventListener('click', (e) => {
        if (e.target === modal) closeModal(modal);
    });
});
