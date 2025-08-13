using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PawRace.Models.Enums;

namespace PawRace.Models.DTO
{
    public class BetTicket
    {
        public string NickName { get; set; } = null!;

        public string IpAddress { get; set; } = null!;

        public List<RaceDogSelection> raceDogSelections { get; set; } = new();

        public decimal Amount { get; set; }

    }
}
