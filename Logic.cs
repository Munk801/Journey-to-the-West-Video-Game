using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Input;

namespace U5Designs
{
    class Logic
    {
        /*
         * This class is where 90% of the logic will happen each update.
         */
        Game game;
        Player player;

        bool spaceDown, a, s, d;

        // initilize
        public Logic(Game me)
        {
            game = me;
            player = new Player();

        }

        
        public void update(FrameEventArgs e)
        {
            DealWithKeys();
            // foreach enemy do ai
            // update player stuff, etc..

        }


        //if its inconvenent to have key detection outside of the update method, move it back in
        private void DealWithKeys()
        {
            //TODO: Change these keys to their final mappings when determined
            
            if (game.Keyboard[Key.Escape])
            {
                game.Exit();
            }
            //********************** space
            if (game.Keyboard[Key.Space] && !spaceDown)
            {
                game.enable3d = !game.enable3d;
                game.updateView();
                spaceDown = true;
            }
            else if (!game.Keyboard[Key.Space])
            {
                spaceDown = false;
            }

            //********************** a
            if (game.Keyboard[Key.A] && !a)
            {
                Console.Out.WriteLine("a pushed");
                a = true;
            }
            else if (!game.Keyboard[Key.A])
            {
                a = false;
            }

            //*********************** s
            if (game.Keyboard[Key.S] && !s)
            {
                Console.Out.WriteLine("s pushed");
                s = true;
            }
            else if (!game.Keyboard[Key.S])
            {
                s = false;
            }
            //********************** d
            if (game.Keyboard[Key.D] && !d)
            {
                Console.Out.WriteLine("d pushed");
                d = true;
            }
            else if (!game.Keyboard[Key.D])
            {
                d = false;
            }
        }
    }

}
