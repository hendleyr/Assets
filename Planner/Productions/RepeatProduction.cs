using System.Collections.Generic;
using Assets.Planner.Scopes;
using UnityEngine;

namespace Assets.Planner.Productions
{
    /// <summary>
    /// Repeat magnitudes are always absolute. 
    /// Create n production scopes along the repeat axis, where n = parent scope axis length / magnitude
    /// </summary>
    public class RepeatProduction : IRenderableProduction
    {
        public ProductionAxis RepetitionAxis { get; set; }
        public float Magnitude { get; set; }
        public RepeatRemainderMode RemainderMode { get; set; }
        public RegistrarProduction ReplicandProduction { get; set; }
        public string[] SnapToPlanes { get; set; }
        public string SnapPlaneKey { get; set; }
        public string Name { get; set; }
        public ProductionScope Scope { get; set; }
        public bool IsDeferred { get; set; }
        public bool IsOccluder { get; set; }
        
        public IList<GameObject> ToGeometry(IRenderableProduction parent)
        {
            var scopes = parent.Scope.Repeat(this);
            var geometry = new List<GameObject>();
            foreach (ProductionScope scope in scopes)
            {
                ReplicandProduction.Scope = scope;
                geometry.AddRange(ReplicandProduction.ToGeometry(parent));
            }

            return geometry;
        }
    }
}
