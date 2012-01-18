using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace U5Designs
{
    /** Main Menu State of the game that will be active while the Main Menu is up **/
    class MainMenuState : GameState
    {
        // Initialize graphics, etc here
        public override void Init()
        {
        }

        // Cleanup anything here like freeing up graphics you loaded in Init
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
            // Handle keyboard and other events here during the Main Menu state

            // If the player decides to quit here then quit
            //eng.Quit();

            // Otherwise if the player starts up the game then move to the next state
            PlayState ps = new PlayState();
            eng.ChangeState(ps);
        }

        public override void Update(GameEngine eng)
        {
        }

        public override void Draw(GameEngine eng)
        {
        }        
    }
}
