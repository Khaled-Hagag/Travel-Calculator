using System.Collections.Generic;
using DataAccessLayer;

namespace BusinessLayer
{
    // =============================================
    // clsRoute
    // =============================================
    public class clsRoute
    {
        public enum enMode { AddNew = 0, Update = 1 }
        public enMode Mode = enMode.AddNew;

        public RouteDTO RDTO
        {
            get { return new RouteDTO(RouteID, FromCityID, ToCityID, TransportTypeID,
                                      TicketPrice, DurationMinutes, DistanceKm, IsActive); }
        }

        public int     RouteID         { get; set; }
        public int     FromCityID      { get; set; }
        public int     ToCityID        { get; set; }
        public int     TransportTypeID { get; set; }
        public decimal TicketPrice     { get; set; }
        public int     DurationMinutes { get; set; }
        public decimal DistanceKm      { get; set; }
        public bool    IsActive        { get; set; }

        public clsRoute()
        {
            RouteID = -1; FromCityID = -1; ToCityID = -1; TransportTypeID = -1;
            TicketPrice = 0; DurationMinutes = 0; DistanceKm = 0; IsActive = true;
            Mode = enMode.AddNew;
        }

        private clsRoute(RouteDTO dto)
        {
            RouteID = dto.RouteID; FromCityID = dto.FromCityID; ToCityID = dto.ToCityID;
            TransportTypeID = dto.TransportTypeID; TicketPrice = dto.TicketPrice;
            DurationMinutes = dto.DurationMinutes; DistanceKm = dto.DistanceKm; IsActive = dto.IsActive;
            Mode = enMode.Update;
        }

        private bool _AddNewRoute()
        {
            RouteID = clsRouteData.AddNewRoute(RDTO);
            return (RouteID != -1);
        }

        private bool _UpdateRoute() => clsRouteData.UpdateRoute(RDTO);

        public static List<RouteDetailsDTO> GetAllRoutes()                           => clsRouteData.GetAllRoutes();
        public static bool DeleteRoute(int routeID)                                  => clsRouteData.DeleteRoute(routeID);
        public static List<RouteDetailsDTO> GetRoutesBetweenCities(int from, int to) => clsRouteData.GetRoutesBetweenCities(from, to);

        public static clsRoute Find(int routeID)
        {
            RouteDTO dto = clsRouteData.GetRouteByID(routeID);
            return dto != null ? new clsRoute(dto) : null;
        }

        public static List<TravelCostDTO> CalculateTravelCost(int fromCityID, int toCityID,
                                                               int daysPerWeek, int weeksPerMonth = 4)
        {
            if (fromCityID == toCityID)                 return new List<TravelCostDTO>();
            if (daysPerWeek < 1 || daysPerWeek > 6)     return new List<TravelCostDTO>();
            if (weeksPerMonth < 1 || weeksPerMonth > 5) return new List<TravelCostDTO>();

            return clsRouteData.CalculateTravelCost(fromCityID, toCityID, daysPerWeek, weeksPerMonth);
        }

        public bool Save()
        {
            if (FromCityID == ToCityID)   return false;
            if (TicketPrice <= 0)         return false;
            if (DurationMinutes <= 0)     return false;
            if (TransportTypeID == -1)    return false;

            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNewRoute()) { Mode = enMode.Update; return true; }
                    return false;
                case enMode.Update:
                    return _UpdateRoute();
            }
            return false;
        }
    }
}
