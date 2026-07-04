using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace DataAccessLayer
{
    public class FuelPriceDTO
    {
        public int      FuelPriceID   { get; set; }
        public string   FuelType      { get; set; }
        public decimal  PricePerLiter { get; set; }
        public DateTime UpdatedAt     { get; set; }

        public FuelPriceDTO(int fuelPriceID, string fuelType, decimal pricePerLiter, DateTime updatedAt)
        {
            FuelPriceID = fuelPriceID; FuelType = fuelType;
            PricePerLiter = pricePerLiter; UpdatedAt = updatedAt;
        }
    }

    // =============================================
    // CarFuelSplitDTO (تكلفة العربية مع قسمة بين الركاب)
    // =============================================
    public class CarFuelSplitDTO
    {
        public decimal OneWayKm              { get; set; }
        public decimal DailyRoundTripKm      { get; set; }
        public string  FuelType              { get; set; }
        public decimal PricePerLiter         { get; set; }
        public decimal MaintenanceCostPerKm  { get; set; }
        public decimal ConsumptionPer100Km   { get; set; }
        public decimal DailyFuelLiters       { get; set; }
        public decimal DailyFuelCost         { get; set; }
        public decimal DailyMaintenanceCost  { get; set; }
        public decimal DailyTotalCost        { get; set; }
        public decimal WeeklyTotalCost       { get; set; }
        public decimal MonthlyTotalCost      { get; set; }
        public int     PassengerCount        { get; set; }
        public decimal DailyCostPerPerson    { get; set; }
        public decimal WeeklyCostPerPerson   { get; set; }
        public decimal MonthlyCostPerPerson  { get; set; }
    }

    public class CarFuelCostDTO
    {
        public decimal OneWayKm          { get; set; }
        public decimal DailyRoundTripKm  { get; set; }
        public string  FuelType          { get; set; }
        public decimal PricePerLiter     { get; set; }
        public decimal ConsumptionPer100 { get; set; }
        public decimal DailyFuelLiters   { get; set; }
        public decimal DailyFuelCost     { get; set; }
        public decimal WeeklyFuelCost    { get; set; }
        public decimal MonthlyFuelCost   { get; set; }

        public CarFuelCostDTO(decimal oneWayKm, decimal dailyRoundTripKm, string fuelType,
                              decimal pricePerLiter, decimal consumptionPer100, decimal dailyFuelLiters,
                              decimal dailyFuelCost, decimal weeklyFuelCost, decimal monthlyFuelCost)
        {
            OneWayKm = oneWayKm; DailyRoundTripKm = dailyRoundTripKm; FuelType = fuelType;
            PricePerLiter = pricePerLiter; ConsumptionPer100 = consumptionPer100;
            DailyFuelLiters = dailyFuelLiters; DailyFuelCost = dailyFuelCost;
            WeeklyFuelCost = weeklyFuelCost; MonthlyFuelCost = monthlyFuelCost;
        }
    }

    public class clsFuelData
    {
        public static List<FuelPriceDTO> GetFuelPrices()
        {
            var list = new List<FuelPriceDTO>();
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_GetFuelPrices", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        list.Add(new FuelPriceDTO(
                            reader.GetInt32(reader.GetOrdinal("FuelPriceID")),
                            reader.GetString(reader.GetOrdinal("FuelType")),
                            reader.GetDecimal(reader.GetOrdinal("PricePerLiter")),
                            reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))));
            }
            return list;
        }

        public static CarFuelCostDTO CalculateCarFuelCost(int fromCityID, int toCityID, string fuelType,
                                                           decimal consumptionPer100, int daysPerWeek, int weeksPerMonth)
        {
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_CalculateCarFuelCost", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FromCityID",        fromCityID);
                cmd.Parameters.AddWithValue("@ToCityID",          toCityID);
                cmd.Parameters.AddWithValue("@FuelType",          fuelType);
                cmd.Parameters.AddWithValue("@ConsumptionPer100", consumptionPer100);
                cmd.Parameters.AddWithValue("@DaysPerWeek",       daysPerWeek);
                cmd.Parameters.AddWithValue("@WeeksPerMonth",     weeksPerMonth);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    if (reader.Read())
                    {
                        int colOneWayKm          = reader.GetOrdinal("OneWayKm");
                        int colDailyRoundTripKm  = reader.GetOrdinal("DailyRoundTripKm");
                        int colFuelType          = reader.GetOrdinal("FuelType");
                        int colPricePerLiter     = reader.GetOrdinal("PricePerLiter");
                        int colConsumption       = reader.GetOrdinal("ConsumptionPer100Km");
                        int colDailyFuelLiters   = reader.GetOrdinal("DailyFuelLiters");
                        int colDailyFuelCost     = reader.GetOrdinal("DailyFuelCost");
                        int colWeeklyFuelCost    = reader.GetOrdinal("WeeklyFuelCost");
                        int colMonthlyFuelCost   = reader.GetOrdinal("MonthlyFuelCost");

                        return new CarFuelCostDTO(
                            reader.IsDBNull(colOneWayKm)         ? 0m    : reader.GetDecimal(colOneWayKm),
                            reader.IsDBNull(colDailyRoundTripKm) ? 0m    : reader.GetDecimal(colDailyRoundTripKm),
                            reader.IsDBNull(colFuelType)         ? ""    : reader.GetString(colFuelType),
                            reader.IsDBNull(colPricePerLiter)    ? 0m    : reader.GetDecimal(colPricePerLiter),
                            reader.IsDBNull(colConsumption)      ? 0m    : reader.GetDecimal(colConsumption),
                            reader.IsDBNull(colDailyFuelLiters)  ? 0m    : reader.GetDecimal(colDailyFuelLiters),
                            reader.IsDBNull(colDailyFuelCost)    ? 0m    : reader.GetDecimal(colDailyFuelCost),
                            reader.IsDBNull(colWeeklyFuelCost)   ? 0m    : reader.GetDecimal(colWeeklyFuelCost),
                            reader.IsDBNull(colMonthlyFuelCost)  ? 0m    : reader.GetDecimal(colMonthlyFuelCost));
                    }
            }
            return null;
        }

        public static CarFuelSplitDTO CalculateCarFuelCostSplit(
            int fromCityID, int toCityID, string fuelType, decimal consumptionPer100,
            int daysPerWeek, int weeksPerMonth, int passengerCount)
        {
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_CalculateCarFuelCostSplit", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FromCityID",        fromCityID);
                cmd.Parameters.AddWithValue("@ToCityID",          toCityID);
                cmd.Parameters.AddWithValue("@FuelType",          fuelType);
                cmd.Parameters.AddWithValue("@ConsumptionPer100", consumptionPer100);
                cmd.Parameters.AddWithValue("@DaysPerWeek",       daysPerWeek);
                cmd.Parameters.AddWithValue("@WeeksPerMonth",     weeksPerMonth);
                cmd.Parameters.AddWithValue("@PassengerCount",    passengerCount);
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new CarFuelSplitDTO
                        {
                            OneWayKm             = reader.GetDecimal(reader.GetOrdinal("OneWayKm")),
                            DailyRoundTripKm     = reader.GetDecimal(reader.GetOrdinal("DailyRoundTripKm")),
                            FuelType             = reader.GetString(reader.GetOrdinal("FuelType")),
                            PricePerLiter        = reader.GetDecimal(reader.GetOrdinal("PricePerLiter")),
                            MaintenanceCostPerKm = reader.GetDecimal(reader.GetOrdinal("MaintenanceCostPerKm")),
                            ConsumptionPer100Km  = reader.GetDecimal(reader.GetOrdinal("ConsumptionPer100Km")),
                            DailyFuelLiters      = reader.GetDecimal(reader.GetOrdinal("DailyFuelLiters")),
                            DailyFuelCost        = reader.GetDecimal(reader.GetOrdinal("DailyFuelCost")),
                            DailyMaintenanceCost = reader.GetDecimal(reader.GetOrdinal("DailyMaintenanceCost")),
                            DailyTotalCost       = reader.GetDecimal(reader.GetOrdinal("DailyTotalCost")),
                            WeeklyTotalCost      = reader.GetDecimal(reader.GetOrdinal("WeeklyTotalCost")),
                            MonthlyTotalCost     = reader.GetDecimal(reader.GetOrdinal("MonthlyTotalCost")),
                            PassengerCount       = reader.GetInt32(reader.GetOrdinal("PassengerCount")),
                            DailyCostPerPerson   = reader.GetDecimal(reader.GetOrdinal("DailyCostPerPerson")),
                            WeeklyCostPerPerson  = reader.GetDecimal(reader.GetOrdinal("WeeklyCostPerPerson")),
                            MonthlyCostPerPerson = reader.GetDecimal(reader.GetOrdinal("MonthlyCostPerPerson"))
                        };
                    }
                }
            }
            return null;
        }
    }
}
