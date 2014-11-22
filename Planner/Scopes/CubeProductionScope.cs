// ReSharper disable RedundantArgumentName
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Planner.Productions;
using UnityEngine;

namespace Assets.Planner.Scopes
{
    public class CubeProductionScope : ProductionScope
    {
        public CubeProductionScope(ProductionScope parent)
            : base(parent) {}

        public CubeProductionScope(Bounds bounds, Quaternion rotation)
            : base(bounds, rotation) {}

        public override ProductionScope[] Select(SelectProduction selector)
        {
            var selectandScopes = new List<ProductionScope>();

            if (selector.Faces.Contains(ScopeFace.Front))
            {
                selectandScopes.Add(SelectionScaleAndShift(selector, Quaternion.identity * Rotation, new Vector3(1, 0, 0), ProductionAxis.X));
            }

            if (selector.Faces.Contains(ScopeFace.Back))
            {
                selectandScopes.Add(SelectionScaleAndShift(selector, Quaternion.AngleAxis(180, Vector3.up) * Rotation, new Vector3(1, 0, 0), ProductionAxis.X));
            }

            if (selector.Faces.Contains(ScopeFace.Left))
            {
                selectandScopes.Add(SelectionScaleAndShift(selector, Quaternion.AngleAxis(90, Vector3.up) * Rotation, new Vector3(1, 0, 0), ProductionAxis.X));
            }

            if (selector.Faces.Contains(ScopeFace.Right))
            {
                selectandScopes.Add(SelectionScaleAndShift(selector, Quaternion.AngleAxis(-90, Vector3.up) * Rotation, new Vector3(1, 0, 0), ProductionAxis.X));
            }

            if (selector.Faces.Contains(ScopeFace.Top))
            {
                selectandScopes.Add(SelectionScaleAndShift(selector, Quaternion.identity * Rotation, new Vector3(0, -1, 0), ProductionAxis.Y));
            }

            if (selector.Faces.Contains(ScopeFace.Bottom))
            {
                var scope = SelectionScaleAndShift(selector, Quaternion.identity * Rotation, new Vector3(0, 1, 0), ProductionAxis.Y);
                scope.Bounds = new Bounds(new Vector3(scope.Bounds.center.x, scope.Bounds.size.y/2f, scope.Bounds.center.z), scope.Bounds.size);
                selectandScopes.Add(scope);
            }

            return selectandScopes.ToArray();
        }

        private CubeProductionScope SelectionScaleAndShift(SelectProduction selector, Quaternion rotation, Vector3 octant, ProductionAxis axis)
        {
            var s = Bounds.size;
            s[(int)axis] = selector.IsAbsolute
                           ? selector.FaceBreadth
                           : s[(int)axis] * selector.FaceBreadth;
            var offset = (Bounds.size[(int)axis] - s[(int)axis]) / 2f;

            var c = Bounds.center - ((rotation * octant) * offset);
            return new CubeProductionScope(new Bounds(c, s), rotation);
        }

        /// <summary>
        /// Divide scope into portions as described by divisors list. 
        /// Add snap planes at the dividend bounds if a snap id is given in the divisor.
        /// If any snap planes in the selector list are found within a dividend scope, snap the bounds to match the nearest snap plane of that selector type.
        /// </summary>
        /// <param name="divider"> </param>
        /// <returns></returns>
        public override ProductionScope[] Divide(DivideProduction divider)
        {
            var cmds = divider.Divisors;

            // step 1: validate divisor magnitudes. scopeLength > absolute length; scopeLength - absolute lengths = availableRelLength
            var absSpan = cmds.Where(div => div.IsAbsolute).Sum(div => div.Magnitude);
            var relSpan = cmds.Where(div => div.IsAbsolute).Sum(div => div.Magnitude);
            if (Math.Abs(relSpan - 1) > Mathf.Epsilon || absSpan > Bounds.size[(int)divider.DivisionAxis])
            {
                throw new Exception("Division length is invalid.");
            }

            var dividends = new ProductionScope[cmds.Count()];
            var frontierPoint = Bounds.min[(int)divider.DivisionAxis];
            for (var i = 0; i < cmds.Length; i++)
            {
                var divisor = cmds[i];
                var dividendSize = Bounds.size;
                // slice volume along our division axis, slicing off [magnitude] volume or [magnitude]% of the volume available for relative divisions
                dividendSize[(int)divider.DivisionAxis] = divisor.IsAbsolute
                                               ? divisor.Magnitude
                                               : relSpan/divisor.Magnitude;

                // compute dividend volume's center; just move from frontier point (min) to terminal point (max) along the division axis
                var dividendCenter = Bounds.center;
                dividendCenter[(int)divider.DivisionAxis] = frontierPoint + dividendSize[(int)divider.DivisionAxis] / 2f;

                var proposedBounds = new Bounds(dividendCenter, dividendSize);
                dividends[i] = new CubeProductionScope(
                    bounds: SnapBoundsToSnapPlanes(divider.DivisionAxis, SelectSnapPlanes(divider.DivisionAxis, divider.SnapToPlanes), proposedBounds),
                    rotation: Rotation);

                if (!string.IsNullOrEmpty(divisor.SnapPlaneKey))
                {
                    // add another snap plane to the module
                    AddSnapPlane(divider.DivisionAxis, divisor.SnapPlaneKey, dividends[i].Bounds);
                }

                // update frontier point
                frontierPoint = dividends[i].Bounds.max[(int)divider.DivisionAxis];
            }

            return dividends;
        }

        /// <summary>
        /// Create scopes for each repetition of the repeat production.
        /// Add snap planes at the replicand bounds if a snap id is given in the repeater.
        /// If any snap planes in the selector list are found within a replicand scope, snap the bounds to match the nearest snap plane of that selector type.
        /// 
        /// Some replicand scopes may vary in size depending on the volume remainder and a specified remainder mode.
        /// Remainders below RepeatEpsilon epsilon are always distributed (to avoid teeny-tiny inserts).
        /// </summary>
        /// <param name="repeater"> </param>
        /// <returns></returns>
        public override ProductionScope[] Repeat(RepeatProduction repeater)
        {
            if (repeater.Magnitude > Bounds.size[(int)repeater.RepetitionAxis] || Math.Abs(repeater.Magnitude - 0) < Mathf.Epsilon)
            {
                throw new Exception("Repetition length is invalid.");
            }

            var repetitions = Bounds.size[(int)repeater.RepetitionAxis] / repeater.Magnitude;
            var repRemainder = repetitions - (int) repetitions;
            var frontierPoint = Bounds.min[(int)repeater.RepetitionAxis];   //todo: apply rotations to scope during calculation

            Vector3 firstRepSize;
            var replicandSize = Bounds.size;
            Vector3 lastRepSize;

            ProductionScope[] replicands;
            if (repeater.RemainderMode == RepeatRemainderMode.DistributeRemainder || repRemainder < RepetitionEpsilon)
            {
                // evenly distribute remainder (if any) to replicands
                replicands = new ProductionScope[(int) repetitions];

                replicandSize[(int)repeater.RepetitionAxis] = replicandSize[(int)repeater.RepetitionAxis] / replicands.Length;
                firstRepSize = replicandSize;
                lastRepSize = replicandSize;
            }
            else
            {
                replicandSize[(int)repeater.RepetitionAxis] = repeater.Magnitude;
                firstRepSize = replicandSize;
                lastRepSize = replicandSize;

                if (repeater.RemainderMode == RepeatRemainderMode.MergeFirst ||
                    repeater.RemainderMode == RepeatRemainderMode.MergeLast ||
                    repeater.RemainderMode == RepeatRemainderMode.MergeFirstAndLast)
                {
                    replicands = new ProductionScope[(int) repetitions];

                    if (repeater.RemainderMode == RepeatRemainderMode.MergeFirst)
                    {
                        firstRepSize[(int)repeater.RepetitionAxis] = replicandSize[(int)repeater.RepetitionAxis] +
                                                   Bounds.size[(int)repeater.RepetitionAxis] % repeater.Magnitude;
                    }
                    else if (repeater.RemainderMode == RepeatRemainderMode.MergeLast)
                    {
                        lastRepSize[(int)repeater.RepetitionAxis] = replicandSize[(int)repeater.RepetitionAxis] + Bounds.size[(int)repeater.RepetitionAxis] % repeater.Magnitude;
                    }
                    else // if (repeater.RemainderMode == RepeatRemainderMode.MergeFirstAndLast)
                    {
                        firstRepSize[(int)repeater.RepetitionAxis] = replicandSize[(int)repeater.RepetitionAxis] + (Bounds.size[(int)repeater.RepetitionAxis] % repeater.Magnitude / 2);
                        lastRepSize[(int)repeater.RepetitionAxis] = replicandSize[(int)repeater.RepetitionAxis] + (Bounds.size[(int)repeater.RepetitionAxis] % repeater.Magnitude / 2);
                    }
                }
                else
                {
                    //if (repeater.RemainderMode == RepeatRemainderMode.InsertFirst || repeater.RemainderMode == RepeatRemainderMode.InsertLast || repeater.RemainderMode == RepeatRemainderMode.InsertFirstAndLast)
                    replicands = new ProductionScope[(int) repetitions + 1];

                    if (repeater.RemainderMode == RepeatRemainderMode.InsertFirst)
                    {
                        // insert replicand for remainder at first position
                        firstRepSize[(int)repeater.RepetitionAxis] = Bounds.size[(int)repeater.RepetitionAxis] % repeater.Magnitude;
                    }
                    else if (repeater.RemainderMode == RepeatRemainderMode.InsertLast)
                    {
                        // insert replicand for remainder at last position
                        lastRepSize[(int)repeater.RepetitionAxis] = Bounds.size[(int)repeater.RepetitionAxis] % repeater.Magnitude;
                    }
                    else //if (repeater.RemainderMode == RepeatRemainderMode.InsertFirstAndLast)
                    {
                        // insert replicands for remainder at first and last positions
                        replicands = new ProductionScope[(int) repetitions + 2];

                        firstRepSize[(int)repeater.RepetitionAxis] = Bounds.size[(int)repeater.RepetitionAxis] % repeater.Magnitude / 2;
                        lastRepSize[(int)repeater.RepetitionAxis] = Bounds.size[(int)repeater.RepetitionAxis] % repeater.Magnitude / 2;
                    }
                }
            }

            for (var i = 0; i < replicands.Length; ++i)
            {
                var replicandCenter = Bounds.center;
                Bounds proposedBounds;

                if (i == 0)
                {
                    replicandCenter[(int)repeater.RepetitionAxis] = frontierPoint + firstRepSize[(int)repeater.RepetitionAxis] / 2f;
                    proposedBounds = new Bounds(replicandCenter, firstRepSize);
                }
                else if (i == replicands.Length - 1)
                {
                    replicandCenter[(int)repeater.RepetitionAxis] = frontierPoint + lastRepSize[(int)repeater.RepetitionAxis] / 2f;
                    proposedBounds = new Bounds(replicandCenter, lastRepSize);
                }
                else
                {
                    // compute dividend volume's center; just move from frontier point (min) to terminal point (max) along the division axis
                    replicandCenter[(int)repeater.RepetitionAxis] = frontierPoint + replicandSize[(int)repeater.RepetitionAxis] / 2f;
                    proposedBounds = new Bounds(replicandCenter, replicandSize);
                }

                replicands[i] = new CubeProductionScope(
                    bounds: SnapBoundsToSnapPlanes(repeater.RepetitionAxis, SelectSnapPlanes(repeater.RepetitionAxis, repeater.SnapToPlanes), proposedBounds),
                    rotation: Rotation);

                if (!string.IsNullOrEmpty(repeater.SnapPlaneKey))
                {
                    // add another snap plane to the module
                    AddSnapPlane(repeater.RepetitionAxis, repeater.SnapPlaneKey, replicands[i].Bounds);
                }

                // update frontier point
                frontierPoint = replicands[i].Bounds.max[(int)repeater.RepetitionAxis];
            }

            return replicands;
        }

        #region Snap Planes
        /// <summary>
        /// Select all snap planes with normal matching a given axis and with name among given selector.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="snapPlaneSelectors"></param>
        /// <returns></returns>
        private IEnumerable<Plane> SelectSnapPlanes(ProductionAxis axis, IEnumerable<string> snapPlaneSelectors)
        {
            var snaps = snapPlaneSelectors != null
                            ? ModuleProduction.SnapPlanes.Where(planeEntry => planeEntry.Value.normal[(int)axis] > 0
                                && snapPlaneSelectors.Any(selector => selector.Equals(planeEntry.Key))).Select(kvp => kvp.Value)
                            : new Plane[0];

            return snaps;
        }

        /// <summary>
        /// Nudge bounds to match with the closest snap plane.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="selectedSnapPlanes">
        /// All selected snap planes have normal vector with a component matching axis of magnitude 1;
        /// eg if ProductionAxis.X then all planes have normal (+/-1, 0, 0)
        /// </param>
        /// <param name="proposedBounds"></param>
        /// <returns></returns>
        private Bounds SnapBoundsToSnapPlanes(ProductionAxis axis, IEnumerable<Plane> selectedSnapPlanes, Bounds proposedBounds)
        {
            var candidates = selectedSnapPlanes
                .OrderBy(plane => Mathf.Abs(plane.distance - proposedBounds.max[(int)axis]))
                .ToArray();

            if (candidates.Length > 0)
            {
                if (candidates[0].distance < proposedBounds.min[(int)axis])
                {
                    var newMax = proposedBounds.max;
                    newMax[(int)axis] = candidates[0].distance;

                    var newCenter = proposedBounds.center;
                    newCenter[(int)axis] = proposedBounds.min[(int)axis] + (newMax[(int)axis] - proposedBounds.min[(int)axis]) / 2f;

                    return new Bounds(newCenter, newMax - proposedBounds.min);
                }
            }
            return proposedBounds;
        }

        private static void AddSnapPlane(ProductionAxis axis, string snapPlaneKey, Bounds volumeTerminus)
        {
            var normal = new Vector3();
            normal[(int)axis] = 1;
            ModuleProduction.SnapPlanes.Add(new KeyValuePair<string, Plane>(snapPlaneKey, new Plane(normal, volumeTerminus.max[(int)axis])));
        }
        #endregion

        #region Occlusion
        public override bool IsOccluded()
        {
            var occluders = ModuleProduction.Occluders.ToArray();

            // get basis axes of the OBB
            Matrix4x4 myMatrix = Matrix4x4.TRS(Vector3.zero, Rotation, Vector3.one);
            Vector3 vI = myMatrix.MultiplyVector(Vector3.right);
            Vector3 vJ = myMatrix.MultiplyVector(Vector3.up);
            Vector3 vK = myMatrix.MultiplyVector(Vector3.forward);

            // transform AABB min and max points by rotation to get OBB extents
            var obbMin = myMatrix.MultiplyPoint(Bounds.min);
            var obbMax = myMatrix.MultiplyPoint(Bounds.max);

            // precalc projected bounds
            var iMin = Vector3.Dot(obbMin, vI);
            var iMax = Vector3.Dot(obbMax, vI);
            var jMin = Vector3.Dot(obbMin, vJ);
            var jMax = Vector3.Dot(obbMax, vJ);
            var kMin = Vector3.Dot(obbMin, vK);
            var kMax = Vector3.Dot(obbMax, vK);

            var collision = false;
            for (var i = 0; i < occluders.Count() /*&& !collision*/; ++i)
            {
                // check if bounding boxes overlap
                if (Bounds.Intersects(occluders[i].Bounds))
                {
                    var occluderMatrix = Matrix4x4.TRS(Vector3.zero, occluders[i].Rotation, Vector3.one);
                    if (occluders[i].GetType() == typeof(CubeProductionScope))
                    {
                        // occluders OBB extents
                        var occMin = occluderMatrix.MultiplyPoint(occluders[i].Bounds.min);
                        var occMax = occluderMatrix.MultiplyPoint(occluders[i].Bounds.max);

                        // occluders projected bounds
                        var iOccMin = Vector3.Dot(occMin, vI);
                        var iOccMax = Vector3.Dot(occMax, vI);
                        var jOccMin = Vector3.Dot(occMin, vJ);
                        var jOccMax = Vector3.Dot(occMax, vJ);
                        var kOccMin = Vector3.Dot(occMin, vK);
                        var kOccMax = Vector3.Dot(occMax, vK);

                        // check projected bounding boxes
                        if (iMin > iOccMax || iOccMin > iMax
                            || jMin > jOccMax || jOccMin > jMax
                            || kMin > kOccMax || kOccMin > kMax)
                        {
                            Console.WriteLine("Found a type 2 (OBB) separating plane; no collision!");
                        }
                        else
                        {
                            Console.WriteLine("Found a an occlusion with {0} and {1}!", Bounds, occluders[i].Bounds);
                            collision = true;
                        }
                    }
                    else if (occluders[i].GetType() == typeof(PrismProductionScope))
                    {
                        throw new NotImplementedException();
                        //todo: OBB occlusion against prisms
                    }
                }
                else
                {
                    Console.WriteLine("Found a type 1 (AABB) separating plane; no collision!");
                }
            }

            return collision;
        }
        #endregion
    }
}