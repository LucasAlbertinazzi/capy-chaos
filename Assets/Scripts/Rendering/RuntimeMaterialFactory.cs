using System;
using UnityEngine;

namespace CapyChaos.Rendering
{
    public static class RuntimeMaterialFactory
    {
        public const string ShaderResourcePath = "Shaders/CapyChaosUnlit";
        public const string ShaderName = "CapyChaos/UnlitColor";

        private static Shader cachedShader;

        public static Shader LoadShader()
        {
            if (cachedShader == null)
            {
                cachedShader = Resources.Load<Shader>(ShaderResourcePath);
            }

            if (cachedShader == null)
            {
                throw new InvalidOperationException($"Shader obrigatório não encontrado em Resources/{ShaderResourcePath}.");
            }

            return cachedShader;
        }

        public static Material Create(string name, Color color, bool enableInstancing = false)
        {
            return new Material(LoadShader())
            {
                name = name,
                color = color,
                enableInstancing = enableInstancing
            };
        }
    }
}
