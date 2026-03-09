using UnityEngine;

public class NameHaver : MonoBehaviour
{
    [SerializeField] private new string name;
    public string Name => name;
}
