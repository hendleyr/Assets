using System;
using System.Collections.Generic;
using Assets.Planner.Productions;
using Assets.Planner.Scopes;
using UnityEngine;

namespace Assets.Planner
{
    public class Planner : MonoBehaviour
    {
        private static readonly Dictionary<string, IRenderableProduction> ProductionDictionary = new Dictionary<string, IRenderableProduction>();
        public GameObject TerminalWall;
        public GameObject WoodPlank;
        // Use this for initialization
        void Start()
        {
            DefineProductions();
            //InstanceProduction("House!");
            InstanceProduction("Test");
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ParseProductions(string input)
        {
            //todo: implement a CSS-like grammar that is more user-friendly than "DefineProductions" below.
            throw new NotImplementedException();
        }

        public void DefineProductions()
        {
            //DefineHouse();
            DefineTestBldg();
        }

        private void DefineTestBldg()
        {
            var wallTerm = new TerminalProduction(TerminalWall);
            var plankTerm = new TerminalProduction(WoodPlank);

            var bldg = new RegistrarProduction("Test", new CubeProductionScope(
                                                           new Bounds(Vector3.zero, new Vector3(50, 50, 50)),
                                                           Quaternion.identity)).Rotate(Quaternion.AngleAxis(45, Vector3.up));
            ProductionDictionary.Add(bldg.Name, bldg);
            var wall = new RegistrarProduction("TestWall", bldg.Scope);

            var floor = new RegistrarProduction("TestFloor", bldg.Scope);
            var f1 = new RegistrarProduction("f1", floor.Scope);
            f1.Repeat(new RepeatProduction
                          {
                              Name = "",
                              IsDeferred = false,
                              IsOccluder = false,
                              Magnitude = 5f,
                              RemainderMode = RepeatRemainderMode.MergeFirstAndLast,
                              RepetitionAxis = ProductionAxis.X,
                              ReplicandProduction = new RegistrarProduction("", floor.Scope).Terminate(plankTerm.Rotate(Quaternion.AngleAxis(180f, Vector3.right))),
                              Scope = floor.Scope
                          });
            floor.Repeat(new RepeatProduction 
                            {
                                Name = "",
                                IsDeferred = false,
                                IsOccluder = false,
                                Magnitude = 1.5f,
                                RemainderMode = RepeatRemainderMode.MergeFirstAndLast,
                                RepetitionAxis = ProductionAxis.Z,
                                ReplicandProduction = f1,
                                Scope = floor.Scope
                            });

            wall.Terminate(wallTerm);

            bldg.Select(new SelectProduction
                            {
                                Faces = new[] {ScopeFace.Front, ScopeFace.Back, ScopeFace.Left, ScopeFace.Right},
                                FaceBreadth = 5f,
                                IsAbsolute = true,
                                Name = "TestWalls",
                                SelectandProduction = wall
                            });
            bldg.Select(new SelectProduction
                            {
                                Faces = new[] {ScopeFace.Bottom},
                                FaceBreadth = 1f,
                                IsAbsolute = true,
                                Name = "TestFloor",
                                SelectandProduction = floor
                            });
        }

        /// <summary>
        /// ProductionDictionary stores the production rules that we want to serve as a basis for generation.
        /// IE, it contains "house" but not "houseFirstFloor" because we don't want "houseFirstFloor" to be generated without also generating "houseRoof."
        /// </summary>
        /// <param name="productionName"></param>
        public void InstanceProduction(string productionName)
        {
            ModuleProduction.Begin(ProductionDictionary[productionName]);
            var geom = ModuleProduction.ToGeometry();
            var e = geom.GetEnumerator();
            while(e.MoveNext())
            {
                //Instantiate(e.Current);
            }
        }
    }
}