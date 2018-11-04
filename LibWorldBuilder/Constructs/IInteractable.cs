using System;
using System.Collections.Generic;
using System.Text;

namespace LibWorldBuilder.Constructs
{
    /// <summary>
    /// A world object that a player can interact with.
    /// </summary>
    public interface IInteractable : IWorldObject
    {
        bool CanInteractWith(IPlayer player);
        void Use(IPlayer player);
    }
}
