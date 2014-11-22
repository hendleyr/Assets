using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Planner.Productions
{
    public class CaseProduction
    {
        public Func<IRenderableProduction, bool> Case { get; set; }
        public IRenderableProduction Body { get; set; }

        public IList<GameObject> ToGeometry(IRenderableProduction parent)
        {
            return Body.ToGeometry(parent);
        }
    }
}
