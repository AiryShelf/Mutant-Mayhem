using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class ShadowCaster2DHelper
{
    static FieldInfo shapePathField;

    public static void SetShapePath(ShadowCaster2D shadowCaster, Vector3[] shapePath)
    {
        if (shapePathField == null)
        {
            // Find the internal field m_ShapePath (Vector3[])
            shapePathField = typeof(ShadowCaster2D).GetField("m_ShapePath", BindingFlags.Instance | BindingFlags.NonPublic);
            if (shapePathField == null)
            {
                Debug.LogError("ShadowCaster2DHelper: m_ShapePath field not found! Unity version may have changed.");
                return;
            }
        }

        // Set the shape path
        shapePathField.SetValue(shadowCaster, shapePath);

        // Force ShadowCaster2D to rebuild its mesh
        // In Unity 2022.3.x, toggling enabled state forces a refresh
        shadowCaster.enabled = false;
        shadowCaster.enabled = true;
    }
}