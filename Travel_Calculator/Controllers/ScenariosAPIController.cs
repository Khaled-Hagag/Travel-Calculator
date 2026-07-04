using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BusinessLayer;
using DataAccessLayer;
using System.Collections.Generic;

namespace TravelCalculator.API.Controllers
{
    [Route("api/Scenarios")]
    [ApiController]
    public class ScenariosAPIController : ControllerBase
    {
        private string GetDeviceID() =>
            Request.Headers["X-Device-Id"].ToString();

        // =============================================
        // Scenarios (الرأس)
        // =============================================

        // GET api/Scenarios
        // يرجع كل السيناريوهات بدون items (للقايمة السريعة)
        [HttpGet(Name = "GetMyScenarios")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<ScenarioDTO>> GetMyScenarios()
        {
            string deviceID = GetDeviceID();
            if (string.IsNullOrWhiteSpace(deviceID))
                return BadRequest("X-Device-Id header is required.");

            var list = clsScenario.GetScenariosByDevice(deviceID);
            if (list == null || list.Count == 0)
                return NotFound("No saved scenarios found.");

            return Ok(list);
        }

        // GET api/Scenarios/{id}
        // يرجع السيناريو كامل مع كل الخيارات
        [HttpGet("{id}", Name = "GetScenarioWithItems")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ScenarioWithItemsDTO> GetScenarioWithItems(int id)
        {
            string deviceID = GetDeviceID();
            if (string.IsNullOrWhiteSpace(deviceID))
                return BadRequest("X-Device-Id header is required.");

            if (id < 1) return BadRequest($"Invalid ID {id}");

            var scenario = clsScenario.GetScenarioWithItems(id, deviceID);
            if (scenario == null)
                return NotFound($"Scenario with ID {id} not found.");

            return Ok(scenario);
        }

        // POST api/Scenarios
        // إنشاء سيناريو جديد (فاضي من غير items)
        [HttpPost(Name = "AddScenario")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<ScenarioDTO> AddScenario([FromBody] ScenarioDTO newScenario)
        {
            string deviceID = GetDeviceID();
            if (string.IsNullOrWhiteSpace(deviceID))
                return BadRequest("X-Device-Id header is required.");

            if (newScenario == null || string.IsNullOrWhiteSpace(newScenario.ScenarioName))
                return BadRequest("Invalid scenario data.");

            clsScenario scenario = new clsScenario();
            scenario.DeviceID     = deviceID;
            scenario.ScenarioName = newScenario.ScenarioName;
            scenario.ScenarioType = newScenario.ScenarioType;

            try
            {
                if (scenario.Save())
                {
                    newScenario.ScenarioID = scenario.ScenarioID;
                    newScenario.DeviceID   = deviceID;
                    return CreatedAtRoute("GetScenarioWithItems", new { id = scenario.ScenarioID }, newScenario);
                }
                return BadRequest("Scenario could not be saved (Save returned false).");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT api/Scenarios/{id}/rename
        [HttpPut("{id}/rename", Name = "RenameScenario")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult RenameScenario(int id, [FromBody] string newName)
        {
            string deviceID = GetDeviceID();
            if (string.IsNullOrWhiteSpace(deviceID))
                return BadRequest("X-Device-Id header is required.");

            if (id < 1 || string.IsNullOrWhiteSpace(newName))
                return BadRequest("Invalid data.");

            if (clsScenario.RenameScenario(id, deviceID, newName))
                return Ok("Scenario renamed successfully.");

            return NotFound("Scenario not found or doesn't belong to this device.");
        }

        // DELETE api/Scenarios/{id}
        // بيمسح السيناريو وكل الـ items جوّاه (CASCADE)
        [HttpDelete("{id}", Name = "DeleteScenario")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteScenario(int id)
        {
            string deviceID = GetDeviceID();
            if (string.IsNullOrWhiteSpace(deviceID))
                return BadRequest("X-Device-Id header is required.");

            if (id < 1) return BadRequest($"Invalid ID {id}");

            if (clsScenario.DeleteScenario(id, deviceID))
                return Ok($"Scenario {id} and all its items have been deleted.");

            return NotFound($"Scenario with ID {id} not found.");
        }

        // =============================================
        // ScenarioItems (الخيارات جوه السيناريو)
        // =============================================

        // POST api/Scenarios/{scenarioId}/items
        // إضافة خيار جديد لسيناريو موجود
        [HttpPost("{scenarioId}/items", Name = "AddScenarioItem")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<ScenarioItemDTO> AddScenarioItem(int scenarioId, [FromBody] ScenarioItemDTO newItem)
        {
            string deviceID = GetDeviceID();
            if (string.IsNullOrWhiteSpace(deviceID))
                return BadRequest("X-Device-Id header is required.");

            if (scenarioId < 1 || newItem == null || string.IsNullOrWhiteSpace(newItem.ItemName))
                return BadRequest("Invalid item data.");

            clsScenarioItem item = new clsScenarioItem();
            item.ScenarioID        = scenarioId;
            item.ItemName          = newItem.ItemName;
            item.ItemType          = newItem.ItemType;
            item.FromCityID        = newItem.FromCityID;
            item.ToCityID          = newItem.ToCityID;
            item.TransportTypeID   = newItem.TransportTypeID;
            item.DaysPerWeek       = newItem.DaysPerWeek;
            item.WeeksPerMonth     = newItem.WeeksPerMonth;
            item.HousingID         = newItem.HousingID;
            item.PassengerCount    = newItem.PassengerCount;
            item.FuelType          = newItem.FuelType;
            item.ConsumptionPer100 = newItem.ConsumptionPer100;
            item.MonthlyCostSnapshot = newItem.MonthlyCostSnapshot;

            if (item.Save())
            {
                newItem.ItemID     = item.ItemID;
                newItem.ScenarioID = scenarioId;
                return StatusCode(StatusCodes.Status201Created, newItem);
            }

            return BadRequest("Item could not be saved. Check required fields for the item type.");
        }

        // DELETE api/Scenarios/{scenarioId}/items/{itemId}
        // حذف خيار واحد من السيناريو
        [HttpDelete("{scenarioId}/items/{itemId}", Name = "DeleteScenarioItem")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteScenarioItem(int scenarioId, int itemId)
        {
            string deviceID = GetDeviceID();
            if (string.IsNullOrWhiteSpace(deviceID))
                return BadRequest("X-Device-Id header is required.");

            if (scenarioId < 1 || itemId < 1)
                return BadRequest("Invalid IDs.");

            if (clsScenarioItem.DeleteScenarioItem(itemId, scenarioId, deviceID))
                return Ok($"Item {itemId} deleted successfully.");

            return NotFound($"Item {itemId} not found.");
        }
    }
}
