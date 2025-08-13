using PawRace.Models.DTO;

namespace Ticket
{
    public interface ITicketService
    {
        Task<int> PlaceBetAsync(decimal amount, List<RaceDogSelection> selections, string nickName, string ipAddress);

        Task<bool> ProcessTickets();

        Task<bool> ActivateTickets();

        Task<TicketStatus> GetTicketById(int ticketId);
    }
}
