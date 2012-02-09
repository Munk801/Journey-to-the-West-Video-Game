﻿using System;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using Engine;
using Engine.Input;
using System.Collections.Generic;

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
	public abstract class GameState {
		//Note!  MakeActive() is called every time the state is activated or re-activated.
		//One time only things should be in the constructor, not in MakeActive()
		public virtual void MakeActive() { }

        public virtual void Update(FrameEventArgs e)
        {
        }

        public virtual void Draw(FrameEventArgs e)
        {
        }

        void ChangeState(GameEngine eng, GameState state)
        {
            eng.ChangeState(state);
        }

        public virtual void loadGameObjects()
        {            
        }       
    }
}
