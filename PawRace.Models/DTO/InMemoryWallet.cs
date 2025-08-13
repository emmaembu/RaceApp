namespace PawRace.Models.DTO;
    public class InMemoryWallet
    {
        public Guid UserGuid { get; set; }

        public decimal Balance { get; set; }

        public InMemoryWallet(Guid userGuid)
        {
            UserGuid = userGuid;
            Balance = 100M;
        }
}
