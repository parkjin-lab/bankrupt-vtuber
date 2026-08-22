using UnityEngine;

namespace BankruptVtuber
{
    public static class StreamBindings
    {
        /// <summary>
        /// A/S/D/F tap once. Space superchat commits once on release (hold-to-charge).
        /// Holding Space must not poll Thanks every frame.
        /// </summary>
        public static bool TryConsumeKind(out ChatKind kind, out bool hold)
        {
            hold = false;
            if (UnityEngine.Input.GetKeyDown(KeyCode.A))
            {
                kind = ChatKind.Positive;
                return true;
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.S))
            {
                kind = ChatKind.Empathy;
                return true;
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.D))
            {
                kind = ChatKind.Laugh;
                return true;
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.F))
            {
                kind = ChatKind.Thanks;
                return true;
            }

            // One resolve per press: charge while held, commit on release.
            if (UnityEngine.Input.GetKeyUp(KeyCode.Space))
            {
                kind = ChatKind.Thanks;
                return true;
            }

            kind = default;
            return false;
        }

        public static bool SuperchatCharging =>
            UnityEngine.Input.GetKey(KeyCode.Space);

        public static bool EventStubPressed(out int index)
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad1))
            {
                index = 1;
                return true;
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad2))
            {
                index = 2;
                return true;
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad3))
            {
                index = 3;
                return true;
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha4) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad4))
            {
                index = 4;
                return true;
            }
            index = 0;
            return false;
        }

        public static bool Confirm =>
            UnityEngine.Input.GetKeyDown(KeyCode.Space) ||
            UnityEngine.Input.GetKeyDown(KeyCode.Return);
    }
}
