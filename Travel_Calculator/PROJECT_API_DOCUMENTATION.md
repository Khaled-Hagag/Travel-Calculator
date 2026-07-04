# API Documentation — حاسبة تكاليف السفر والسكن

**Base URL:** `https://localhost:7275/api`

**Auth:** `X-Device-Id` header (required for Scenarios endpoints).

**Content-Type:** `application/json`

---

## Cities

### GET /api/Cities/All
جلب جميع المدن.

**Response 200:**
```json
[
  { "cityID": 1, "cityName": "القاهرة" },
  { "cityID": 2, "cityName": "الإسكندرية" }
]
```

### GET /api/Cities/{id}
جلب مدينة بواسطة ID.

**Response 200:**
```json
{ "cityID": 1, "cityName": "القاهرة" }
```

**Response 400:** `Invalid ID 0`
**Response 404:** `City with ID 99 not found.`

---

## Routes

### GET /api/Routes/All
جلب جميع خطوط النقل.

### GET /api/Routes/{id}
جلب خط نقل بواسطة ID.

### GET /api/Routes/Between?fromCityID=1&toCityID=2
جلب خطوط النقل بين مدينتين.

**Response 200:**
```json
[
  {
    "routeID": 1, "fromCity": "القاهرة", "toCity": "الإسكندرية",
    "transportType": "ميكروباص", "ticketPrice": 150.00,
    "durationMinutes": 180, "distanceKm": 220.00
  }
]
```

### GET /api/Routes/Calculate?fromCityID=1&toCityID=2&daysPerWeek=5&weeksPerMonth=4
حساب تكلفة المواصلات الشهرية.

**Response 200:**
```json
[
  {
    "transportTypeID": 3, "transportType": "ميكروباص",
    "oneWayPrice": 150.00, "dailyRoundTrip": 300.00,
    "weeklyCost": 1500.00, "monthlyCost": 6000.00,
    "durationMinutes": 180, "distanceKm": 220.00
  }
]
```

---

## Fuel

### GET /api/Fuel/Prices
جلب أسعار الوقود.

**Response 200:**
```json
[
  { "fuelType": "بنزين 80", "pricePerLiter": 12.50 },
  { "fuelType": "بنزين 92", "pricePerLiter": 15.00 }
]
```

### GET /api/Fuel/Calculate?fromCityID=1&toCityID=2&fuelType=بنزين 92&consumptionPer100=8&daysPerWeek=5&weeksPerMonth=4
حساب تكلفة وقود السيارة (بدون تقسيم الركاب).

### GET /api/Fuel/CalculateSplit?fromCityID=1&toCityID=2&fuelType=بنزين 92&consumptionPer100=8&daysPerWeek=5&weeksPerMonth=4&passengerCount=2
حساب تكلفة وقود السيارة مع تقسيم الركاب.

**Response 200:**
```json
{
  "dailyRoundTripKm": 440.00, "dailyFuelCost": 316.80,
  "dailyMaintenanceCost": 66.00, "dailyTotalCost": 382.80,
  "monthlyTotalCost": 7656.00, "passengerCount": 2,
  "monthlyCostPerPerson": 3828.00
}
```

---

## Housing

### GET /api/Housing/ByCity?cityID=2
جلب خيارات السكن لمدينة.

### GET /api/Housing/{id}
جلب خيار سكن بواسطة ID.

### GET /api/Housing/Compare?fromCityID=1&toCityID=2&daysPerWeek=5&weeksPerMonth=4
مقارنة السكن بالسفر الشهري.

**Response 200:**
```json
[
  {
    "housingID": 1, "housingType": "سكن جامعي",
    "monthlyRent": 1000.00, "cheapestMonthlyCommute": 2520.00,
    "difference": -1520.00, "recommendation": "السكن أوفر"
  }
]
```

**Response 400:** `Invalid city IDs.`

---

## Scenarios

**All Scenario endpoints require `X-Device-Id` header.**

### GET /api/Scenarios
جلب جميع سيناريوهات المستخدم (بدون items).

**Response 200:**
```json
[
  {
    "scenarioID": 1, "deviceID": "uuid-...",
    "scenarioName": "مشوار الجامعة",
    "scenarioType": "Commute",
    "createdAt": "2026-07-01T12:00:00"
  }
]
```

### GET /api/Scenarios/{id}
جلب سيناريو مع كل خياراته (items).

**Response 200:**
```json
{
  "scenarioID": 1, "scenarioName": "مشوار الجامعة",
  "scenarioType": "Commute", "createdAt": "2026-07-01T12:00:00",
  "items": [
    {
      "itemID": 1, "itemName": "قطار", "itemType": "Commute",
      "monthlyCostSnapshot": 1200.00, "isRecommended": false,
      "fromCityName": "القاهرة", "toCityName": "الجيزة",
      "transportTypeName": "قطار", "daysPerWeek": 5, "weeksPerMonth": 4
    }
  ]
}
```

### POST /api/Scenarios
إنشاء سيناريو جديد.

**Request:**
```json
{
  "scenarioID": 0,
  "deviceID": "uuid-...",
  "scenarioName": "مشوار الجامعة",
  "scenarioType": "Commute",
  "createdAt": "2026-07-01T12:00:00"
}
```

**Response 201:** السيناريو المُنشأ مع `scenarioID` حقيقي.

### PUT /api/Scenarios/{id}/rename
إعادة تسمية سيناريو.

**Request:** (plain text body)
```
"الاسم الجديد"
```

### DELETE /api/Scenarios/{id}
حذف سيناريو وكل items المرتبطة به (CASCADE).

---

## Scenario Items

### POST /api/Scenarios/{scenarioId}/items
إضافة خيار جديد لسيناريو موجود.

**Request (Commute):**
```json
{
  "scenarioID": 1, "itemName": "قطر", "itemType": "Commute",
  "fromCityID": 1, "toCityID": 2, "transportTypeID": 1,
  "daysPerWeek": 5, "weeksPerMonth": 4, "monthlyCostSnapshot": 1200.00
}
```

**Request (CarTrip):**
```json
{
  "scenarioID": 1, "itemName": "عربيتي", "itemType": "CarTrip",
  "fromCityID": 1, "toCityID": 2, "daysPerWeek": 5, "weeksPerMonth": 4,
  "fuelType": "بنزين 92", "consumptionPer100": 8, "passengerCount": 2,
  "monthlyCostSnapshot": 3828.00
}
```

**Request (Housing):**
```json
{
  "scenarioID": 1, "itemName": "سكن جامعي", "itemType": "Housing",
  "housingID": 3, "monthlyCostSnapshot": 1000.00
}
```

**Response 201:** الخيار المُنشأ مع `itemID` حقيقي.
**Response 400:** `Item could not be saved. Check required fields for the item type.`

### DELETE /api/Scenarios/{scenarioId}/items/{itemId}
حذف خيار واحد من السيناريو.

---

## Error Codes

| Status | المعنى |
|--------|--------|
| 200 | نجاح |
| 201 | تم الإنشاء |
| 400 | خطأ في البيانات المدخلة (missing/invalid fields) |
| 404 | غير موجود |
| 500 | خطأ في الخادم |
