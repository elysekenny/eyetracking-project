using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewColour", menuName = "New Scriptable Object/Colour Data")]
public class NewColour : ScriptableObject
{
    [Header("Information")]
    public String DisplayName;
    public Sprite MaskShape;
    public Color TintColour;
    public Sprite BackgroundSprite;

    [Header("Interaction Data")]
    public NewInteractable[] InteractableObjects;
    public NewEnemy[] InteractableEnemies;
}
