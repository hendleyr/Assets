using System.Collections.Generic;
using Assets.Planner.Scopes;
using UnityEngine;

namespace Assets.Planner.Productions
{
    public class TerminalProduction : IRenderableProduction
    {
        public string Name { get; set; }
        public ProductionScope Scope { get; set; }
        public bool IsDeferred { get; set; }
        public bool IsOccluder { get; set; }
        private Quaternion _quat = Quaternion.identity;
        private Quaternion Rotation { get { return _quat; } set { _quat = value; } }

        public GameObject Terminal { get; set; }

        public TerminalProduction(GameObject asset)
        {
            Terminal = asset;
        }

        public IList<GameObject> ToGeometry(IRenderableProduction parent)
        {
            var terminal = (GameObject)Object.Instantiate(Terminal, parent.Scope.Bounds.center, Quaternion.identity);

            var terminalBounds = terminal.GetComponentInChildren<MeshRenderer>().bounds;
            terminal.GetComponentInChildren<MeshRenderer>().transform.localScale = new Vector3(
                1 / (terminalBounds.size.x / parent.Scope.Bounds.size.x),
                1 / (terminalBounds.size.y / parent.Scope.Bounds.size.y),
                1 / (terminalBounds.size.z / parent.Scope.Bounds.size.z)
            );
            terminal.transform.rotation = parent.Scope.Rotation * Rotation;

            return new List<GameObject> {terminal};
        }

        public TerminalProduction Rotate(Quaternion quaternion)
        {
            Rotation = quaternion;
            return this;
        }
    }
}