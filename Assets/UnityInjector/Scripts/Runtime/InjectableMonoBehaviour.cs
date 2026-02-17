using System.Linq;
using UnityEngine;

namespace Osiris.DI
{
    public abstract class InjectableMonoBehaviour : MonoBehaviour
    {
        protected virtual void Awake()
        {
            Inject();
        }

        protected void Inject()
        {
            var context = GetComponentInParent<IContext>();

            // var scene = gameObject.scene;
            // var context = scene
            //     .GetRootGameObjects()
            //     .SelectMany(go => go.GetComponentsInChildren<IContext>(true))
            //     .FirstOrDefault();

            if (context == null)
            {
                // Debug.LogError(
                //     $"No Context found in scene {scene.name} for {GetType().Name}");

                Debug.LogError(
                    $"No Context found in scene for {GetType().Name}");
                return;
            }

            context.Container.Inject(this);
        }
    }
}