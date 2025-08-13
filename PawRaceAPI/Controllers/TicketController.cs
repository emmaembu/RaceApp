using Microsoft.AspNetCore.Mvc;
using PawRace.Models.DTO;
using Ticket;
using Microsoft.AspNetCore.SignalR;
using PawRaceAPI.Hubs;
using Microsoft.IdentityModel.Tokens;

namespace OfferAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly ILogger<TicketController> _logger;
        private readonly IHubContext<MessageHub> _iHub;
        public TicketController
        (
            ITicketService ticketService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<TicketController> logger,
            IHubContext<MessageHub> iHub
        )
        {
            this._ticketService = ticketService;
            this._logger = logger;
            this._iHub = iHub;
        }



        [HttpGet]
        public async Task<TicketStatus> GetTicketById([FromQuery]int ticketId)
        {
            return await _ticketService.GetTicketById(ticketId);
        }

        [HttpPost("ticket")]
        public async Task<IActionResult> PlaceTicket([FromBody] BetTicket ticket)
        {
            if (ticket.raceDogSelections == null || !ticket.raceDogSelections.Any())
                return BadRequest("No races selected.");

            int success = await _ticketService.PlaceBetAsync(ticket.Amount,  ticket.raceDogSelections, ticket.NickName, string.IsNullOrWhiteSpace(ticket.IpAddress) ? HttpContext.Connection.RemoteIpAddress?.ToString()! : ticket.IpAddress);
            
            if(success != 0)
            {
               await _iHub.Clients.User(ticket.NickName).SendAsync("Ticket has been accepted and deposit has been made!", ticket.Amount);
            }
            else
            {
                await _iHub.Clients.User(ticket.NickName).SendAsync("Ticket has been rejected and deposit has not been made:", ticket.Amount);
            }
            return success != 0 ? Ok(new { TicketId = success, Message = "Ticket accepted" }) : BadRequest(new { TicketId = success, Message = "Ticket rejected" });
        }
    }
}
