using UnityEngine;

namespace BankruptVtuber
{
    public static class StreamBindings
    {
        static ChatKind? _queuedKind;
        static bool _queuedHold;
        static int _queuedEvent;
        static int _queuedPromo;
        static bool _padCharging;

        /// <summary>On-screen kind / superchat pad-down. LiveStream plays the keycap click.</summary>
        public static System.Action OnLanePadPress;

        /// <summary>
        /// Arrows are the documented map. A/S/D/F and WASD are aliases.
        /// Space/Enter superchat commits once on release (hold-to-charge).
        /// </summary>
        public static bool TryConsumeKind(out ChatKind kind, out bool hold)
        {
            if (_queuedKind.HasValue)
            {
                kind = _queuedKind.Value;
                hold = _queuedHold;
                _queuedKind = null;
                _queuedHold = false;
                return true;
            }

            hold = false;
            if (PositiveDown())
            {
                kind = ChatKind.Positive;
                return true;
            }
            if (EmpathyDown())
            {
                kind = ChatKind.Empathy;
                return true;
            }
            if (LaughDown())
            {
                kind = ChatKind.Laugh;
                return true;
            }
            if (ThanksDown())
            {
                kind = ChatKind.Thanks;
                return true;
            }

            if (UnityEngine.Input.GetKeyUp(KeyCode.Space)
                || UnityEngine.Input.GetKeyUp(KeyCode.Return)
                || UnityEngine.Input.GetKeyUp(KeyCode.KeypadEnter))
            {
                kind = ChatKind.Thanks;
                return true;
            }

            kind = default;
            return false;
        }

        public static bool SuperchatCharging =>
            _padCharging
            || UnityEngine.Input.GetKey(KeyCode.Space)
            || UnityEngine.Input.GetKey(KeyCode.Return)
            || UnityEngine.Input.GetKey(KeyCode.KeypadEnter);

        /// <summary>Keyboard kind / superchat pad-down (not release / not judge).</summary>
        public static bool LaneKeyboardPressDown() =>
            PositiveDown()
            || EmpathyDown()
            || LaughDown()
            || ThanksDown()
            || UnityEngine.Input.GetKeyDown(KeyCode.Space)
            || UnityEngine.Input.GetKeyDown(KeyCode.Return)
            || UnityEngine.Input.GetKeyDown(KeyCode.KeypadEnter);

        public static bool EventKeyPressed(out int index)
        {
            if (_queuedEvent > 0)
            {
                index = _queuedEvent;
                _queuedEvent = 0;
                return true;
            }
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

        public static bool PromoConfirmDown()
        {
            if (_queuedPromo > 0)
            {
                _queuedPromo = 0;
                return true;
            }
            return UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow)
                || UnityEngine.Input.GetKeyDown(KeyCode.UpArrow)
                || UnityEngine.Input.GetKeyDown(KeyCode.A)
                || UnityEngine.Input.GetKeyDown(KeyCode.W)
                || UnityEngine.Input.GetKeyDown(KeyCode.F);
        }

        public static bool PromoSkipDown()
        {
            if (_queuedPromo < 0)
            {
                _queuedPromo = 0;
                return true;
            }
            return UnityEngine.Input.GetKeyDown(KeyCode.RightArrow)
                || UnityEngine.Input.GetKeyDown(KeyCode.DownArrow)
                || UnityEngine.Input.GetKeyDown(KeyCode.D)
                || UnityEngine.Input.GetKeyDown(KeyCode.S);
        }

        public static void QueueKind(ChatKind kind, bool hold = false)
        {
            _queuedKind = kind;
            _queuedHold = hold;
        }

        public static void QueueEvent(int index)
        {
            if (index >= 1 && index <= 4)
                _queuedEvent = index;
        }

        public static void QueuePromo(bool confirm)
        {
            _queuedPromo = confirm ? 1 : -1;
        }

        public static void BeginSuperchatCharge()
        {
            _padCharging = true;
        }

        public static void EndSuperchatCharge()
        {
            if (!_padCharging)
                return;
            _padCharging = false;
            _queuedKind = ChatKind.Thanks;
            _queuedHold = false;
        }

        public static void DiscardLaneQueue()
        {
            _queuedKind = null;
            _queuedHold = false;
            _padCharging = false;
        }

        static bool PositiveDown() =>
            UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow)
            || UnityEngine.Input.GetKeyDown(KeyCode.A);

        static bool EmpathyDown() =>
            UnityEngine.Input.GetKeyDown(KeyCode.DownArrow)
            || UnityEngine.Input.GetKeyDown(KeyCode.S);

        static bool LaughDown() =>
            UnityEngine.Input.GetKeyDown(KeyCode.RightArrow)
            || UnityEngine.Input.GetKeyDown(KeyCode.D);

        static bool ThanksDown() =>
            UnityEngine.Input.GetKeyDown(KeyCode.UpArrow)
            || UnityEngine.Input.GetKeyDown(KeyCode.F)
            || UnityEngine.Input.GetKeyDown(KeyCode.W);
    }
}
