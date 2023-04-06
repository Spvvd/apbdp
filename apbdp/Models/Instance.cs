using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace apbdp.Models
{
    class Instance
    {
        public int Id { get; set; }
        public int MapId { get; set; }
        public string Map { get; set; }
        public string stateText { get; set; }
        public string imageLarge { get; set; }
        public int UId { get; set; }
        public int Enforcers { get; set; }
        public int Criminals { get; set; }
        public int Status { get; set; }
        public int QueueSize { get; set; }


        public void ParseMapId(string line)
        {
            // Gets characters in brackets as string, splits the string by a comma and
            // returns the first (MapId) in array
            this.MapId = Int32.Parse(
                Regex.Match(line, @"\(([^)]*)\)").Groups[1].Value.Split(',').First());
        }

        public void ParseUId(string line)
        {
            // Gets characters in brackets as string, splits the string by a comma and
            // returns the last (TypeId) in array 
            this.UId = Int32.Parse(
                Regex.Match(line, @"\(([^)]*)\)").Groups[1].Value.Split(',').Last());
        }

        public int ParseLastAsInteger(string line)
        {
            // Splits string by spaces and returns last in array as integer
            return Int32.Parse(line.Split(new char[0]).Last());
        }

        public void SetInstanceDetails()
        {
            switch (this.MapId)
            {
                case 1:
                case 4:
                    InstanceDetail("Financial", "Roaming", "fin_large"); break;
                case 3:
                    InstanceDetail("Financial", "In Event District", "fin_large"); break;
                case 10:
                    InstanceDetail("Abington Towers", "In Fight Club", "as_large"); break;
                case 14:
                    InstanceDetail("Baylan Shipping Storage", "In Fight Club", "bs_large"); break;
                case 16:
                    InstanceDetail("Breakwater Marina", "Roaming", "bm_large"); break;
                case 28:
                    InstanceDetail("Waterfront", "In Event District", "wf_large"); break;
                case 26:
                case 29:
                    InstanceDetail("Waterfront", "Roaming", "wf_large"); break;
                case 42:
                    InstanceDetail("Financial", "In RIOT District", "fin_large"); break;
            }
        }

        private void InstanceDetail(string mapName, string stateText, string imageImage)
        {
            this.Map = InstanceMapName(mapName);
            this.stateText = stateText;
            this.imageLarge = imageImage;
        }

        private string InstanceMapName(string mapName)
        {
            return $"{mapName}-{this.UId}";
        }
    }
}
