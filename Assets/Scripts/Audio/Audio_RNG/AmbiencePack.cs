using System;
using UnityEngine;
using FMODUnity;

[CreateAssetMenu(fileName = "AmbiencePack", menuName = "Game/Ambience Pack")]
public class AmbiencePack : ScriptableObject
{
    [Serializable]
    public class ZoneAmbience
    {
        public string zoneId;
        public EventReference[] sounds;
    }

    [SerializeField] private ZoneAmbience[] zones;

    public ZoneAmbience[] Zones => zones;

    public EventReference[] GetSoundsForZone(string zoneId)
    {
        foreach (var zone in zones)
            if (zone.zoneId == zoneId)
                return zone.sounds;
        return null;
    }
}