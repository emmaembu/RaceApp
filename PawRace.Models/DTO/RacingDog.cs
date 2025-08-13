namespace PawRace.Models.DTO;

    public class RacingDog
    {
        public int DogId  { get; set; }

        public int RaceId { get; set; }
        public int StartPosition { get; set; } 
        public decimal Odds { get; set; }
        public bool IsWinner{ get; set; }
    }

