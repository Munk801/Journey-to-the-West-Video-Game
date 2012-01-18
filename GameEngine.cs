using System;
using System.Collections.Generic;

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
    public class GameEngine : GameWindow
    {
        /**
         * Only the current state can know when it’s time to change to the next state.
         * 
         * 
         * HandleEvents()
         * Update()
         * Draw()
         * will call the corresponding methods in the State class that is on the "stack" List<>
         * 
         * */

        /** Init and Cleanup are to be used to initialize the Game and Cleanup the game when done **/
        public void Init()
        {
            states = new Stack<GameState>();

            // Set the running flag
            m_running = true;

            // Set the screen resolution (Fullscreen / windowed)

            // Set the title bar of the window etc
        }

        public void Cleanup()
        {
            while (states.Count > 0)
            {
                // Pop off the top of the Stack and clean it up
                GameState st = states.Pop();
                st.Cleanup();
            }
        }


        /** These 3 methods are for State handling **/
        public void ChangeState(GameState state)
        {
            // Cleanup the current state
            if (states.Count != 0)
            {
                GameState st = states.Pop();
                st.Cleanup();
            }

            // Store and INIT the new state
            states.Push(state);
            state.Init();
        }

        public void PushState(GameState state)
        {
            // pause the current state
            if (states.Count != 0)
            {
                GameState st = states.Peek();
                st.Pause();
            }

            // store and INIT the new state
            states.Push(state);
            state.Init();
        }

        public void PopState()
        {
            // cleanup the current state
            if (states.Count != 0)
            {
                GameState st = states.Peek();
                st.Cleanup();
                states.Pop();
            }

            // resume the previous state
            if (states.Count != 0)
            {
                GameState st = states.Peek();
                st.Resume();
            }
        }

        /** These are the methods to process game stuff most likely where the game logic or calls will go**/
        public void HandleEvents()
        {
            // let the state handle events
            states.Peek().HandleEvents(this);
        }

        public void Update()
        {
            // let the state update the game
            states.Peek().Update(this);
        }

        public void Draw()
        {
            // let the state draw the screen
            states.Peek().Draw(this);
        }


        /** Use these 2 methods in the Main() loop in EntryPoint to check if the game is still running and to Quit**/
        public bool Running()
        {
            return m_running;
        }

        public void Quit()
        {
            m_running = false;
        }
             
        // This is the "Stack" of states with an LIFO structure mimicing an actual memory Stack
        Stack<GameState> states;

        // FLAG that tells if the game is running or not
        bool m_running;
    }
}
