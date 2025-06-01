using UnityEngine;

[CreateAssetMenu(menuName = "EnemyEvolution/Genes/Leg")]
public class LegGeneSO : ScriptableObject       // 🔸 NEW
{
    public string id = "Leg_Default";
    public Sprite lSprite;
    public Sprite rSprite;
}