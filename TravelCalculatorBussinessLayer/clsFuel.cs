using System.Collections.Generic;
using DataAccessLayer;

namespace BusinessLayer
{
    // =============================================
    // clsFuel
    // =============================================
    public partial class clsFuel
    {
        public static List<FuelPriceDTO> GetFuelPrices() => clsFuelData.GetFuelPrices();

        public static CarFuelCostDTO CalculateCarFuelCost(int fromCityID, int toCityID, string fuelType,
                                                           decimal consumptionPer100, int daysPerWeek, int weeksPerMonth = 4)
        {
            if (fromCityID == toCityID)                       return null;
            if (consumptionPer100 <= 0 || consumptionPer100 > 50) return null;
            if (daysPerWeek < 1 || daysPerWeek > 6)           return null;

            return clsFuelData.CalculateCarFuelCost(fromCityID, toCityID, fuelType,
                                                    consumptionPer100, daysPerWeek, weeksPerMonth);
        }

        public static CarFuelSplitDTO CalculateCarFuelCostSplit(
            int fromCityID, int toCityID, string fuelType, decimal consumptionPer100,
            int daysPerWeek, int weeksPerMonth = 4, int passengerCount = 1)
        {
            // Business Rules
            if (fromCityID == toCityID)                            return null;
            if (consumptionPer100 <= 0 || consumptionPer100 > 50) return null;
            if (daysPerWeek < 1 || daysPerWeek > 6)                return null;
            if (passengerCount < 1) passengerCount = 1;
            if (passengerCount > 8) passengerCount = 8;   // حد منطقي لعدد ركاب عربية

            return clsFuelData.CalculateCarFuelCostSplit(
                fromCityID, toCityID, fuelType, consumptionPer100,
                daysPerWeek, weeksPerMonth, passengerCount);
        }
    }
}