using System;

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

        private void Register(Lifetime lifetime)
        {
            if (_contractType.IsInterface && _concreteType == _contractType)
            {
                throw new InvalidOperationException(
                    $"Binding interface {_contractType.Name} requires To<TConcrete>()");
            }

            _container.AddBinding(_contractType, new Binding
            {
                Factory = _factory ?? (args =>
                {
                    var finalArgs = args != null && args.Length > 0
                        ? args
                        : _bindingArgs;

                    return _container.Create(_concreteType, finalArgs);
                }),
                Lifetime = lifetime,
                OwnsInstance = _ownsInstance
            });
        }
    }

}