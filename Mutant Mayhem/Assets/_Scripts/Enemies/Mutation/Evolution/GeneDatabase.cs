using System.Collections.Generic;
using UnityEngine;

public static class GeneDatabase
{
    static Dictionary<string, BodyGeneSO> _bodies;
    static Dictionary<string, HeadGeneSO> _heads;
    static Dictionary<string, LegGeneSO>  _legs;

    static bool _initialised;

    public static void InitialiseIfNeeded()
    {
        if (_initialised) return;

        _bodies = new Dictionary<string, BodyGeneSO>();
        _heads  = new Dictionary<string, HeadGeneSO>();
        _legs   = new Dictionary<string, LegGeneSO>();

        foreach (var b in Resources.LoadAll<BodyGeneSO>("")) _bodies[b.id] = b;
        foreach (var h in Resources.LoadAll<HeadGeneSO>("")) _heads[h.id]  = h;
        foreach (var l in Resources.LoadAll<LegGeneSO>(""))  _legs[l.id]   = l;

        _initialised = true;
    }

    public static BodyGeneSO Body(string id) { InitialiseIfNeeded(); return _bodies[id]; }
    public static HeadGeneSO Head(string id) { InitialiseIfNeeded(); return _heads[id]; }
    public static LegGeneSO  Leg(string id)  { InitialiseIfNeeded(); return _legs[id]; }
}