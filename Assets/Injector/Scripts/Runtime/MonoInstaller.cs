using UnityEngine;

namespace Osiris.DI
{
    public abstract class MonoInstaller : MonoBehaviour, IInstaller
    {
        public abstract void Install(DiContainer container);
    }
}