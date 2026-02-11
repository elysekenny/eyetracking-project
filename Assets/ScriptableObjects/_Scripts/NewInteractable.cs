using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewInteractable", menuName = "New Scriptable Object/Interactable Data")]
public class NewInteractable : ScriptableObject
{
    [Header("Information")]
    public String DisplayName;
    public Sprite ObjectSprite;

    [Header("World Data")]
    public GameObject WorldReference;
}
