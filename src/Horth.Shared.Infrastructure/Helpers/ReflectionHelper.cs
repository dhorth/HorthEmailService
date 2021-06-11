using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Irc.Infrastructure.IRC.Code
{
    public class ReflectionHelper
    {
        [DebuggerStepThrough]
        public static object InvokeMethod(object target, string methodName)
        {
            return InvokeMethod(target, methodName, null);
        }

        [DebuggerStepThrough]
        public static object InvokeMethod(object target, string methodName, object[] args)
        {
            return RunInstanceMethod(target.GetType(), methodName, target, args);
        }

        [DebuggerStepThrough]
        public static object InvokeEvent(object target, string methodName)
        {
            return InvokeEvent(target, methodName, null, null);
        }

        [DebuggerStepThrough]
        public static object InvokeEvent(object target, string methodName, object sender, object eventArg)
        {
            var args = new[] { sender, eventArg };
            return RunInstanceMethod(target.GetType(), methodName, target, args);
        }

        [DebuggerStepThrough]
        public static object GetPropertyValue(object target, string propertyName)
        {
            return InvokeMethod(target, "get_" + propertyName, null);
        }
        [DebuggerStepThrough]
        public static object SetPropertyValue(object target, string propertyName, object value)
        {
            return InvokeMethod(target, "set_" + propertyName, new[] { value });
        }

        public static object RunStaticMethod(Type objectType, string methodName, object[] args)
        {
            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            return RunMethod(objectType, methodName, null, args, bindingFlags);
        }

        public static object RunInstanceMethod(Type objectType, string methodName, object objInstance, object[] args)
        {
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return RunMethod(objectType, methodName, objInstance, args, bindingFlags);
        }

        private static object RunMethod(Type objectType, string methodName, object objInstance, object[] args,
                                        BindingFlags bindingFlags)
        {
            var mi = objectType.GetMethod(methodName, bindingFlags);
            if (mi == null)
                throw new ArgumentException("There is no method '" + methodName + "' for type " + objectType);

            object ret = null;
            if (mi.ReturnType.Name == "Void")
                mi.Invoke(objInstance, args);
            else
                ret = mi.Invoke(objInstance, args);
            return ret;
        }

        protected static bool HasAttribute(Type objectType, string methodName, MethodAttributes attribute)
        {
            var mi = objectType.GetMethod(methodName);
            if (mi == null)
                throw new ArgumentException("There is no method '" + methodName + "' for type " + objectType);
            return (mi.Attributes & attribute) == 0;
        }

        protected static bool HasOnlyPrivateConstructor(Type objectType)
        {
            var constructors = objectType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Any(constructorInfo => (constructorInfo.Attributes & MethodAttributes.Public) > 0))
                return false;
            return constructors.Length > 0;
        }

        protected static T GetField<T>(object target, string name)
        {
            return (T)target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target);
        }
        public static string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            return (propertyExpression.Body as MemberExpression).Member.Name;
        }

        //[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        //public static void DoEvents(int count = 1)
        //{
        //    for (int i = 0; i < count; i++)
        //    {
        //        var frame = new DispatcherFrame();
        //        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
        //                                                 new DispatcherOperationCallback(ExitFrame), frame);
        //        Dispatcher.PushFrame(frame);
        //        if (i != count)
        //            Thread.Sleep(10);
        //    }
        //}

        //private static object ExitFrame(object frame)
        //{
        //    ((DispatcherFrame)frame).Continue = false;
        //    return null;
        //}

        public static List<PropertyInfo> GetPropertyInfo<T>(object element)
        {
            Type myType = element.GetType();
            //use display order
            var fields = myType.GetProperties()
                .OrderBy(a =>
                {
                    var da = ((DisplayAttribute)a.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault());
                    return da == null ? 0 : da.Order;
                })
                .ToList();

            return fields;
        }

        public static void SetValue(object inputObject, string propertyName, object propertyVal)
        {

            //find out the type
            Type type = inputObject.GetType();

            //get the property information based on the type
            System.Reflection.PropertyInfo propertyInfo = type.GetProperty(propertyName);

            //find the property type
            Type propertyType = propertyInfo.PropertyType;

            if (!IsNullableType(propertyType) && propertyVal == null)
                return;

            //Convert.ChangeType does not handle conversion to nullable types
            //if the property type is nullable, we need to get the underlying type of the property
            var targetType = IsNullableType(propertyType) ? Nullable.GetUnderlyingType(propertyType) : propertyType;

            //Returns an System.Object with the specified System.Type and whose value is
            //equivalent to the specified object.
            propertyVal = Convert.ChangeType(propertyVal, targetType);

            //Set the value of the property
            if(propertyInfo.CanWrite)
                propertyInfo.SetValue(inputObject, propertyVal, null);

        }
        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }
    }
}
