using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Osiris.DI
{
    public static class ContextRegistry
    {
        private static Dictionary<Scene, IContext> _contexts = new();

        public static void Register(IContext context, Scene scene)
        {
            _contexts[scene] = context;
        }

        public static void Unregister(Scene scene)
        {
            if (_contexts.ContainsKey(scene))
            {
                _contexts.Remove(scene);
            }
        }

        public static IContext GetContext(Scene scene)
        {
            return _contexts.TryGetValue(scene, out var c) ? c : null;
        }
    }
}