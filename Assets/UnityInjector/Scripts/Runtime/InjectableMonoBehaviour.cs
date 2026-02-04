using UnityEngine;

namespace Osiris.DI
{
    public abstract class InjectableMonoBehaviour : MonoBehaviour
    {
        protected virtual void Awake()
        {
            if (DI.Container == null)
            {
                Debug.LogError(
                    $"{GetType().Name} was created before DI container was ready");
                return;
            }

            DI.Container.Inject(this);
        }
    }
}