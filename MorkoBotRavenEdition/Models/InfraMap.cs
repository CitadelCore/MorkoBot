using System;
using System.Collections.Generic;
using System.Text;

namespace MorkoBotRavenEdition.Models
{
    class InfraMap
    {
        public InfraMap(string name) { Name = name; }

        public string Name { get; private set; }
        public string BspName;
        public string ThumbUrl;
        public string WikiUrl;

        // Statistics
        public int PhotoSpots;
        public int CorruptionSpots;
        public int RepairSpots;
        public int MistakeSpots;
        public int Geocaches;
        public int FlowMeters;
    }
}
