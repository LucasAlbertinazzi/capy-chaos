using System.Collections.Generic;
using UnityEngine;

namespace CapyChaos.Environment
{
    public enum TrackObjectKind
    {
        Block,
        Cylinder,
        Sphere,
        Obstacle,
        Coin,
        Zoomies
    }

    public sealed class TrackObjectPool : MonoBehaviour
    {
        private readonly Dictionary<TrackObjectKind, Stack<GameObject>> available = new Dictionary<TrackObjectKind, Stack<GameObject>>();

        public GameObject Rent(TrackObjectKind kind, PrimitiveType primitiveType, Transform parent)
        {
            if (!available.TryGetValue(kind, out Stack<GameObject> objects))
            {
                objects = new Stack<GameObject>();
                available.Add(kind, objects);
            }

            GameObject item = objects.Count > 0 ? objects.Pop() : Create(kind, primitiveType);
            item.transform.SetParent(parent, false);
            item.SetActive(true);
            return item;
        }

        public void Return(TrackObjectKind kind, GameObject item)
        {
            if (item == null)
            {
                return;
            }

            item.SetActive(false);
            item.transform.SetParent(transform, false);
            if (!available.TryGetValue(kind, out Stack<GameObject> objects))
            {
                objects = new Stack<GameObject>();
                available.Add(kind, objects);
            }

            objects.Push(item);
        }

        private static GameObject Create(TrackObjectKind kind, PrimitiveType primitiveType)
        {
            GameObject item = GameObject.CreatePrimitive(primitiveType);
            item.name = $"Pooled {kind}";
            return item;
        }
    }
}
