using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BusinessLayer;
using DataAccessLayer;

namespace TravelCalculator.API.Controllers
{
    // =============================================
    // Fuel Controller
    // =============================================
    [Route("api/Fuel")]
    [ApiController]
    public class FuelAPIController : ControllerBase
    {
        // GET api/Fuel/Prices
        [HttpGet("Prices", Name = "GetFuelPrices")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<FuelPriceDTO>> GetFuelPrices()
        {
            var list = clsFuel.GetFuelPrices();
            if (list == null || list.Count == 0) return NotFound("No fuel prices found!");
            return Ok(list);
        }

        // GET api/Fuel/Calculate?fromCityID=1&toCityID=2&fuelType=بنزين 92&consumptionPer100=8&daysPerWeek=4&weeksPerMonth=4
        [HttpGet("Calculate", Name = "CalculateCarFuelCost")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<CarFuelCostDTO> CalculateCarFuelCost(
            [FromQuery] int     fromCityID,
            [FromQuery] int     toCityID,
            [FromQuery] string  fuelType,
            [FromQuery] decimal consumptionPer100,
            [FromQuery] int     daysPerWeek,
            [FromQuery] int     weeksPerMonth = 4)
        {
            if (fromCityID < 1 || toCityID < 1 || fromCityID == toCityID)
                return BadRequest("Invalid city IDs.");

            if (consumptionPer100 <= 0 || consumptionPer100 > 50)
                return BadRequest("Consumption must be between 1 and 50 L/100km.");

            if (daysPerWeek < 1 || daysPerWeek > 6)
                return BadRequest("Days per week must be between 1 and 6.");

            var result = clsFuel.CalculateCarFuelCost(fromCityID, toCityID, fuelType,
                                                      consumptionPer100, daysPerWeek, weeksPerMonth);
            if (result == null)
                return NotFound("Could not calculate fuel cost. Check route or fuel type.");

            return Ok(result);
        }

        // GET api/Fuel/CalculateSplit?fromCityID=1&toCityID=2&fuelType=بنزين 92&consumptionPer100=8&daysPerWeek=4&weeksPerMonth=4&passengerCount=3
        [HttpGet("CalculateSplit", Name = "CalculateCarFuelCostSplit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<CarFuelSplitDTO> CalculateCarFuelCostSplit(
            [FromQuery] int     fromCityID,
            [FromQuery] int     toCityID,
            [FromQuery] string  fuelType,
            [FromQuery] decimal consumptionPer100,
            [FromQuery] int     daysPerWeek,
            [FromQuery] int     weeksPerMonth = 4,
            [FromQuery] int     passengerCount = 1)
        {
            if (fromCityID < 1 || toCityID < 1 || fromCityID == toCityID)
                return BadRequest("Invalid city IDs.");

            if (consumptionPer100 <= 0 || consumptionPer100 > 50)
                return BadRequest("Consumption must be between 1 and 50 L/100km.");

            if (daysPerWeek < 1 || daysPerWeek > 6)
                return BadRequest("Days per week must be between 1 and 6.");

            if (passengerCount < 1 || passengerCount > 8)
                return BadRequest("Passenger count must be between 1 and 8.");

            var result = clsFuel.CalculateCarFuelCostSplit(fromCityID, toCityID, fuelType,
                                                            consumptionPer100, daysPerWeek,
                                                            weeksPerMonth, passengerCount);
            if (result == null)
                return NotFound("Could not calculate split fuel cost. Check route or fuel type.");

            return Ok(result);
        }
    }
}
