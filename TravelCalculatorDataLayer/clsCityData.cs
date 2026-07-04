using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace DataAccessLayer
{
    // =============================================
    // CityDTO
    // =============================================
    public class CityDTO
    {
        public int    CityID   { get; set; }
        public string CityName { get; set; }
        public bool   IsActive { get; set; }

        public CityDTO(int cityID, string cityName, bool isActive)
        {
            CityID = cityID; CityName = cityName; IsActive = isActive;
        }
    }

    // =============================================
    // clsCityData
    // =============================================
    public class clsCityData
    {
        public static List<CityDTO> GetAllCities()
        {
            var list = new List<CityDTO>();
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_GetAllCities", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        list.Add(new CityDTO(
                            reader.GetInt32(reader.GetOrdinal("CityID")),
                            reader.GetString(reader.GetOrdinal("CityName")),
                            reader.GetBoolean(reader.GetOrdinal("IsActive"))));
            }
            return list;
        }

        public static CityDTO GetCityByID(int cityID)
        {
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_GetCityByID", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CityID", cityID);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    if (reader.Read())
                        return new CityDTO(
                            reader.GetInt32(reader.GetOrdinal("CityID")),
                            reader.GetString(reader.GetOrdinal("CityName")),
                            reader.GetBoolean(reader.GetOrdinal("IsActive")));
            }
            return null;
        }

        public static bool IsCityExists(string cityName)
        {
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_IsCityExists", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CityName", cityName);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return (result != null && (int)result > 0);
            }
        }

        public static int AddNewCity(CityDTO dto)
        {
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_AddNewCity", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CityName", dto.CityName);
                cmd.Parameters.AddWithValue("@IsActive",  dto.IsActive);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return (result != null && int.TryParse(result.ToString(), out int id)) ? id : -1;
            }
        }

        public static bool UpdateCity(CityDTO dto)
        {
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_UpdateCity", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CityID",   dto.CityID);
                cmd.Parameters.AddWithValue("@CityName", dto.CityName);
                cmd.Parameters.AddWithValue("@IsActive",  dto.IsActive);
                conn.Open();
                return (cmd.ExecuteNonQuery() > 0);
            }
        }

        public static bool DeleteCity(int cityID)
        {
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_DeleteCity", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CityID", cityID);
                conn.Open();
                return (cmd.ExecuteNonQuery() > 0);
            }
        }
    }
}
