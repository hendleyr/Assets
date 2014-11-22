using System;
using Assets.Planner.Scopes;

namespace Assets.Planner.Productions
{
    public class ProductionCondition
    {
        public Func<ProductionScope, bool> Validate { get; private set; }
    }
}
