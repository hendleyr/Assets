using System.Collections.Generic;
using System.Linq;
using Assets.Planner.Scopes;
using UnityEngine;

namespace Assets.Planner.Productions
{
    public class SwitchProduction : IRenderableProduction
    {
        public IRenderableProduction DefaultProduction { get; set; }
        public IList<CaseProduction> Cases { get; set; }

        public string Name { get; set; }
        public ProductionScope Scope { get; set; }
        public bool IsDeferred { get; set; }
        public bool IsOccluder { get; set; }

        public IList<GameObject> ToGeometry(IRenderableProduction parent)
        {
            var switchResult = Cases.FirstOrDefault(prod => prod.Case(parent));
            
            return switchResult != null 
                ? switchResult.ToGeometry(parent) 
                : DefaultProduction.ToGeometry(parent);
        }
    }
}
