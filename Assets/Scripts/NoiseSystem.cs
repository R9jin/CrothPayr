using System;
using UnityEngine;

public struct NoiseEvent
{
    public Vector3 position;
    public float radius;
    public GameObject source;
    public float timestamp;

    public NoiseEvent(Vector3 pos, float rad, GameObject src)
    {
        position = pos;
        radius = rad;
        source = src;
        timestamp = Time.time;
    }
}

public static class NoiseSystem
{
    public static event Action<NoiseEvent> OnNoiseEmitted;

    /// <summary>
    /// Emits a sound/noise in the world. Any listener within (listenerRadius + noiseRadius) can detect it.
    /// </summary>
    public static void Emit(Vector3 position, float radius, GameObject source)
    {
        if (radius <= 0f) return;

        NoiseEvent evt = new NoiseEvent(position, radius, source);
        OnNoiseEmitted?.Invoke(evt);
    }
}
