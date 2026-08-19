using UnityEngine;

namespace CapyChaos.Input
{
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
        private Vector2 touchStart;
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
                trackingTouch = true;
                return;
            }

            if (!trackingTouch || (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled))
            {
                return;
            }

            trackingTouch = false;
            Vector2 swipe = touch.position - touchStart;
            if (swipe.magnitude < MinimumSwipeDistance)
            {
                return;
            }

            if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
            {
                command.LaneDelta = swipe.x > 0f ? 1 : -1;
            }
            else if (swipe.y > 0f)
            {
                command.Jump = true;
            }
            else
            {
                command.Slide = true;
            }
        }
    }
}
