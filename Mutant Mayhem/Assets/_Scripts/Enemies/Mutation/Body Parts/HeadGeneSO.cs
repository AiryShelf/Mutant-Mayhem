using UnityEngine;

[CreateAssetMenu(menuName = "EnemyEvolution/Genes/Head")]
public class HeadGeneSO : ScriptableObject      // 🔸 NEW
{
    public string id = "Head_Default";
    public Sprite sprite;
}