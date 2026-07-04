using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BusinessLayer;
using DataAccessLayer;

namespace TravelCalculator.API.Controllers
{
    // =============================================
    // Routes Controller
    // =============================================
    [Route("api/Routes")]
    [ApiController]
    public class RoutesAPIController : ControllerBase
    {
        [HttpGet("All", Name = "GetAllRoutes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<RouteDetailsDTO>> GetAllRoutes()
        {
            var list = clsRoute.GetAllRoutes();
            if (list == null || list.Count == 0) return NotFound("No routes found!");
            return Ok(list);
        }

        [HttpGet("{id}", Name = "GetRouteByID")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<RouteDTO> GetRouteByID(int id)
        {
            if (id < 1) return BadRequest($"Invalid ID {id}");

            clsRoute route = clsRoute.Find(id);
            if (route == null) return NotFound($"Route with ID {id} not found.");

            return Ok(route.RDTO);
        }

        // GET api/Routes/Between?fromCityID=1&toCityID=2
        [HttpGet("Between", Name = "GetRoutesBetweenCities")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<RouteDetailsDTO>> GetRoutesBetweenCities(
            [FromQuery] int fromCityID, [FromQuery] int toCityID)
        {
            if (fromCityID < 1 || toCityID < 1 || fromCityID == toCityID)
                return BadRequest("Invalid city IDs.");

            var list = clsRoute.GetRoutesBetweenCities(fromCityID, toCityID);
            if (list == null || list.Count == 0)
                return NotFound("No routes found between these cities.");

            return Ok(list);
        }

        // GET api/Routes/Calculate?fromCityID=1&toCityID=2&daysPerWeek=4&weeksPerMonth=4
        [HttpGet("Calculate", Name = "CalculateTravelCost")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<TravelCostDTO>> CalculateTravelCost(
            [FromQuery] int fromCityID,
            [FromQuery] int toCityID,
            [FromQuery] int daysPerWeek,
            [FromQuery] int weeksPerMonth = 4)
        {
            if (fromCityID < 1 || toCityID < 1 || fromCityID == toCityID)
                return BadRequest("Invalid city IDs.");

            if (daysPerWeek < 1 || daysPerWeek > 6)
                return BadRequest("Days per week must be between 1 and 6.");

            var list = clsRoute.CalculateTravelCost(fromCityID, toCityID, daysPerWeek, weeksPerMonth);
            if (list == null || list.Count == 0)
                return NotFound("No routes found between these cities.");

            return Ok(list);
        }

        [HttpPost(Name = "AddRoute")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<RouteDTO> AddRoute(RouteDTO newRouteDTO)
        {
            if (newRouteDTO == null || newRouteDTO.TicketPrice <= 0 || newRouteDTO.DurationMinutes <= 0)
                return BadRequest("Invalid route data.");

            clsRoute route = new clsRoute();
            route.FromCityID      = newRouteDTO.FromCityID;
            route.ToCityID        = newRouteDTO.ToCityID;
            route.TransportTypeID = newRouteDTO.TransportTypeID;
            route.TicketPrice     = newRouteDTO.TicketPrice;
            route.DurationMinutes = newRouteDTO.DurationMinutes;
            route.DistanceKm      = newRouteDTO.DistanceKm;
            route.IsActive        = newRouteDTO.IsActive;

            if (route.Save())
            {
                newRouteDTO.RouteID = route.RouteID;
                return CreatedAtRoute("GetRouteByID", new { id = route.RouteID }, newRouteDTO);
            }

            return BadRequest("Route could not be saved.");
        }

        [HttpPut("{id}", Name = "UpdateRoute")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<RouteDTO> UpdateRoute(int id, RouteDTO updatedDTO)
        {
            if (id < 1 || updatedDTO == null || updatedDTO.TicketPrice <= 0)
                return BadRequest("Invalid data.");

            clsRoute route = clsRoute.Find(id);
            if (route == null) return NotFound($"Route with ID {id} not found.");

            route.FromCityID      = updatedDTO.FromCityID;
            route.ToCityID        = updatedDTO.ToCityID;
            route.TransportTypeID = updatedDTO.TransportTypeID;
            route.TicketPrice     = updatedDTO.TicketPrice;
            route.DurationMinutes = updatedDTO.DurationMinutes;
            route.DistanceKm      = updatedDTO.DistanceKm;
            route.IsActive        = updatedDTO.IsActive;

            if (route.Save()) return Ok(route.RDTO);
            return BadRequest("Update failed.");
        }

        [HttpDelete("{id}", Name = "DeleteRoute")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteRoute(int id)
        {
            if (id < 1) return BadRequest($"Invalid ID {id}");

            if (clsRoute.DeleteRoute(id))
                return Ok($"Route with ID {id} has been deleted.");

            return NotFound($"Route with ID {id} not found.");
        }
    }
}
