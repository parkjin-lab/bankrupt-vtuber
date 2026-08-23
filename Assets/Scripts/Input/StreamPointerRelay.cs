using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BankruptVtuber
{
    /// <summary>
    /// Pointer/touch for LiveStream while StandaloneInputModule is disabled.
    /// Uses this canvas GraphicRaycaster only — no UI navigation.
    /// </summary>
    public class StreamPointerRelay : MonoBehaviour
    {
        GraphicRaycaster _raycaster;
        readonly List<RaycastResult> _hits = new List<RaycastResult>();
        StreamPadButton _held;
        int _heldFinger = -1;

        void Awake()
        {
            _raycaster = GetComponent<GraphicRaycaster>();
        }

        void Update()
        {
            if (UnityEngine.Input.touchCount > 0)
            {
                for (int i = 0; i < UnityEngine.Input.touchCount; i++)
                {
                    var touch = UnityEngine.Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Began)
                        Down(touch.position, touch.fingerId);
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                        Up(touch.fingerId);
                }
                return;
            }

            if (UnityEngine.Input.GetMouseButtonDown(0))
                Down(UnityEngine.Input.mousePosition, -2);
            if (UnityEngine.Input.GetMouseButtonUp(0))
                Up(-2);
        }

        void Down(Vector2 screen, int id)
        {
            var btn = Hit(screen);
            if (btn == null)
                return;
            if (_held != null)
                _held.Release();
            _held = btn;
            _heldFinger = id;
            btn.Press();
        }

        void Up(int id)
        {
            if (_held == null)
                return;
            if (_heldFinger != id)
                return;
            _held.Release();
            _held = null;
            _heldFinger = -1;
        }

        StreamPadButton Hit(Vector2 screen)
        {
            if (_raycaster != null && EventSystem.current != null)
            {
                var ped = new PointerEventData(EventSystem.current) { position = screen };
                _hits.Clear();
                _raycaster.Raycast(ped, _hits);
                for (int i = 0; i < _hits.Count; i++)
                {
                    var btn = _hits[i].gameObject.GetComponent<StreamPadButton>();
                    if (btn == null)
                        btn = _hits[i].gameObject.GetComponentInParent<StreamPadButton>();
                    if (btn != null && btn.isActiveAndEnabled)
                        return btn;
                }
            }

            var pads = GetComponentsInChildren<StreamPadButton>(false);
            for (int i = 0; i < pads.Length; i++)
            {
                var rt = pads[i].transform as RectTransform;
                if (rt == null || !pads[i].isActiveAndEnabled)
                    continue;
                if (RectTransformUtility.RectangleContainsScreenPoint(rt, screen, null))
                    return pads[i];
            }
            return null;
        }
    }
}
