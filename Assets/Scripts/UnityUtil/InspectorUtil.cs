using System.Reflection;

namespace Aspekt.Hex.Util
{
    public static class InspectorUtil
    {
        public static T GetPrivateValue<T, O>(string fieldName, O obj)
        {
            var fieldInfo = GetPrivateField<O>(fieldName);
            if (fieldInfo == null) return default;
            return (T)fieldInfo.GetValue(obj);
        }
        
        public static FieldInfo GetPrivateField<T>(string fieldName)
        {
            return typeof(T).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}