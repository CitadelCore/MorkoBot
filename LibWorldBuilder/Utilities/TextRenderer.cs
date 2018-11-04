using LibWorldBuilder.Constructs;
using LibWorldBuilder.Constructs.Render;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibWorldBuilder.Utilities
{
    public class TextRenderer
    {
        /// <summary>
        /// Renders a room in text from a room object.
        /// </summary>
        public static string[] RenderRoom(IRoom room)
        {
            string rowPad = string.Concat(Enumerable.Repeat("#", room.MaxPos.XPos + 2));
            IList<string> roomRender = new List<string>
            {
                rowPad
            };

            for (int i1 = 0; i1 < room.MaxPos.YPos; i1++)
            {
                StringBuilder rowRender = new StringBuilder();

                IList<IWorldCell> rowCells = new List<IWorldCell>(room.GetCellsInRoom()).FindAll(c => (c.GetWorldPosition().YPos == i1));

                foreach (IWorldCell cell in rowCells)
                {
                    IWorldObject obj = cell.GetUnderlyingObject();

                    if (obj.GetType() != typeof(IConsoleRenderable))
                    {
                        IConsoleRenderable render = (IConsoleRenderable) obj;
                        rowRender.Append(render.GetRenderableChar());
                    }
                }

                roomRender.Add("#" + rowRender.ToString() + "#");
            }

            roomRender.Add(rowPad);
            return roomRender.ToArray();
        }
    }
}
