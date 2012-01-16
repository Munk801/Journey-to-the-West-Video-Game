using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Engine.Input
{
    public class Keyboard
    {
        [DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        Control _openGLControl;
        public KeyPressEventHandler KeyPressEvent;

        class KeyState
        {
            bool isKeyPressed = false;

            public bool isHeld { get; set; }
            public bool isPressed { get; set; }

            public KeyState()
            {
                isHeld = false;
                isPressed = false;
            }

            internal void onDown()
            {
                if (isHeld == false)
                    isKeyPressed = true;

                isHeld = true;
            }

            internal void onUp()
            {
                isHeld = false;
            }

            internal void Press()
            {
                isPressed = false;
                if (isKeyPressed)
                {
                    isPressed = true;
                    isKeyPressed = false;
                }
            }
        }
        // Create the dictionary of our keys matched to being pressed
        Dictionary<Keys, KeyState> keyStates = new Dictionary<Keys, KeyState>();

        public Keyboard(Control openGlControl)
        {
            _openGLControl = openGlControl;
            _openGLControl.KeyDown += new KeyEventHandler(OnKeyDown);
            _openGLControl.KeyUp += new KeyEventHandler(OnKeyUp);
            _openGLControl.KeyPress += new KeyPressEventHandler(OnKeyPress);
        }

        void OnKeyDown(object sender, KeyEventArgs e)
        {
            AddKeyState(e.KeyCode);
            keyStates[e.KeyCode].onDown();
        }

        void OnKeyUp(object sender, KeyEventArgs e)
        {
            AddKeyState(e.KeyCode);
            keyStates[e.KeyCode].onUp();
        }

        void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            if (KeyPressEvent != null)
            {
                KeyPressEvent(sender, e);
            }
        }

        // Called to add a key to the dictionary
        private void AddKeyState(Keys k)
        {
            if (!keyStates.Keys.Contains(k))
            {
                keyStates.Add(k, new KeyState());
            }
        }

        // Functions that return the local bool var
        public bool IsKeyPressed(Keys key)
        {
            AddKeyState(key);
            return keyStates[key].isPressed;
        }

        public bool IsKeyHeld(Keys key)
        {
            AddKeyState(key);
            return keyStates[key].isHeld;
        }

        // Used for asynchronous key detection
        private bool asynchKeyPress(Keys key)
        {
            return (GetAsyncKeyState((int)key) != 0);
        }

        // Track all the controls that we will need.
        private void ProcessKeyGameControls()
        {
            UpdateKeyGameControls(Keys.W);
            UpdateKeyGameControls(Keys.A);
            UpdateKeyGameControls(Keys.S);
            UpdateKeyGameControls(Keys.D);

            // Process everything
            foreach (KeyState state in keyStates.Values)
            {
                state.isPressed = false;
                state.Press();
            }
        }

        // Handles the update of the keys
        private void UpdateKeyGameControls(Keys keys)
        {
            // If the asynchKeyPress returns true, the user pressed a key and we need to fire the keyDown event
            if (asynchKeyPress(keys))
            {
                OnKeyDown(this, new KeyEventArgs(keys));
            }
            //Else fire the key up event
            else
            {
                OnKeyUp(this, new KeyEventArgs(keys));
            }   
        }
    }
}
