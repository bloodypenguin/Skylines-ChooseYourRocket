using System;
using System.Reflection;
using UnityEngine;

namespace ChooseYourRocket
{
    public static class Util
    {
        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                           | BindingFlags.Static;
            var field = type.GetField(fieldName, bindFlags);
            if (field == null)
            {
                throw new Exception(string.Format("Type '{0}' doesn't have field '{1}", type, fieldName));
            }
            return field.GetValue(instance);
        }

        public static bool IsCargoStation(Building station)
        {
            if (station.m_flags == Building.Flags.None)
            {
                return false;
            }
            if (station.Info == null)
            {
                return false;
            }
            return station.Info.m_buildingAI is CargoStationAI;
        }

        public static ushort StationBuildingIdByPosition(Vector3 position)
        {
            for (ushort i = 0; i < BuildingManager.instance.m_buildings.m_size; i++)
            {
                var building = BuildingManager.instance.m_buildings.m_buffer[i];
                if (!IsCargoStation(building))
                {
                    continue;
                }
                if (position.Equals(building.m_position))
                {
                    return i;
                }
            }
            throw new Exception(string.Format("No building was found for position {0}", position));
        }
    }
}