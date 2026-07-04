using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BusinessLayer;
using DataAccessLayer;

namespace TravelCalculator.API.Controllers
{
    // =============================================
    // Housing Controller
    // =============================================
    [Route("api/Housing")]
    [ApiController]
    public class HousingAPIController : ControllerBase
    {
        // GET api/Housing/ByCity?cityID=2
        [HttpGet("ByCity", Name = "GetHousingByCity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<HousingDTO>> GetHousingByCity([FromQuery] int cityID)
        {
            if (cityID < 1) return BadRequest($"Invalid City ID {cityID}");

            var list = clsHousing.GetHousingByCityID(cityID);
            if (list == null || list.Count == 0)
                return NotFound("No housing options found for this city.");

            return Ok(list);
        }

        [HttpGet("{id}", Name = "GetHousingByID")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<HousingDTO> GetHousingByID(int id)
        {
            if (id < 1) return BadRequest($"Invalid ID {id}");

            clsHousing housing = clsHousing.Find(id);
            if (housing == null) return NotFound($"Housing with ID {id} not found.");

            return Ok(housing.HDTO);
        }

        // GET api/Housing/Compare?fromCityID=1&toCityID=2&daysPerWeek=4&weeksPerMonth=4
        [HttpGet("Compare", Name = "CompareHousingVsCommuting")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<HousingComparisonDTO>> CompareHousingVsCommuting(
            [FromQuery] int fromCityID, [FromQuery] int toCityID,
            [FromQuery] int daysPerWeek, [FromQuery] int weeksPerMonth = 4)
        {
            if (fromCityID < 1 || toCityID < 1 || fromCityID == toCityID)
                return BadRequest("Invalid city IDs.");

            if (daysPerWeek < 1 || daysPerWeek > 6)
                return BadRequest("Days per week must be between 1 and 6.");

            var list = clsHousing.CompareHousingVsCommuting(fromCityID, toCityID, daysPerWeek, weeksPerMonth);
            if (list == null || list.Count == 0)
                return NotFound("No comparison data found.");

            return Ok(list);
        }

        [HttpPost(Name = "AddHousing")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<HousingDTO> AddHousing(HousingDTO newHousingDTO)
        {
            if (newHousingDTO == null || string.IsNullOrWhiteSpace(newHousingDTO.HousingType) || newHousingDTO.MonthlyRent <= 0)
                return BadRequest("Invalid housing data.");

            clsHousing housing = new clsHousing();
            housing.CityID      = newHousingDTO.CityID;
            housing.HousingType = newHousingDTO.HousingType;
            housing.MonthlyRent = newHousingDTO.MonthlyRent;
            housing.Description = newHousingDTO.Description;
            housing.IsActive    = newHousingDTO.IsActive;

            if (housing.Save())
            {
                newHousingDTO.HousingID = housing.HousingID;
                return CreatedAtRoute("GetHousingByID", new { id = housing.HousingID }, newHousingDTO);
            }

            return BadRequest("Housing option could not be saved.");
        }

        [HttpPut("{id}", Name = "UpdateHousing")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<HousingDTO> UpdateHousing(int id, HousingDTO updatedDTO)
        {
            if (id < 1 || updatedDTO == null || updatedDTO.MonthlyRent <= 0)
                return BadRequest("Invalid data.");

            clsHousing housing = clsHousing.Find(id);
            if (housing == null) return NotFound($"Housing with ID {id} not found.");

            housing.CityID      = updatedDTO.CityID;
            housing.HousingType = updatedDTO.HousingType;
            housing.MonthlyRent = updatedDTO.MonthlyRent;
            housing.Description = updatedDTO.Description;
            housing.IsActive    = updatedDTO.IsActive;

            if (housing.Save()) return Ok(housing.HDTO);
            return BadRequest("Update failed.");
        }

        [HttpDelete("{id}", Name = "DeleteHousing")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteHousing(int id)
        {
            if (id < 1) return BadRequest($"Invalid ID {id}");

            if (clsHousing.DeleteHousing(id))
                return Ok($"Housing with ID {id} has been deleted.");

            return NotFound($"Housing with ID {id} not found.");
        }
    }
}
