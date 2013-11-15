using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace IrwanUnitTestFramework
{
  public static class ReflectionUtilityExtensions
  {
    public static object CallBase(this object theObject, string methodName, params object[] parameters)
    {
      MethodInfo methodInfo = ReflectionUtility.GetInstanceAndNonPublicMethodInfo(theObject, methodName, parameters);

      return InvokeMethod(methodInfo.GetBaseDefinition(), theObject, methodName, parameters);
    }
    

    public static object CallOverLoad(this object theObject,  string methodName, Type[] paramsTypes, params object[] parameters)
    {
      MethodInfo methodInfo = ReflectionUtility.GetTypeOfTheObject(theObject).GetMethod(methodName
                                                                                        , BindingFlags.Instance
                                                                                          |
                                                                                          BindingFlags.FlattenHierarchy
                                                                                          | BindingFlags.NonPublic
                                                                                          | BindingFlags.InvokeMethod
                                                                                          | BindingFlags.IgnoreCase
                                                                                        , Type.DefaultBinder
                                                                                        , paramsTypes
                                                                                        , null);
      return InvokeMethod(methodInfo, theObject, methodName, parameters);
    }

    private static object InvokeMethod(MethodInfo methodInfo, object theObject, string methodName, params object[] parameters)
    {
      if (methodInfo == null)
      {
        methodInfo = ReflectionUtility.GetStaticAndNonPublicMethodInfo(theObject, methodName, parameters);
      }
      if (methodInfo == null)
      {
        throw new ApplicationException(string.Format("ReflectionUtilityExtensions.Call(this object theObject, string methodName, params object[] parameters). Method with name:{0} not found.", methodName));
      }
      if (methodInfo.ContainsGenericParameters)
      {
        List<Type> genericParams = new List<Type>();
        int currentParamIndex = 0;
        foreach (var parameterInfo in methodInfo.GetParameters())
        {
          if (parameterInfo.ParameterType.IsGenericParameter)
          {
            genericParams.Add(parameters[currentParamIndex].GetType());
          }
          currentParamIndex++;
        }

        methodInfo = methodInfo.MakeGenericMethod(genericParams.ToArray());
      }

      object retVal = null;
        
      retVal = methodInfo.Invoke(theObject, parameters);
      return retVal;
    }

    public static object Call(this object theObject, string methodName, params object[] parameters)
    {
      MethodInfo methodInfo = ReflectionUtility.GetInstanceAndNonPublicMethodInfo(theObject, methodName, parameters);
      return InvokeMethod(methodInfo, theObject, methodName, parameters);

    }

    public static T Call<T>(this object theObject, string methodName, params object[] parameters)
    {
      MethodInfo methodInfo = ReflectionUtility.GetInstanceAndNonPublicMethodInfo(theObject, methodName, parameters);
//      return (T) Convert.ChangeType(InvokeMethod(methodInfo, theObject, methodName, parameters), typeof(T));

      return (T) InvokeMethod(methodInfo, theObject, methodName, parameters);
    }

    public static void SetValue(this object theObject, string fieldName, object value)
    {
      FieldInfo fieldInfo = ReflectionUtility.GetInstanceAndNonPublicFieldInfo(theObject, fieldName);

      if (fieldInfo == null)
      {
        fieldInfo = ReflectionUtility.GetStaticAndNonPublicFieldInfo(theObject, fieldName);
      }

      if (fieldInfo == null)
      {
        throw new ApplicationException(string.Format("ReflectionUtility.SetValue(this object theObject, string fieldName, object value). Field name: {0} not found", fieldName));
      }

      fieldInfo.SetValue(theObject, value);
    }

    public static T GetValue<T>(this object theObject, string fieldName)
    {
      return (T)Convert.ChangeType(GetValue(theObject, fieldName), typeof(T));
    }

    public static object GetValue(this object theObject, string fieldName)
    {
      FieldInfo fieldInfo = ReflectionUtility.GetInstanceAndNonPublicFieldInfo(theObject, fieldName);

      if (fieldInfo == null)
      {
        fieldInfo = ReflectionUtility.GetStaticAndNonPublicFieldInfo(theObject, fieldName);
      }

      if (fieldInfo == null)
      {
        throw new ApplicationException(string.Format("ReflectionUtility.GetValue(this object theObject, string fieldName). Field name: {0} not found", fieldName));
      }

      return fieldInfo.GetValue(theObject);
    }
    
    public static ReflectionFieldInfo Field(this object theObject, string fieldName)
    {
      return new ReflectionFieldInfo(theObject, fieldName, ReflectionInfoEnum.Field);
    }
    public static ReflectionMethodInfo Method(this object theObject, string methodName)
    {
      return new ReflectionMethodInfo(theObject, methodName, ReflectionInfoEnum.Method);
    }
  }

  public class ReflectionInfo
  {
    public ReflectionInfoEnum ReflectionType { get; set; }
    public string Name { get; set; }
    public object TheObject;

    public ReflectionInfo(object theObject, string name, ReflectionInfoEnum reflectionType)
    {
      TheObject = theObject;
      Name = name;
      ReflectionType = reflectionType;
    }    
  }

  public class ReflectionFieldInfo : ReflectionInfo
  {
    public ReflectionFieldInfo(object theObject, string name, ReflectionInfoEnum reflectionType) : base(theObject, name, reflectionType)
    {
    }
    public FieldInfo WhichInstanceAndNonPublic()
    {
      return ReflectionUtility.GetInstanceAndNonPublicFieldInfo(this.TheObject, this.Name);
    }
  }

  public class ReflectionMethodInfo : ReflectionInfo
  {
    public object[] Parameters;
    public ReflectionMethodInfo(object theObject, string name, ReflectionInfoEnum reflectionType, params object[] parameters) : base(theObject, name, reflectionType)
    {
      Parameters = parameters;
    }
    public MethodInfo WhichInstanceAndNonPublic()
    {
      MethodInfo result = ReflectionUtility.GetInstanceAndNonPublicMethodInfo(this.TheObject, this.Name, Parameters);

      if (this.ReflectionType == ReflectionInfoEnum.MethodCall)
      {
        ReflectionUtility.GetInstanceAndNonPublicMethodInfo(this.TheObject, this.Name, Parameters).Invoke(TheObject, Parameters);        
      }

      return result;
    }
  }

  public enum ReflectionInfoEnum
  {
    Field = 0
    , Method = 1
    , MethodCall = 2
  }
}