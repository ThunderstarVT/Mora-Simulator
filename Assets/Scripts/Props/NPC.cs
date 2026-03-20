using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Ragdoll)), RequireComponent(typeof(Flammable)), RequireComponent(typeof(NameHaver))]
public class NPC : MonoBehaviour
{
    [SerializeField] private Ragdoll ragdoll;
    [SerializeField] private Flammable flammable;
    [SerializeField] private NameHaver nameHaver;
    
    [Space] 
    [SerializeField] private List<string> npcNames = new();

    private void Start()
    {
        int randomIndex = Random.Range(0, npcNames.Count);
        nameHaver.SetName(npcNames[randomIndex]);

        flammable.OnBurnEnd += ragdoll.SetActive;
    }
}
