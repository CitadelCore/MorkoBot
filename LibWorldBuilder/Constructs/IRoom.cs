using System;
using System.Collections.Generic;
using System.Text;

namespace LibWorldBuilder.Constructs
{
    /// <summary>
    /// Defines a world room, containing a specific number of cells.
    /// </summary>
    public interface IRoom
    {
        IEnumerable<IWorldCell> GetCellsInRoom();
        void SetCellAtPosition(WorldPosition pos, IWorldObject obj);
        void SetCellsVector(WorldPosition min, WorldPosition max, IWorldObject obj);
        IWorldObject GetObjectAtPosition(WorldPosition pos);
        WorldPosition MaxPos { get; }
    }
}
