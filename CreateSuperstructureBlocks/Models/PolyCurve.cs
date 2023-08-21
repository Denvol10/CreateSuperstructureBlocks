using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace CreateSuperstructureBlocks.Models
{
    public class PolyCurve
    {
        public List<Curve> Curves { get; set; }
        public List<(Curve Line, double Start, double Finish)> ParametricCurves { get; set; }

        public PolyCurve(IEnumerable<Curve> curves)
        {
            int countCurves = curves.Count();
            int countIter = curves.Count();

            if (countCurves == 0)
            {
                throw new Exception("Линии не выбраны");
            }

            if (countCurves > 1)
            {
                Curve firstUnreverseCurve = null;
                var firstCurve = GetFirstCurve(curves, out firstUnreverseCurve);

                Curves = new List<Curve>()
            {
                firstCurve
            };

                var unsortedCurves = new List<Curve>
            {
                firstCurve,
                firstUnreverseCurve
            };

                countIter--;

                Curve lastCurve = firstCurve;

                while (countIter != 0)
                {
                    foreach (var curve in curves)
                    {
                        if (IsNextCurve(lastCurve, curve) && !unsortedCurves.Contains(curve))
                        {

                            if (IsNeedReverseCurves(lastCurve, curve))
                            {
                                Curve reverseCurve = curve.CreateReversed();
                                Curves.Add(reverseCurve);
                                unsortedCurves.Add(reverseCurve);
                                unsortedCurves.Add(curve);
                                lastCurve = reverseCurve;
                            }
                            else
                            {
                                Curves.Add(curve);
                                unsortedCurves.Add(curve);
                                lastCurve = curve;
                            }
                        }
                    }
                    countIter--;
                }
            }
            else
            {
                Curves = new List<Curve> { curves.First() };
            }

            ParametricCurves = new List<(Curve Line, double Start, double Finish)>();

            double length = Curves.Select(c => c.Length).Sum();
            double restOfLenght = length;

            foreach (var curve in Curves)
            {
                ParametricCurves.Add((curve, length - restOfLenght, length - restOfLenght + curve.Length));
                restOfLenght = restOfLenght - curve.Length;
            }
        }

        public bool Intersect(Curve curve, out Curve interCurve, out double parameter)
        {
            interCurve = null;
            parameter = 0;
            foreach (var paramCurve in ParametricCurves)
            {
                var result = new IntersectionResultArray();
                var compResult = paramCurve.Line.Intersect(curve, out result);
                if (compResult == SetComparisonResult.Overlap)
                {
                    interCurve = paramCurve.Line;
                    foreach (var elem in result)
                    {
                        if (elem is IntersectionResult interResult)
                        {
                            double normalizedParameter = interCurve.ComputeNormalizedParameter(interResult.UVPoint.U);
                            parameter = paramCurve.Start + normalizedParameter * interCurve.Length;
                        }
                    }

                    return true;
                }
            }
            return false;
        }

        public XYZ GetPointOnPolycurve(double parameter, out Curve targetCurve)
        {
            XYZ point = null;
            targetCurve = null;

            foreach (var curve in ParametricCurves)
            {
                if (curve.Start <= parameter && curve.Finish >= parameter)
                {
                    targetCurve = curve.Line;
                    double normalized = (parameter - curve.Start) / (curve.Finish - curve.Start);
                    point = curve.Line.Evaluate(normalized, true);
                    return point;
                }
            }
            return null;
        }

        public Plane GetPlaneOnPolycurve(double parameter)
        {
            XYZ originPoint = null;
            XYZ normal = null;
            Curve targetCurve = null;

            foreach (var curve in ParametricCurves)
            {
                if (curve.Start <= parameter && curve.Finish >= parameter)
                {
                    targetCurve = curve.Line;
                    double normalized = (parameter - curve.Start) / (curve.Finish - curve.Start);
                    originPoint = curve.Line.Evaluate(normalized, true);
                }

                switch (targetCurve)
                {
                    case Line line:
                        normal = (line.GetEndPoint(1) - line.GetEndPoint(0)).Normalize();
                        break;
                    case Arc arc:
                        XYZ centerPoint = arc.Center;
                        XYZ radiusDirection = originPoint - centerPoint;
                        XYZ normalPlane = arc.Normal;
                        normal = radiusDirection.CrossProduct(normalPlane);
                        break;
                }
            }

            Plane plane = Plane.CreateByNormalAndOrigin(normal, originPoint);

            return plane;
        }

        public double GetLength()
        {
            double length = Curves.Select(c => c.Length).Sum();

            return length;
        }

        private static bool IsNextCurve(Curve curve1, Curve curve2)
        {

            XYZ start1 = curve1.GetEndPoint(0);
            XYZ finish1 = curve1.GetEndPoint(1);

            XYZ start2 = curve2.GetEndPoint(0);
            XYZ finish2 = curve2.GetEndPoint(1);

            if (start1.IsAlmostEqualTo(start2)
                || start1.IsAlmostEqualTo(finish2)
                || finish1.IsAlmostEqualTo(start2)
                || finish1.IsAlmostEqualTo(finish2))
            {
                return true;
            }
            return false;
        }

        public XYZ GetProjectPoint(XYZ point)
        {
            double minDistanse = double.PositiveInfinity;
            XYZ projectPoint = null;
            foreach(var curve in ParametricCurves)
            {
                IntersectionResult interResult = new IntersectionResult();
                interResult =  curve.Line.Project(point);
                if(interResult.Distance < minDistanse)
                {
                    projectPoint = interResult.XYZPoint;
                }
            }

            return projectPoint;
        }

        private static bool IsNeedReverseCurves(Curve curve1, Curve curve2)
        {
            XYZ finishPointCurve1 = curve1.GetEndPoint(1);
            XYZ startPointCurve2 = curve2.GetEndPoint(0);

            if (finishPointCurve1.IsAlmostEqualTo(startPointCurve2))
            {
                return false;
            }

            return true;
        }

        private static Curve GetFirstCurve(IEnumerable<Curve> curves, out Curve unreverseCurve)
        {
            unreverseCurve = null;
            bool isNeedReverse = false;
            foreach (var curve in curves)
            {
                XYZ startPoint = curve.GetEndPoint(0);
                XYZ finishPoint = curve.GetEndPoint(1);
                int jointPoints = 0;

                foreach (var currCurve in curves)
                {
                    if (curve != currCurve)
                    {
                        XYZ startPointCurr = currCurve.GetEndPoint(0);
                        XYZ finishPointCurr = currCurve.GetEndPoint(1);

                        if (startPoint.IsAlmostEqualTo(startPointCurr)
                            || startPoint.IsAlmostEqualTo(finishPointCurr)
                            || finishPoint.IsAlmostEqualTo(startPointCurr)
                            || finishPoint.IsAlmostEqualTo(finishPointCurr))
                        {
                            jointPoints++;
                        }
                        if (startPoint.IsAlmostEqualTo(startPointCurr) || startPoint.IsAlmostEqualTo(finishPointCurr))
                        {
                            isNeedReverse = true;
                        }
                    }
                }
                if (jointPoints == 1)
                {
                    unreverseCurve = curve;
                    if (isNeedReverse)
                    {
                        return curve.CreateReversed();
                    }

                    return curve;
                }
            }

            return null;
        }
    }
}