using System.Collections.Generic;
using UnityEngine;

namespace Osiris.DI
{
    public sealed class SceneContext : MonoBehaviour, IContext
    {
        [SerializeField] private List<MonoInstaller> _installers;

        public DiContainer Container { get; private set; }

        private void Awake()
        {
            Container = new DiContainer(AppContext.Container);
            Install(Container);

            ContextRegistry.Register(this, gameObject.scene);
        }

        public void Install(DiContainer diContainer)
        {
            _installers.ForEach(x => x.Install(diContainer));
        }

        private void OnDestroy()
        {
            Container.Dispose();
            ContextRegistry.Unregister(gameObject.scene);
        }
    }
}