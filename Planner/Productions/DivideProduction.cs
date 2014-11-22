using System.Collections.Generic;
using Assets.Planner.Scopes;
using UnityEngine;

namespace Assets.Planner.Productions
{
    public class DivideProduction: IRenderableProduction
    {
        public DivisorProduction[] Divisors { get; set; }
        public ProductionAxis DivisionAxis { get; set; }
        public string[] SnapToPlanes { get; set; }

        public string Name { get; set; }
        public ProductionScope Scope { get; set; }
        public bool IsDeferred { get; set; }
        public bool IsOccluder { get; set; }

        public IList<GameObject> ToGeometry(IRenderableProduction parent)
        {
            var scopes = parent.Scope.Divide(this);
            var geometry = new List<GameObject>();
            for (var i = 0; i < Divisors.Length; i++)
            {
                Divisors[i].DividendProduction.Scope = scopes[i];
                var dividendGeometry = Divisors[i].DividendProduction.ToGeometry(parent);
                if (dividendGeometry.Count > 0)
                {
                    geometry.AddRange(dividendGeometry);
                }
            }

            return geometry;
        }
    }
}
