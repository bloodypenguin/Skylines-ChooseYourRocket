using ChooseYourRocket.Detour;
using ICities;
using UnityEngine;

namespace ChooseYourRocket
{
    public class LoadingExtension : LoadingExtensionBase
    {
        private static GameObject _gameObject;

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            RocketLaunchAIDetour.Deploy();
        }

        public override void OnReleased()
        {
            base.OnReleased();
            RocketLaunchAIDetour.Revert();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            if (mode != LoadMode.NewGame && mode != LoadMode.LoadGame && mode != LoadMode.NewGameFromScenario)
            {
                return;
            }
            if (_gameObject != null)
            {
                return;
            }
            _gameObject = new GameObject("ChooseYourRocketPanelExtender");
            _gameObject.AddComponent<GamePanelExtender>();
            PatchCountdownEffects();
        }

        private static void PatchCountdownEffects()
        {
            var defaultRocketAi = PrefabLister.DefaultRocket?.m_vehicleAI as RocketAI;
            if (defaultRocketAi == null)
            {
                return;
            }
            PrefabLister.ListRockets()
                .ForEach(r =>
                {
                    var ai = r.m_vehicleAI as RocketAI;
                    if (ai == null)
                    {
                        return;
                    }
                    ai.m_countdownEffect1 = defaultRocketAi.m_countdownEffect1;
                    ai.m_countdownEffect2 = defaultRocketAi.m_countdownEffect2;
                });
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            Configuration.Reset();
            if (_gameObject == null)
            {
                return;
            }
            Object.Destroy(_gameObject);
            _gameObject = null;
        } 
    }
}