using UnityEngine;

namespace CapyChaos.Gameplay
{
    public sealed class RunnerObstacle : MonoBehaviour
    {
    }

    public sealed class CoinPickup : MonoBehaviour
    {
        private void Update()
        {
            transform.Rotate(0f, 120f * Time.deltaTime, 0f, Space.World);
        }

        public void Collect()
        {
            gameObject.SetActive(false);
        }
    }

    public sealed class ZoomiesPickup : MonoBehaviour
    {
        [field: SerializeField] public float Duration { get; private set; } = 4f;

        private void Update()
        {
            transform.Rotate(0f, 90f * Time.deltaTime, 0f, Space.World);
        }

        public void Collect()
        {
            gameObject.SetActive(false);
        }
    }
}
