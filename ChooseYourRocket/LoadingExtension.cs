using System;
using System.Linq;
using ChooseYourRocket.Detour;
using ColossalFramework.Plugins;
using ICities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChooseYourRocket
{
    public class LoadingExtension : LoadingExtensionBase
    {
        private static GameObject _gameObject;

        public static int MaxVehicleCount;

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            RocketLaunchAIDetour.Deploy();
            if (Util.IsModActive(1764208250))
            {
                UnityEngine.Debug.LogWarning("ChooseYourRocket: More Vehicles is enabled, applying compatibility workaround");
                MaxVehicleCount = ushort.MaxValue + 1;
            }
            else
            {
                UnityEngine.Debug.Log("ChooseYourRocket: More Vehicles is not enabled");
                MaxVehicleCount = VehicleManager.MAX_VEHICLE_COUNT;
            }
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