using System;
using System.Collections.Generic;
using DataAccessLayer;

namespace BusinessLayer
{
    // =============================================
    // clsScenario  →  BLL
    // =============================================
    public class clsScenario
    {
        public int      ScenarioID   { get; set; }
        public string   DeviceID     { get; set; }
        public string   ScenarioName { get; set; }
        public string   ScenarioType { get; set; }

        public ScenarioDTO SDTO
        {
            get
            {
                return new ScenarioDTO
                {
                    DeviceID     = this.DeviceID,
                    ScenarioName = this.ScenarioName,
                    ScenarioType = this.ScenarioType
                };
            }
        }

        public clsScenario()
        {
            ScenarioID = -1;
        }

        // ---- Static Methods ----

        public static List<ScenarioDTO> GetScenariosByDevice(string deviceID)
        {
            if (string.IsNullOrWhiteSpace(deviceID)) return new List<ScenarioDTO>();
            return clsScenarioData.GetScenariosByDevice(deviceID);
        }

        public static ScenarioWithItemsDTO GetScenarioWithItems(int scenarioID, string deviceID)
        {
            if (scenarioID < 1 || string.IsNullOrWhiteSpace(deviceID)) return null;
            return clsScenarioData.GetScenarioWithItems(scenarioID, deviceID);
        }

        public static bool DeleteScenario(int scenarioID, string deviceID)
        {
            if (string.IsNullOrWhiteSpace(deviceID)) return false;
            return clsScenarioData.DeleteScenario(scenarioID, deviceID);
        }

        public static bool RenameScenario(int scenarioID, string deviceID, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) return false;
            return clsScenarioData.RenameScenario(scenarioID, deviceID, newName);
        }

        // ---- Save ----
        public bool Save()
        {
            if (string.IsNullOrWhiteSpace(DeviceID))     return false;
            if (string.IsNullOrWhiteSpace(ScenarioName)) return false;

            var validTypes = new[] { "Commute", "Housing", "CarTrip" };
            if (Array.IndexOf(validTypes, ScenarioType) == -1) return false;

            ScenarioID = clsScenarioData.AddScenario(SDTO);
            return (ScenarioID != -1);
        }
    }

    // =============================================
    // clsScenarioItem  →  BLL
    // =============================================
    public class clsScenarioItem
    {
        public int      ItemID              { get; set; }
        public int      ScenarioID          { get; set; }
        public string   ItemName            { get; set; }
        public string   ItemType            { get; set; }

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

        public ScenarioItemDTO IDTO
        {
            get
            {
                return new ScenarioItemDTO
                {
                    ScenarioID          = this.ScenarioID,
                    ItemName            = this.ItemName,
                    ItemType            = this.ItemType,
                    FromCityID          = this.FromCityID,
                    ToCityID            = this.ToCityID,
                    TransportTypeID     = this.TransportTypeID,
                    DaysPerWeek         = this.DaysPerWeek,
                    WeeksPerMonth       = this.WeeksPerMonth,
                    HousingID           = this.HousingID,
                    PassengerCount      = this.PassengerCount,
                    FuelType            = this.FuelType,
                    ConsumptionPer100   = this.ConsumptionPer100,
                    MonthlyCostSnapshot = this.MonthlyCostSnapshot
                };
            }
        }

        public clsScenarioItem()
        {
            ItemID         = -1;
            WeeksPerMonth  = 4;
            PassengerCount = 1;
        }

        // ---- Static Methods ----

        public static bool DeleteScenarioItem(int itemID, int scenarioID, string deviceID)
        {
            if (itemID < 1 || scenarioID < 1 || string.IsNullOrWhiteSpace(deviceID))
                return false;
            return clsScenarioData.DeleteScenarioItem(itemID, scenarioID, deviceID);
        }

        // ---- Save ----
        public bool Save()
        {
            if (ScenarioID < 1)                       return false;
            if (string.IsNullOrWhiteSpace(ItemName))  return false;

            // Business Rules حسب النوع
            switch (ItemType)
            {
                case "Commute":
                    if (FromCityID == null || ToCityID == null ||
                        TransportTypeID == null || DaysPerWeek == null)
                        return false;
                    if (FromCityID == ToCityID) return false;
                    break;

                case "Housing":
                    if (HousingID == null) return false;
                    break;

                case "CarTrip":
                    if (FromCityID == null || ToCityID == null ||
                        string.IsNullOrWhiteSpace(FuelType) ||
                        ConsumptionPer100 == null || DaysPerWeek == null)
                        return false;
                    if (FromCityID == ToCityID)                            return false;
                    if (ConsumptionPer100 <= 0 || ConsumptionPer100 > 50)  return false;
                    if (PassengerCount == null || PassengerCount < 1)      PassengerCount = 1;
                    if (PassengerCount > 8)                                PassengerCount = 8;
                    break;

                default:
                    return false;
            }

            ItemID = clsScenarioData.AddScenarioItem(IDTO);
            return (ItemID != -1);
        }
    }
}
