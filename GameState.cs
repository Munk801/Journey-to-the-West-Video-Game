using System;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using Engine;
using Engine.Input;

namespace U5Designs
{
    /** Base State class that other States will inherit from such as MenuState, GameState, etc 
     * 
     * A game state also needs to be able to load graphics and initialize itself,
     * as well as clean up resources when it’s done. Also, there are times when we will want to pause a state,
     * and resume it at a later time. An example of this would be pausing the game state to bring up a menu on top of it.
     * 
     * **/

    /**
     *  If you use an in game menu STATE then you need to have that state POP itself off the game engines state stack in the 
     *  HandleEvents() method of the in game menu state
     * */
    public abstract class GameState
    {
        public virtual void Init(GameEngine eng)
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

        public virtual void HandleEvents(GameEngine eng)
        {
        }

        public virtual void Update(GameEngine gameEng, FrameEventArgs e)
        {
        }

        public virtual void Draw(GameEngine gameEng, FrameEventArgs e)
        {
        }

        void ChangeState(GameEngine eng, GameState state)
        {
            eng.ChangeState(state);
        }

        public virtual void updateView(GameEngine eng)
        {
        }
    }
}
