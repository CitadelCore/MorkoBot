using LibWorldBuilder.Constructs;
using LibWorldBuilder.Utilities;
using LibWorldBuilder.World.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldBuilderTest
{
    class WorldBuilderTest
    {
        static void Main(string[] args)
        {
            IRoom room = new BaseRoom(new WorldPosition(10, 10));
            room.SetCellsVector(new WorldPosition(0, 0), new WorldPosition(10, 10), new TestObject());

            string[] ar = TextRenderer.RenderRoom(room);

            foreach (string line in ar)
            {
                Console.WriteLine(line);
            }

            Console.ReadLine();
        }
    }
}
