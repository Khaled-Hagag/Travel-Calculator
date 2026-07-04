using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BusinessLayer;
using DataAccessLayer;

namespace TravelCalculator.API.Controllers
{
    // =============================================
    // Cities Controller
    // =============================================
    [Route("api/Cities")]
    [ApiController]
    public class CitiesAPIController : ControllerBase
    {


        [HttpGet("All", Name = "GetAllCities")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<CityDTO>> GetAllCities()
        {
            var list = clsCity.GetAllCities();
            if (list == null || list.Count == 0)
                return NotFound("No cities found!");
            return Ok(list);
        }

        ////////////////////////////////////
        [HttpGet("{id}", Name = "GetCityByID")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<CityDTO> GetCityByID(int id)
        {
            if (id < 1) return BadRequest($"Invalid ID {id}");

            clsCity city = clsCity.Find(id);
            if (city == null) return NotFound($"City with ID {id} not found.");

            return Ok(city.CDTO);
        }



        [HttpPost(Name = "AddCity")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<CityDTO> AddCity(CityDTO newCityDTO)
        {
            if (newCityDTO == null || string.IsNullOrWhiteSpace(newCityDTO.CityName))
                return BadRequest("Invalid city data.");

            clsCity city = new clsCity();
            city.CityName = newCityDTO.CityName;
            city.IsActive = newCityDTO.IsActive;

            if (city.Save())
            {
                newCityDTO.CityID = city.CityID;
                return CreatedAtRoute("GetCityByID", new { id = city.CityID }, newCityDTO);
            }

            return BadRequest("City already exists or could not be saved.");
        }

        [HttpPut("{id}", Name = "UpdateCity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<CityDTO> UpdateCity(int id, CityDTO updatedDTO)
        {
            if (id < 1 || updatedDTO == null || string.IsNullOrWhiteSpace(updatedDTO.CityName))
                return BadRequest("Invalid data.");

            clsCity city = clsCity.Find(id);
            if (city == null) return NotFound($"City with ID {id} not found.");

            city.CityName = updatedDTO.CityName;
            city.IsActive = updatedDTO.IsActive;

            if (city.Save()) return Ok(city.CDTO);
            return BadRequest("Update failed.");
        }

        [HttpDelete("{id}", Name = "DeleteCity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteCity(int id)
        {
            if (id < 1) return BadRequest($"Invalid ID {id}");

            if (clsCity.DeleteCity(id))
                return Ok($"City with ID {id} has been deleted.");

            return NotFound($"City with ID {id} not found.");
        }
    }
}
