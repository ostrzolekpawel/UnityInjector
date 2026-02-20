using UnityEngine;

namespace Osiris.DI.Demo
{
    public class Installer : MonoInstaller
    {
        public override void Install(DiContainer container)
        {
            container.Bind<IEnemy>().To<Enemy>().WithArguments(115).AsSingle();
            container.Bind<ITest>().To<TestDefault>().AsSingle();
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
}
