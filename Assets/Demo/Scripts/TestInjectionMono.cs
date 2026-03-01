namespace Osiris.DI.Demo
{
    public class TestInjectionMono : MonoBehaviourInjectable
    {
        [Inject] private ITest _test;
        [Inject] private IEnemy _enemy;

        private void Start()
        {
            _test.Calc();
            _enemy.Info();
        }

        [Inject]
        public void Init(INumberGenerator roomNumberGenerator)
        {

        }
    }
}
