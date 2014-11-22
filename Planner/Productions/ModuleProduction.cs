using System.Collections.Generic;
using Assets.Planner.Scopes;
using UnityEngine;

namespace Assets.Planner.Productions
{
    /// <summary>
    /// Maintains information about snap planes, occlusions, and connectivity.
    /// Defines the master scope, ModuleScope, from which all other scopes descend.
    /// </summary>
    public static class ModuleProduction
    {
        public static CubeProductionScope ModuleScope { get; set; }
        public static IDictionary<string, Plane> SnapPlanes { get; set; }
        public static IList<ProductionScope> Occluders { get; set; }
        public static IRenderableProduction BaseProduction { get; set; }

        public static void Begin(IRenderableProduction baseProduction, CubeProductionScope scope)
        {
            BaseProduction = baseProduction;
            ModuleScope = scope;
            SnapPlanes = new Dictionary<string, Plane>();
            Occluders = new List<ProductionScope>();
        }

        public static void Begin(IRenderableProduction baseProduction)
        {
            Begin(baseProduction, new CubeProductionScope(baseProduction.Scope.Bounds, Quaternion.identity));
        }

        public static void Clear()
        {
            ModuleScope = null;
            BaseProduction = null;
            SnapPlanes = null;
            Occluders = null;
        }

        public static IList<GameObject> ToGeometry()
        {
            return BaseProduction.ToGeometry(null);
        }
    }
}
