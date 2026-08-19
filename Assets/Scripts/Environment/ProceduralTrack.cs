using System.Collections.Generic;
using CapyChaos.Gameplay;
using CapyChaos.Rendering;
using UnityEngine;

namespace CapyChaos.Environment
{
    public sealed class ProceduralTrack : MonoBehaviour
    {
        private const float TileLength = 30f;
        private const int TileCount = 9;
        private readonly Queue<ThemeTile> tiles = new Queue<ThemeTile>();
        private Transform runner;
        private int nextTileIndex;
        private TrackObjectPool objectPool;
        private RunDifficultyTuning difficultyTuning;

        public void Initialize(Transform runnerTransform, RunDifficultyTuning tuning)
        {
            if (tiles.Count > 0)
            {
                return;
            }

            runner = runnerTransform;
            difficultyTuning = tuning;
            objectPool = GetComponent<TrackObjectPool>();
            if (objectPool == null)
            {
                objectPool = gameObject.AddComponent<TrackObjectPool>();
            }

            for (int i = 0; i < TileCount; i++)
            {
                ThemeTile tile = CreateTile();
                PositionTile(tile, nextTileIndex++);
                tiles.Enqueue(tile);
            }
        }

        private void Update()
        {
            if (runner == null || tiles.Count == 0)
            {
                return;
            }

            ThemeTile first = tiles.Peek();
            if (runner.position.z - first.transform.position.z < TileLength)
            {
                return;
            }

            tiles.Dequeue();
            PositionTile(first, nextTileIndex++);
            tiles.Enqueue(first);
        }

        public BrazilTheme GetThemeAt(float worldZ)
        {
            int tileIndex = Mathf.Max(0, Mathf.FloorToInt(worldZ / TileLength));
            return ThemeSequence.GetTheme(tileIndex);
        }

        private ThemeTile CreateTile()
        {
            GameObject tileObject = new GameObject("Theme Tile");
            tileObject.transform.SetParent(transform, false);
            ThemeTile tile = tileObject.AddComponent<ThemeTile>();
            tile.Initialize(objectPool);
            return tile;
        }

        private void PositionTile(ThemeTile tile, int index)
        {
            tile.transform.position = new Vector3(0f, 0f, index * TileLength + TileLength * 0.5f);
            tile.Configure(index, ThemeSequence.GetTheme(index), ThemeSequence.GetVariant(index), difficultyTuning);
        }
    }

    public sealed class ThemeTile : MonoBehaviour
    {
        private const float TileLength = 30f;
        private const float TileWidth = 10f;
        private readonly List<PooledContent> content = new List<PooledContent>();
        private TrackObjectPool objectPool;

        private readonly struct PooledContent
        {
            public PooledContent(GameObject item, TrackObjectKind kind)
            {
                Item = item;
                Kind = kind;
            }

            public GameObject Item { get; }
            public TrackObjectKind Kind { get; }
        }

        public void Initialize(TrackObjectPool pool)
        {
            objectPool = pool;
        }

        public void Configure(int tileIndex, BrazilTheme theme, int variant, RunDifficultyTuning tuning)
        {
            gameObject.name = $"Tile {tileIndex:000} - {theme} v{variant + 1}";
            ClearContent();
            ThemeDefinition definition = ThemeSequence.Describe(theme);

            CreateBlock("Ground", new Vector3(0f, -0.25f, 0f), new Vector3(TileWidth, 0.5f, TileLength), definition.Ground, true);
            CreateLaneMarkers(definition.Accent);
            CreateThemeProps(theme, definition, variant);

            if (tileIndex > 0)
            {
                CreateGameplay(tileIndex, definition, tuning);
            }
        }

        private void CreateGameplay(int tileIndex, ThemeDefinition definition, RunDifficultyTuning tuning)
        {
            DifficultyProfile difficulty = DifficultyProgression.Evaluate(tileIndex, tuning);
            int firstObstacleLane = (tileIndex * 7 % 3) - 1;
            for (int i = 0; i < difficulty.ObstacleCount; i++)
            {
                int lane = ((firstObstacleLane + 1 + i * 2) % 3 + 3) % 3 - 1;
                bool overhead = difficulty.OverheadObstaclesEnabled && (tileIndex + i) % 3 == 0;
                Vector3 position = new Vector3(lane * 2.6f, overhead ? 1.65f : 0.55f, -1f + i * 6.5f);
                Vector3 scale = overhead ? new Vector3(1.55f, 0.7f, 0.9f) : new Vector3(1.35f, 1.1f, 1.2f);
                GameObject obstacle = CreateBlock(overhead ? "Overhead Obstacle" : "Jump Obstacle", position, scale, definition.Secondary, true, TrackObjectKind.Obstacle);
                if (obstacle.GetComponent<RunnerObstacle>() == null)
                {
                    obstacle.AddComponent<RunnerObstacle>();
                }
            }

            int coinLane = firstObstacleLane == 0 ? 1 : 0;
            for (int i = 0; i < difficulty.CoinCount; i++)
            {
                GameObject coin = CreatePrimitive("Coin", PrimitiveType.Cylinder, new Vector3(coinLane * 2.6f, 1.05f, -10f + i * 1.8f), new Vector3(0.18f, 0.48f, 0.48f), new Color(1f, 0.76f, 0.08f), TrackObjectKind.Coin);
                coin.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                Collider collider = coin.GetComponent<Collider>();
                collider.isTrigger = true;
                if (coin.GetComponent<CoinPickup>() == null)
                {
                    coin.AddComponent<CoinPickup>();
                }
            }

            if (tileIndex % 5 == 0)
            {
                GameObject boost = CreatePrimitive("Zoomies", PrimitiveType.Sphere, new Vector3(-coinLane * 2.6f, 1.15f, -7f), Vector3.one * 0.75f, new Color(0.18f, 0.9f, 0.95f), TrackObjectKind.Zoomies);
                boost.GetComponent<Collider>().isTrigger = true;
                if (boost.GetComponent<ZoomiesPickup>() == null)
                {
                    boost.AddComponent<ZoomiesPickup>();
                }
            }
        }

        private void CreateLaneMarkers(Color color)
        {
            for (int laneEdge = -1; laneEdge <= 1; laneEdge += 2)
            {
                CreateBlock("Lane Marker", new Vector3(laneEdge * 1.3f, 0.015f, 0f), new Vector3(0.06f, 0.03f, TileLength), color, false);
            }
        }

        private void CreateThemeProps(BrazilTheme theme, ThemeDefinition definition, int variant)
        {
            switch (theme)
            {
                case BrazilTheme.SaoPaulo:
                    CreateSaoPaulo(definition, variant);
                    break;
                case BrazilTheme.RioDeJaneiro:
                    CreateRio(definition, variant);
                    break;
                case BrazilTheme.Brasilia:
                    CreateBrasilia(definition, variant);
                    break;
                case BrazilTheme.Amazonia:
                    CreateAmazonia(definition, variant);
                    break;
            }
        }

        private void CreateSaoPaulo(ThemeDefinition definition, int variant)
        {
            CreateBlock("Concrete Bank L", new Vector3(-5.4f, 1.4f, 0f), new Vector3(1.2f, 2.8f, TileLength), definition.Secondary, true);
            CreateBlock("Concrete Bank R", new Vector3(5.4f, 1.4f, 0f), new Vector3(1.2f, 2.8f, TileLength), definition.Secondary, true);
            for (int i = 0; i < 5 + variant; i++)
            {
                float x = -3.7f + (i * 1.7f) % 7.4f;
                float z = -12f + i * 5f;
                CreatePrimitive("Foam and Trash", PrimitiveType.Sphere, new Vector3(x, 0.08f, z), new Vector3(0.45f, 0.08f, 0.65f), new Color(0.72f, 0.76f, 0.58f));
            }
            CreatePrimitive("Drain Pipe", PrimitiveType.Cylinder, new Vector3(-4.65f, 1.25f, 8f), new Vector3(0.65f, 1.1f, 0.65f), new Color(0.16f, 0.17f, 0.17f)).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }

        private void CreateRio(ThemeDefinition definition, int variant)
        {
            CreateBlock("Ocean", new Vector3(6.2f, 0.05f, 0f), new Vector3(3f, 0.15f, TileLength), definition.Accent, false);
            CreateBlock("Favela Hill", new Vector3(-6.1f, 2f, 0f), new Vector3(3.2f, 4f, TileLength), new Color(0.22f, 0.42f, 0.22f), true);
            for (int i = 0; i < 7 + variant * 2; i++)
            {
                Color houseColor = Color.HSVToRGB((i * 0.13f) % 1f, 0.65f, 0.88f);
                CreateBlock("Community Home", new Vector3(-5.15f - (i % 2) * 0.8f, 1.2f + (i % 3) * 0.85f, -12f + i * 3.8f), new Vector3(0.7f, 0.7f, 1.1f), houseColor, false);
            }
            for (int i = 0; i < 4; i++)
            {
                CreatePrimitive("Palm", PrimitiveType.Cylinder, new Vector3(4.45f, 1.1f, -11f + i * 7f), new Vector3(0.16f, 1.1f, 0.16f), new Color(0.39f, 0.24f, 0.11f));
                CreatePrimitive("Palm Crown", PrimitiveType.Sphere, new Vector3(4.45f, 2.35f, -11f + i * 7f), new Vector3(0.9f, 0.25f, 0.9f), new Color(0.12f, 0.5f, 0.16f));
            }
        }

        private void CreateBrasilia(ThemeDefinition definition, int variant)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 4; i++)
                {
                    CreatePrimitive("Monumental Column", PrimitiveType.Cylinder, new Vector3(side * 4.5f, 1.8f, -11f + i * 7f), new Vector3(0.42f, 1.8f, 0.42f), Color.white);
                }
            }
            for (int i = 0; i < 3 + variant; i++)
            {
                CreateBlock("Anonymous Briefcase", new Vector3((i % 2 == 0 ? -1 : 1) * 4f, 0.55f, -8f + i * 6f), new Vector3(1f, 0.8f, 0.35f), new Color(0.22f, 0.11f, 0.05f), false);
                CreatePrimitive("Symbolic Money", PrimitiveType.Cylinder, new Vector3((i % 2 == 0 ? -1 : 1) * 3.8f, 1.2f, -7.7f + i * 6f), new Vector3(0.15f, 0.46f, 0.46f), definition.Accent);
            }
        }

        private void CreateAmazonia(ThemeDefinition definition, int variant)
        {
            int treeCount = variant == 0 ? 10 : variant == 1 ? 6 : 3;
            for (int i = 0; i < treeCount; i++)
            {
                int side = i % 2 == 0 ? -1 : 1;
                float x = side * (4.2f + (i % 3) * 0.65f);
                float z = -13f + i * 2.8f;
                CreatePrimitive("Tree Trunk", PrimitiveType.Cylinder, new Vector3(x, 1.2f, z), new Vector3(0.28f, 1.2f, 0.28f), definition.Secondary);
                CreatePrimitive("Canopy", PrimitiveType.Sphere, new Vector3(x, 2.8f, z), new Vector3(1.25f, 1.1f, 1.25f), definition.Accent);
            }
            for (int i = treeCount; i < 10; i++)
            {
                int side = i % 2 == 0 ? -1 : 1;
                CreatePrimitive("Tree Stump", PrimitiveType.Cylinder, new Vector3(side * 4.5f, 0.28f, -13f + i * 2.8f), new Vector3(0.42f, 0.28f, 0.42f), definition.Secondary);
            }
        }

        private GameObject CreateBlock(string name, Vector3 position, Vector3 scale, Color color, bool colliderEnabled, TrackObjectKind kind = TrackObjectKind.Block)
        {
            GameObject block = CreatePrimitive(name, PrimitiveType.Cube, position, scale, color, kind);
            Collider collider = block.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = colliderEnabled;
            }
            return block;
        }

        private GameObject CreatePrimitive(string name, PrimitiveType primitiveType, Vector3 position, Vector3 scale, Color color, TrackObjectKind? requestedKind = null)
        {
            TrackObjectKind kind = requestedKind ?? KindFor(primitiveType);
            GameObject item = objectPool.Rent(kind, primitiveType, transform);
            item.name = name;
            item.transform.localPosition = position;
            item.transform.localRotation = Quaternion.identity;
            item.transform.localScale = scale;
            item.GetComponent<Renderer>().sharedMaterial = MaterialPalette.Get(color);
            Collider collider = item.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = true;
                collider.isTrigger = false;
            }
            content.Add(new PooledContent(item, kind));
            return item;
        }

        private void ClearContent()
        {
            for (int i = 0; i < content.Count; i++)
            {
                objectPool.Return(content[i].Kind, content[i].Item);
            }
            content.Clear();
        }

        private static TrackObjectKind KindFor(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.Cylinder:
                    return TrackObjectKind.Cylinder;
                case PrimitiveType.Sphere:
                    return TrackObjectKind.Sphere;
                default:
                    return TrackObjectKind.Block;
            }
        }
    }

    internal static class MaterialPalette
    {
        private static readonly Dictionary<Color, Material> Materials = new Dictionary<Color, Material>();

        public static Material Get(Color color)
        {
            if (Materials.TryGetValue(color, out Material material))
            {
                return material;
            }

            material = RuntimeMaterialFactory.Create($"Track {ColorUtility.ToHtmlStringRGBA(color)}", color, true);
            Materials.Add(color, material);
            return material;
        }
    }
}
