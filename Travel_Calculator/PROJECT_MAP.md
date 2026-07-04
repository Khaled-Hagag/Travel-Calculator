# PROJECT MAP — حاسبة تكاليف السفر والسكن

---

## TECH STACK

| الطبقة | التقنية |
|--------|---------|
| Frontend | HTML5 + CSS3 (Glassmorphism, Custom Properties) + Vanilla JS (ES6) |
| Web API | ASP.NET Core 8 (Minimal Controllers, System.Text.Json) |
| BLL | .NET 8 Class Library — منطق الأعمال والتحقق |
| DAL | .NET 8 Class Library — Microsoft.Data.SqlClient |
| Database | SQL Server (2019+) — Stored Procedures |
| Fonts | Cairo (Google Fonts) |
| Icons | Font Awesome 6.4 |
| Auth | Device ID عبر header مخصص (`X-Device-Id`) |

---

## ARCHITECTURE

```
┌─────────────────────────────────────────────────────────────┐
│                    Frontend (index.html)                     │
│  app.js  ←→  fetch(API_BASE)  ←→  HTML + CSS Rendering     │
└──────────────┬──────────────────────────────────────────────┘
               │ HTTP / JSON (REST)
               ▼
┌─────────────────────────────────────────────────────────────┐
│          Web API — Travel_Calculator (ASP.NET Core 8)       │
│  ┌────────────┐ ┌──────────┐ ┌───────────┐ ┌────────────┐  │
│  │ CitiesAPI  │ │ RoutesAPI│ │ HousingAPI│ │ScenariosAPI│  │
│  │ FuelAPI    │ │          │ │           │ │            │  │
│  └─────┬──────┘ └────┬─────┘ └─────┬─────┘ └──────┬─────┘  │
└────────┼──────────────┼─────────────┼──────────────┼────────┘
         │              │             │              │
         ▼              ▼             ▼              ▼
┌─────────────────────────────────────────────────────────────┐
│      BLL — TravelCalculatorBussinessLayer (.NET 8)          │
│  clsCity  clsRoute  clsHousing  clsFuel  clsScenario       │
│  clsScenarioItem                                            │
│  (Business Rules Validation + Delegation to DAL)            │
└──────────┬──────────────┬─────────────┬──────────────────────┘
           │              │             │
           ▼              ▼             ▼
┌─────────────────────────────────────────────────────────────┐
│      DAL — TravelCalculatorDataLayer (.NET 8)               │
│  clsCityData  clsRouteData  clsHousingData                 │
│  clsFuelData  clsScenarioData                               │
│  (DTOs + SqlCommand → Stored Procedures)                    │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
               ┌─────────────────────┐
               │    SQL Server DB     │
               │  TravelCalculatorDB │
               └─────────────────────┘
```

### Data Flow

1. المستخدم يتفاعل مع الـ DOM (select, input, click)
2. `app.js` يبني fetch call إلى API endpoint
3. API Controller يستقبل request، يستدعي BLL method
4. BLL يتحقق من صحة البيانات، يستدعي DAL method
5. DAL يشغّل Stored Procedure عبر SqlCommand
6. SP يُرجع Result Set ← DAL يقرأها ← يبني DTO list ← يرجع لـ BLL
7. BLL يرجع لـ Controller ← يرجع JSON للـ Frontend
8. `app.js` يستلم JSON ← يبني HTML ديناميكي ويعرضه

---

## SYSTEM FLOW

```
[فتح الصفحة]
    │
    ├── loadCities()         ← GET /api/Cities/All
    ├── loadFuelTypes()      ← GET /api/Fuel/Prices
    └── loadScenarios()      ← GET /api/Scenarios
         │
         ▼
[المستخدم يختار مدينتين + بيانات]
    │
    ▼
[calculateBtn click]
    │
    ├── GET /api/Routes/Calculate?fromCityID=..&toCityID=..&daysPerWeek=..&weeksPerMonth=..
    ├── GET /api/Fuel/CalculateSplit?fromCityID=..&toCityID=..&fuelType=..&consumptionPer100=..&daysPerWeek=..&weeksPerMonth=..&passengerCount=..
    └── GET /api/Housing/Compare?fromCityID=..&toCityID=..&daysPerWeek=..&weeksPerMonth=..
         │
         ▼
    [عرض النتائج: displayRoutes / displayCarCost / displayHousing]
         │
         ▼
[حفظ كسيناريو]
    │
    ├── POST /api/Scenarios          ← { name, type, deviceID }
    └── POST /api/Scenarios/{id}/items ← { itemName, itemType, transportTypeID, housingID, ... }
         │
         ▼
[سيناريوهاتي المحفوظة]
    │
    ├── GET /api/Scenarios/{id}      ← عرض التفاصيل مع الـ items
    ├── DELETE /api/Scenarios/{id}   ← حذف سيناريو
    ├── DELETE /api/Scenarios/{id}/items/{itemId} ← حذف خيار
    └── POST /api/Scenarios/{id}/items ← إضافة خيار لسيناريو موجود
```

---

## ORPHANS & PENDING

### 1. ✅ CSS — تم الإصلاح

- `.cards-grid .fade-in:nth-child(1-5)` — تم حذف القواعد الميتة، وتم إضافة `.fade-in` مباشرة في JS للـ items المولدة مع `animation-delay` ديناميكي.
- `.data-display` — تم إضافة قواعد CSS (flex column).

### 2. ✅ JS — تم الإصلاح

- `carExtrasRow` — تم حذف المتغير الميت من `app.js`.

### 3. ✅ Template Files — تم الحذف

| الملف | الإجراء |
|-------|---------|
| `Travel_Calculator/WeatherForecast.cs` | ✅ تم الحذف |
| `Travel_Calculator/Controllers/WeatherForecastController.cs` | ✅ تم الحذف |
| `Travel_Calculator/Travel_Calculator.http` | ✅ تم الحذف |

### 4. ✅ Stored Procedures — SQL Scripts جاهزة

تم استخراج جميع الـ SPs من قاعدة البيانات إلى `Database_SPs.sql` (28 SP، 21KB).

### 5. API Endpoints Not Consumed by Frontend (17)

| الكونتورولر | الـ Endpoint | عدد الـ Actions |
|------------|-------------|-----------------|
| CitiesAPIController | GET/POST/PUT/DELETE `api/Cities` | 4 |
| FuelAPIController | GET `api/Fuel/Calculate` | 1 |
| RoutesAPIController | GET/POST/PUT/DELETE `api/Routes` | 6 |
| HousingAPIController | GET/POST/PUT/DELETE `api/Housing` | 5 |
| ScenariosAPIController | PUT `api/Scenarios/{id}/rename` | 1 |

### 6. ✅ Pending Improvements — تم الإنجاز

| الأولوية | الوصف | الحالة |
|----------|-------|--------|
| HIGH | تنظيف ملفات القالب (WeatherForecast.cs, WeatherForecastController.cs, Travel_Calculator.http) | ✅ |
| MEDIUM | إزالة `carExtrasRow` من app.js | ✅ |
| MEDIUM | إضافة `.fade-in` للـ items المولدة (routes, housing) ديناميكياً + حذف القواعد الميتة | ✅ |
| LOW | إضافة قواعد CSS لكلاس `.data-display` | ✅ |
| LOW | عمل Script لقاعدة البيانات لكل الـ SPs الناقصة | ✅ (`Database_SPs.sql`)
