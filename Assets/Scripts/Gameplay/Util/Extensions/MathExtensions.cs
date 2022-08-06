using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Gameplay;
using Gameplay.Conrollers;
using Gameplay.Core;
using Gameplay.World;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace Util
{

    /// <summary>
    /// Math and Vector extension methods
    /// </summary>
    public static class MathExtensions
    {
        /// <summary>
        /// Rotate 2d vector by angle
        /// </summary>
        public static Vector2 Rotate(this Vector2 v, float angle) {
            return new Vector2(
                v.x * Mathf.Cos(angle) - v.y * Mathf.Sin(angle),
                v.x * Mathf.Sin(angle) + v.y * Mathf.Cos(angle)
            );
        }
        
        /// <summary>
        /// Clamp vector in horizontal axis (XZ)
        /// </summary>
        public static Vector3 ClampHorizontal(this Vector3 vector, float maxMagnitude)
        {
            float y = vector.y;
            vector.y = 0;
            if (vector.sqrMagnitude > maxMagnitude * maxMagnitude) vector = vector.normalized * maxMagnitude;

            vector.y = y;
            return vector;
        }

        /// <summary>
        /// Convert to Vector2 (XY)
        /// </summary>
        public static Vector2 ToVector2(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.y);
        }

        /// <summary>
        /// Convert to Vector3 (XY)
        /// </summary>
        public static Vector3 ToVector3(this Vector2 vector)
        {
            return new Vector3(vector.x, vector.y,0);
        }

        /// <summary>
        /// Convert to Vector2 (XY)
        /// </summary>
        public static Vector2 ToVector2(this Vector3Int vector)
        {
            return new Vector2(vector.x, vector.y);
        }
        
        /// <summary>
        /// Convert to Vector2 (XY)
        /// </summary>
        public static Vector3 ToVector3(this Vector2Int vector)
        {
            return new Vector3(vector.x,vector.y, 0);
        }
        
        public static Vector2Int ToVector2Int(this Vector3 vector)
        {
            return new Vector2Int(Mathf.RoundToInt(vector.x),Mathf.RoundToInt(vector.y));
        }
        
        public static Vector3Int ToVector3Int(this Vector3 vector)
        {
            return new Vector3Int(Mathf.RoundToInt(vector.x),Mathf.RoundToInt(vector.y),Mathf.RoundToInt(vector.z));
        }

        /// <summary>
        /// Change this Vector3 by setting it's Y axis value
        /// </summary>
        public static Vector3 WithY(this Vector3 vector, float y)
        {
            return new Vector3(vector.x, y, vector.z);
        }
        
        /// <summary>
        /// Abs vector values per axis
        /// </summary>
        public static Vector2 Abs(this Vector2 vector)
        {
            return new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
        }

        /// <summary>
        /// Are all values of this vector less than another
        /// </summary>
        public static bool Less(this Vector3 vec1, Vector3 vec2)
        {
            return vec1.x <= vec2.x && vec1.y <= vec2.y && vec1.z <= vec2.z;
        }

        /// <summary>
        /// Are all values of this vector greater than another
        /// </summary>
        public static bool Greater(this Vector3 vec1, Vector3 vec2)
        {
            return vec1.x >= vec2.x && vec1.y >= vec2.y && vec1.z >= vec2.z;
        }
        
        /// <summary>
        /// Are all values of this vector less than another
        /// </summary>
        public static bool Less(this Vector2 vec1, Vector2 vec2)
        {
            return vec1.x <= vec2.x && vec1.y <= vec2.y;
        }

        /// <summary>
        /// Are all values of this vector greater than another
        /// </summary>
        public static bool Greater(this Vector2 vec1, Vector2 vec2)
        {
            return vec1.x >= vec2.x && vec1.y >= vec2.y;
        }

        /// <summary>
        /// Get normal on plane for 2d vector
        /// </summary>
        public static Vector2 GetNormal(this Vector2 vector)
        {
            return new Vector2(vector.y, -vector.x);
        }

        /// <summary>
        /// Check if two values are approximately equal
        /// </summary>
        public static bool Approximately(float a, float b, float tolerance = 1e-5f)
        {
            return Mathf.Abs(a - b) <= tolerance;
        }

        /// <summary>
        /// 2D cross product on a plane
        /// </summary>
        public static float CrossProduct2D(Vector2 a, Vector2 b)
        {
            return a.x * b.y - b.x * a.y;
        }

        /// <summary>
        /// Check if two lines defined by end and start are parallel
        /// </summary>
        public static bool AreParralel(Vector2 p1start, Vector2 p1end, Vector2 p2start, Vector2 p2end)
        {
            Vector2 r = p1end - p1start;
            Vector2 s = p2end - p2start;

            float cross_rs = CrossProduct2D(r.normalized, s.normalized);

            return Approximately(cross_rs, 0f, 0.05f);
        }

        /// <summary>
        /// Project vector onto a line defined by end and start points
        /// </summary>
        /// <returns>value, in which 0 corresponds to point being at a, and 1 corresponds to b.</returns>
        public static float GetProjectionT(Vector2 a, Vector2 b, Vector2 point)
        {
            Vector2 d = b - a;
            float tu = (point.x - a.x) * (b.x - a.x) + (point.y - a.y) * (b.y - a.y);

            return tu / d.sqrMagnitude;
        }


        /// <summary>
        /// Find intersection of 2 2D lines, defined by end and start points
        /// </summary>
        /// <param name="p1start">Start point of the first line</param>
        /// <param name="p1end">End point of the first line</param>
        /// <param name="p2start">Start point of the second line</param>
        /// <param name="p2end">End point of the second line</param>
        /// <returns>Intersection point</returns>
        public static Vector2 IntersectLineSegments(Vector2 p1start, Vector2 p1end, Vector2 p2start, Vector2 p2end)
        {
            Vector2 r = p1end - p1start;
            Vector2 s = p2end - p2start;

            float cross_rs = CrossProduct2D(r, s);
            Vector2 qminusp = p2start - p1start;

            float t = CrossProduct2D(qminusp, s) / cross_rs;
            return p1start + t * r;
        }

        /// <summary>
        /// Apply function to all components of vector
        /// </summary>
        public static Vector2 Apply(this Vector2 vec, Func<float, float> func)
        {
            return new Vector2(func(vec.x), func(vec.y));
        }
        
        /// <summary>
        /// Apply function to all components of vector
        /// </summary>
        public static Vector3 Apply(this Vector3 vec, Func<float, float> func)
        {
            return new Vector3(func(vec.x), func(vec.y), func(vec.z));
        }
        
        /// <summary>
        /// Apply function to all components of vector
        /// </summary>
        public static Vector2Int Apply(this Vector2 vec, Func<float, int> func)
        {
            return new Vector2Int(func(vec.x), func(vec.y));
        }
        
        /// <summary>
        /// Apply function to all components of vector
        /// </summary>
        public static Vector2Int Apply(this Vector2Int vec, Func<int, int> func)
        {
            return new Vector2Int(func(vec.x), func(vec.y));
        }

        /// <summary>
        /// Get center of given points
        /// </summary>
        public static Vector2 GetCenter(this IList<Vector2> points)
        {
            return points.Aggregate((a, b) => a + b) / points.Count;
        }
        
        /// <summary>
        /// Get center of given points
        /// </summary>
        public static Vector2 GetCenter(this IEnumerable<Vector2> points, int count)
        {
            return points.Aggregate((a, b) => a + b) / count;
        }

        /// <summary>
        /// Check if vector values have infinity
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static bool IsInfinity(this Vector2 vector)
        {
            return float.IsInfinity(vector.x) || float.IsInfinity(vector.y) ||
                   float.IsNaN(vector.x) || float.IsNaN(vector.y);
        }
        
        /// <summary>
        /// Mod operation that correctly works with negative numbers
        /// </summary>
        public static int Mod(this int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }
        
        /// <summary>
        /// Randomly shuffle a list, such that no element can stay in it's original index
        /// </summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            HashSet<int> blacklist = new HashSet<int>();
            
            int currentIndex = 0;
            int newIndex = -1;
            
            T currentElement = list[0];
            blacklist.Add(0);

            while (newIndex != 0)
            {
                try
                {
                    // ReSharper disable once AccessToModifiedClosure
                    newIndex = RandomUtils.ConstrainedRandom(num => num != currentIndex && !blacklist.Contains(num), 0, list.Count);
                }
                catch (ArgumentException)
                {
                    newIndex = 0;
                }
                
                blacklist.Add(newIndex);
                (list[newIndex], currentElement) = (currentElement, list[newIndex]);
                currentIndex = newIndex;
            }
        }
        
        
        /// <summary>
        /// Determines if the given point is inside the polygon
        /// </summary>
        /// <param name="polygon">the vertices of polygon</param>
        /// <param name="testPoint">the given point</param>
        /// <returns>true if the point is inside the polygon; otherwise, false</returns>
        public static bool IsPointInPolygon4(this List<Vector2> polygon, Vector2 testPoint)
        {
            bool result = false;
            int j = polygon.Count - 1;
            for (int i = 0; i < polygon.Count; i++)
            {
                if (polygon[i].y < testPoint.y && polygon[j].y >= testPoint.y || polygon[j].y < testPoint.y && polygon[i].y >= testPoint.y)
                {
                    if (polygon[i].x + (testPoint.y - polygon[i].y) / (polygon[j].y - polygon[i].y) * (polygon[j].x - polygon[i].x) < testPoint.x)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }
        
        /// <summary>
        /// Check list of points winding order
        /// </summary>
        public static bool IsClockwise(this List<Vector2> vertices)
        {
            double sum = 0.0;
            Vector2 v1 = vertices[vertices.Count - 1];
            
            foreach (Vector2 v2 in vertices)
            {
                sum += (v2.x - v1.x) * (v2.y + v1.y);
                v1 = v2;
            }
            return sum > 0.0;
        }
        
        /// <summary>
        /// Extends/shrinks the rect by extendDistance to each side and then restricts the given vector to the resulting rect.
        /// </summary>
        /// <param name="rect">The Rect.</param>
        /// <param name="position">A position that should be restricted to the rect.</param>
        /// <param name="extendDistance">The distance to extend/shrink the rect to each side.</param>
        /// <returns>The vector, clamped to the Rect.</returns>
        public static Vector2 Clamp2(this Rect rect, Vector2 position, float extendDistance = 0f)
        {
            return new Vector2(Mathf.Clamp(position.x, rect.xMin - extendDistance, rect.xMax + extendDistance),
                Mathf.Clamp(position.y, rect.yMin - extendDistance, rect.yMax + extendDistance));
        }
        
        /// <summary>
        /// Extends/shrinks the rect by extendDistance to each side and then restricts the given vector to the resulting rect.
        /// The z component is kept.
        /// </summary>
        /// <param name="rect">The Rect.</param>
        /// <param name="position">A position that should be restricted to the rect.</param>
        /// <param name="extendDistance">The distance to extend/shrink the rect to each side.</param>
        /// <returns>The vector, clamped to the Rect.</returns>
        public static Vector3 Clamp3(this Rect rect, Vector3 position, float extendDistance = 0f)
        {
            return new Vector3(Mathf.Clamp(position.x, rect.xMin - extendDistance, rect.xMax + extendDistance),
                Mathf.Clamp(position.y, rect.yMin - extendDistance, rect.yMax + extendDistance),
                position.z);
        }

        /// <summary>
        /// Calculate running exponential average
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public  static float Average(this float current, float newValue, int total)
        {
            return current * (total - 1) / total + newValue / total;
        }

        /// <summary>
        /// Combine two running exponential average values.
        /// </summary>
        /// <param name="current">current average</param>
        /// <param name="other">total count</param>
        /// <param name="currentCount">other average</param>
        /// <param name="otherCount">other count</param>
        /// <returns>average value</returns>
        public static float Average(this float current, float other, int currentCount, int otherCount)
        {
            return current * currentCount / (currentCount + otherCount) + other * otherCount / (currentCount + otherCount);
        }

        public static void Draw(this Rect rect, Color color)
        {
            Debug.DrawLine(new Vector3(rect.x, rect.y), new Vector3(rect.x + rect.width, rect.y ),color);
            Debug.DrawLine(new Vector3(rect.x, rect.y), new Vector3(rect.x , rect.y + rect.height), color);
            Debug.DrawLine(new Vector3(rect.x + rect.width, rect.y + rect.height), new Vector3(rect.x + rect.width, rect.y), color);
            Debug.DrawLine(new Vector3(rect.x + rect.width, rect.y + rect.height), new Vector3(rect.x, rect.y + rect.height), color);
        }

        
        public static Vector2 ToWorldPos(this Vector2Int gridPos)
        {
            return gridPos * 2 + Vector2Int.one;
        }

        public static Vector2Int ToGridPos(this Vector3 worldPos)
        {
            return ((worldPos - Vector3.one) / 2).ToVector2Int();
        }

        /// <summary>
        /// Rounds a Vector3 to world axis or relative to another transform
        /// </summary>
        /// <param name="vector">Vector to round</param>
        /// <param name="relativeTo">Optional relative transform space axis</param>
        /// <returns></returns>
        public static Vector3 AxisRound(this Vector3 vector)
        {
            int largestIndex = 0;
            for (int i = 1; i < 3; i++)
            {
                largestIndex = Mathf.Abs(vector[i]) > Mathf.Abs(vector[largestIndex]) ? i : largestIndex;
            }
            float newLargest = vector[largestIndex] > 0 ? 1 : -1;
            vector = Vector3.zero;
            vector[largestIndex] = newLargest;

            return vector;
        }
        
        /// <summary>
        /// Calculate camera orthographic screen size in world space
        /// </summary>
        public static Vector2 OrthographicSize(this Camera camera, PixelPerfectCamera perfectCamera)
        {
            float sizeCorrection = perfectCamera.refResolutionX * perfectCamera.pixelRatio / (float)Screen.width;
            
            float screenAspect = Screen.width / (float)Screen.height;
            float cameraHeight = camera.orthographicSize * 2;
            return new Vector2(cameraHeight * screenAspect * sizeCorrection, cameraHeight * sizeCorrection);
        }
    }
}