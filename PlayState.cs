using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace U5Designs
{
    /** Main State of the game that will be active while the player is Playing **/
    class PlayState : GameState
    {
        // Initialize graphics here for the game playing
        public override void Init()
        {
        }

        // Cleanup any resources you created here
        public override void Cleanup()
        {
        }

        public override void Pause()
        {
        }

        public override void Resume()
        {
        }

        public override void HandleEvents(GameEngine eng)
        {
            // Handle in game events here or make calls out to something that will deal with them

            // Quit if the player chooses to
            //eng.Quit();

            // If the player asks for the in game menu load it here
            //InGameMenuState is = new InGameMenuState();
            //eng.PushState(is);
        }

        public override void Update(GameEngine eng)
        {
        }

        public override void Draw(GameEngine eng)
        {
        }
    }
}
