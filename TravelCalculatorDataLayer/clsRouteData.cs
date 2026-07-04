using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace DataAccessLayer
{
    // =============================================
    // RouteDTO
    // =============================================
    public class RouteDTO
    {
        public int     RouteID         { get; set; }
        public int     FromCityID      { get; set; }
        public int     ToCityID        { get; set; }
        public int     TransportTypeID { get; set; }
        public decimal TicketPrice     { get; set; }
        public int     DurationMinutes { get; set; }
        public decimal DistanceKm      { get; set; }
        public bool    IsActive        { get; set; }

        public RouteDTO(int routeID, int fromCityID, int toCityID, int transportTypeID,
                        decimal ticketPrice, int durationMinutes, decimal distanceKm, bool isActive)
        {
            RouteID = routeID; FromCityID = fromCityID; ToCityID = toCityID;
            TransportTypeID = transportTypeID; TicketPrice = ticketPrice;
            DurationMinutes = durationMinutes; DistanceKm = distanceKm; IsActive = isActive;
        }
    }

    // =============================================
    // RouteDetailsDTO
    // =============================================
    public class RouteDetailsDTO
    {
        public int     RouteID         { get; set; }
        public string  FromCity        { get; set; }
        public string  ToCity          { get; set; }
        public string  TransportType   { get; set; }
        public decimal TicketPrice     { get; set; }
        public int     DurationMinutes { get; set; }
        public decimal DistanceKm      { get; set; }

        public RouteDetailsDTO(int routeID, string fromCity, string toCity, string transportType,
                               decimal ticketPrice, int durationMinutes, decimal distanceKm)
        {
            RouteID = routeID; FromCity = fromCity; ToCity = toCity;
            TransportType = transportType; TicketPrice = ticketPrice;
            DurationMinutes = durationMinutes; DistanceKm = distanceKm;
        }
    }

    // =============================================
    // TravelCostDTO
    // =============================================
    public class TravelCostDTO
    {
        public int     TransportTypeID { get; set; }
        public string  TransportType   { get; set; }
        public decimal OneWayPrice     { get; set; }
        public decimal DailyRoundTrip  { get; set; }
        public decimal WeeklyCost      { get; set; }
        public decimal MonthlyCost     { get; set; }
        public int     DurationMinutes { get; set; }
        public decimal DistanceKm      { get; set; }

        /// <summary>
        /// //////
        /// </summary>

        public TravelCostDTO(int transportTypeID, string transportType, decimal oneWayPrice,
                        decimal dailyRoundTrip, decimal weeklyCost,
                        decimal monthlyCost, int durationMinutes, decimal distanceKm)
        {
            TransportTypeID = transportTypeID;
            TransportType = transportType;
            OneWayPrice = oneWayPrice;
            DailyRoundTrip = dailyRoundTrip;
            WeeklyCost = weeklyCost;
            MonthlyCost = monthlyCost;
            DurationMinutes = durationMinutes;
            DistanceKm = distanceKm;
        }
    }

    // =============================================
    // clsRouteData
    // =============================================
    public class clsRouteData
    {
        public static List<RouteDetailsDTO> GetAllRoutes()
        {
            var list = new List<RouteDetailsDTO>();
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_GetAllRoutes", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        list.Add(new RouteDetailsDTO(
                            reader.GetInt32(reader.GetOrdinal("RouteID")),
                            reader.GetString(reader.GetOrdinal("FromCity")),
                            reader.GetString(reader.GetOrdinal("ToCity")),
                            reader.GetString(reader.GetOrdinal("TransportType")),
                            reader.GetDecimal(reader.GetOrdinal("TicketPrice")),
                            reader.GetInt32(reader.GetOrdinal("DurationMinutes")),
                            reader.GetDecimal(reader.GetOrdinal("DistanceKm"))));
            }
            return list;
        }

        public static RouteDTO GetRouteByID(int routeID)
        {
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_GetRouteByID", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@RouteID", routeID);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    if (reader.Read())
                        return new RouteDTO(
                            reader.GetInt32(reader.GetOrdinal("RouteID")),
                            reader.GetInt32(reader.GetOrdinal("FromCityID")),
                            reader.GetInt32(reader.GetOrdinal("ToCityID")),
                            reader.GetInt32(reader.GetOrdinal("TransportTypeID")),
                            reader.GetDecimal(reader.GetOrdinal("TicketPrice")),
                            reader.GetInt32(reader.GetOrdinal("DurationMinutes")),
                            reader.GetDecimal(reader.GetOrdinal("DistanceKm")),
                            reader.GetBoolean(reader.GetOrdinal("IsActive")));
            }
            return null;
        }

        public static List<RouteDetailsDTO> GetRoutesBetweenCities(int fromCityID, int toCityID)
        {
            var list = new List<RouteDetailsDTO>();
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_GetRoutesBetweenCities", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FromCityID", fromCityID);
                cmd.Parameters.AddWithValue("@ToCityID",   toCityID);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        list.Add(new RouteDetailsDTO(
                            reader.GetInt32(reader.GetOrdinal("RouteID")),
                            reader.GetString(reader.GetOrdinal("FromCity")),
                            reader.GetString(reader.GetOrdinal("ToCity")),
                            reader.GetString(reader.GetOrdinal("TransportType")),
                            reader.GetDecimal(reader.GetOrdinal("TicketPrice")),
                            reader.GetInt32(reader.GetOrdinal("DurationMinutes")),
                            reader.GetDecimal(reader.GetOrdinal("DistanceKm"))));
            }
            return list;
        }

        public static List<TravelCostDTO> CalculateTravelCost(int fromCityID, int toCityID,
                                                               int daysPerWeek, int weeksPerMonth)
        {
            var list = new List<TravelCostDTO>();
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_CalculateTravelCost", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FromCityID",    fromCityID);
                cmd.Parameters.AddWithValue("@ToCityID",      toCityID);
                cmd.Parameters.AddWithValue("@DaysPerWeek",   daysPerWeek);
                cmd.Parameters.AddWithValue("@WeeksPerMonth", weeksPerMonth);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        list.Add(new TravelCostDTO(
                            reader.GetInt32(reader.GetOrdinal("TransportTypeID")),
                            reader.GetString(reader.GetOrdinal("TransportType")),
                            reader.GetDecimal(reader.GetOrdinal("OneWayPrice")),
                            reader.GetDecimal(reader.GetOrdinal("DailyRoundTrip")),
                            reader.GetDecimal(reader.GetOrdinal("WeeklyCost")),
                            reader.GetDecimal(reader.GetOrdinal("MonthlyCost")),
                            reader.GetInt32(reader.GetOrdinal("DurationMinutes")),
                            reader.GetDecimal(reader.GetOrdinal("DistanceKm"))));
            }
            return list;
        }

        public static int AddNewRoute(RouteDTO dto)
        {
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_AddNewRoute", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FromCityID",      dto.FromCityID);
                cmd.Parameters.AddWithValue("@ToCityID",        dto.ToCityID);
                cmd.Parameters.AddWithValue("@TransportTypeID", dto.TransportTypeID);
                cmd.Parameters.AddWithValue("@TicketPrice",     dto.TicketPrice);
                cmd.Parameters.AddWithValue("@DurationMinutes", dto.DurationMinutes);
                cmd.Parameters.AddWithValue("@DistanceKm",      dto.DistanceKm);
                cmd.Parameters.AddWithValue("@IsActive",        dto.IsActive);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return (result != null && int.TryParse(result.ToString(), out int id)) ? id : -1;
            }
        }

        public static bool UpdateRoute(RouteDTO dto)
        {
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_UpdateRoute", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@RouteID",         dto.RouteID);
                cmd.Parameters.AddWithValue("@FromCityID",      dto.FromCityID);
                cmd.Parameters.AddWithValue("@ToCityID",        dto.ToCityID);
                cmd.Parameters.AddWithValue("@TransportTypeID", dto.TransportTypeID);
                cmd.Parameters.AddWithValue("@TicketPrice",     dto.TicketPrice);
                cmd.Parameters.AddWithValue("@DurationMinutes", dto.DurationMinutes);
                cmd.Parameters.AddWithValue("@DistanceKm",      dto.DistanceKm);
                cmd.Parameters.AddWithValue("@IsActive",        dto.IsActive);
                conn.Open();
                return (cmd.ExecuteNonQuery() > 0);
            }
        }

        public static bool DeleteRoute(int routeID)
        {
            using (var conn = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (var cmd  = new SqlCommand("SP_DeleteRoute", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@RouteID", routeID);
                conn.Open();
                return (cmd.ExecuteNonQuery() > 0);
            }
        }
    }
}
