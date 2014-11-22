using System.Collections.Generic;
using Assets.Planner.Scopes;
using UnityEngine;

namespace Assets.Planner.Productions
{
    public interface IRenderableProduction
    {
        string Name { get; }
        ProductionScope Scope { get; set; }
        bool IsDeferred { get; set; }
        bool IsOccluder { get; set; }

        IList<GameObject> ToGeometry(IRenderableProduction parent);
    }
}
