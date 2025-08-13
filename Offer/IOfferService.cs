using PawRace.Models.DTO;

namespace Offer
{
    public interface IOfferService
    {
       Task<List<Dog>> GetDogsAsync();

        Task EnsureMinimumScheduledRacesAsync(int minimumCount, TimeSpan interval);

        Task FinishRacesAsync();

        Task ActivateScheduledRacesAsync();

        Task DetermineWinner();
        Task<List<PawRace.Models.DTO.RacingDog>> GetUpcomingRacesAsync();

        Task<PawRace.Models.DTO.Race> GetRaceByIdAsync(int raceId);

    }
}
