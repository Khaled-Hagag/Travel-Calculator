using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace DataAccessLayer
{
    // =============================================
    // DTOs
    // =============================================

    // رأس السيناريو (بدون items)
    public class ScenarioDTO
    {
        public int      ScenarioID    { get; set; }
        public string   DeviceID      { get; set; }
        public string   ScenarioName  { get; set; }
        public string   ScenarioType  { get; set; }
        public DateTime CreatedAt     { get; set; }

        public ScenarioDTO() { }

        public ScenarioDTO(int scenarioID, string deviceID, string scenarioName,
                           string scenarioType, DateTime createdAt)
        {
            ScenarioID   = scenarioID;
            DeviceID     = deviceID;
            ScenarioName = scenarioName;
            ScenarioType = scenarioType;
            CreatedAt    = createdAt;
        }
    }

    // خيار واحد جوه السيناريو (للحفظ)
    public class ScenarioItemDTO
    {
        public int      ItemID              { get; set; }
        public int      ScenarioID          { get; set; }
        public string   ItemName            { get; set; }
        public string   ItemType            { get; set; }   // Commute / Housing / CarTrip

        // Commute
        public int?     FromCityID          { get; set; }
        public int?     ToCityID            { get; set; }
        public int?     TransportTypeID     { get; set; }
        public int?     DaysPerWeek         { get; set; }
        public int?     WeeksPerMonth       { get; set; }

        // Housing
        public int?     HousingID           { get; set; }

        // CarTrip
        public int?     PassengerCount      { get; set; }
        public string? FuelType { get; set; }
        public decimal? ConsumptionPer100   { get; set; }

        // النتيجة
        public decimal? MonthlyCostSnapshot { get; set; }
        public bool     IsRecommended       { get; set; }

        public ScenarioItemDTO() { }
    }

    // خيار واحد مع الأسماء (للعرض)
    public class ScenarioItemDetailsDTO
    {
        public int      ItemID              { get; set; }
        public string   ItemName            { get; set; }
        public string   ItemType            { get; set; }
        public decimal? MonthlyCostSnapshot { get; set; }
        public bool     IsRecommended       { get; set; }

        // Commute
        public string   FromCityName        { get; set; }
        public string   ToCityName          { get; set; }
        public string   TransportTypeName   { get; set; }
        public int?     DaysPerWeek         { get; set; }
        public int?     WeeksPerMonth       { get; set; }

        // Housing
        public string   HousingType         { get; set; }
        public decimal? MonthlyRent         { get; set; }

        // CarTrip
        public int?     PassengerCount      { get; set; }
        public string? FuelType { get; set; }
        public decimal? ConsumptionPer100   { get; set; }
    }

    // السيناريو كامل مع كل خياراته (للعرض)
    public class ScenarioWithItemsDTO
    {
        public int                          ScenarioID    { get; set; }
        public string                       ScenarioName  { get; set; }
        public string                       ScenarioType  { get; set; }
        public DateTime                     CreatedAt     { get; set; }
        public List<ScenarioItemDetailsDTO> Items         { get; set; } = new List<ScenarioItemDetailsDTO>();
    }

    // =============================================
    // clsScenarioData  →  DAL
    // =============================================
    public class clsScenarioData
    {
        // ---- Scenarios (الرأس) ----

        public static int AddScenario(ScenarioDTO dto)
        {
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_AddScenario", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@DeviceID",     dto.DeviceID);
                cmd.Parameters.AddWithValue("@ScenarioName", dto.ScenarioName);
                cmd.Parameters.AddWithValue("@ScenarioType", dto.ScenarioType);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return (result != null && int.TryParse(result.ToString(), out int id)) ? id : -1;
            }
        }

        public static List<ScenarioDTO> GetScenariosByDevice(string deviceID)
        {
            var list = new List<ScenarioDTO>();
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_GetScenariosByDevice", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@DeviceID", deviceID);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        list.Add(new ScenarioDTO(
                            reader.GetInt32(reader.GetOrdinal("ScenarioID")),
                            deviceID,
                            reader.GetString(reader.GetOrdinal("ScenarioName")),
                            reader.GetString(reader.GetOrdinal("ScenarioType")),
                            reader.GetDateTime(reader.GetOrdinal("CreatedAt"))));
            }
            return list;
        }

        public static bool DeleteScenario(int scenarioID, string deviceID)
        {
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_DeleteScenario", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ScenarioID", scenarioID);
                cmd.Parameters.AddWithValue("@DeviceID",   deviceID);
                conn.Open();
                // SP_DeleteScenario uses SET NOCOUNT OFF so ExecuteNonQuery returns correct row count
                int rows = cmd.ExecuteNonQuery();
                return (rows > 0);
            }
        }

        public static bool RenameScenario(int scenarioID, string deviceID, string newName)
        {
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_RenameScenario", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ScenarioID",   scenarioID);
                cmd.Parameters.AddWithValue("@DeviceID",     deviceID);
                cmd.Parameters.AddWithValue("@ScenarioName", newName);
                conn.Open();
                return (cmd.ExecuteNonQuery() > 0);
            }
        }

        // ---- ScenarioItems (الخيارات) ----

        public static int AddScenarioItem(ScenarioItemDTO dto)
        {
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_AddScenarioItem", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ScenarioID",          dto.ScenarioID);
                cmd.Parameters.AddWithValue("@ItemName",             dto.ItemName);
                cmd.Parameters.AddWithValue("@ItemType",             dto.ItemType);
                cmd.Parameters.AddWithValue("@FromCityID",           (object)dto.FromCityID       ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ToCityID",             (object)dto.ToCityID         ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TransportTypeID",      (object)dto.TransportTypeID  ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DaysPerWeek",          (object)dto.DaysPerWeek      ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@WeeksPerMonth",        (object)(dto.WeeksPerMonth   ?? 4));
                cmd.Parameters.AddWithValue("@HousingID",            (object)dto.HousingID        ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PassengerCount",       (object)(dto.PassengerCount  ?? 1));
                cmd.Parameters.AddWithValue("@FuelType",             (object)dto.FuelType         ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ConsumptionPer100",    (object)dto.ConsumptionPer100 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MonthlyCostSnapshot",  (object)dto.MonthlyCostSnapshot ?? DBNull.Value);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return (result != null && int.TryParse(result.ToString(), out int id)) ? id : -1;
            }
        }

        public static ScenarioWithItemsDTO GetScenarioWithItems(int scenarioID, string deviceID)
        {
            ScenarioWithItemsDTO scenario = null;

            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_GetScenarioWithItems", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ScenarioID", scenarioID);
                cmd.Parameters.AddWithValue("@DeviceID",   deviceID);
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    // Result Set 1: رأس السيناريو
                    if (reader.Read())
                    {
                        scenario = new ScenarioWithItemsDTO
                        {
                            ScenarioID   = reader.GetInt32(reader.GetOrdinal("ScenarioID")),
                            ScenarioName = reader.GetString(reader.GetOrdinal("ScenarioName")),
                            ScenarioType = reader.GetString(reader.GetOrdinal("ScenarioType")),
                            CreatedAt    = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                        };
                    }

                    if (scenario == null) return null;

                    // Result Set 2: الخيارات
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            scenario.Items.Add(new ScenarioItemDetailsDTO
                            {
                                ItemID              = reader.GetInt32(reader.GetOrdinal("ItemID")),
                                ItemName            = reader.GetString(reader.GetOrdinal("ItemName")),
                                ItemType            = reader.GetString(reader.GetOrdinal("ItemType")),
                                MonthlyCostSnapshot = reader.IsDBNull(reader.GetOrdinal("MonthlyCostSnapshot")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("MonthlyCostSnapshot")),
                                IsRecommended       = reader.GetBoolean(reader.GetOrdinal("IsRecommended")),
                                FromCityName        = reader.IsDBNull(reader.GetOrdinal("FromCityName"))      ? null : reader.GetString(reader.GetOrdinal("FromCityName")),
                                ToCityName          = reader.IsDBNull(reader.GetOrdinal("ToCityName"))        ? null : reader.GetString(reader.GetOrdinal("ToCityName")),
                                TransportTypeName   = reader.IsDBNull(reader.GetOrdinal("TransportTypeName")) ? null : reader.GetString(reader.GetOrdinal("TransportTypeName")),
                                DaysPerWeek         = reader.IsDBNull(reader.GetOrdinal("DaysPerWeek"))       ? (int?)null : reader.GetInt32(reader.GetOrdinal("DaysPerWeek")),
                                WeeksPerMonth       = reader.IsDBNull(reader.GetOrdinal("WeeksPerMonth"))     ? (int?)null : reader.GetInt32(reader.GetOrdinal("WeeksPerMonth")),
                                HousingType         = reader.IsDBNull(reader.GetOrdinal("HousingType"))       ? null : reader.GetString(reader.GetOrdinal("HousingType")),
                                MonthlyRent         = reader.IsDBNull(reader.GetOrdinal("MonthlyRent"))       ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("MonthlyRent")),
                                PassengerCount      = reader.IsDBNull(reader.GetOrdinal("PassengerCount"))    ? (int?)null : reader.GetInt32(reader.GetOrdinal("PassengerCount")),
                                FuelType            = reader.IsDBNull(reader.GetOrdinal("FuelType"))          ? null : reader.GetString(reader.GetOrdinal("FuelType")),
                                ConsumptionPer100   = reader.IsDBNull(reader.GetOrdinal("ConsumptionPer100")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("ConsumptionPer100"))
                            });
                        }
                    }
                }
            }
            return scenario;
        }

        public static bool DeleteScenarioItem(int itemID, int scenarioID, string deviceID)
        {
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_DeleteScenarioItem", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ItemID",     itemID);
                cmd.Parameters.AddWithValue("@ScenarioID", scenarioID);
                cmd.Parameters.AddWithValue("@DeviceID",   deviceID);
                conn.Open();
                // SP returns SELECT @DeletedRows so we use ExecuteScalar
                object result = cmd.ExecuteScalar();
                return (result != null && int.TryParse(result.ToString(), out int deleted) && deleted > 0);
            }
        }
    }
}
