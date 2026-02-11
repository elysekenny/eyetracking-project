using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "New Scriptable Object/Enemy Data")]
public class NewEnemy : ScriptableObject
{
    [Header("Information")]
    public String DisplayName;
    public Sprite HudIcon;

    [Header("World Data")]
    public GameObject WorldReference;
    public Color SpriteColour;

    [Header("Combat Data")]
    public Sprite FullSprite;

    // TODO: flesh out a weakpoint system using the mask mapping
}
