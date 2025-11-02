using UnityEngine;
using System.Collections.Generic;

namespace SantasWorkshop.Utilities
{
    /// <summary>
    /// Common C# extension methods for Unity development.
    /// </summary>
    public static class Extensions
    {
        #region Transform Extensions

        /// <summary>
        /// Resets the transform's local position, rotation, and scale to default values.
        /// </summary>
        public static void ResetLocal(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Sets the X component of the transform's position.
        /// </summary>
        public static void SetPositionX(this Transform transform, float x)
        {
            Vector3 position = transform.position;
            position.x = x;
            transform.position = position;
        }

        /// <summary>
        /// Sets the Y component of the transform's position.
        /// </summary>
        public static void SetPositionY(this Transform transform, float y)
        {
            Vector3 position = transform.position;
            position.y = y;
            transform.position = position;
        }

        /// <summary>
        /// Sets the Z component of the transform's position.
        /// </summary>
        public static void SetPositionZ(this Transform transform, float z)
        {
            Vector3 position = transform.position;
            position.z = z;
            transform.position = position;
        }

        #endregion

        #region Vector3 Extensions

        /// <summary>
        /// Returns a new Vector3 with the X component replaced.
        /// </summary>
        public static Vector3 WithX(this Vector3 vector, float x)
        {
            return new Vector3(x, vector.y, vector.z);
        }

        /// <summary>
        /// Returns a new Vector3 with the Y component replaced.
        /// </summary>
        public static Vector3 WithY(this Vector3 vector, float y)
        {
            return new Vector3(vector.x, y, vector.z);
        }

        /// <summary>
        /// Returns a new Vector3 with the Z component replaced.
        /// </summary>
        public static Vector3 WithZ(this Vector3 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }

        /// <summary>
        /// Returns a Vector2 with the X and Z components of the Vector3.
        /// </summary>
        public static Vector2 XZ(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.z);
        }

        #endregion

        #region GameObject Extensions

        /// <summary>
        /// Gets or adds a component to the GameObject.
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// Destroys all children of the GameObject.
        /// </summary>
        public static void DestroyChildren(this GameObject gameObject)
        {
            Transform transform = gameObject.transform;
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Destroys all children of the Transform.
        /// </summary>
        public static void DestroyChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        #endregion

        #region Collection Extensions

        /// <summary>
        /// Returns a random element from the list.
        /// </summary>
        public static T GetRandom<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                return default(T);
            }
            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Returns a random element from the array.
        /// </summary>
        public static T GetRandom<T>(this T[] array)
        {
            if (array == null || array.Length == 0)
            {
                return default(T);
            }
            return array[Random.Range(0, array.Length)];
        }

        /// <summary>
        /// Shuffles the list in place using Fisher-Yates algorithm.
        /// </summary>
        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        #endregion

        #region String Extensions

        /// <summary>
        /// Returns true if the string is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Returns true if the string is null, empty, or contains only whitespace.
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        #endregion

        #region Float Extensions

        /// <summary>
        /// Remaps a value from one range to another.
        /// </summary>
        public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
        }

        #endregion
    }
}
