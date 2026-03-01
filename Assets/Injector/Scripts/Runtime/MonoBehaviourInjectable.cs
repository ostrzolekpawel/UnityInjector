using UnityEngine;

namespace Osiris.DI
{
    public abstract class MonoBehaviourInjectable : MonoBehaviour
    {
        protected virtual void Awake()
        {
            Inject();
        }

        protected void Inject()
        {
            var context = ContextRegistry.GetContext(gameObject.scene);

            if (context == null)
            {
                Debug.LogError(
                    $"No Context found in scene for {GetType().Name}");
                return;
            }

            context.Container.Inject(this);
        }
    }
}