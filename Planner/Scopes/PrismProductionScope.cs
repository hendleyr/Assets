﻿using System;
using Assets.Planner.Productions;

namespace Assets.Planner.Scopes
{
    public class PrismProductionScope : ProductionScope
    {
        public PrismProductionScope(ProductionScope parent)
            : base(parent)
        {
        }

        public override ProductionScope[] Select(SelectProduction selector)
        {
            throw new NotImplementedException();
        }

        public override ProductionScope[] Divide(DivideProduction divider)
        {
            throw new NotImplementedException();
        }

        public override ProductionScope[] Repeat(RepeatProduction repeater)
        {
            throw new NotImplementedException();
        }

        public override bool IsOccluded()
        {
            throw new NotImplementedException();
        }
    }
}
