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
    public int Health;

    public string AttackName;
    public int BaseDamage;
    public int DamageVariance;

    public string SpecialName;
    public TurnController.ENEMY_SPECIAL_TYPES SpecialType;
    public float SpecialValue; // Health regained/ Attack damage/ Defence value this is SO versatile
    public float SpecialChance;
    public float CriticalChance;
    public float TurnDuration;

    public Sprite FullSprite;
    public Vector3 OverrideSpriteScale;
    public Vector3 SpriteMaskDimensions;
    public Color WeakpointMaskColour;

    [Header("Healthbar Data")]
    public Sprite HealthBarMask;
    public Sprite HealthBarOutline;
    public Color BackingColour;
    public Color TweenColour;
    public Color FillColour;
    public Color SecondaryTint;

    // TODO: flesh out a weakpoint system using the mask mapping
}
