using System;
using System.Collections.Generic;
using System.Text;

namespace LibWorldBuilder.Constructs
{
    public interface IWorldCell
    {
        IWorldObject GetUnderlyingObject();
        WorldPosition GetWorldPosition();
    }
}
