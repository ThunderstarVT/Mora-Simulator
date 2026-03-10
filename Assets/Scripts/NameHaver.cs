using UnityEngine;

public class NameHaver : MonoBehaviour
{
    [SerializeField] private new string name = "Null";
    public string Name => name;
}
