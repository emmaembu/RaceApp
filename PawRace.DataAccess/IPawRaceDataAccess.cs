using PawRace.DataAccess.Models;
namespace PawRace.DataAccess
{
    public interface IPawRaceDataAccess
    {
        Task<List<Models.Dog>> GetDogsAsync();

        Task<int> GetCountScheduledRacesAsync();

        Task<DateTime?> GetLastRaceScheduledTime();

        Task<List<Models.Race>> GetRacesToActivate();

        Task<bool> UpdateRaceStatusAndTimeAsync(int raceId, byte status, DateTime datetime);

        Task<int> AddRaceAsync(byte status, DateTime? datetime);

        Task<List<Models.Race>> GetRacesToFinish();

        Task<Models.Race> GetRaceByIdAsync(int raceId);

        Task<Models.Dog?> GetDogByIdAsync(int dogId);

        Task<bool> CheckIfDogAndRaceValidbyIdAsync(int dogId, int raceId);

        Task<bool> AddRacingDogAsync(int raceId, RaceDog dog);

        Task<List<Models.Race>> GetInProgressRacesWithDogsAsync(DateTime currentTime);

        Task<int> SaveChangesAsync();

        Task<int> AddTicketAsync(Guid user, decimal amount, DateTime datetime,byte status);

        Task<int> AddTicketDetailsAsync(TicketDetail ticket);

        Task<bool> UpdateTicketAsync(int ticketId, byte status);

        Task<List<Models.Race>> GetRacesByIdsAsync(List<int> raceIds);

        Task<Models.User> GetUserAsync(string nickname, string ipAddress);

        Task<bool> AddUserAsync(Models.User user);

        Task<List<RaceDog>> GetRaceDogsForTicketAsync(int ticketId);

        Task<bool> InsertPotentialWinningAsync(int ticketId, decimal potentialWinning);

        Task<List<Models.RaceDog>> GetUpcomingRacesAsync();

        Task<List<Race>> GetRacesByTicketIdAsync(int ticketId);

        Task<Ticket?> GetTicketByTicketIdAsync(int ticketId);

        Task<List<Ticket>> GetPendingTicketsAsync();

        Task<RaceDog> GetRaceDogByIdAsync(int raceId);

        Task<bool> UpdateRaceDogWinnerAsync(int raceId, int dogId);

        Task<List<Ticket>> GetActiveTicketsAsync();
    }
}
