using CapyChaos.Environment;
using CapyChaos.Player;
using CapyChaos.UI;
using UnityEngine;

namespace CapyChaos.Core
{
    [DefaultExecutionOrder(-1000)]
    public sealed class CapyChaosBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsurePrototypeExists()
        {
            if (FindFirstObjectByType<CapyChaosBootstrap>() == null)
            {
                new GameObject("CAPY CHAOS - Prototype").AddComponent<CapyChaosBootstrap>();
            }
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            GameSession session = GetOrCreate<GameSession>("Game Session");
            CapybaraRunner runner = CreateRunner();
            ProceduralTrack track = GetOrCreate<ProceduralTrack>("Brazil Theme Track");
            track.Initialize(runner.transform);

            Camera camera = ConfigureCamera(runner.transform);
            ConfigureLighting();

            PrototypeHud hud = GetOrCreate<PrototypeHud>("Prototype HUD");
            hud.Initialize(session, track, runner.transform);
            camera.gameObject.AddComponent<CameraFollow>().Initialize(runner.transform);
        }

        private static CapybaraRunner CreateRunner()
        {
            GameObject player = new GameObject("Capivara");
            player.transform.position = new Vector3(0f, 0.05f, 2f);
            player.AddComponent<CharacterController>();

            GameObject visualObject = new GameObject("Visual - Biped and Zoomies");
            visualObject.transform.SetParent(player.transform, false);
            CapybaraVisual visual = visualObject.AddComponent<CapybaraVisual>();
            visual.Build();

            return player.AddComponent<CapybaraRunner>();
        }

        private static Camera ConfigureCamera(Transform runner)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                camera = new GameObject("Main Camera").AddComponent<Camera>();
                camera.tag = "MainCamera";
            }

            camera.fieldOfView = 62f;
            camera.nearClipPlane = 0.15f;
            camera.farClipPlane = 500f;
            camera.backgroundColor = new Color(0.48f, 0.72f, 0.86f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.transform.position = runner.position + new Vector3(0f, 5.2f, -8.4f);
            camera.transform.LookAt(runner.position + new Vector3(0f, 1.2f, 7f));
            return camera;
        }

        private static void ConfigureLighting()
        {
            Light light = FindFirstObjectByType<Light>();
            if (light == null)
            {
                light = new GameObject("Sun").AddComponent<Light>();
            }

            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.color = new Color(1f, 0.93f, 0.8f);
            light.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            RenderSettings.ambientLight = new Color(0.42f, 0.46f, 0.5f);
        }

        private static T GetOrCreate<T>(string objectName) where T : Component
        {
            T existing = FindFirstObjectByType<T>();
            return existing != null ? existing : new GameObject(objectName).AddComponent<T>();
        }
    }

    public sealed class CameraFollow : MonoBehaviour
    {
        private Transform target;
        private Vector3 velocity;

        public void Initialize(Transform followTarget)
        {
            target = followTarget;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desired = target.position + new Vector3(0f, 5.2f, -8.4f);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, 0.12f);
            transform.LookAt(target.position + new Vector3(0f, 1.15f, 7f));
        }
    }
}
