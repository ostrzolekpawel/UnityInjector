using System;

namespace Osiris.DI
{
    internal class Binding : IDisposable
    {
        public Func<object[], object> Factory;
        public Lifetime Lifetime;
        public bool OwnsInstance = true;

        private object _cachedInstance;

        public object GetInstance(object[] args = null)
        {
            switch (Lifetime)
            {
                case Lifetime.Transient:
                    return Factory?.Invoke(args) ?? throw new InvalidOperationException();

                case Lifetime.Cached:
                case Lifetime.Single:
                    return _cachedInstance ??= (Factory?.Invoke(args) ?? throw new InvalidOperationException());

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Dispose()
        {
            if (!OwnsInstance)
                return;

            if (_cachedInstance is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}