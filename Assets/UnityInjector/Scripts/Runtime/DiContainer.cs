using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Osiris.DI
{
    public class DiContainer
    {
        private readonly Dictionary<Type, Binding> _bindings = new Dictionary<Type, Binding>();

        public BindingBuilder<T> Bind<T>()
        {
            return new BindingBuilder<T>(this);
        }

        internal void AddBinding(Type type, Binding binding)
        {
            _bindings[type] = binding;
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {
            if (_bindings.TryGetValue(type, out var binding))
            {
                return binding.GetInstance();
            }

            return Create(type);
        }

        internal object Create(Type type)
        {
            var ctor = SelectConstructor(type);
            var args = ctor.GetParameters()
                           .Select(p => Resolve(p.ParameterType))
                           .ToArray();

            var instance = ctor.Invoke(args);

            InjectMembers(instance);
            InjectMethods(instance);

            return instance;
        }

        private ConstructorInfo SelectConstructor(Type type)
        {
            var ctors = type.GetConstructors();

            var injectCtor = ctors.FirstOrDefault(c =>
                c.IsDefined(typeof(InjectAttribute), true));

            return injectCtor ??
                   ctors.OrderByDescending(c => c.GetParameters().Length).First();
        }

        public void Inject(object target)
        {
            InjectMembers(target);
            InjectMethods(target);
        }

        private void InjectMembers(object instance)
        {
            var type = instance.GetType();

            foreach (var field in type.GetFields(
                         BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!field.IsDefined(typeof(InjectAttribute), true)) continue;
                field.SetValue(instance, Resolve(field.FieldType));
            }

            foreach (var prop in type.GetProperties(
                         BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!prop.IsDefined(typeof(InjectAttribute), true)) continue;
                if (!prop.CanWrite) continue;
                prop.SetValue(instance, Resolve(prop.PropertyType));
            }
        }

        private void InjectMethods(object instance)
        {
            var type = instance.GetType();

            foreach (var method in type.GetMethods(
                         BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!method.IsDefined(typeof(InjectAttribute), true)) continue;

                var args = method.GetParameters()
                                 .Select(p => Resolve(p.ParameterType))
                                 .ToArray();

                method.Invoke(instance, args);
            }
        }
    }
}