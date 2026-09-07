using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Graph", menuName = "Graph/Standard Graph")]
public class StandardGraph : BaseGraph
{
#if UNITY_EDITOR
    public override List<string> GetCategories()
    {
        return new List<string> { "Standard" };
    }
#endif
}
