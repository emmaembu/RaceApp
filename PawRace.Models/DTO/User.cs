namespace PawRace.Models.DTO
{
    public class User
    {
        public Guid GuidId { get; set; }
        public string NickName { get; set; } = null!;

        public string IpAddress { get; set; } = null!;

        public ICollection<BetTicket> Tickets { get; set; } = new List<BetTicket>();
    }
}
