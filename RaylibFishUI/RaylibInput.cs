using FishUI;
using Raylib_cs;
using System;
using System.Numerics;
using System.Collections.Generic;

namespace RaylibFishGfx
{
    public class RaylibInput : IFishUIInput
    {
        private readonly Dictionary<int, Vector2> _previousTouches = new Dictionary<int, Vector2>();
        public FishKey GetKeyPressed()
        {
            int K = Raylib.GetKeyPressed();
            if (K == 0)
                return FishKey.None;

            return (FishKey)K;
        }

        public int GetCharPressed()
        {
            return Raylib.GetCharPressed();
        }

        public Vector2 GetMousePosition()
        {
            return Raylib.GetMousePosition();
        }

        public float GetMouseWheelMove()
        {
            return Raylib.GetMouseWheelMove();
        }

        public FishTouchPoint[] GetTouchPoints()
        {
            int count = Raylib.GetTouchPointCount();
            if (count <= 0 && _previousTouches.Count == 0)
                return System.Array.Empty<FishTouchPoint>();

            var current = new Dictionary<int, Vector2>(Math.Max(0, count));
            var points = new List<FishTouchPoint>(Math.Max(count, _previousTouches.Count));
            for (int i = 0; i < count; i++)
            {
                int id = Raylib.GetTouchPointId(i);
                Vector2 position = Raylib.GetTouchPosition(i);
                current[id] = position;
                bool existed = _previousTouches.TryGetValue(id, out Vector2 previous);
                points.Add(new FishTouchPoint
                {
                    Id = id,
                    Position = position,
                    Delta = existed ? position - previous : Vector2.Zero,
                    TouchType = existed ? FishTouchType.Motion : FishTouchType.Press
                });
            }

            foreach (KeyValuePair<int, Vector2> previous in _previousTouches)
            {
                if (current.ContainsKey(previous.Key)) continue;
                points.Add(new FishTouchPoint
                {
                    Id = previous.Key,
                    Position = previous.Value,
                    Delta = Vector2.Zero,
                    TouchType = FishTouchType.Release
                });
            }

            _previousTouches.Clear();
            foreach (KeyValuePair<int, Vector2> touch in current)
                _previousTouches.Add(touch.Key, touch.Value);
            return points.Count == 0 ? System.Array.Empty<FishTouchPoint>() : points.ToArray();
        }

        public bool IsKeyDown(FishKey Key)
        {
            return Raylib.IsKeyDown((KeyboardKey)Key);
        }

        public bool IsKeyPressed(FishKey Key)
        {
            return Raylib.IsKeyPressed((KeyboardKey)Key);
        }

        public bool IsKeyReleased(FishKey Key)
        {
            return Raylib.IsKeyReleased((KeyboardKey)Key);
        }

        public bool IsKeyUp(FishKey Key)
        {
            return Raylib.IsKeyUp((KeyboardKey)Key);
        }

        public bool IsMouseDown(FishMouseButton Button)
        {
            return Raylib.IsMouseButtonDown((MouseButton)Button);
        }

        public bool IsMousePressed(FishMouseButton Button)
        {
            return Raylib.IsMouseButtonPressed((MouseButton)Button);
        }

        public bool IsMouseReleased(FishMouseButton Button)
        {
            return Raylib.IsMouseButtonReleased((MouseButton)Button);
        }

        public bool IsMouseUp(FishMouseButton Button)
        {
            return Raylib.IsMouseButtonUp((MouseButton)Button);
        }

        public string GetClipboardText()
        {
            return Raylib.GetClipboardText_();
        }

        public void SetClipboardText(string text)
        {
            Raylib.SetClipboardText(text ?? "");
        }
    }
}
