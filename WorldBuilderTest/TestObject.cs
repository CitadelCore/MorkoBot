using LibWorldBuilder.Constructs.Render;
using LibWorldBuilder.World.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldBuilderTest
{
    class TestObject : BaseObject, IConsoleRenderable
    {
        public char GetRenderableChar()
        {
            return 'T';
        }
    }
}
