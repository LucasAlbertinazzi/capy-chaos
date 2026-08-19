using UnityEngine;

namespace CapyChaos.Input
{
    public enum SwipeDirection
    {
        None,
        Left,
        Right,
        Up,
        Down
    }

    public struct RunnerCommand
    {
        public int LaneDelta;
        public bool Jump;
        public bool Slide;
        public bool DebugZoomies;
    }

    public sealed class RunnerInput
    {
        private const float MinimumSwipeDistance = 55f;
        private const float ScreenSwipeRatio = 0.06f;
        private const float MaximumSwipeDuration = 1.25f;
        private Vector2 touchStart;
        private float touchStartTime;
        private bool trackingTouch;

        public RunnerCommand Read()
        {
            RunnerCommand command = new RunnerCommand
            {
                LaneDelta = ReadKeyboardLane(),
                Jump = UnityEngine.Input.GetKeyDown(KeyCode.W) || UnityEngine.Input.GetKeyDown(KeyCode.UpArrow),
                Slide = UnityEngine.Input.GetKeyDown(KeyCode.S) || UnityEngine.Input.GetKeyDown(KeyCode.DownArrow),
                DebugZoomies = UnityEngine.Input.GetKeyDown(KeyCode.LeftShift)
            };

            ReadTouch(ref command);
            return command;
        }

        private static int ReadKeyboardLane()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.A) || UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow))
            {
                return -1;
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.D) || UnityEngine.Input.GetKeyDown(KeyCode.RightArrow))
            {
                return 1;
            }

            return 0;
        }

        private void ReadTouch(ref RunnerCommand command)
        {
            if (UnityEngine.Input.touchCount == 0)
            {
                trackingTouch = false;
                return;
            }

            Touch touch = UnityEngine.Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                touchStart = touch.position;
                touchStartTime = Time.unscaledTime;
                trackingTouch = true;
                return;
            }

            if (!trackingTouch || (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled))
            {
                return;
            }

            trackingTouch = false;
            Vector2 swipe = touch.position - touchStart;
            float threshold = Mathf.Max(MinimumSwipeDistance, Mathf.Min(Screen.width, Screen.height) * ScreenSwipeRatio);
            if (Time.unscaledTime - touchStartTime > MaximumSwipeDuration)
            {
                return;
            }

            ApplySwipe(InterpretSwipe(swipe, threshold), ref command);
        }

        public static SwipeDirection InterpretSwipe(Vector2 swipe, float minimumDistance)
        {
            if (swipe.sqrMagnitude < minimumDistance * minimumDistance)
            {
                return SwipeDirection.None;
            }

            if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
            {
                return swipe.x > 0f ? SwipeDirection.Right : SwipeDirection.Left;
            }

            return swipe.y > 0f ? SwipeDirection.Up : SwipeDirection.Down;
        }

        private static void ApplySwipe(SwipeDirection direction, ref RunnerCommand command)
        {
            switch (direction)
            {
                case SwipeDirection.Left:
                    command.LaneDelta = -1;
                    break;
                case SwipeDirection.Right:
                    command.LaneDelta = 1;
                    break;
                case SwipeDirection.Up:
                    command.Jump = true;
                    break;
                case SwipeDirection.Down:
                    command.Slide = true;
                    break;
            }
        }
    }
}
