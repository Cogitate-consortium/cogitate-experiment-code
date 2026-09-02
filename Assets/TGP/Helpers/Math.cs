// NS_ABSORB
using Helpers.Engine;

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TGP
{
    namespace Helpers
    {
        public static class Math_Helper
        {
            #region Probabilities
            public static float QueryCDF(Dictionary<float, float> CDF)
            {
                float r = Utility_Helper.RandomRange(0f, 1f);
                return QueryCDF(CDF, r);
            }

            public static float QueryCDF(Dictionary<float, float> CDF, float r)
            {
                float v = 0;
                if (QueryCDF(CDF, r, out v))
                    return v;
                return 0;
            }

            public static bool QueryCDF(Dictionary<float, float> CDF, float r, out float v)
            {
                v = 0;

                float maxV = 0;

                foreach (KeyValuePair<float, float> kVP in CDF)
                    if (kVP.Value >= r)
                    {
                        // [TODO] It's between this and the next value, but we're good enough for now (lerp later)

                        v = kVP.Key;
                        return true;
                    }
                    else
                        maxV = Mathf.Max(v, maxV);

                v = maxV;

                Debug_Helper.LogWarning(typeof(Math_Helper), "This shouldn't have happened, but we handled it - r: {0}\n{1}"._Format(r, CDF.ToReadableString()));

                return true;
            }

            public static float SumOfProducts(this Dictionary<float, float> dictionary)
            {
                float sum = 0;

                foreach(KeyValuePair<float, float> kVP in dictionary)
                {
                    float product = kVP.Key * kVP.Value;
                    sum += product;
                }

                return sum;
            }

            public static Dictionary<float, float> TruncExp_GetCumProb(TruncatedExpConfig config)
            {
                return TruncExp_GetCDF(config.λ, config.min, config.max, config.numBins);
            }

            public static Dictionary<float, float> TruncExp_GetCDF(float λ, float min, float max, int numBins)
            {
                Dictionary<float, float> CDF = new Dictionary<float, float>();

                float dX = (max - min) / numBins;

                float e_λmin = Mathf.Exp(-λ * min);
                float e_λmax = Mathf.Exp(-λ * max);

                CDF.Add(min, 0); // No probability under min

                for (int i = 0; i < numBins - 1; i++)
                {
                    float x_mi = min + i * dX + dX / 2;

                    float e_λx = Mathf.Exp(-λ * x_mi);

                    float c_xA = (e_λmin - e_λx) / (e_λmin - e_λmax);

                    /*
                    // Failsafes to account for numerical errors
                    float x_lo = min + i * dX;
                    float x_hi = min + (i + 1) * dX;

                    float e_λx_lo = Mathf.Exp(-λ * x_lo);
                    float e_λx_hi = Mathf.Exp(-λ * x_hi);

                    float c_lo = (e_λmin - e_λx_lo) / (e_λmin - e_λmax);
                    float c_hi = (e_λmin - e_λx_hi) / (e_λmin - e_λmax);

                    // float x = Mathf.Lerp(x_lo, x_hi, 0.5f);
                    float c_xB = Mathf.Lerp(c_lo, c_hi, 0.5f);

                    // Double-bake!
                    float c_x = Mathf.Lerp(c_xA, c_xB, 0.5f);
                    */

                    CDF.Add(x_mi, c_xA);
                }

                CDF.Add(max, 1); // All probability under max

                return CDF;
            }

            private static float TruncExp_GetPDF(float x, float θ, float b)
            {
                if (x <= 0 || x > b) return 0;
                
                float pdx = θ * Mathf.Exp(-θ * x) / (1 - Mathf.Exp(-θ * b));

                pdx = Mathf.Clamp01(pdx);

                return pdx;
            }

            public static float TruncExp_GetExpectedMean(float θ, float b, float min = 0)
            {
                return 1 / θ - b / (Mathf.Exp(θ * b) - 1) + min;
            }
            #endregion


            #region Random
            public static Vector2 GetRandomVector2(float min = Mathf.NegativeInfinity, float max = Mathf.Infinity)
            {
                return new Vector2(
                    Utility_Helper.RandomRange(min, max),
                    Utility_Helper.RandomRange(min, max));
            }

            public static Vector3 GetRandomVector3(float min = Mathf.NegativeInfinity, float max = Mathf.Infinity)
            {
                return new Vector3(
                    Utility_Helper.RandomRange(min, max),
                    Utility_Helper.RandomRange(min, max),
                    Utility_Helper.RandomRange(min, max));
            }

            public static Vector3 GetRandomVector3BothSigns(float min = Mathf.NegativeInfinity, float max = Mathf.Infinity)
            {
                return new Vector3(
                    RandomRangeBothSigns(min, max),
                    RandomRangeBothSigns(min, max),
                    RandomRangeBothSigns(min, max));
            }

            public static Vector2 GetRandomVector2BothSigns(float min = Mathf.NegativeInfinity, float max = Mathf.Infinity)
            {
                return new Vector2(
                    RandomRangeBothSigns(min, max),
                    RandomRangeBothSigns(min, max));
            }

            public static float RandomBetween(this Vector2 v2)
            {
                return Utility_Helper.RandomRange(v2.x, v2.y);
            }

            public static float RandomRangeBothSigns(float from, float to)
            {
                float sgn = Mathf.Sign(Utility_Helper.RandomRange(-1f, 1f));
                if (sgn == 0)
                    sgn = 1;

                return Utility_Helper.RandomRange(from, to) * sgn;
            }

            /// <summary>
            /// Make sure to pass in mean (μ) and variance (σ^2); not standard deviation (σ);
            /// </summary>
            /// <param name="mean"></param>
            /// <param name="variance"></param>
            /// <returns></returns>
            public static float RandomGaussianValue(float mean, float variance)
            {
                double stdDev = Mathf.Sqrt(variance);

                System.Random rand = new System.Random(); //reuse this if you are generating many
                double u1 = rand.NextDouble(); //these are uniform(0,1) random doubles
                double u2 = rand.NextDouble();
                double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                             Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                double randNormal =
                             mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
#if UNITY_EDITOR
                // this.Log(mean.ToString("#.00") + " -> " + randNormal.ToString("#.00"));
#endif
                return (float)randNormal;
            }

            public static int GetNumDigits(this int number)
            {
                return Mathf.FloorToInt(Mathf.Log10(Mathf.Max(1, number))) + 1;
            }

            #endregion


            #region Trigonometry
            public static float TwoPiT { get { return 2 * Mathf.PI * TimeWrapper.time_NotTS; } }

            public static Vector3 Abs(this Vector3 v3)
            {
                return new Vector3(Mathf.Abs(v3.x), Mathf.Abs(v3.y), Mathf.Abs(v3.z));
            }

            public static Vector3 Cos(this Vector3 v3)
            {
                return new Vector3(Mathf.Cos(v3.x), Mathf.Cos(v3.y), Mathf.Cos(v3.z));
            }

            public static Vector3 Sin(this Vector3 v3)
            {
                return new Vector3(Mathf.Sin(v3.x), Mathf.Sin(v3.y), Mathf.Sin(v3.z));
            }

            public static Vector2 Cos(this Vector2 v2)
            {
                return new Vector2(Mathf.Cos(v2.x), Mathf.Cos(v2.y));
            }

            public static Vector2 Sin(this Vector2 v2)
            {
                return new Vector2(Mathf.Sin(v2.x), Mathf.Sin(v2.y));
            }

            /// <summary>
            /// Signed angle from one V3 to another, assuming up direction
            /// </summary>
            public static float AngleSigned(Vector3 from, Vector3 to, Vector3 up)
            {
                if (from.magnitude == 0 || to.magnitude == 0) return 0;
                return Mathf.Atan2(
                    Vector3.Dot(up, Vector3.Cross(from, to)),
                    Vector3.Dot(from, to)) * Mathf.Rad2Deg;
            }

            public static float AngleSigned(Vector2 from, Vector2 to, bool clockwise = true)
            {
                return AngleSigned(from, to, clockwise ? Vector3.back : Vector3.forward);
            }
            #endregion


            #region 3D Math
            public static Vector3 localRight(this Transform t)
            {
                return t.localRotation * Vector3.right;
            }
            public static Vector3 localUp(this Transform t)
            {
                return t.localRotation * Vector3.up;
            }
            public static Vector3 localForward(this Transform t)
            {
                return t.localRotation * Vector3.forward;
            }

            public static float GetRelativeDistance(this Vector3 a, Vector3 b)
            {
                return Vector3.Distance(a, b) / a.magnitude;
            }

            public static float GetDistanceToLine(this Vector2 point, Vector2 lineA, Vector2 lineB)
            {
                return GetDistanceToLine(point.x, point.y, lineA.x, lineA.y, lineB.x, lineB.y);
            }

            // https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line#Line_defined_by_two_points
            public static float GetDistanceToLine(float x0, float y0, float x1, float y1, float x2, float y2)
            {
                float denominator = Mathf.Abs((y2 - y1) * x0 - (x2 - x1) * y0 + x2 * y1 - y2 * x1);
                float numerator = Mathf.Sqrt(Mathf.Pow(y2 - y1, 2) + Mathf.Pow(x2 - x1, 2));
                return denominator / numerator;
            }

            /// <summary>
            /// Picks a random point inside a CONVEX mesh.
            /// Taking advantage of Convexity, we can produce more evenly distributed points
            /// </summary> 
            public static Vector3 GetRandomPointInsideConvex(this Mesh m)
            {
                // Grab two points on the surface
                Vector3 randomPointOnSurfaceA = m.GetRandomPointOnSurface();
                Vector3 randomPointOnSurfaceB = m.GetRandomPointOnSurface();

                // Interpolate between them
                return Vector3.Lerp(randomPointOnSurfaceA, randomPointOnSurfaceB, Utility_Helper.RandomRange(0f, 1f));
            }

            /// <summary>
            /// Picks a random point inside a NON-CONVEX mesh.
            /// The only way to get good approximations is by providing a point (if there is one)
            /// that has line of sight to most other points in the non-convex shape.
            /// </summary> 
            public static Vector3 GetRandomPointInsideNonConvex(this Mesh m, Vector3 pointWhichSeesAll)
            {
                // Grab one point (and the center which we assume has line of sight with this point)
                Vector3 randomPointOnSurface = m.GetRandomPointOnSurface();

                // Interpolate between them
                return Vector3.Lerp(pointWhichSeesAll, randomPointOnSurface, Utility_Helper.RandomRange(0f, 1f));
            }

            /// <summary>
            /// Picks a random point on the mesh's surface.
            /// </summary> 
            public static Vector3 GetRandomPointOnSurface(this Mesh m, Transform debugTransform = null)
            {
                // Pick a random triangle (each triangle is 3 integers in a row in m.triangles)
                // So Pick a random origin (0, 3, 6, .. m.triangles.Length - 3)
                // -> Random (0.. m.triangles.Length / 3) * 3
                float randSeed = Utility_Helper.RandomRange(0f, Mathf.Max(m.triangles.Length, 100000000));
                int triangleOrigin = Mathf.FloorToInt(randSeed % m.triangles.Length / 3f) * 3;

                // Grab the 3 points that consist of the triangle
                Vector3 vertexA = m.vertices[m.triangles[triangleOrigin]];
                Vector3 vertexB = m.vertices[m.triangles[triangleOrigin + 1]];
                Vector3 vertexC = m.vertices[m.triangles[triangleOrigin + 2]];

                // Pick a random point on the triangle
                // For a uniform distribution, we pick randomly according to this:
                // http://mathworld.wolfram.com/TrianglePointPicking.html
                // From the point of origin (vertexA) move a random distance towards vertexB and from there a random distance in the direction of (vertexC - vertexB)
                // The only (temporary) downside is that we might end up with points outside our triangle as well, which have to be mapped back
                // The good thing is that these points can only end up in the triangle's "reflection" across the AC side (forming a quad AB, BC, CD, DA)

                Vector3 dAB = vertexB - vertexA;
                Vector3 dBC = vertexC - vertexB;

                float rAB = Utility_Helper.RandomRange(0f, 1f);
                float rBC = Utility_Helper.RandomRange(0f, 1f);

                Vector3 randPoint = vertexA + rAB * dAB + rBC * dBC;

                // We have produces random points on a quad (the extension of our triangle)
                // To map back to the triangle, first we check if we are on the extension of the triangle
                // Since we can be on one of two triangles this is equivalent with checking if we are on the correct side of the AC line
                // If we are on the correct side (towards B) we are on the triangle - else we are not.

                // To check that we can compare the direction of our point towards any point on that line (say, C)
                // with the direction of the height of side AC (Cross (triangleNormal, dirBC)))
                Vector3 dirPC = (vertexC - randPoint).normalized;

                Vector3 dirAB = (vertexB - vertexA).normalized;
                Vector3 dirAC = (vertexC - vertexA).normalized;

                Vector3 triangleNormal = Vector3.Cross(dirAC, dirAB).normalized;

                Vector3 dirH_AC = Vector3.Cross(triangleNormal, dirAC).normalized;

                // If the two are alligned, we're in the wrong side
                float dot = Vector3.Dot(dirPC, dirH_AC);

                // We are on the right side, we're done
                if (dot >= 0)
                {
                    // Otherwise, we need to find the symmetric to the center of the "quad" which is on the intersection of side AC with the bisecting line of angle (BA, BC)
                    // Given by
                    Vector3 centralPoint = (vertexA + vertexC) / 2;

                    // And the symmetric point is given by the equation c - p = p_Sym - c => p_Sym = 2c - p
                    Vector3 symmetricRandPoint = 2 * centralPoint - randPoint;

                    if (debugTransform)
                        UnityEngine.Debug.DrawLine(debugTransform.TransformPoint(randPoint), debugTransform.TransformPoint(symmetricRandPoint), Color.red, 10);
                    randPoint = symmetricRandPoint;
                }

                // For debugging purposes
                if (debugTransform)
                {
                    UnityEngine.Debug.DrawLine(debugTransform.TransformPoint(randPoint), debugTransform.TransformPoint(vertexA), Color.cyan, 10);
                    UnityEngine.Debug.DrawLine(debugTransform.TransformPoint(randPoint), debugTransform.TransformPoint(vertexB), Color.green, 10);
                    UnityEngine.Debug.DrawLine(debugTransform.TransformPoint(randPoint), debugTransform.TransformPoint(vertexC), Color.blue, 10);
                    // Debug.DrawRay(debugTransform.TransformPoint(randPoint), triangleNormal, Color.cyan, 10); 
                }

                return randPoint;
            }

            /// <summary>
            /// Returns the mesh's center.
            /// </summary> 
            public static Vector3 GetCenterPoint(this Mesh m)
            {
                Vector3 center = Vector3.zero;
                foreach (Vector3 v in m.vertices)
                    center += v;
                return center / m.vertexCount;
            }

            // Contained In Hull <-> Contained in any of the hull's triangles
            public static bool ConvexHullContains(this IList<Vector2> hull, Vector2 point)
            {
                // We need at least 3 points to form a hull
                if (hull == null || hull.Count < 3) return false;

                // Grab 3 points, check if we're in them
                for (int i = 0; i < hull.Count - 2; i++)
                    for (int j = i + 1; j < hull.Count - 1; j++)
                        for (int k = j + 1; k < hull.Count; k++)
                            if (point.IsContainedInTriangle(hull[i], hull[j], hull[k]))
                                return true;

                // Guess we're not
                return false;
            }

            public static bool IsContainedInTriangle(this Vector2 s, Vector2 a, Vector2 b, Vector2 c)
            {
                // http://stackoverflow.com/questions/2049582/how-to-determine-a-point-in-a-2d-triangle
                float as_x = s.x - a.x;
                float as_y = s.y - a.y;

                bool s_ab = (b.x - a.x) * as_y - (b.y - a.y) * as_x > 0;

                if ((c.x - a.x) * as_y - (c.y - a.y) * as_x > 0 == s_ab) return false;

                if ((c.x - b.x) * (s.y - b.y) - (c.y - b.y) * (s.x - b.x) > 0 != s_ab) return false;

                return true;
            }

            /// <summary>Computes the convex hull of a polygon, in clockwise order in a Y-up 
            /// coordinate system (counterclockwise in a Y-down coordinate system).</summary>
            /// <remarks>Uses the Monotone Chain algorithm, a.k.a. Andrew's Algorithm.</remarks>
            public static List<Vector2> ComputeConvexHull(this IList<Vector2> points)
            {
                var list = new List<Vector2>(points);
                return ComputeConvexHull(list, true);
            }

            public static List<Vector2> ComputeConvexHull(this List<Vector2> points, bool sortInPlace)
            {
                if (!sortInPlace)
                    points = new List<Vector2>(points);
                points.Sort((a, b) =>
                  a.x == b.x ? a.y.CompareTo(b.y) : (a.x > b.x ? 1 : -1));

                // Importantly, DList provides O(1) insertion at beginning and end
                List<Vector2> hull = new List<Vector2>();
                int L = 0, U = 0; // size of lower and upper hulls

                // Builds a hull such that the output polygon starts at the leftmost point.
                for (int i = points.Count - 1; i >= 0; i--)
                {
                    Vector2 p = points[i], p1;

                    // build lower hull (at end of output list)
                    while (L >= 2 && (p1 = hull.GetLast()).Sub(hull[hull.Count - 2]).Cross(p.Sub(p1)) >= 0)
                    {
                        hull.RemoveAt(hull.Count - 1);
                        L--;
                    }
                    hull.Add(p);
                    L++;

                    // build upper hull (at beginning of output list)
                    while (U >= 2 && (p1 = hull[0]).Sub(hull[1]).Cross(p.Sub(p1)) <= 0)
                    {
                        hull.RemoveAt(0);
                        U--;
                    }
                    if (U != 0) // when U=0, share the point added above
                        hull.Insert(0, p);
                    U++;
                    Debug.Assert(U + L == hull.Count + 1);
                }
                hull.RemoveAt(hull.Count - 1);
                return hull;
            }
            
            /// <summary>
            /// Returns false if our collider's bounding box intersects any of the colliders
            /// </summary> 
            public static bool IntersectsAny(this Collider c, IList<Collider> opposingColliders, bool debug = false)
            {
                foreach (Collider oC in opposingColliders)
                    if (c.bounds.Intersects(oC.bounds))
                    {
                        if (debug)
                            Debug_Helper.Log(typeof(Math_Helper), "{0} intersects {1}"._Format(c.name, oC.name));
                        return true;
                    }
                return false;
            }

            // TODO: NEEDS REWORK - DOESNT WORK WITH ROTATED OBJECTS
            /// <summary>
            /// Returns false if our collider's bounding box intersects any of the colliders
            /// </summary> 
            public static bool Intersects(this Collider c, Collider opposingCollider)
            {
                return c.bounds.Intersects(opposingCollider.bounds);
            }

            #endregion


            #region Retargetting
            public static Vector3 Retargeted(this Vector3 v3, Vector3 currentMinMax, Vector3 targetMinMax, bool clamp = true)
            {
                return new Vector3(v3.x.Retargeted(currentMinMax, targetMinMax, clamp), v3.y.Retargeted(currentMinMax, targetMinMax, clamp), v3.z.Retargeted(currentMinMax, targetMinMax, clamp));
            }

            public static Vector2 Retargeted(this Vector2 v2, Vector2 currentMinMax, Vector2 targetMinMax, bool clamp = true)
            {
                return new Vector2(v2.x.Retargeted(currentMinMax, targetMinMax, clamp), v2.y.Retargeted(currentMinMax, targetMinMax, clamp));
            }

            public static float Retargeted(this float x, float currentMin, float currentMax, float targetMin, float targetMax, bool clamp = true)
            {
                float temp = x;

                // Normalize
                temp -= currentMin;
                temp /= (currentMax - currentMin);

                // if (clamp)
                //    temp = Mathf.Clamp(temp, 0, 1);

                // Retarget
                temp *= (targetMax - targetMin);
                temp += targetMin;

                if (clamp)
                {
                    temp = (temp > targetMax) ? targetMax : temp;
                    temp = (temp < targetMin) ? targetMin : temp;
                }

                return temp;
            }

            public static float Retargeted(this float x, Vector2 currentMinMax, Vector2 targetMinMax, bool clamp = true)
            {
                return x.Retargeted(currentMinMax.x, currentMinMax.y, targetMinMax.x, targetMinMax.y, clamp);

            }
            
            public static Vector2 Clamped(this Vector2 v2, float min, float max)
            {
                float _min = Mathf.Min(min, max);
                float _max = Mathf.Max(min, max);
                return new Vector2(v2.x.Clamped(_min, _max), v2.y.Clamped(_min, _max));
            }

            public static float Clamped01(this float x)
            {
                return x.Clamped(0, 1);
            }

            public static float Clamped(this float x, float min, float max)
            {
                return Mathf.Clamp(x, min, max);
            }

            public static float Clamped(this float x, Vector2 minMax)
            {
                return Mathf.Clamp(x, minMax.x, minMax.y);
            }

            public static float RetargetedTo_01(this float x, float currentMin, float currentMax, bool clamp = true)
            {
                return x.Retargeted(currentMin, currentMax, 0, 1, clamp);
            }

            public static float RetargetedTo_01(this float x, Vector2 currentMinMax, bool clamp = true)
            {
                return x.RetargetedTo_01(currentMinMax.x, currentMinMax.y, clamp);
            }

            public static float RetargetedFrom_01(this float x, float targetMin, float targetMax, bool clamp = true)
            {
                return x.Retargeted(0, 1, targetMin, targetMax, clamp);
            }

            public static float RetargetedFrom_01(this float x, Vector2 targetMinMax, bool clamp = true)
            {
                return x.RetargetedFrom_01(targetMinMax.x, targetMinMax.y, clamp);
            }

            public static float RetargetedFrom0_1To05_2(this float x01)
            {
                x01 = Mathf.Clamp01(x01);
                if (x01 <= 0.5f)
                    return x01 + 0.5f;
                else
                    return x01 * 2;
            }

            public static float RetargetedFrom05_2To0_1(this float x05_2)
            {
                x05_2 = Mathf.Clamp(x05_2, 0.5f, 2f);
                if (x05_2 <= 1)
                    return x05_2 - 0.5f;
                else
                    return x05_2 / 2;
            }

            public static Vector2 Retargeted(this Vector2 v2, Vector2 currentMinMax, Vector2 targetMinMax)
            {

                return new Vector2(v2.x.Retargeted(currentMinMax, targetMinMax), v2.y.Retargeted(currentMinMax, targetMinMax));
            }

            public static float Retargeted(this float x, Vector2 currentMinMax, Vector2 targetMinMax)
            {

                float temp = x;

                // Normalize
                temp = temp.Normalized(currentMinMax);

                // Retarget
                temp *= (targetMinMax.y - targetMinMax.x);
                temp += targetMinMax.x;

                return temp;

            }

            public static Vector3 GetShiftedAndScaled(this Vector3 _point, Vector3 center, Vector3 scale)
            {
                Vector3 point = _point;
                point -= center;

                point.x /= (scale.x);
                point.y /= (scale.y);

                return point;
            }

            public static float Normalized(this float x, Vector2 currentMinMax)
            {
                float temp = x;

                // Normalize
                temp -= currentMinMax.x;
                temp /= (currentMinMax.y - currentMinMax.x);

                return temp;
            }

            public static Vector2 NormalizeInRect(this Vector2 point, RectTransform rect, float innerRadius, float outterRadius)
            {
                Vector2 _pos = point.NormalizeInRect(rect);

                Vector2 pos = _pos.normalized * Mathf.Clamp(_pos.magnitude - innerRadius, 0, outterRadius - innerRadius);

                pos /= (outterRadius - innerRadius);

                return pos;
            }

            public static Vector2 NormalizeInRect(this Vector2 point, RectTransform rect)
            {
                Vector2 scale = new Vector2(
                    rect.rect.width * rect.transform.lossyScale.x,
                    rect.rect.height * rect.transform.lossyScale.y);

                Vector2 center = rect.transform.position;

                Vector2 localPoint = point - center;

                Vector2 normalizedLocalPoint = new Vector2(
                    localPoint.x / (scale.x / 2), localPoint.y / (scale.y / 2));

                return normalizedLocalPoint;
            }

            /// <summary>
            /// Smooths the first, the latter or both halfs of a 01 clamped value.
            /// </summary>
            /// <param name="percentile"></param>
            /// <param name="smoothType"></param>
            public static float Smooth01(this float percentile, SmoothType smoothType)
            {
                float value = 0;

                float sign = Mathf.Sign(percentile);

                percentile *= sign;

                percentile = Mathf.Clamp01(percentile);

                if (smoothType == SmoothType.None)
                    value = percentile;
                else if (percentile <= 0.5f
                    && (smoothType == SmoothType.EaseIn || smoothType == SmoothType.EaseInOut))
                    value = Mathf.Sin(percentile * Mathf.PI - Mathf.PI / 2) / 2 + 0.5f;
                else if (percentile >= 0.5f
                    && (smoothType == SmoothType.EaseOut || smoothType == SmoothType.EaseInOut))
                    value = Mathf.Sin(percentile * Mathf.PI / 2) / 2 + 0.5f;
                else if (smoothType == SmoothType.Exp)
                    value = (Mathf.Exp(percentile) - Mathf.Exp(0)) / Mathf.Exp(1);
                else if (smoothType == SmoothType.Sqrt)
                    // value = Mathf.Log(percentile + 1) / Mathf.Log(2);
                    value = Mathf.Sqrt(percentile);
                else
                    value = percentile;

                return sign * value;
            }

            /// <summary>
            /// Smooths an unbounded value
            /// </summary>
            /// <param name="init">In π</param>
            public static float SmoothMin2Max(this float t, SmoothType sType, float duration, float min, float max, float init = 0)
            {
                if (sType == SmoothType.EaseIn)
                    return min + (max - min) * (1 + Mathf.Cos(t * Mathf.PI / duration - Mathf.PI - init)) / 2;
                else if (sType == SmoothType.EaseOut)
                    return min + (max - min) * (1 + Mathf.Sin(t * Mathf.PI / duration - Mathf.PI / 2 - init)) / 2;
                else
                    return t;
            }

            /// <summary>
            /// Logarithmically smooths outliers - start with default parameters and tune if necessary
            /// </summary>
            /// <param name="outlierTolerance">High Limit -> Mean * (1 + tolerance) || Low Limit -> Mean / (1 + tolerance) </param>
            /// <param name="outlierReductor">Log base for reduction</param>
            public static Vector3 SmoothOutlier(this Vector3 currentMean, Vector3 possibleOutlier, float outlierTolerance = 1, float outlierReductor = 10)
            {
                possibleOutlier.x = currentMean.x.SmoothOutlier(possibleOutlier.x, outlierTolerance, outlierReductor);
                possibleOutlier.y = currentMean.y.SmoothOutlier(possibleOutlier.y, outlierTolerance, outlierReductor);
                possibleOutlier.z = currentMean.z.SmoothOutlier(possibleOutlier.z, outlierTolerance, outlierReductor);

                return possibleOutlier;
            }

            /// <summary>
            /// Logarithmically smooths outliers - start with default parameters and tune if necessary
            /// </summary>
            /// <param name="outlierTolerance">High Limit -> Mean * (1 + tolerance) || Low Limit -> Mean / (1 + tolerance) </param>
            /// <param name="outlierReductor">Log base for reduction</param>
            public static Vector2 SmoothOutlier(this Vector2 currentMean, Vector2 possibleOutlier, float outlierTolerance = 1, float outlierReductor = 10)
            {
                possibleOutlier.x = currentMean.x.SmoothOutlier(possibleOutlier.x, outlierTolerance, outlierReductor);
                possibleOutlier.y = currentMean.y.SmoothOutlier(possibleOutlier.y, outlierTolerance, outlierReductor);

                return possibleOutlier;
            }

            /// <summary>
            /// Logarithmically smooths outliers - start with default parameters and tune if necessary
            /// </summary>
            /// <param name="outlierTolerance">High Limit -> Mean * (1 + tolerance) || Low Limit -> Mean / (1 + tolerance) </param>
            /// <param name="outlierReductor">Log base for reduction</param>
            public static float SmoothOutlier(this float currentMean, float possibleOutlier, float outlierTolerance = 1, float outlierReductor = 10)
            {
                if (currentMean == 0)
                {
                    // Debug.LogWarning("CurrentMean was 0, returned the original value");
                    return possibleOutlier;
                }

                float highOutlierTolerance = Mathf.Max(
                    currentMean * (1 + outlierTolerance), currentMean / (1 + outlierTolerance));
                float lowOutlierTolerance = Mathf.Min(
                    currentMean * (1 + outlierTolerance), currentMean / (1 + outlierTolerance));
                outlierReductor = Mathf.Max(2, outlierReductor);

                // Much larger
                if (possibleOutlier > highOutlierTolerance)
                    possibleOutlier = highOutlierTolerance +
                        Mathf.Log(1 + Mathf.Abs(possibleOutlier - lowOutlierTolerance), outlierReductor);

                // Much smaller
                else if (possibleOutlier < lowOutlierTolerance)
                    possibleOutlier = lowOutlierTolerance -
                        Mathf.Log(1 + Mathf.Abs(possibleOutlier - lowOutlierTolerance), outlierReductor);

                return possibleOutlier;
            }

            /// <summary>
            /// Returns the sigmoid value of x.
            /// </summary>
            /// <param name="x">Input</param>
            /// <param name="a">Steepness</param>
            /// <param name="b">Center</param>
            /// <returns></returns>
            public static float Sigmoid(this float x, float a = 1, float b = 0)
            {
                return 1 / (1 + Mathf.Exp(-a * (x - b)));
            }

            /// <summary>
            /// Sigmoid that produces the full spectrum of 0...1 values when given input in the range [0... 2*b]
            /// </summary>
            /// <param name="x"></param>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static float Sigmoid01(this float x, float a = 1, float b = 1)
            {
                return Mathf.Clamp01((x.Sigmoid(a, b) - 0.5f) / (-((0f).Sigmoid(a, b) - 0.5f)) * 0.5f + 0.5f);
            }

            // Typical sigmoid with parameter A
            public static float Sigmoid(this float x, float a = 1)
            {
                return 1 / (1 + Mathf.Exp(-a * x));

            }

            // Shifted and scaled sigmoid so that it returns a value between 0 and 1 (technically -1 and 1)
            public static float Sigmoid01(this float x, float a = 1)
            {
                return 2 * x.Sigmoid(a) - 1;

            }

            /// <summary>
            /// Sigmoid with properly calculated parameter A, so that at x = 1 it returns v.
            /// </summary>
            /// <param name="x">Input</param>
            /// <param name="v">Wanted value for input 1</param>
            /// <returns>The customized sigmoid, evaluated at x</returns>
            public static float Sigmoid01WithValueAt1(this float x, float v = 0.5f)
            {
                return x.Sigmoid01(CalculateSigmoid01A(v));

            }

            public static float CalculateSigmoid01A(this float wantedValueAt1)
            {

                return -Mathf.Log(2 / (Mathf.Clamp01(wantedValueAt1) + 1) - 1, Mathf.Exp(1));
            }
            #endregion


            #region Basics
            public static void BreakIntoShorter(int data, int max, out short codeMSB, out short codeLSB)
            {
                codeMSB = (short)Mathf.FloorToInt(data / (max + 1));
                codeLSB = (short)(data - codeMSB * (max + 1));
            }

            public static float Fold(this float f, float max)
            {
                f = f % max;
                if (f <= max / 2)
                    return f;
                else
                    return max - f;
            }

            public static float Lerp(this Vector2 v2, float interpolator, bool clamp = true)
            {
                if (clamp) interpolator = interpolator.Clamped01();
                return Mathf.Lerp(v2.x, v2.y, interpolator);
            }

            public static Quaternion MuteAxes(this Quaternion q, bool muteX, bool muteY, bool muteZ)
            {
                Vector3 euler = q.eulerAngles;

                Quaternion corrector = Quaternion.identity;

                if (muteX) corrector *= Quaternion.Euler(euler.x * Vector3.right);
                if (muteY) corrector *= Quaternion.Euler(euler.y * Vector3.up);
                if (muteZ) corrector *= Quaternion.Euler(euler.z * Vector3.forward);

                Quaternion r = q * Quaternion.Inverse(corrector);

                // Make sure we got the right one
                Log(q + "\n" + r);
                return r;
            }

            public static Quaternion Equivalent(this Quaternion q)
            {
                return new Quaternion(-q.x, -q.y, -q.z, -q.w);
            }

            public static Vector3 SignedPow(this Vector3 v3, float pow)
            {
                return new Vector3(v3.x.SignedPow(pow), v3.y.SignedPow(pow), v3.z.SignedPow(pow));
            }

            public static Vector2 SignedPow(this Vector2 v2, float pow)
            {
                return new Vector2(v2.x.SignedPow(pow), v2.y.SignedPow(pow));
            }

            public static float SignedPow(this float f, float pow)
            {
                return Mathf.Sign(f) * Mathf.Pow(Mathf.Abs(f), pow);
            }

            public static Vector3 Lerp(this Vector3 from, Vector3 to, Vector3 lerper)
            {
                return new Vector3(
                    Mathf.Lerp(from.x, to.x, lerper.x),
                    Mathf.Lerp(from.y, to.y, lerper.y),
                    Mathf.Lerp(from.z, to.z, lerper.z));
            }

            public static float NegMod(this float f, float m)
            {
                return (f % m + m) % m;
            }

            public static int NegMod(this int f, int m)
            {
                return (int)((float)f).NegMod(m);
            }

            public static float CircularDistance(this float a, float b, float m, MinOrMax minOrMax)
            {
                // Bring both a and b in the [0, m] 
                a = a.NegMod(m);
                b = b.NegMod(m);

                float dist1 = (b - a).NegMod(m);
                float dist2 = (a - b).NegMod(m);

                // Keep either the shortest or the longest of the two distances
                if (minOrMax == MinOrMax.Min)
                    return Mathf.Min(dist1, dist2);
                else

                    return Mathf.Max(dist1, dist2);
            }

            public static int CircularDistance(this int a, int b, int m, MinOrMax minOrMax)
            {
                return (int)((float)a).CircularDistance(b, m, minOrMax);
            }

            public static bool IsNaN(this Vector3 v3)
            {
                return v3.x.IsNaN() || v3.y.IsNaN() || v3.z.IsNaN();
            }

            public static bool IsNaN(this Vector2 v2)
            {
                return v2.x.IsNaN() || v2.y.IsNaN();
            }

            public static bool IsNaN(this float f)
            {
                return float.IsNaN(f);
            }

            public static bool IsBetween(this float v, Vector2 limits)
            {
                return v.IsBetween(limits.x, limits.y);
            }

            public static bool IsBetween(this double v, double from, double to)
            {
                return (v >= Math.Min(from, to) && v <= Math.Max(from, to));
            }

            public static bool IsBetween(this float v, float from, float to)
            {
                return (v >= Mathf.Min(from, to) && v <= Mathf.Max(from, to));
            }

            public static bool IsBetween(this int v, float from, float to)
            {
                return (v >= Mathf.Min(from, to) && v <= Mathf.Max(from, to));
            }

            public static bool IsInsideCircularRect(this Vector2 point, RectTransform rect)
            {
                return (point.NormalizeInRect(rect).magnitude <= 1);
            }

            public static Polar GetPolar(this Vector3 _point, Vector3 center, Vector3 scale)
            {

                Vector3 point = _point.GetShiftedAndScaled(center, scale);

                Polar polarCoords = Polar.zero;

                polarCoords.r = Mathf.Sqrt(Mathf.Pow(point.x, 2) + Mathf.Pow(point.y, 2));
                polarCoords.θ = Mathf.PI / 2 - Mathf.Atan2(point.y, point.x);

                // We are outside our system
                if (polarCoords.r > 1)
                    return Polar.zero;

                return polarCoords;
            }
            /// <summary>
            /// Round to closest 
            /// </summary>
            /// <param name="f"></param>
            /// <param name="snapValues"></param>
            /// <returns></returns>
            public static float Round(this float f, float snapValues)
            {
                if (snapValues == 0) return 0;
                return Mathf.Round(f / snapValues) * snapValues;
            }

            public static Vector3 XZ(this Vector3 v3)
            {
                if (v3.y.IsNaN() || v3.y == Mathf.Infinity || v3.y == Mathf.NegativeInfinity)
                    return new Vector3(v3.x, 0, v3.y);
                return v3 - v3.y * Vector3.up;
            }

            public static void SplitVector3GoodBad(this Vector3 v3, Vector3 relativeTo, out Vector3 good, out Vector3 bad)
            {
                float dot = Vector3.Dot(v3, relativeTo);
                good = (dot > 0) ? Vector3.Project(v3, relativeTo) : Vector3.zero;
                bad = v3 - good;
            }

            public static float GetSqrtOfSumOfSquares(this IList<float> iList)
            {
                float result = 0;
                for (int i = 0; i < iList.Count; i++)
                    result += Mathf.Pow(iList[i], 2);
                return Mathf.Sqrt(result);
            }

            public static bool LogicalAnd(this IList<bool> list)
            {
                bool and = true;
                foreach (bool b in list)
                    and = and && b;

                return and;
            }

            public static bool LogicalOr(this IList<bool> list)
            {
                bool or = false;
                foreach (bool b in list)
                    or = or || b;

                return or;
            }

            public static Vector3 GetNext(this Vector3 current, Vector3 wantedNext, Vector3 speed, bool dontOvershoot = true)
            {
                return new Vector3
                    (
                        current.x.GetNext(wantedNext.x, speed.x),
                        current.y.GetNext(wantedNext.y, speed.y),
                        current.z.GetNext(wantedNext.z, speed.z)
                    );
            }

            public static Vector2 GetNext(this Vector2 current, Vector2 wantedNext, Vector2 speed, bool dontOvershoot = true)
            {
                return new Vector2
                    (
                        current.x.GetNext(wantedNext.x, speed.x),
                        current.y.GetNext(wantedNext.y, speed.y)
                    );
            }

            public static float GetNext(this float current, float wantedNext, float speed, bool dontOvershoot = true)
            {
                float diff = wantedNext - current;
                float step = Mathf.Sign(diff) * Mathf.Abs(speed);
                if (Mathf.Sign(step) != Mathf.Sign(diff - step) && dontOvershoot)
                    step = diff;
                float nextX = current + step;
                return nextX;
            }

            public static float Min(this Vector3 v3)
            {
                return Mathf.Min(v3.x, v3.y, v3.z);
            }

            public static float Max(this Vector3 v3)
            {
                return Mathf.Max(v3.x, v3.y, v3.z);
            }

            public static float Min(this Vector2 v2)
            {
                return Mathf.Min(v2.x, v2.y);
            }

            public static float Max(this Vector2 v2)
            {
                return Mathf.Max(v2.x, v2.y);
            }
            
            public static Vector2 Sub(this Vector2 a, Vector2 b)
            {
                return a - b;
            }

            public static float Cross(this Vector2 a, Vector2 b)
            {
                return a.x * b.y - a.y * b.x;
            }
            
            public static Vector3 Sub(this Vector3 a, Vector3 b)
            {
                return a - b;
            }

            public static Vector3 DivideBy(this Vector3 vA, Vector3 vB)
            {
                return new Vector3(
                    vB.x == 0 ? float.NaN : vA.x / vB.x,
                    vB.y == 0 ? float.NaN : vA.y / vB.y,
                    vB.z == 0 ? float.NaN : vA.z / vB.z);
            }
            public static Vector2 DivideBy(this Vector2 vA, Vector2 vB)
            {
                return new Vector2(
                    vB.x == 0 ? float.NaN : vA.x / vB.x,
                    vB.y == 0 ? float.NaN : vA.y / vB.y);
            }

            public static Vector3 MultiplyBy(this Vector3 vA, Vector3 vB)
            {
                return new Vector3(vA.x * vB.x, vA.y * vB.y, vA.z * vB.z);
            }

            public static Vector2 MultiplyBy(this Vector2 vA, Vector2 vB)
            {
                return new Vector2(vA.x * vB.x, vA.y * vB.y);
            }
            #endregion


            private static void Log(object obj)
            {
                Debug_Helper.Log(typeof(Math_Helper), obj);
            }
        }

        [Serializable]
        public struct Vector2PreciseString
        {
            public Vector2 vector;
            public Vector2PreciseString(Vector2 vector)
            {
                this.vector = vector;
            }

            public static string ToString(Vector2 vector)
            {
                return vector.ToStringPrecise();
            }

            public override string ToString()
            {
                return ToString(vector);
            }
        }

        [System.Serializable]
        public struct MinMax
        {
            public float min;
            public float max;

            public MinMax(float min, float max)
            {
                this.min = min;
                this.max = max;
            }

            public float avg { get { return Mathf.Lerp(min, max, 0.5f); } }

            // https://math.stackexchange.com/questions/975541/what-are-the-formal-names-of-operands-and-results-for-basic-operations

            public static MinMax operator *(MinMax multiplicand, float multiplier)
            {
                MinMax product = new MinMax();

                product.min = multiplicand.min * multiplier;
                product.max = multiplicand.max * multiplier;

                return product;
            }

            public static MinMax operator /(MinMax divident, float divisor)
            {
                MinMax quotient = new MinMax();

                quotient.min = divident.min / divisor;
                quotient.max = divident.max / divisor;

                return quotient;
            }

            public static MinMax operator +(MinMax augend, float addend)
            {
                MinMax sum = new MinMax();

                sum.min = augend.min + addend;
                sum.max = augend.max + addend;

                return sum;
            }

            public static MinMax operator -(MinMax minuend, float subtrahend)
            {
                MinMax difference = new MinMax();

                difference.min = minuend.min - subtrahend;
                difference.max = minuend.max - subtrahend;

                return difference;
            }

            public float RetargetFrom01(float x01)
            {
                float temp = x01;

                // Retarget
                temp *= (max - min);
                temp += min;

                temp = (temp > max) ? max : temp;
                temp = (temp < min) ? min : temp;

                return temp;

                // This method had performance issues for large data sets (5x slower)
                //return x01.RetargetedFrom_01(min, max);
            }
        }

        public struct Polar
        {
            public static Polar zero { get { Polar polar; polar.r = 0; polar.θ = 0; return polar; } }
            public float r;
            public float θ;
            /// <summary>
            /// <see cref="θ"/> in degrees
            /// </summary>
            public float θ_Deg
            {
                get { return θ * Mathf.Rad2Deg; }
                set { θ = value * Mathf.Deg2Rad; }
            }

            public Vector2 GetCartessian()
            {
                Vector2 v2 = new Vector2();
                v2.x = r * Mathf.Cos(θ);
                v2.y = r * Mathf.Sin(θ);

                return v2;
            }
        }


        [System.Serializable]
        public struct TruncatedExpConfig
        {
            public float λ;
            public float min;
            public float max;
            public int numBins;

            public TruncatedExpConfig(float λ, float min, float max, int numBins)
            {
                this.λ = λ;
                this.min = min;
                this.max = max;
                this.numBins = numBins;
            }

            public MinMax GetMinMax()
            {
                return new MinMax(min, max);
            }
        }
    }
}
