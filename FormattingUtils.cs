using UnityEngine;

namespace ValheimRcon
{
    internal static class FormattingUtils
    {
        public static string ToDisplayFormat(this Vector3 vector)
        {
            return $"({vector.x:0.##} {vector.y:0.##} {vector.z:0.##})";
        }

        public static string ToDisplayFormat(this Quaternion quaternion)
        {
            var euler = quaternion.eulerAngles;
            return ToDisplayFormat(euler);
        }

        public static string ToDisplayFormat(this float value)
        {
            return $"{value:0.##}";
        }

        public static string ToDisplayFormat(this Vector2i vector)
        {
            return string.Format("({0} {1})", vector.x, vector.y);
        }
    }
}
