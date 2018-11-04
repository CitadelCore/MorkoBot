using LibWorldBuilder.Constructs;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibWorldBuilder.World
{
    class BaseCell : IWorldCell
    {
        protected WorldPosition worldPosition;
        protected IWorldObject worldObject;
        public BaseCell(WorldPosition pos, IWorldObject obj)
        {
            worldPosition = pos;
            worldObject = obj;
        }
        public IWorldObject GetUnderlyingObject()
        {
            return worldObject;
        }

        public WorldPosition GetWorldPosition()
        {
            return worldPosition;
        }
    }
}
