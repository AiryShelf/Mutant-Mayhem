using UnityEngine;

[CreateAssetMenu(menuName = "EnemyEvolution/Genes/Head")]
public class HeadGeneSO : ScriptableObject
{
    public string id = "Head_Default";
    public Sprite sprite;
    public Color  color = Color.white;
    public float  scale = 1f;
}