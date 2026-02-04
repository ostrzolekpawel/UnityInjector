using System;

namespace Osiris.DI
{
    internal class Binding : IDisposable
    {
        public Func<object> Factory;
        public LifeTime Lifetime;
        public bool OwnsInstance = true;

        private object _cachedInstance;

        public object GetInstance()
        {
            switch (Lifetime)
            {
                case LifeTime.Transient:
                    return Factory();

                case LifeTime.Cached:
                    return _cachedInstance ??= Factory();

                case LifeTime.Single:
                    return _cachedInstance ??= Factory();

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