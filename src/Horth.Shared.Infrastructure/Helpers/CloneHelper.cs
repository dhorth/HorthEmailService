using System;
using System.Linq;
using System.Reflection;

namespace Horth.Service.Email.Shared
{
    public static class CloneHelper
    {
        public static void Clone<T>(T dest, object src) where T : new()
        {
            Type t = typeof(T);
            PropertyInfo[] srcFields = src.GetType().GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);

            PropertyInfo[] destFields = t.GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);

            foreach (var property in srcFields)
            {
                var d = destFields.FirstOrDefault(x => x.Name == property.Name);
                if (d != null && d.CanWrite)
                    d.SetValue(dest, property.GetValue(src, null), null);
            }

        }
    }
}
