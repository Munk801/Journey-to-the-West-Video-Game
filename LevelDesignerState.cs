using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace U5Designs
{
    /// <summary>
    /// This state will act as the designer state for which levels can be created.
    /// The state will act just like the play state with some exceptions.
    /// - There will be no player
    /// - Everything will be assumed in a pause state
    /// - User is given master rights *can move anywhere in the level -- even out of bounds*
    /// 
    /// There are three main components
    /// - Movement - User needs the ability to move around
    /// - Adding/Removing - User needs the ability to add or remove elements from the level
    /// - Saving/Loading - User needs the ability to load or save the level that was designed
    /// 
    /// Movement
    /// WASD - Allows the user to move left and right, and zoom in and out in both 2d and 3d
    /// 
    /// Adding/Removing
    /// User will be able to select between either obstacles or enemies
    /// obstacles - ground / other objects in game
    /// enemies - placements of enemies
    /// Left Click - Place selected object in place
    /// Left Hold - if mouse is in the spot of object, move that object
    /// Right Click - Remove object in that spot
    /// 
    /// Saving/Loading
    /// User will be able to save the current state of the level
    /// As the level is being built, this will be placed in memory
    /// Once, the user saves the level, it can then be placed in file for loading later
    /// </summary>
    class LevelDesignerState
    {
    }
}
