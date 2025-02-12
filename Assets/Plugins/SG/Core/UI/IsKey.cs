using UnityEngine;

namespace SG.UI
{
    public static class IsKeyUp
    {
        public static bool Number(out int number)
        {
            number = -1;

            if (Input.GetKeyUp(KeyCode.Keypad1) || Input.GetKeyUp(KeyCode.Alpha1))
            { number = 1; return true; }
            else if (Input.GetKeyUp(KeyCode.Keypad2) || Input.GetKeyUp(KeyCode.Alpha2))
            { number = 2; return true; }
            else if (Input.GetKeyUp(KeyCode.Keypad3) || Input.GetKeyUp(KeyCode.Alpha3))
            { number = 3; return true; }
            else if (Input.GetKeyUp(KeyCode.Keypad4) || Input.GetKeyUp(KeyCode.Alpha4))
            { number = 4; return true; }
            else if (Input.GetKeyUp(KeyCode.Keypad5) || Input.GetKeyUp(KeyCode.Alpha5))
            { number = 5; return true; }
            else if (Input.GetKeyUp(KeyCode.Keypad6) || Input.GetKeyUp(KeyCode.Alpha6))
            { number = 6; return true; }
            else if (Input.GetKeyUp(KeyCode.Keypad7) || Input.GetKeyUp(KeyCode.Alpha7))
            { number = 7; return true; }
            else if (Input.GetKeyUp(KeyCode.Keypad8) || Input.GetKeyUp(KeyCode.Alpha8))
            { number = 8; return true; }
            else if (Input.GetKeyUp(KeyCode.Keypad9) || Input.GetKeyUp(KeyCode.Alpha9))
            { number = 9; return true; }
            else if (Input.GetKeyUp(KeyCode.Keypad0) || Input.GetKeyUp(KeyCode.Alpha0))
            { number = 0; return true; }

            return false;
        }
    }

    public static class IsKeyHold
    {
        public static bool control => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        public static bool command => Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
        public static bool tab => Input.GetKey(KeyCode.Tab);

        public static bool Number(byte number)
        {
            switch (number)
            {
                case 1:
                    return Input.GetKey(KeyCode.Keypad1) || Input.GetKey(KeyCode.Alpha1);
                case 2:
                    return Input.GetKey(KeyCode.Keypad2) || Input.GetKey(KeyCode.Alpha2);
                case 3:
                    return Input.GetKey(KeyCode.Keypad3) || Input.GetKey(KeyCode.Alpha3);
                case 4:
                    return Input.GetKey(KeyCode.Keypad4) || Input.GetKey(KeyCode.Alpha4);
                case 5:
                    return Input.GetKey(KeyCode.Keypad5) || Input.GetKey(KeyCode.Alpha5);
                case 6:
                    return Input.GetKey(KeyCode.Keypad6) || Input.GetKey(KeyCode.Alpha6);
                case 7:
                    return Input.GetKey(KeyCode.Keypad7) || Input.GetKey(KeyCode.Alpha7);
                case 8:
                    return Input.GetKey(KeyCode.Keypad8) || Input.GetKey(KeyCode.Alpha8);
                case 9:
                    return Input.GetKey(KeyCode.Keypad9) || Input.GetKey(KeyCode.Alpha9);
                case 0:
                    return Input.GetKey(KeyCode.Keypad0) || Input.GetKey(KeyCode.Alpha0);
                default:
                    return false;
            }
        }

        public static bool OnlyNumber(byte[] numbers)
        {
            foreach (var number in new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 })
            {
                var isNeedPress = numbers.Contains(number);
                var isPressed = Number(number);
                if ((isNeedPress && !isPressed) || (!isNeedPress && isPressed))
                    return false;
            }

            return true;
        }

        public static bool Keys(KeyCode[] keys)
        {
            foreach (var key in keys)
                if (!Input.GetKey(key))
                    return false;
            return true;
        }
    }

    public static class IsKeyDown
    {
        public static bool esc => Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton0);
        public static bool enter => Input.GetKeyDown(KeyCode.Return);
        public static bool tab => Input.GetKeyDown(KeyCode.Tab);
        public static bool delete => Input.GetKeyDown(KeyCode.Delete);

        public static bool control => Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);
        public static bool command => Input.GetKeyDown(KeyCode.LeftCommand) || Input.GetKeyDown(KeyCode.RightCommand);

        public static bool copy => Utils.isMac ? command : control;

        public static bool a => Input.GetKeyDown(KeyCode.A);
        public static bool b => Input.GetKeyDown(KeyCode.B);
        public static bool c => Input.GetKeyDown(KeyCode.C);
        public static bool l => Input.GetKeyDown(KeyCode.L);
        public static bool p => Input.GetKeyDown(KeyCode.P);
        public static bool s => Input.GetKeyDown(KeyCode.S);
        public static bool q => Input.GetKeyDown(KeyCode.Q);
        public static bool t => Input.GetKeyDown(KeyCode.T);

        public static bool F11 => Input.GetKeyDown(KeyCode.F11);
        public static bool F12 => Input.GetKeyDown(KeyCode.F12);

        public static bool Number(out int number)
        {
            number = -1;

            if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1))
            { number = 1; return true; }
            else if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2))
            { number = 2; return true; }
            else if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3))
            { number = 3; return true; }
            else if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4))
            { number = 4; return true; }
            else if (Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Alpha5))
            { number = 5; return true; }
            else if (Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Alpha6))
            { number = 6; return true; }
            else if (Input.GetKeyDown(KeyCode.Keypad7) || Input.GetKeyDown(KeyCode.Alpha7))
            { number = 7; return true; }
            else if (Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.Alpha8))
            { number = 8; return true; }
            else if (Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.Alpha9))
            { number = 9; return true; }
            else if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0))
            { number = 0; return true; }

            return false;
        }

        //public bool isPress_Touch { get { return Input.touchCount > 0; } }
        //public bool isPress_MouseButton { get { return Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2); } }
        //public bool isPress_AnyKey { get { return isPress_MouseButton || isPress_Touch || isPress_Esc; } }
        //public bool isPress_AltPlus { get { return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt); } }
    }
}