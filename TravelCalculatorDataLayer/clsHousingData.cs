using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace DataAccessLayer
{
    public class HousingDTO
    {
        public int     HousingID    { get; set; }
        public int     CityID       { get; set; }
        public string  HousingType  { get; set; }
        public decimal MonthlyRent  { get; set; }
        public string  Description  { get; set; }
        public bool    IsActive     { get; set; }

        public HousingDTO(int housingID, int cityID, string housingType,
                          decimal monthlyRent, string description, bool isActive)
        {
            HousingID = housingID; CityID = cityID; HousingType = housingType;
            MonthlyRent = monthlyRent; Description = description; IsActive = isActive;
        }
    }

    public class HousingComparisonDTO
    {
        public int     HousingID              { get; set; }
        public string  HousingType            { get; set; }
        public decimal MonthlyRent            { get; set; }
        public decimal CheapestMonthlyCommute { get; set; }
        public decimal Difference             { get; set; }
        public string  Recommendation        { get; set; }

        public HousingComparisonDTO(int housingID, string housingType, decimal monthlyRent,
                                    decimal cheapestMonthlyCommute, decimal difference, string recommendation)
        {
            HousingID = housingID; HousingType = housingType; MonthlyRent = monthlyRent;
            CheapestMonthlyCommute = cheapestMonthlyCommute;
            Difference = difference; Recommendation = recommendation;
        }
    }

    public class clsHousingData
    {
        public static List<HousingDTO> GetHousingByCityID(int cityID)
        {
            var list = new List<HousingDTO>();
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_GetHousingOptions", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CityID", cityID);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        list.Add(new HousingDTO(
                            reader.GetInt32(reader.GetOrdinal("HousingID")),
                            reader.GetInt32(reader.GetOrdinal("CityID")),
                            reader.GetString(reader.GetOrdinal("HousingType")),
                            reader.GetDecimal(reader.GetOrdinal("MonthlyRent")),
                            reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString(reader.GetOrdinal("Description")),
                            reader.GetBoolean(reader.GetOrdinal("IsActive"))));
            }
            return list;
        }

        public static HousingDTO GetHousingByID(int housingID)
        {
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_GetHousingByID", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@HousingID", housingID);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    if (reader.Read())
                        return new HousingDTO(
                            reader.GetInt32(reader.GetOrdinal("HousingID")),
                            reader.GetInt32(reader.GetOrdinal("CityID")),
                            reader.GetString(reader.GetOrdinal("HousingType")),
                            reader.GetDecimal(reader.GetOrdinal("MonthlyRent")),
                            reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString(reader.GetOrdinal("Description")),
                            reader.GetBoolean(reader.GetOrdinal("IsActive")));
            }
            return null;
        }

        public static List<HousingComparisonDTO> CompareHousingVsCommuting(int fromCityID, int toCityID,
                                                                             int daysPerWeek, int weeksPerMonth)
        {
            var list = new List<HousingComparisonDTO>();
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_CompareCommutingVsHousing", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FromCityID",    fromCityID);
                cmd.Parameters.AddWithValue("@ToCityID",      toCityID);
                cmd.Parameters.AddWithValue("@DaysPerWeek",   daysPerWeek);
                cmd.Parameters.AddWithValue("@WeeksPerMonth", weeksPerMonth);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        list.Add(new HousingComparisonDTO(
                            reader.GetInt32(reader.GetOrdinal("HousingID")),
                            reader.GetString(reader.GetOrdinal("HousingType")),
                            reader.GetDecimal(reader.GetOrdinal("MonthlyRent")),
                            reader.GetDecimal(reader.GetOrdinal("CheapestMonthlyCommute")),
                            reader.GetDecimal(reader.GetOrdinal("Difference")),
                            reader.GetString(reader.GetOrdinal("Recommendation"))));
            }
            return list;
        }

        public static int AddNewHousing(HousingDTO dto)
        {
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_AddNewHousing", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CityID",      dto.CityID);
                cmd.Parameters.AddWithValue("@HousingType", dto.HousingType);
                cmd.Parameters.AddWithValue("@MonthlyRent", dto.MonthlyRent);
                cmd.Parameters.AddWithValue("@Description", dto.Description ?? "");
                cmd.Parameters.AddWithValue("@IsActive",    dto.IsActive);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return (result != null && int.TryParse(result.ToString(), out int id)) ? id : -1;
            }
        }

        public static bool UpdateHousing(HousingDTO dto)
        {
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_UpdateHousing", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@HousingID",   dto.HousingID);
                cmd.Parameters.AddWithValue("@CityID",      dto.CityID);
                cmd.Parameters.AddWithValue("@HousingType", dto.HousingType);
                cmd.Parameters.AddWithValue("@MonthlyRent", dto.MonthlyRent);
                cmd.Parameters.AddWithValue("@Description", dto.Description ?? "");
                cmd.Parameters.AddWithValue("@IsActive",    dto.IsActive);
                conn.Open();
                return (cmd.ExecuteNonQuery() > 0);
            }
        }

        public static bool DeleteHousing(int housingID)
        {
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_DeleteHousing", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@HousingID", housingID);
                conn.Open();
                return (cmd.ExecuteNonQuery() > 0);
            }
        }
    }
}
