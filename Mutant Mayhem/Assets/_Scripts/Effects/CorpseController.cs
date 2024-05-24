using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorpseController : MonoBehaviour
{
    [SerializeField] Sprite[] corpseSprites;
    SpriteRenderer mySr;

    void Start()
    {
        // Select random corpseSprite
        mySr = GetComponent<SpriteRenderer>();
        int randIndex = Random.Range(0, corpseSprites.Length);
        mySr.sprite = corpseSprites[randIndex];

        // Random flip 
        int sign = Random.Range(0, 2) * 2 - 1; // Randomly 1 or -1
        //Debug.Log(sign);
        mySr.transform.localScale = new Vector3 (mySr.transform.localScale.x, 
                                                 mySr.transform.localScale.y * sign, 
                                                 mySr.transform.localScale.z);
        
    }

}
