using System.Collections.Generic;

namespace ChooseYourRocket
{
    public static class PrefabLister
    {
        public static List<VehicleInfo> ListRockets()
        {
            var rockets = new List<VehicleInfo>();
            for (uint index = 0; index < PrefabCollection<VehicleInfo>.LoadedCount(); index++)
            {
                var vehicle = PrefabCollection<VehicleInfo>.GetLoaded(index);
                var ai = vehicle?.m_vehicleAI as RocketAI;
                if (ai == null || ai.m_isCrawler)
                {
                    continue;
                }
                rockets.Add(vehicle);
            }
            return rockets;
        }

        public static VehicleInfo DefaultRocket => PrefabCollection<VehicleInfo>.FindLoaded("ChirpX Rocket");
        public static VehicleInfo DefaultCrawler => PrefabCollection<VehicleInfo>.FindLoaded("ChirpX Crawler");
    }
}