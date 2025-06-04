using System.Collections.Generic;
using UnityEngine;

public static class GeneDatabase
{
    static Dictionary<string, BodyGeneSO> _allBodies;
    static Dictionary<string, HeadGeneSO> _allHeads;
    static Dictionary<string, LegGeneSO>  _allLegs;

    static bool _initialised;

    public static void InitialiseIfNeeded()
    {
        if (_initialised) return;

        _allBodies = new Dictionary<string, BodyGeneSO>();
        _allHeads  = new Dictionary<string, HeadGeneSO>();
        _allLegs   = new Dictionary<string, LegGeneSO>();

        foreach (var b in Resources.LoadAll<BodyGeneSO>("")) _allBodies[b.id] = b;
        foreach (var h in Resources.LoadAll<HeadGeneSO>("")) _allHeads[h.id]  = h;
        foreach (var l in Resources.LoadAll<LegGeneSO>(""))  _allLegs[l.id]   = l;

        _initialised = true;
    }

    public static BodyGeneSO Body(string id) { InitialiseIfNeeded(); return _allBodies[id]; }
    public static HeadGeneSO Head(string id) { InitialiseIfNeeded(); return _allHeads[id]; }
    public static LegGeneSO  Leg(string id)  { InitialiseIfNeeded(); return _allLegs[id]; }
}