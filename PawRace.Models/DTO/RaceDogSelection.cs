using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawRace.Models.DTO
{
    public class RaceDogSelection
    {
        public int RaceId {  get; set; }

        public int DogId { get; set; } // this should be list to implement bet on first three dogs

    }
}
