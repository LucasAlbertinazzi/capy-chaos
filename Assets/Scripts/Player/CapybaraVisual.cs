using UnityEngine;

namespace CapyChaos.Player
{
    public sealed class CapybaraVisual : MonoBehaviour
    {
        private Transform body;
        private Transform head;
        private Transform[] legs;
        private bool built;

        public void Build()
        {
            if (built)
            {
                return;
            }

            Material fur = CreateMaterial("Capy Fur", new Color(0.48f, 0.27f, 0.12f));
            Material muzzle = CreateMaterial("Capy Muzzle", new Color(0.68f, 0.45f, 0.24f));
            Material dark = CreateMaterial("Capy Details", new Color(0.055f, 0.045f, 0.035f));

            body = CreatePart("Body", PrimitiveType.Capsule, fur);
            head = CreatePart("Head", PrimitiveType.Sphere, fur);
            Transform snout = CreatePart("Snout", PrimitiveType.Sphere, muzzle, head);
            snout.localPosition = new Vector3(0f, -0.08f, 0.42f);
            snout.localScale = new Vector3(0.55f, 0.38f, 0.42f);

            CreateEye(new Vector3(-0.21f, 0.18f, 0.38f), dark);
            CreateEye(new Vector3(0.21f, 0.18f, 0.38f), dark);
            CreateEar(new Vector3(-0.28f, 0.34f, 0.02f), fur);
            CreateEar(new Vector3(0.28f, 0.34f, 0.02f), fur);

            legs = new Transform[4];
            for (int i = 0; i < legs.Length; i++)
            {
                legs[i] = CreatePart($"Leg {i + 1}", PrimitiveType.Capsule, fur);
            }

            built = true;
            SetState(false, false);
        }

        public void SetState(bool zoomies, bool sliding)
        {
            if (!built)
            {
                return;
            }

            if (zoomies || sliding)
            {
                body.localPosition = new Vector3(0f, 0.88f, 0f);
                body.localRotation = Quaternion.Euler(90f, 0f, 0f);
                body.localScale = new Vector3(0.72f, 0.92f, 0.72f);
                head.localPosition = new Vector3(0f, 0.98f, 0.75f);
                head.localScale = Vector3.one * 0.72f;
                SetLeg(0, -0.38f, 0.35f);
                SetLeg(1, 0.38f, 0.35f);
                SetLeg(2, -0.38f, -0.48f);
                SetLeg(3, 0.38f, -0.48f);
            }
            else
            {
                body.localPosition = new Vector3(0f, 1.08f, 0f);
                body.localRotation = Quaternion.identity;
                body.localScale = new Vector3(0.78f, 0.82f, 0.68f);
                head.localPosition = new Vector3(0f, 1.72f, 0.12f);
                head.localScale = Vector3.one * 0.72f;
                SetLeg(0, -0.28f, 0.18f);
                SetLeg(1, 0.28f, 0.18f);
                SetLeg(2, -0.28f, -0.12f);
                SetLeg(3, 0.28f, -0.12f);
            }
        }

        private void SetLeg(int index, float x, float z)
        {
            legs[index].localPosition = new Vector3(x, 0.38f, z);
            legs[index].localRotation = Quaternion.identity;
            legs[index].localScale = new Vector3(0.28f, 0.42f, 0.28f);
        }

        private void CreateEye(Vector3 position, Material material)
        {
            Transform eye = CreatePart("Eye", PrimitiveType.Sphere, material, head);
            eye.localPosition = position;
            eye.localScale = Vector3.one * 0.13f;
        }

        private void CreateEar(Vector3 position, Material material)
        {
            Transform ear = CreatePart("Ear", PrimitiveType.Sphere, material, head);
            ear.localPosition = position;
            ear.localScale = new Vector3(0.22f, 0.3f, 0.18f);
        }

        private static Transform CreatePart(string name, PrimitiveType type, Material material, Transform parent = null)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            return part.transform;
        }

        private static Material CreateMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material material = new Material(shader) { name = name, color = color };
            return material;
        }
    }
}
