using System.Collections.Generic;
using UnityEngine;

namespace Osiris.DI
{
    public sealed class AppContext : MonoBehaviour
    {
        [SerializeField] private List<MonoInstaller> _installers;
        public static DiContainer Container { get; private set; }

        private void Awake()
        {
            Container = new DiContainer();
            Install(Container);
        }

        public void Install(DiContainer container)
        {
            _installers.ForEach(x => x.Install(container));
        }

        private void OnDestroy()
        {
            Container.Dispose();
            Container = null;
        }
    }
}