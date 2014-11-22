using System.Collections.Generic;
using Assets.Planner.Scopes;
using UnityEngine;

namespace Assets.Planner.Productions
{
    public class SelectProduction : IRenderableProduction
    {
        public ScopeFace[] Faces { get; set; }
        public float FaceBreadth { get; set; }
        public bool IsAbsolute { get; set; }
        public RegistrarProduction SelectandProduction { get; set; }
        public string Name { get; set; }
        public ProductionScope Scope { get; set; }
        public bool IsDeferred { get; set; }
        public bool IsOccluder { get; set; }

        public IList<GameObject> ToGeometry(IRenderableProduction parent)
        {
            var scopes = parent.Scope.Select(this);
            var geometry = new List<GameObject>();
            foreach (var scope in scopes)
            {
                SelectandProduction.Scope = scope;
                geometry.AddRange(SelectandProduction.ToGeometry(parent));
            }

            return geometry;
        }
    }
}