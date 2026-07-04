using System.Collections.Generic;
using DataAccessLayer;

namespace BusinessLayer
{
    // =============================================
    // clsCity
    // =============================================
    public class clsCity
    {
        public enum enMode { AddNew = 0, Update = 1 }
        public enMode Mode = enMode.AddNew;

        public CityDTO CDTO
        {
            get { return new CityDTO(CityID, CityName, IsActive); }
        }

        public int    CityID   { get; set; }
        public string CityName { get; set; }
        public bool   IsActive { get; set; }

        public clsCity()
        {
            CityID = -1; CityName = ""; IsActive = true;
            Mode = enMode.AddNew;
        }

        private clsCity(CityDTO dto)
        {
            CityID = dto.CityID; CityName = dto.CityName; IsActive = dto.IsActive;
            Mode = enMode.Update;
        }

        private bool _AddNewCity()
        {
            CityID = clsCityData.AddNewCity(CDTO);
            return (CityID != -1);
        }

        private bool _UpdateCity()
        {
            return clsCityData.UpdateCity(CDTO);
        }

        public static List<CityDTO> GetAllCities()   => clsCityData.GetAllCities();
        public static bool IsCityExists(string name) => clsCityData.IsCityExists(name);
        public static bool DeleteCity(int cityID)    => clsCityData.DeleteCity(cityID);

        public static clsCity Find(int cityID)
        {
            CityDTO dto = clsCityData.GetCityByID(cityID);
            return dto != null ? new clsCity(dto) : null;
        }

        public bool Save()
        {
            if (string.IsNullOrWhiteSpace(CityName)) return false;
            if (Mode == enMode.AddNew && IsCityExists(CityName)) return false;

            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNewCity()) { Mode = enMode.Update; return true; }
                    return false;
                case enMode.Update:
                    return _UpdateCity();
            }
            return false;
        }
    }
}
