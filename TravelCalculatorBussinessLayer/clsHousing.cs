using System.Collections.Generic;
using DataAccessLayer;

namespace BusinessLayer
{
    // =============================================
    // clsHousing
    // =============================================
    public class clsHousing
    {
        public enum enMode { AddNew = 0, Update = 1 }
        public enMode Mode = enMode.AddNew;

        public HousingDTO HDTO
        {
            get { return new HousingDTO(HousingID, CityID, HousingType, MonthlyRent, Description, IsActive); }
        }

        public int     HousingID   { get; set; }
        public int     CityID      { get; set; }
        public string  HousingType { get; set; }
        public decimal MonthlyRent { get; set; }
        public string  Description { get; set; }
        public bool    IsActive    { get; set; }

        public clsHousing()
        {
            HousingID = -1; CityID = -1; HousingType = ""; MonthlyRent = 0; Description = ""; IsActive = true;
            Mode = enMode.AddNew;
        }

        private clsHousing(HousingDTO dto)
        {
            HousingID = dto.HousingID; CityID = dto.CityID; HousingType = dto.HousingType;
            MonthlyRent = dto.MonthlyRent; Description = dto.Description; IsActive = dto.IsActive;
            Mode = enMode.Update;
        }

        private bool _AddNewHousing()
        {
            HousingID = clsHousingData.AddNewHousing(HDTO);
            return (HousingID != -1);
        }

        private bool _UpdateHousing() => clsHousingData.UpdateHousing(HDTO);

        public static List<HousingDTO> GetHousingByCityID(int cityID) => clsHousingData.GetHousingByCityID(cityID);
        public static bool DeleteHousing(int housingID)               => clsHousingData.DeleteHousing(housingID);

        public static clsHousing Find(int housingID)
        {
            HousingDTO dto = clsHousingData.GetHousingByID(housingID);
            return dto != null ? new clsHousing(dto) : null;
        }

        public static List<HousingComparisonDTO> CompareHousingVsCommuting(int fromCityID, int toCityID,
                                                                             int daysPerWeek, int weeksPerMonth = 4)
        {
            if (fromCityID == toCityID)              return new List<HousingComparisonDTO>();
            if (daysPerWeek < 1 || daysPerWeek > 6) return new List<HousingComparisonDTO>();

            return clsHousingData.CompareHousingVsCommuting(fromCityID, toCityID, daysPerWeek, weeksPerMonth);
        }

        public bool Save()
        {
            if (CityID == -1)                           return false;
            if (string.IsNullOrWhiteSpace(HousingType)) return false;
            if (MonthlyRent <= 0)                       return false;

            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNewHousing()) { Mode = enMode.Update; return true; }
                    return false;
                case enMode.Update:
                    return _UpdateHousing();
            }
            return false;
        }
    }
}
