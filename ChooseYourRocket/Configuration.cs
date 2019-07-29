using System.Collections.Generic;
using System.Linq;
using SpawnPositionInverter;
using UnityEngine;

namespace ChooseYourRocket
{
    public static class Configuration
    {

        private static Dictionary<ushort, ConfigurationItem> _state;
        private static VehicleInfo defaultRocket = null;
        private static VehicleInfo defaultCrawler = null;

        public static void Reset()
        {
            _state = null;
            defaultCrawler = null;
            defaultRocket = null;
        }

        public static void SetRocket(ushort eventId, string vehicleName)
        {
            if (_state == null)
            {
                _state = new Dictionary<ushort, ConfigurationItem>();
            }
            _state.Remove(eventId);
            _state.Add(eventId, new ConfigurationItem()
            {
                Rocket = PrefabCollection<VehicleInfo>.FindLoaded(vehicleName)
            });
        }

        public static ConfigurationItem Get(ushort eventID)
        {
            if (_state != null)
            {
                return Bind(_state.ContainsKey(eventID) ? _state[eventID] : new ConfigurationItem());
            }
            _state = new Dictionary<ushort, ConfigurationItem>();
            var config2 = new ConfigurationItem();
            if (SerializableDataExtension.RawState == null)
            {
                return Bind(config2);
            }
            var config3 = new ConfigurationItem();
            if (!SerializableDataExtension.RawState.ContainsKey(eventID))
            {
                return Bind(config3);
            }
            SerializableDataExtension.RawItem rawItem = SerializableDataExtension.RawState[eventID];
            if (rawItem.Rocket != null)
            {
                var rocket = PrefabCollection<VehicleInfo>.FindLoaded(rawItem.Rocket);
                config3.Rocket = rocket;
            }
            if (rawItem.Crawler != null)
            {
                var crawler = PrefabCollection<VehicleInfo>.FindLoaded(rawItem.Crawler);
                config3.Crawler = crawler;
            }
            _state.Remove(eventID);
            _state.Add(eventID, Bind(config3));
            return Bind(_state[eventID]);
        }

        private static ConfigurationItem Bind(ConfigurationItem item)
        {   
            if (item.Crawler == null)
            {
                if (defaultCrawler == null)
                {
                    defaultCrawler = PrefabLister.DefaultCrawler;

                }
                item.Crawler = defaultCrawler;
            }
            if (item.Rocket == null)
            {
                if (defaultRocket == null)
                {
                    defaultRocket = PrefabLister.DefaultRocket;
                }
                item.Rocket = defaultRocket;
            }

            return item;
        }

        public static Dictionary<ushort, SerializableDataExtension.RawItem> ToRawConfig()
        {
            if (_state == null)
            {
                return new Dictionary<ushort, SerializableDataExtension.RawItem>();
            }
            return _state.ToDictionary(kvp => kvp.Key, kvp => new SerializableDataExtension.RawItem()
            {
                Rocket = kvp.Value.Rocket.name,
                Crawler = kvp.Value.Crawler.name
            });

        }

        public class ConfigurationItem
        {
            public VehicleInfo Rocket { get; set; }

            public VehicleInfo Crawler { get; set; }
        }
    }
}
