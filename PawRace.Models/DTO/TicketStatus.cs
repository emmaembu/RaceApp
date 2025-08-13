using PawRace.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawRace.Models.DTO
{
    public class TicketStatus
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public decimal PotentialWinning { get; set; }

        public string Status { get; set; } = string.Empty;

        public string NickName { get; set; } = string.Empty;

        public string IpAddress { get; set; } = string.Empty;
    }
}
