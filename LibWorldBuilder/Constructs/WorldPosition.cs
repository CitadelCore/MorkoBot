using System;
using System.Collections.Generic;
using System.Text;

namespace LibWorldBuilder.Constructs
{
    public class WorldPosition
    {
        public int XPos { get; private set; }
        public int YPos { get; private set; }

        public WorldPosition(int XPos, int YPos)
        {
            this.XPos = XPos;
            this.YPos = YPos;
        }
    }
}
