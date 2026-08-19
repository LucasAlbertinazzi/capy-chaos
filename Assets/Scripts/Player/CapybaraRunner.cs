using CapyChaos.Core;
using CapyChaos.Gameplay;
using CapyChaos.Input;
using UnityEngine;

namespace CapyChaos.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class CapybaraRunner : MonoBehaviour
    {
        [SerializeField] private float laneWidth = 2.6f;
        [SerializeField] private float laneChangeSpeed = 11f;
        [SerializeField] private float jumpHeight = 2.4f;
        [SerializeField] private float gravity = -24f;
        [SerializeField] private float slideDuration = 0.75f;

        private readonly RunnerInput runnerInput = new RunnerInput();
        private CharacterController characterController;
        private GameSession session;
        private CapybaraVisual characterVisual;
        private int currentLane;
        private float verticalVelocity;
        private float slideRemaining;

        public int CurrentLane => currentLane;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            characterController.height = 2f;
            characterController.radius = 0.42f;
            characterController.center = Vector3.up;
            characterController.skinWidth = 0.04f;
            characterVisual = GetComponentInChildren<CapybaraVisual>();
        }

        private void Start()
        {
            session = FindFirstObjectByType<GameSession>();
        }

        private void Update()
        {
            if (session == null || session.State != RunState.Running)
            {
                return;
            }

            RunnerCommand command = runnerInput.Read();
            if (command.LaneDelta != 0)
            {
                currentLane = RunnerRules.ClampLane(currentLane + command.LaneDelta);
            }

            if (command.Jump && characterController.isGrounded)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            if (command.Slide && characterController.isGrounded)
            {
                slideRemaining = slideDuration;
            }

            if (command.DebugZoomies)
            {
                session.ActivateZoomies(3f);
            }

            UpdateSlide();
            MoveRunner();

            if (characterVisual != null)
            {
                characterVisual.SetState(session.IsZoomiesActive, slideRemaining > 0f);
            }
        }

        private void MoveRunner()
        {
            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;
            float targetX = currentLane * laneWidth;
            float horizontalDelta = Mathf.MoveTowards(transform.position.x, targetX, laneChangeSpeed * Time.deltaTime) - transform.position.x;
            Vector3 movement = new Vector3(horizontalDelta, verticalVelocity * Time.deltaTime, session.CurrentSpeed * Time.deltaTime);
            characterController.Move(movement);
        }

        private void UpdateSlide()
        {
            slideRemaining = Mathf.Max(0f, slideRemaining - Time.deltaTime);
            bool sliding = slideRemaining > 0f;
            characterController.height = sliding ? 1f : 2f;
            characterController.center = Vector3.up * (characterController.height * 0.5f);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.collider.GetComponent<RunnerObstacle>() != null)
            {
                session.EndRun();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            CoinPickup coin = other.GetComponent<CoinPickup>();
            if (coin != null)
            {
                session.CollectCoin();
                coin.Collect();
                return;
            }

            ZoomiesPickup boost = other.GetComponent<ZoomiesPickup>();
            if (boost != null)
            {
                session.ActivateZoomies(boost.Duration);
                boost.Collect();
            }
        }
    }
}
