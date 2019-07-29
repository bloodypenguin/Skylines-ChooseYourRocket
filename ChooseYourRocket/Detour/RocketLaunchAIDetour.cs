using System;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Math;
using SpawnPositionInverter.Detour;
using UnityEngine;

namespace ChooseYourRocket.Detour
{
    public class RocketLaunchAIDetour : EventAI
    {
        private static RedirectCallsState _state1;
        private static RedirectCallsState _state2;

        private static readonly MethodInfo Method1 = typeof(RocketLaunchAI).GetMethod("FindVehicles", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo Method2 = typeof(RocketLaunchAI).GetMethod("SimulationStep", BindingFlags.Instance | BindingFlags.Public);
        private static readonly MethodInfo Detour1 = typeof(RocketLaunchAIDetour).GetMethod("FindVehicles", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo Detour2 = typeof(RocketLaunchAIDetour).GetMethod("SimulationStep", BindingFlags.Instance | BindingFlags.Public);


        private static bool _deployed;

        public static void Deploy()
        {
            if (_deployed)
            {
                return;
            }
            try
            {
                _state1 = RedirectionHelper.RedirectCalls(Method1, Detour1);
                _state2 = RedirectionHelper.RedirectCalls(Method2, Detour2);

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            _deployed = true;
        }

        public static void Revert()
        {
            if (!_deployed)
            {
                return;
            }
            try
            {
                RedirectionHelper.RevertRedirect(Method1, _state1);
                RedirectionHelper.RevertRedirect(Method2, _state2);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            _deployed = false;
        }

        private void FindVehicles(ushort eventID, ref EventData data, out ushort crawler, out ushort rocket)
        {
            crawler = (ushort)0;
            rocket = (ushort)0;
            if ((int)data.m_building == 0)
                return;
            BuildingManager instance1 = Singleton<BuildingManager>.instance;
            VehicleManager instance2 = Singleton<VehicleManager>.instance;
            ushort num1 = instance1.m_buildings.m_buffer[(int)data.m_building].m_ownVehicles;
            int num2 = 0;
            while ((int)num1 != 0)
            {
                VehicleInfo info = instance2.m_vehicles.m_buffer[(int)num1].Info;
                //begin mod
                if (info == Configuration.Get(eventID).Crawler)
                    crawler = num1;
                else if (info == Configuration.Get(eventID).Rocket)
                    rocket = num1;
                //end mod
                num1 = instance2.m_vehicles.m_buffer[(int)num1].m_nextOwnVehicle;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }
        }

        public override void SimulationStep(ushort eventID, ref EventData data)
        {
            //begin mod
            var crawlerVehicle = Configuration.Get(eventID).Crawler;
            var rocketVehicle = Configuration.Get(eventID).Rocket;
            //end mod
            VehicleManager instance1 = Singleton<VehicleManager>.instance;
            BuildingManager instance2 = Singleton<BuildingManager>.instance;
            EventManager instance3 = Singleton<EventManager>.instance;
            ushort vehicleID1;
            ushort vehicleID2;
            this.FindVehicles(eventID, ref data, out vehicleID1, out vehicleID2);
            bool flag1 = false;
            if ((data.m_flags & (EventData.Flags.Active | EventData.Flags.Expired | EventData.Flags.Completed | EventData.Flags.Disorganizing | EventData.Flags.Cancelled)) == EventData.Flags.None)
            {
                if ((data.m_flags & EventData.Flags.Ready) == EventData.Flags.None && this.m_resourceConsumption != 0)
                {
                    int num1 = (int)data.m_popularityDelta * 10;
                    int num2 = Mathf.Clamp(num1 / this.m_resourceConsumption, 0, 256);
                    data.m_startFrame += (uint)(256 - num2);
                    data.m_expireFrame += (uint)(256 - num2);
                    int num3 = num1 - num2 * this.m_resourceConsumption;
                    data.m_popularityDelta = (short)Mathf.Clamp(num3 / 10, 0, (int)short.MaxValue);
                    int num4 = 0;
                    uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
                    if (data.m_startFrame > currentFrameIndex)
                        num4 = ((int)data.m_startFrame - (int)currentFrameIndex) * this.m_resourceConsumption;
                    if ((int)data.m_building != 0 && num4 > num3)
                    {
                        int num5 = 100000;
                        ushort vehicleID3 = instance2.m_buildings.m_buffer[(int)data.m_building].m_guestVehicles;
                        int num6 = 0;
                        while ((int)vehicleID3 != 0)
                        {
                            switch ((TransferManager.TransferReason)instance1.m_vehicles.m_buffer[(int)vehicleID3].m_transferType)
                            {
                                case TransferManager.TransferReason.Coal:
                                case TransferManager.TransferReason.Petrol:
                                    int size;
                                    int max;
                                    instance1.m_vehicles.m_buffer[(int)vehicleID3].Info.m_vehicleAI.GetSize(vehicleID3, ref instance1.m_vehicles.m_buffer[(int)vehicleID3], out size, out max);
                                    num3 += Mathf.Min(size, max);
                                    break;
                            }
                            vehicleID3 = instance1.m_vehicles.m_buffer[(int)vehicleID3].m_nextGuestVehicle;
                            if (++num6 > 16384)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                                break;
                            }
                        }
                        if (num3 <= num5 - 8000)
                            Singleton<TransferManager>.instance.AddIncomingOffer((data.m_flags & EventData.Flags.Preparing) == EventData.Flags.None ? TransferManager.TransferReason.Coal : TransferManager.TransferReason.Petrol, new TransferManager.TransferOffer()
                            {
                                Priority = Mathf.Max(1, (num5 - num3) * 8 / num5),
                                Building = data.m_building,
                                Position = instance2.m_buildings.m_buffer[(int)data.m_building].m_position,
                                Amount = 1,
                                Active = false
                            });
                        if (num3 < 16000)
                            flag1 = true;
                    }
                }
                if ((data.m_flags & (EventData.Flags.Preparing | EventData.Flags.Ready)) != EventData.Flags.None && (int)data.m_building != 0)
                {
                    if ((int)vehicleID1 == 0 && crawlerVehicle != null)
                    {
                        Vector3 position = instance2.m_buildings.m_buffer[(int)data.m_building].CalculatePosition(this.m_rocketSpawnPosition);
                        if (instance1.CreateVehicle(out vehicleID1, ref Singleton<SimulationManager>.instance.m_randomizer, crawlerVehicle, position, TransferManager.TransferReason.None, false, false))
                        {
                            crawlerVehicle.m_vehicleAI.SetSource(vehicleID1, ref instance1.m_vehicles.m_buffer[(int)vehicleID1], data.m_building);
                            crawlerVehicle.m_vehicleAI.SetTarget(vehicleID1, ref instance1.m_vehicles.m_buffer[(int)vehicleID1], data.m_building);
                        }
                    }
                    if ((int)vehicleID1 != 0 && (int)vehicleID2 == 0 && rocketVehicle != null)
                    {
                        Vector3 position = instance2.m_buildings.m_buffer[(int)data.m_building].CalculatePosition(this.m_rocketSpawnPosition);
                        if (instance1.CreateVehicle(out vehicleID2, ref Singleton<SimulationManager>.instance.m_randomizer, rocketVehicle, position, TransferManager.TransferReason.None, false, false))
                        {
                            rocketVehicle.m_vehicleAI.SetSource(vehicleID2, ref instance1.m_vehicles.m_buffer[(int)vehicleID2], data.m_building);
                            rocketVehicle.m_vehicleAI.SetTarget(vehicleID2, ref instance1.m_vehicles.m_buffer[(int)vehicleID2], data.m_building);
                            ushort lastVehicle = instance1.m_vehicles.m_buffer[(int)vehicleID1].GetLastVehicle(vehicleID1);
                            if ((int)lastVehicle != 0)
                            {
                                instance1.m_vehicles.m_buffer[(int)vehicleID2].m_leadingVehicle = lastVehicle;
                                instance1.m_vehicles.m_buffer[(int)lastVehicle].m_trailingVehicle = vehicleID2;
                            }
                        }
                    }
                }
            }
            if ((int)data.m_building != 0)
            {
                ushort num = instance2.m_buildings.m_buffer[(int)data.m_building].m_eventIndex;
                if ((int)num == (int)eventID)
                {
                    Notification.Problem problems = instance2.m_buildings.m_buffer[(int)data.m_building].m_problems;
                    Notification.Problem problem = Notification.RemoveProblems(problems, Notification.Problem.NoResources);
                    if (flag1)
                    {
                        instance2.m_buildings.m_buffer[(int)data.m_building].m_incomingProblemTimer = (byte)Mathf.Min((int)byte.MaxValue, (int)instance2.m_buildings.m_buffer[(int)data.m_building].m_incomingProblemTimer + 1);
                        if ((int)instance2.m_buildings.m_buffer[(int)data.m_building].m_incomingProblemTimer >= 4)
                            problem = Notification.AddProblems(problem, Notification.Problem.NoResources);
                    }
                    else
                        instance2.m_buildings.m_buffer[(int)data.m_building].m_incomingProblemTimer = (byte)0;
                    if (problem != problems)
                    {
                        instance2.m_buildings.m_buffer[(int)data.m_building].m_problems = problem;
                        Singleton<BuildingManager>.instance.UpdateNotifications(data.m_building, problems, problem);
                    }
                    GuideController properties = Singleton<GuideManager>.instance.m_properties;
                    if (properties != null)
                        Singleton<BuildingManager>.instance.m_rocketLaunchSite.Activate(properties.m_rocketLaunchSite, data.m_building);
                }
                if ((int)num != 0 && (instance3.m_events.m_buffer[(int)num].m_flags & (EventData.Flags.Preparing | EventData.Flags.Active | EventData.Flags.Completed | EventData.Flags.Cancelled | EventData.Flags.Ready)) == EventData.Flags.None)
                    num = instance3.m_events.m_buffer[(int)num].m_nextBuildingEvent;
                bool flag2 = false;
                if ((int)vehicleID1 != 0 && crawlerVehicle != null)
                {
                    Vector3 position = instance2.m_buildings.m_buffer[(int)data.m_building].CalculatePosition(this.m_doorPosition);
                    Vector3 lastFramePosition = instance1.m_vehicles.m_buffer[(int)vehicleID1].GetLastFramePosition();
                    Vector3 targetPos0 = (Vector3)instance1.m_vehicles.m_buffer[(int)vehicleID1].m_targetPos0;
                    Vector3 b = lastFramePosition + Vector3.ClampMagnitude(targetPos0 - lastFramePosition, crawlerVehicle.m_maxSpeed * 16f);
                    flag2 = (double)Vector3.Distance(position, b) < 40.0;
                }
                if (flag2)
                {
                    if ((data.m_flags & EventData.Flags.Loading) == EventData.Flags.None)
                    {
                        data.m_flags |= EventData.Flags.Loading;
                        if (this.m_doorOpenEffect != null && (int)num == (int)eventID)
                        {
                            InstanceID instance4 = new InstanceID();
                            instance4.Event = eventID;
                            EffectInfo.SpawnArea spawnArea = new EffectInfo.SpawnArea(Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)data.m_building].CalculatePosition(this.m_doorPosition), Vector3.up, 10f);
                            Singleton<EffectManager>.instance.DispatchEffect(this.m_doorOpenEffect, instance4, spawnArea, Vector3.zero, 0.0f, 1f, Singleton<AudioManager>.instance.DefaultGroup);
                        }
                    }
                }
                else if ((data.m_flags & EventData.Flags.Loading) != EventData.Flags.None)
                {
                    data.m_flags &= ~EventData.Flags.Loading;
                    if (this.m_doorCloseEffect != null && (int)num == (int)eventID)
                    {
                        InstanceID instance4 = new InstanceID();
                        instance4.Event = eventID;
                        EffectInfo.SpawnArea spawnArea = new EffectInfo.SpawnArea(Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)data.m_building].CalculatePosition(this.m_doorPosition), Vector3.up, 10f);
                        Singleton<EffectManager>.instance.DispatchEffect(this.m_doorCloseEffect, instance4, spawnArea, Vector3.zero, 0.0f, 1f, Singleton<AudioManager>.instance.DefaultGroup);
                    }
                }
            }
            base.SimulationStep(eventID, ref data);
        }

        private EffectInfo m_doorCloseEffect => (EffectInfo)typeof(RocketLaunchAI).GetField("m_doorCloseEffect",
            BindingFlags.Instance | BindingFlags.Public).GetValue(this);

        private EffectInfo m_doorOpenEffect => (EffectInfo)typeof(RocketLaunchAI).GetField("m_doorOpenEffect",
            BindingFlags.Instance | BindingFlags.Public).GetValue(this);

        private int m_resourceConsumption => (int)typeof(RocketLaunchAI).GetField("m_resourceConsumption",
            BindingFlags.Instance | BindingFlags.Public).GetValue(this);

        private Vector3 m_doorPosition => (Vector3)typeof(RocketLaunchAI).GetField("m_doorPosition",
            BindingFlags.Instance | BindingFlags.Public).GetValue(this);

        private Vector3 m_rocketSpawnPosition => (Vector3)typeof(RocketLaunchAI).GetField("m_rocketSpawnPosition",
            BindingFlags.Instance | BindingFlags.Public).GetValue(this);
    }
}