namespace PawRace.Models.DTO;

    public class RaceDetails
    {
        public int Id { get; set; }

        public DateTime ScheduledAt { get; set; }

        public int RaceStatus { get; set; }

        public List<RacingDog> RacingDogs { get; set; } = null!;
    }

