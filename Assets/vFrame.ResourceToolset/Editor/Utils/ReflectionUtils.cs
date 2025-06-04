using vFrame.ResourceToolset.Editor.Exceptions;

namespace vFrame.ResourceToolset.Editor.Utils
{
    internal static class ReflectionUtils
    {
        public static void SetPropertyValue(object obj, string propertyName, object value) {
            var propertyInfo = obj.GetType().GetProperty(propertyName);
            if (null == propertyInfo) {
                throw new ResourceToolsetException("Target does not have property: " + propertyName);
            }
            propertyInfo.SetValue(obj, value);
        }

        public static object GetPropertyValue(object obj, string propertyName) {
            var propertyInfo = obj.GetType().GetProperty(propertyName);
            if (null == propertyInfo) {
                throw new ResourceToolsetException("Target does not have property: " + propertyName);
            }
            return propertyInfo.GetValue(obj, null);
        }

        public static void SetPropertyValue<T1, T2>(T1 obj, string propertyName, T2 value) {
            SetPropertyValue((object)obj, propertyName, (object)value);
        }

        public static TR GetPropertyValue<TR, T1>(T1 obj, string propertyName) {
            return (TR)GetPropertyValue(obj, propertyName);
        }

        public static void SetFieldValue(object obj, string fieldName, object value) {
            var fieldInfo = obj.GetType().GetField(fieldName);
            if (null == fieldInfo) {
                throw new ResourceToolsetException("Target does not have property: " + fieldName);
            }
            fieldInfo.SetValue(obj, value);
        }

        public static object GetFieldValue(object obj, string fieldName) {
            var fieldInfo = obj.GetType().GetField(fieldName);
            if (null == fieldInfo) {
                throw new ResourceToolsetException("Target does not have field: " + fieldName);
            }
            return fieldInfo.GetValue(obj);
        }

        public static void SetFieldValue<T1, T2>(T1 obj, string fieldName, T2 value) {
            SetFieldValue((object)obj, fieldName, (object)value);
        }

        public static TR GetFieldValue<TR, T1>(T1 obj, string fieldName) {
            return (TR)GetFieldValue(obj, fieldName);
        }
    }
}