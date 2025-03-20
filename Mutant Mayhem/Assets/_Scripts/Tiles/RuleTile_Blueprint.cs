using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "RuleTile_Blueprint", menuName = "Tiles and Structures/RuleTile_Blueprint")]
public class RuleTile_Blueprint : RuleTile
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
    {
        // Call the base method to retrieve default tile data.
        base.GetTileData(location, tilemap, ref tileData);

        // Remove the LockTransform flag to allow custom rotation modifications from the tilemap to persist.
        tileData.flags &= ~TileFlags.LockTransform;

        // By not setting tileData.transform here, we maintain the rotation already defined in the tilemap's transform matrix.
        // If further customization is needed, you can modify the transform by combining the current matrix with additional rotations.
    }
}
