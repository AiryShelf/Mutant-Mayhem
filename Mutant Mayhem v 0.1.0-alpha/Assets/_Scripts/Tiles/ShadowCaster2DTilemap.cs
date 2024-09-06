using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(CompositeCollider2D))]
public class ShadowCaster2DTileMap : MonoBehaviour
{
    [Space]
    [SerializeField]
    private bool selfShadows = true;

    public TilemapCollider2D tilemapCollider;
    public CompositeCollider2D tilemapCompCollider;


    static readonly FieldInfo meshField = typeof(ShadowCaster2D).GetField(
                                        "m_Mesh", BindingFlags.NonPublic | BindingFlags.Instance);
    static readonly FieldInfo shapePathField = typeof(ShadowCaster2D).GetField(
                                        "m_ShapePath", BindingFlags.NonPublic | BindingFlags.Instance);
    static readonly FieldInfo shapePathHashField = typeof(ShadowCaster2D).GetField(
                                        "m_ShapePathHash", BindingFlags.NonPublic | BindingFlags.Instance);
    static readonly MethodInfo generateShadowMeshMethod = typeof(ShadowCaster2D)
                                    .Assembly
                                    .GetType("UnityEngine.Rendering.Universal.ShadowUtility")
                                    .GetMethod("GenerateShadowMesh", BindingFlags.Public | BindingFlags.Static);
    
    void Start()
    {
        //tilemapCollider = GetComponent<TilemapCollider2D>();
        //tilemapCompCollider = GetComponent<CompositeCollider2D>();
    }

    void Update()
    {
        // For debugging
        if (Input.GetKeyDown("g"))
            Generate();
    }
    
    public void Generate()
    {
        DestroyAllChildren();
        tilemapCollider.ProcessTilemapChanges();
        tilemapCompCollider.GenerateGeometry();   

        for (int i = 0; i < tilemapCompCollider.pathCount; i++)
        {
            Vector2[] pathVertices = new Vector2[tilemapCompCollider.GetPathPointCount(i)];
            tilemapCompCollider.GetPath(i, pathVertices);
            GameObject shadowCaster = new GameObject("shadow_caster_" + i);
            shadowCaster.transform.parent = gameObject.transform;
            ShadowCaster2D shadowCasterComponent = shadowCaster.AddComponent<ShadowCaster2D>();
            shadowCasterComponent.selfShadows = this.selfShadows;

            Vector3[] testPath = new Vector3[pathVertices.Length];
            for (int j = 0; j < pathVertices.Length; j++)
            {
                testPath[j] = pathVertices[j];
            }

            shapePathField.SetValue(shadowCasterComponent, testPath);
            shapePathHashField.SetValue(shadowCasterComponent, Random.Range(int.MinValue, int.MaxValue));
            meshField.SetValue(shadowCasterComponent, new Mesh());
            generateShadowMeshMethod.Invoke(shadowCasterComponent, new object[] { meshField.GetValue(shadowCasterComponent), shapePathField.GetValue(shadowCasterComponent) });
        }

        // Debug.Log("Generate");

    }
    public void DestroyAllChildren()
    {

        var tempList = transform.Cast<Transform>().ToList();
        foreach (var child in tempList)
        {
            Destroy(child.gameObject);
        }
    }
}