using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    /** Base State class that other States will inherit from such as MenuState, GameState, etc **/
    public abstract class GameState
    {
        public virtual void Init()
        {
        }

        public virtual void Cleanup()
        {
        }

        public virtual void Pause()
        {
        }

        public virtual void Resume()
        {
        }

        public virtual void HandleEvents()
        {
        }

        public virtual void Update()
        {
        }

        public virtual void Draw()
        {
        }
    }
}
