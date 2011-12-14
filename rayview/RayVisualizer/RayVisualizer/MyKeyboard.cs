using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Input;

namespace RayVisualizer
{
    public class MyKeyboard
    {
        private Dictionary<Key, KeyState> keys;

        public MyKeyboard(KeyboardDevice keyboard)
        {
            keys = new Dictionary<Key, KeyState>();
            keyboard.KeyRepeat = false;
            keyboard.KeyDown += new EventHandler<KeyboardKeyEventArgs>(keyboard_KeyDown);
            keyboard.KeyUp += new EventHandler<KeyboardKeyEventArgs>(keyboard_KeyUp);
        }

        private void keyboard_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            keys[e.Key] = KeyState.UP;
        }

        private void keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            keys[e.Key] = KeyState.UNHANDLED;
        }

        public void MarkKeysHandled()
        {
            List<Key> toMark = new List<Key>();
            foreach (KeyValuePair<Key, KeyState> pair in keys)
                if (pair.Value == KeyState.UNHANDLED)
                {
                    toMark.Add(pair.Key);
                }
            foreach(Key  key in toMark)
                  keys[key] = KeyState.HANDLED;
        }

        public bool IsFirstPress(Key k)
        {
            return keys.ContainsKey(k) && keys[k] == KeyState.UNHANDLED;
        }

        public bool IsDown(Key k)
        {
            return keys.ContainsKey(k) && keys[k] != KeyState.UP;
        }

        private enum KeyState { UP, UNHANDLED, HANDLED }
    }
}
