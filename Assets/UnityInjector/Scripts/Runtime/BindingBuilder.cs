using System;

namespace Osiris.DI
{
    public class BindingBuilder<TContract>
    {
        private readonly DiContainer _container;

        private Type _contractType;
        private Type _concreteType;
        private Func<object> _factory;

        internal BindingBuilder(DiContainer container)
        {
            _container = container;
            _contractType = typeof(TContract);
            _concreteType = typeof(TContract);
        }

        public BindingBuilder<TContract> To<TConcrete>()
            where TConcrete : TContract
        {
            _concreteType = typeof(TConcrete);
            return this;
        }

        public BindingBuilder<TContract> FromNew()
        {
            _factory = () => _container.Create(_concreteType);
            return this;
        }

        public BindingBuilder<TContract> FromInstance(TContract instance)
        {
            _factory = () => instance;
            return this;
        }

        public BindingBuilder<TContract> FromFactory(Func<TContract> factory)
        {
            _factory = () => factory();
            return this;
        }

        public void AsTransient()
        {
            Register(LifeTime.Transient);
        }

        public void AsCached()
        {
            Register(LifeTime.Cached);
        }

        public void AsSingle()
        {
            Register(LifeTime.Single);
        }

        private void Register(LifeTime lifetime)
        {
            if (_contractType.IsInterface && _concreteType == _contractType)
            {
                throw new InvalidOperationException(
                    $"Binding interface {_contractType.Name} requires To<TConcrete>()");
            }

            _container.AddBinding(_contractType, new Binding
            {
                Factory = _factory ?? (() => _container.Create(_concreteType)),
                Lifetime = lifetime
            });
        }
    }

}