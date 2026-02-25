using UnityEngine;
using UnityEngine.iOS;

namespace Osiris.DI.Demo
{
    public class Installer : MonoInstaller
    {
        public override void Install(DiContainer container)
        {
            container.Bind<IEnemy>().To<Enemy>().WithArguments(115).AsSingle();
            container.Bind<ITest>().To<TestDefault>().AsSingle();
            var seed = unchecked((uint)Device.vendorIdentifier.GetHashCode());
            container.Bind<INumberGenerator>().To<UniqueIntGenerator>().WithArguments((uint)Device.vendorIdentifier.GetHashCode()).AsSingle();
        }
    }

    public interface ITest
    {
        void Calc();
    }

    public class TestDefault : ITest
    {
        public void Calc()
        {
            Debug.Log("Calc!!!!");
        }
    }

    public interface IEnemy
    {
        void Info();
    }

    public class Enemy : IEnemy
    {
        private readonly int _health;
        public Enemy(int health)
        {
            _health = health;
        }

        public void Info()
        {
            Debug.Log($"health: {_health}");
        }
    }

    public interface INumberGenerator
    {
        int Next();
    }

    public sealed class UniqueIntGenerator : INumberGenerator
    {
        private const uint A = 1664525;
        private const uint C = 1013904223;

        private uint _state;

        public UniqueIntGenerator(uint seed)
        {
            _state = seed;
        }

        public int Next()
        {
            _state = unchecked(A * _state + C);

            // mapowanie na zakres 0..int.MaxValue
            return (int)(_state & 0x7FFFFFFF);
        }
    }
}
