using System;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace vFrame.ResourceToolset.Editor.Utils
{
    public static class InformationDumpUtils
    {
        public static void PrintAssetDependencies(string path) {
            var builder = new StringBuilder();
            builder.Append("Dependencies of asset ");
            builder.Append(path);
            builder.Append(":\n");

            var dependencies = AssetDatabase.GetDependencies(path, false);
            foreach (var dependency in dependencies) {
                builder.Append("- ");
                builder.Append(dependency);
                builder.Append("\n");
            }

            Debug.Log(builder.ToString());
        }

        public static void PrintAssetGuid(string path) {
            var guid = AssetDatabase.AssetPathToGUID(path);
            Debug.Log("Asset guid: " + guid);
        }

        public static void PrintAssetGuidAndFileId(Object obj) {
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var guid, out long fileId)) {
                Debug.Log("Get asset guid and file id failed!");
                return;
            }

            Debug.LogFormat("Asset guid: {0}, fileId: {1}", guid, fileId);
        }

        public static void PrintAssetSerializedData(string path) {
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (!asset)
                return;
            PrintAssetSerializedData(asset);
        }

        public static void PrintAssetSerializedData(Object obj) {
            if (!obj)
                throw new ArgumentNullException(nameof(obj));

            var sb = new StringBuilder();
            sb.AppendFormat("Dumping Asset: {0}, Type: {1}, InstanceID: {2}",
                obj.name,
                obj.GetType().FullName,
                obj.GetInstanceID());
            sb.Append("\n\n");

            PrintAssetSerializedDataInternal(sb, obj);

            var ret = sb.ToString();
            const int maxCount = 15000;
            var index = 0;
            do {
                var count = Math.Min(maxCount, ret.Length - index);
                Debug.Log(ret.Substring(index, count));
                index += count;
            }
            while (index < ret.Length);
        }

        private static void PrintAssetSerializedDataInternal(StringBuilder sb, Object obj) {
            using (var serializedObject = new SerializedObject(obj)) {
                var property = serializedObject.GetIterator();
                if (property.Next(true)) {
                    PrintAssetSerializedDataInternal(sb, property);
                }
            }
        }

        private static void PrintAssetSerializedDataInternal(StringBuilder sb, SerializedProperty property) {
            do {
                var depth = Math.Max(property.depth, 0);
                var tab = new string(' ', depth * 2);
                sb.Append(tab);
                sb.Append(property.displayName);
                sb.Append(": ");
                sb.Append(GetPropertyDisplayValue(property));
                sb.Append("\n");
            }
            while (property.Next(true));
        }

        private static string GetPropertyDisplayValue(SerializedProperty property) {
            switch (property.propertyType) {
                case SerializedPropertyType.Generic:
                    break;
                case SerializedPropertyType.Integer:
                    return property.type == "char"
                        ? Convert.ToChar(property.intValue).ToString()
                        : property.intValue.ToString();
                case SerializedPropertyType.Boolean:
                    return property.boolValue.ToString();
                case SerializedPropertyType.Float:
                    return property.floatValue.ToString(CultureInfo.CurrentCulture);
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Color:
                    return property.colorValue.ToString("F6");
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue
                        ? property.objectReferenceValue.ToString()
                        : "";
                case SerializedPropertyType.LayerMask:
                    return property.intValue.ToString();
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex.ToString();
                case SerializedPropertyType.Vector2:
                    return property.vector2Value.ToString("F6");
                case SerializedPropertyType.Vector3:
                    return property.vector3Value.ToString("F6");
                case SerializedPropertyType.Vector4:
                    return property.vector4Value.ToString("F6");
                case SerializedPropertyType.Rect:
                    return property.rectValue.ToString("F6");
                case SerializedPropertyType.ArraySize:
                    return property.isArray ? property.arraySize.ToString() : "0";
                case SerializedPropertyType.Character:
                    return Convert.ToChar(property.intValue).ToString();
                case SerializedPropertyType.AnimationCurve:
                    return property.animationCurveValue.ToString();
                case SerializedPropertyType.Bounds:
                    return property.boundsValue.ToString("F6");
                case SerializedPropertyType.Gradient:
                    return "<Gradient>";
                case SerializedPropertyType.Quaternion:
                    return property.quaternionValue.ToString("F6");
                case SerializedPropertyType.ExposedReference:
                    return property.exposedReferenceValue != null
                        ? property.exposedReferenceValue.ToString()
                        : "<null>";
                case SerializedPropertyType.FixedBufferSize:
                    return property.fixedBufferSize.ToString();
                case SerializedPropertyType.Vector2Int:
                    return property.vector2IntValue.ToString();
                case SerializedPropertyType.Vector3Int:
                    return property.vector3IntValue.ToString();
                case SerializedPropertyType.RectInt:
                    return property.rectIntValue.ToString();
                case SerializedPropertyType.BoundsInt:
                    return property.boundsIntValue.ToString();
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return "";
        }
    }
}