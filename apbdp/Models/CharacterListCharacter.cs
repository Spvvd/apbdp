using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apbdp.Models
{
    internal class CharacterListCharacter
    {
        public int SlotNumber { get; set; }
        public int Faction { get; set; }
        public int WorldStatus { get; set; }
        public int WorldId { get; set; }
        public string CharacterName { get; set; }
        public string WorldName { get; set; }
        public int Rating { get; set; }
    }
}
