using System;

namespace Osiris.DI
{
    internal class Binding
    {
        public Func<object> Factory;
        public LifeTime Lifetime;

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
    }
}