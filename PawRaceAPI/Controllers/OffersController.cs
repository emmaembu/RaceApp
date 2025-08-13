using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Offer;
using PawRace.Models.DTO;
using PawRace.Models.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace OfferAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OffersController : ControllerBase
    {
        private readonly IOfferService _iOfferService;
        private readonly ILogger<OffersController> _logger;
        public OffersController
        (
            IOfferService offerService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<OffersController> logger
        )
        {
            this._iOfferService = offerService;
            this._logger = logger;
        }

        [HttpPost("Dogs")]
        public async Task<List<Dog>> GetDogs()
        {
            return await _iOfferService.GetDogsAsync();
        }

        [HttpGet("Race/upcoming-with-dogs")]
        public async Task<IActionResult>  GetUpcomingRaces()
        {
            var races = await _iOfferService.GetUpcomingRacesAsync();

            return Ok(races);
        }

        [HttpGet("Race")]
        public async Task<IActionResult> GetRace([FromQuery] int raceId)
        {
            var race = await _iOfferService.GetRaceByIdAsync(raceId);

            return race != null ? Ok(new { Message = ((RaceStatus)race.RaceStatus).ToString()}): BadRequest("Race is empty.");
        }

    }
}
