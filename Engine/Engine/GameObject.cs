using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/* Handles the objects in a game.  These can include game states and other objects.  The main functionality of 
 * a game object is to update and to render to the screen. */
namespace Engine
{
    public interface GameObject
    {

        void Update(double elapsedTime);
        void Render();
    }
}
