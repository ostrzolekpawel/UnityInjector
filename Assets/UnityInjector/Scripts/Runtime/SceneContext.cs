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
        }

        public void Install(DiContainer diContainer)
        {
            _installers.ForEach(x => x.Install(diContainer));
        }

        private void OnDestroy()
        {
            Container.Dispose();
        }
    }

    public interface IInstaller
    {
        void Install(DiContainer container);
    }

    public interface IContext
    {
        DiContainer Container { get; }
    }
}