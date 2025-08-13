using PawRace.Models.Enums;

namespace PawRace.Models.DTO;

    public class Race
    {
        public int Id { get; set; }
        public DateTime ScheduledAt { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime FinishedAt { get; set; }
        public int RaceStatus { get; set; }

    }

