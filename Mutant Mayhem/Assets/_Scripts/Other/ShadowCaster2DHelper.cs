using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class ShadowCaster2DHelper
{
    static FieldInfo shapePathField;
    static FieldInfo shapePathHashField;
    static FieldInfo meshField;
    static MethodInfo onEnableMethod;

    static bool triedToInit;

    static void EnsureReflection()
    {
        if (triedToInit)
            return;

        triedToInit = true;

        var t = typeof(ShadowCaster2D);

        // Path used for the custom silhouette
        shapePathField = t.GetField("m_ShapePath",
            BindingFlags.Instance | BindingFlags.NonPublic);

        // Hash and mesh are used to detect changes / cache the mesh
        shapePathHashField = t.GetField("m_ShapePathHash",
            BindingFlags.Instance | BindingFlags.NonPublic);

        meshField = t.GetField("m_Mesh",
            BindingFlags.Instance | BindingFlags.NonPublic);

        // Private OnEnable is where Unity usually builds the shadow mesh
        onEnableMethod = t.GetMethod("OnEnable",
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (shapePathField == null)
        {
            Debug.LogError("ShadowCaster2DHelper: m_ShapePath field not found. Unity version may have changed.");
        }
    }

    public static void SetShapePath(ShadowCaster2D shadowCaster, Vector3[] shapePath)
    {
        if (shadowCaster == null || shapePath == null || shapePath.Length < 3)
            return;

        EnsureReflection();

        if (shapePathField == null)
            return;

        // 1) Set the new shape path
        shapePathField.SetValue(shadowCaster, shapePath);

        // 2) Update the hash if the field exists (prevents Unity from thinking nothing changed)
        if (shapePathHashField != null)
        {
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < shapePath.Length; i++)
                    hash = hash * 31 + shapePath[i].GetHashCode();

                shapePathHashField.SetValue(shadowCaster, hash);
            }
        }
    }
}