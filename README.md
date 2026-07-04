# حاسبة تكاليف السفر والسكن

تطبيق ويب لمقارنة تكاليف السفر اليومي بين المدن مقابل تكاليف السكن، مع دعم السيناريوهات لتوثيق المقارنات.

## المتطلبات

- .NET 8 SDK
- SQL Server (2019 أو أحدث)
- متصفح حديث

## هيكلة المشروع

```
Travel Calculator project/
├── Frontend/                          # واجهة المستخدم (HTML/CSS/JS)
│   ├── index.html                     # الصفحة الرئيسية
│   ├── styles.css                     # التصميم (Glassmorphism)
│   └── app.js                         # منطق الواجهة
├── Travel_Calculator/                 # Web API (ASP.NET Core 8)
│   ├── Controllers/                   # 5 API Controllers
│   │   ├── CitiesAPIController.cs
│   │   ├── RoutesAPIController.cs
│   │   ├── HousingAPIController.cs
│   │   ├── FuelAPIController.cs
│   │   └── ScenariosAPIController.cs
│   └── Program.cs                     # نقطة الدخول (CORS, Swagger)
├── TravelCalculatorBussinessLayer/    # BLL (Business Logic Layer)
│   ├── clsCity.cs
│   ├── clsRoute.cs
│   ├── clsHousing.cs
│   ├── clsFuel.cs
│   └── clsScenario.cs                 # clsScenario + clsScenarioItem
├── TravelCalculatorDataLayer/         # DAL (Data Access Layer)
│   ├── clsDataAccessSettings.cs       # Connection String
│   ├── clsCityData.cs                 # CityDTO
│   ├── clsRouteData.cs                # RouteDTO, RouteDetailsDTO, TravelCostDTO
│   ├── clsHousingData.cs              # HousingDTO, HousingComparisonDTO
│   ├── clsFuelData.cs                 # FuelPriceDTO, CarFuelSplitDTO, CarFuelCostDTO
│   └── clsScenarioData.cs             # ScenarioDTO, ScenarioItemDTO, ScenarioItemDetailsDTO, ScenarioWithItemsDTO
├── Database_SPs.sql                   # جميع Stored Procedures (28 SP) — لل rebuild
├── Fix_AddScenario.sql                # إصلاح SP_AddScenario
├── Fix_Missing_IDs.sql                # إصلاح SP_CalculateTravelCost + SP_CompareCommutingVsHousing
├── README.md
├── PROJECT_MAP.md
└── API_DOCUMENTATION.md
```

## التشغيل

### . API
```bash
cd Travel_Calculator
dotnet run
# يستمع على: https://localhost:7275
```

### . Frontend
افتح `Frontend/index.html` في المتصفح مباشرة أو قدّمه عبر أي HTTP server.

## سير العمل الأساسي

1. اختر مدينتين (انطلاق → وجهة)
2. حدد أيام الدوام وبيانات السيارة (اختياري)
3. اضغط "احسب التكلفة" — تظهر المواصلات، السيارة، السكن
4. احفظ كـ "سيناريو" للمقارنة لاحقاً
