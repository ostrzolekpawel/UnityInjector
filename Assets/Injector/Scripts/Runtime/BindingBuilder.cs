using System;
using UnityEngine;

namespace Osiris.DI
{
    public class BindingBuilder<TContract>
    {
        private readonly DiContainer _container;

        private Type _contractType;
        private Type _concreteType;
        private Func<object[], object> _factory;
        private object[] _bindingArgs;
        private bool _ownsInstance = true;
        private Action<TContract> _onInstantiated;

        internal BindingBuilder(DiContainer container)
        {
            _container = container;
            _contractType = typeof(TContract);
            _concreteType = typeof(TContract);
        }

        public BindingBuilder<TContract> WithArguments(params object[] args)
        {
            _bindingArgs = args;
            return this;
        }

        public BindingBuilder<TContract> To<TConcrete>()
            where TConcrete : TContract
        {
            _concreteType = typeof(TConcrete);
            return this;
        }

        public BindingBuilder<TContract> FromNew()
        {
            _factory = (args) => _container.Create(_concreteType, args);
            return this;
        }

        public BindingBuilder<TContract> FromInstance(TContract instance)
        {
            _factory = (args) => instance;
            _ownsInstance = false;
            return this;
        }

        public BindingBuilder<TContract> FromFactory(Func<object[], TContract> factory)
        {
            _factory = (args) => factory(args);
            return this;
        }

        public BindingBuilder<TContract> FromNewPrefab(GameObject prefab)
        {
            var concreteType = _concreteType;

            _factory = (args) =>
            {
                var go = UnityEngine.Object.Instantiate(prefab);
                var component = go.GetComponent(concreteType);

                if (component == null)
                {
                    UnityEngine.Object.Destroy(go);
                    throw new InvalidOperationException(
                        $"FromNewPrefab: component of type '{concreteType.Name}' " +
                        $"was not found on prefab '{prefab.name}'.");
                }

                _container.Inject(component);
                return component;
            };

            _ownsInstance = false;
            return this;
        }

        public BindingBuilder<TContract> FromNewPrefab(Component prefabComponent)
        {
            _factory = (args) =>
            {
                var instance = UnityEngine.Object.Instantiate(prefabComponent);
                _container.Inject(instance);
                return instance;
            };

            _ownsInstance = false;
            return this;
        }

        public BindingBuilder<TContract> OnInstantiated(Action<TContract> callback)
        {
            _onInstantiated = callback;
            return this;
        }

        public void AsTransient()
        {
            Register(Lifetime.Transient);
        }

        public void AsCached()
        {
            Register(Lifetime.Cached);
        }

        public void AsSingle()
        {
            Register(Lifetime.Single);
        }

        public void NonLazy()
        {
            Register(Lifetime.Single);
            _container.MarkNonLazy(_contractType);
        }

        private void Register(Lifetime lifetime)
        {
            if (_contractType.IsInterface && _concreteType == _contractType && _factory == null)
            {
                throw new InvalidOperationException(
                    $"Binding interface {_contractType.Name} requires To<TConcrete>()");
            }

            Func<object[], object> rawFactory = _factory ?? (args =>
            {
                var finalArgs = args != null && args.Length > 0 ? args : _bindingArgs;
                return _container.Create(_concreteType, finalArgs);
            });

            var onInstantiated = _onInstantiated;
            _container.AddBinding(_contractType, new Binding
            {
                Factory = onInstantiated != null
                    ? (args) => { var i = rawFactory(args); onInstantiated((TContract)i); return i; }
                    : rawFactory,
                Lifetime = lifetime,
                OwnsInstance = _ownsInstance
            });
        }
    }

}