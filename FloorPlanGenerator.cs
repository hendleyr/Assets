using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets
{
    public class FloorPlanGenerator : MonoBehaviour
    {
        private const int MinSides = 3;
        private const int MaxSides = 8;
        private const double Epsilon = 0.0001;
        /*
function GenerateFloorPlan {

	Shapes = GenerateShape() K times
	FloorPlan = new VerticesList();
	processedShapes;
	processedTris;
	
	for each shape in Shapes {
	
		var tris = CalcTris(shape);
		fpInside = GetInsidePoints(FloorPlan, tris);
		shInside = GetInsidePoints(shape.Vertices, processedTris);
		
		merge(FloorPlan, fpInside, shape, shInside);
		
		processedShapes.Add(shape);
		processedTris.Add(tris);
	}
	

	return FloorPlan;
}

function merge(map, insideMap, shapeB, insideB) {

	var imEdges = [ 
		{ insideMap[0], insideMap[0].next }, 
		{ insideMap[0].prev, insideMap[0] },
			etc...
	], ibEdges = [ same ];
	
	var cur = map[0];
	do {
	
		//if cur in map, check edge(cur, cur.next) against ibEdges;
		//if cur in insideB, check edge(cur, cur.next) against imEdges;
		intersections = FindIntersections(cur, imEdges/ibEdges);
		newVertex = intersection closest to cur;
		
		cur.next = newVertex;
		newVertex.next = imEdges/ibEdges .next;
	} while cur != map[0];
}

*/

        // Use this for initialization
        void Start () {

        }
	
        // Update is called once per frame
        void Update () {
	
        }
        /*
        public LinkedList<Vector2> MergePolygons(LinkedList<Vector2> poly1, LinkedList<Vector2> poly2)
        {
            //todo
            var result = new LinkedList<Vector2>();

            var poly1Tris = poly1.ToTriangleFan();
            var poly2Tris = poly2.ToTriangleFan();

            var poly1InVerts = poly1.Where(v => poly2Tris.AsEnumerable().Any(tri => v.IsInTriangle(tri)));
            var poly1Edges = poly1.Select((v, index) => new[] { 
                                                                  new[] {
                                                                            poly1.ElementAt(index),
                                                                            poly1.ElementAt(index + 1 % poly1.Count())
                                                                        }
                                                              });

            var poly2InVerts = poly2.Where(v => poly1Tris.AsEnumerable().Any(tri => v.IsInTriangle(tri)));
            var poly2Edges = poly2.Select((v, index) => new[] { 
                                                                  new[] {
                                                                            poly2.ElementAt(index),
                                                                            poly2.ElementAt(index + 1 % poly2.Count())
                                                                        }
                                                              });

            return result;
        }

        /// <summary>
        /// Finds intersection point between lines (p11 - p12) and (p21 - p22)
        /// (We can skip line segment checks because we know an endpoint is inside some triangle.)
        /// </summary>
        /// <returns>Null if no intersection</returns>
        public Vector2? EdgeIntersection(Vector2 p11, Vector2 p12, Vector2 p21, Vector2 p22)
        {
            // segment eq. 1
            var a1 = p12.y - p11.y;
            var b1 = p11.x - p12.x;
            var c1 = a1 * p11.x + b1 * p11.y;

            // segment eq. 2
            var a2 = p22.y - p21.y;
            var b2 = p21.x - p22.x;
            var c2 = a2 * p21.x + b2 * p21.y;

            var denom = a1*b2 - a2*b1;
            if(Math.Abs(denom - 0) < Mathf.Epsilon) {
                // lines are parallel; but you shouldn't be sending parallels anyway
                return null;
            }

            var x = (b2*c1 - b1*c2)/denom;
            var y = (a1*c2 - a2*c1)/denom;

            return new Vector2(x, y);
        }

        /// <summary>
        /// Generates a polygon
        /// </summary>
        /// <returns></returns>
        public LinkedList<Vector2> GeneratePolygon()
        {
            var vertices = Random.Range(MinSides, MaxSides);
            var polygon = new LinkedList<Vector2>();

            for (var i = 0; i < vertices; ++i)
            {
                var vertex = new Vector2(Mathf.Cos(2 * Mathf.PI * i / vertices), Mathf.Sin(2 * Mathf.PI * i / vertices));
                polygon.AddLast(vertex);
            }

            return polygon;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public bool IsInPolygon(Vector2 point, Vector2[] polygon)
        {
            var triangles = polygon.ToTriangleFan();
            return triangles.Any(tri => point.IsInTriangle(tri));
        }
        */
    }

    public static class Vector2Extension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static IEnumerable<Vector2[]> ToTriangleFan(this IEnumerable<Vector2> polygon)
        {
            var triangles = new List<Vector2[]>();
            var enumerable = polygon as Vector2[] ?? polygon.ToArray();

            for (var i = 1; i < enumerable.Count(); ++i)
            {
                triangles.Add(new[] { enumerable.ElementAt(0), enumerable.ElementAt(i), enumerable.ElementAt(i + 1) });
            }

            return triangles;
        }

        /// <summary>
        /// Barycentric technique for point-in-triangle test
        /// </summary>
        /// <param name="point"></param>
        /// <param name="tri"></param>
        /// <returns></returns>
        public static bool IsInTriangle(this Vector2 point, Vector2[] tri)
        {
            // Compute vectors
            Vector2 v0 = tri[2] - tri[0];
            Vector2 v1 = tri[1] - tri[0];
            Vector2 v2 = point - tri[0];

            // Compute dot products
            float dot00 = Vector2.Dot(v0, v0);
            float dot01 = Vector2.Dot(v0, v1);
            float dot02 = Vector2.Dot(v0, v2);
            float dot11 = Vector2.Dot(v1, v1);
            float dot12 = Vector2.Dot(v1, v2);

            // Compute barycentric coordinates
            var invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
            var u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            var v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            // Check if point is in triangle
            return (u >= 0) && (v >= 0) && (u + v < 1);
        }
    }
}