using Unity.VisualScripting;
using UnityEngine;

public class LoadEnemy : MonoBehaviour
{
    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    public NewEnemy EnemyToLoad;
}
