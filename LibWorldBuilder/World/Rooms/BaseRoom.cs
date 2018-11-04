using LibWorldBuilder.Constructs;
using LibWorldBuilder.World.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibWorldBuilder.World.Rooms
{
    public class BaseRoom : IRoom
    {
        protected IWorldCell[,] WorldCells;
        public WorldPosition MaxPos { get; private set; }

        public BaseRoom(WorldPosition max)
        {
            MaxPos = max;

            WorldCells = new IWorldCell[MaxPos.XPos, MaxPos.YPos];
        }

        public void SetCell(IWorldCell cell)
        {
            
        }

        public IEnumerable<IWorldCell> GetCellsInRoom()
        {
            IList<IWorldCell> cells = new List<IWorldCell>();

            foreach (IWorldCell cell in WorldCells)
                cells.Add(cell);

            return cells;
        }

        /// <summary>
        /// Retrieves the object at the specified world position.
        /// </summary>
        public IWorldObject GetObjectAtPosition(WorldPosition pos)
        {
            if (WorldCells.GetValue(pos.XPos, pos.YPos) is IWorldCell cell)
                return cell.GetUnderlyingObject();

            return new BaseObject();
        }

        public void SetCellAtPosition(WorldPosition pos, IWorldObject obj)
        {
            WorldCells[pos.XPos, pos.YPos] = new BaseCell(pos, obj);
        }

        public void SetCellsVector(WorldPosition min, WorldPosition max, IWorldObject obj)
        {
            for (int x = 0; x < (max.XPos - min.XPos); x++)
            {
                for (int y = 0; y < (max.YPos - min.YPos); y++)
                {
                    WorldCells[x, y] = new BaseCell(new WorldPosition(x, y), obj);
                }
            }
        }
    }
}
