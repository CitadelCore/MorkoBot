using System;
using System.Collections.Generic;
using System.Text;

namespace LibWorldBuilder.Constructs
{
    /// <summary>
    /// Represents a game world, which includes several linked rooms.
    /// </summary>
    public interface IWorld
    {
        IEnumerable<IRoom> GetRooms();
    }
}