using System;
using System.Collections.Generic;
using Assets.Planner.Scopes;
using UnityEngine;

namespace Assets.Planner.Productions
{
    public class RegistrarProduction : IRenderableProduction
    {
        public static RegistrarProduction Empty = new RegistrarProduction("Empty", new CubeProductionScope(new Bounds(), Quaternion.identity));

        private bool _finalFlag;
        public string Name { get; private set; }
        public ProductionScope Scope { get; set; }
        public bool IsDeferred { get; set; }
        public bool IsOccluder { get; set; }
        public IList<IRenderableProduction> ChildProductions { get; private set; }
        
        public RegistrarProduction(string name, ProductionScope scope)
        {
            _finalFlag = false;
            Name = name;
            Scope = new CubeProductionScope(scope);
            ChildProductions = new List<IRenderableProduction>();
        }

        public IList<GameObject> ToGeometry(IRenderableProduction parent)
        {
            var myTerminals = new List<GameObject>();
            foreach (var production in ChildProductions)
            {
                var childTerminals = production.ToGeometry(this);
                if (childTerminals.Count > 0)
                {
                    myTerminals.AddRange(childTerminals);
                }
            }

            return myTerminals;
        }

        #region Finalizing Productions
        /// <summary>
        ///  Will set child productions to the default production, or the FIRST conditional production which passes its truth test.
        /// </summary>
        /// <param name="switchProduction"></param>
        public RegistrarProduction Switch(SwitchProduction switchProduction)
        {
            if (!_finalFlag)
            {
                ChildProductions.Add(switchProduction);
                _finalFlag = true;
            }
            else
            {
                Debug.Log(string.Format("Encountered an illegal production definition: Registrar {0} already finalized, may not parse Switch production.", Name));
            }

            return this;
        }

        public RegistrarProduction Divide(DivideProduction divideProduction)
        {
            if (!_finalFlag)
            {
                ChildProductions.Add(divideProduction);
                _finalFlag = true;
            }
            else
            {
                Debug.Log(string.Format("Encountered an illegal production definition: Registrar {0} already finalized, may not parse Divide production.", Name));
            }

            return this;
        }

        public RegistrarProduction Repeat(RepeatProduction repeater)
        {
            if (!_finalFlag)
            {
                ChildProductions.Add(repeater);
                _finalFlag = true;
            }
            else
            {
                Debug.Log(string.Format("Encountered an illegal production definition: Registrar {0} already finalized, may not parse Repeat production.", Name));
            }

            return this;
        }

        public RegistrarProduction Terminate(TerminalProduction terminal)
        {
            if (!_finalFlag)
            {
                ChildProductions.Add(terminal);
                _finalFlag = true;
            }
            else
            {
                Debug.Log(string.Format("Encountered an illegal production definition: Registrar {0} already finalized, may not parse Terminate production.", Name));
            }
            
            return this;
        }
        #endregion

        #region Non-Finalizing Productions
        public RegistrarProduction Select(SelectProduction selectProduction)
        {
            ChildProductions.Add(selectProduction);
            return this;
        }

        public RegistrarProduction BoxSplit(RegistrarProduction box)
        {
            ChildProductions.Add(box);
            return this;
        }

        public RegistrarProduction PrismSplit()
        {
            throw new NotImplementedException();
            return this;
        }

        public RegistrarProduction CylinderSplit()
        {
            throw new NotImplementedException();
            return this;
        }

        public RegistrarProduction Move(Vector3 move)
        {
            Scope.Bounds = new Bounds(Scope.Bounds.center + move, Scope.Bounds.size);
            return this;
        }

        public RegistrarProduction Scale(ScaleProduction scale)
        {
            if (scale.Magnitude == null || scale.Magnitude.Length < 3 || scale.IsAbsolute == null || scale.IsAbsolute.Length < 3)
            {
                Debug.Log(string.Format("Encountered an illegal production definition: Scale production is not fully defined."));
                throw new Exception(string.Format("Encountered an illegal production definition: Scale production is not fully defined."));
            }

            var s = Scope.Bounds.size;

            for (var i = 0; i < 3; ++i)
            {
                s[i] = scale.IsAbsolute[i]
                        ? scale.Magnitude[i]       
                        : s[i] * scale.Magnitude[i];
            }
            Scope.Bounds = new Bounds(Scope.Bounds.center, s);

            return this;
        }

        public RegistrarProduction Rotate(Quaternion quaternion)
        {
            Scope.Rotation = quaternion*Scope.Rotation;
            //Scope.Rotation = Scope.Rotation * quaternion;
            return this;
        }
        #endregion
    }
}
