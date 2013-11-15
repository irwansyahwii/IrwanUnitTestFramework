using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace IrwanUnitTestFramework
{
  public class ReflectionUtility
  {
    public static FieldInfo GetInstanceAndNonPublicFieldInfo(object theObject, string fieldName)
    {
      FieldInfo fieldInfo = GetTypeOfTheObject(theObject)
                                     .GetField(fieldName,
                                               BindingFlags.Instance
                                                                 | BindingFlags.FlattenHierarchy
                                                                 | BindingFlags.NonPublic                                                                 
                                                                 | BindingFlags.IgnoreCase                                                                 
                                                                 );


      return fieldInfo;
    }

    public static FieldInfo GetStaticAndNonPublicFieldInfo(object theObject, string fieldName)
    {
      var reflectedObject = GetTypeOfTheObject(theObject);
      FieldInfo fieldInfo = reflectedObject
                                     .GetField(fieldName,
                                               BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
      return fieldInfo;
    }

    public static Type GetTypeOfTheObject(object theObject)
    {
      System.Type reflectedObject = theObject as System.Type;
      if (reflectedObject == null)
      {
        reflectedObject = theObject.GetType();
      }
      return reflectedObject;
    }

    public static MethodInfo GetStaticAndNonPublicMethodInfo(object theObject, string methodName, params object[] args)
    {
      MethodInfo methodInfo = GetTypeOfTheObject(theObject).GetMethod(methodName, BindingFlags.Static
                                                                                  | BindingFlags.NonPublic);
      return methodInfo;
    }

    public static MethodInfo GetInstanceAndNonPublicMethodInfo(object theObject, string methodName, params object[] args)
    {
      MethodInfo methodInfo = null;
      try
      {
        if (methodName.StartsWith("get:") || methodName.StartsWith("set:"))
        {
          string realMethodName = methodName.Substring(4);

          PropertyInfo propertyInfo = GetTypeOfTheObject(theObject).GetProperty(realMethodName,
                                                                                BindingFlags.IgnoreCase |
                                                                                BindingFlags.Instance |
                                                                                BindingFlags.NonPublic
                                                                                | BindingFlags.Public);

          if (propertyInfo != null)
          {
            if (methodName.StartsWith("get:"))
            {
              methodInfo = propertyInfo.GetGetMethod(true);
            }
            if (methodName.StartsWith("set:"))
            {
              methodInfo = propertyInfo.GetSetMethod(true);
            }
          }
        }
        else
        {
          methodInfo = GetTypeOfTheObject(theObject)
                                 .GetMethod(methodName,
                                           BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public                                       
                                           );

          if (methodInfo == null && GetTypeOfTheObject(theObject).BaseType != null)
          {
            methodInfo =
              GetTypeOfTheObject(theObject).BaseType.GetMethods(BindingFlags.NonPublic 
              | BindingFlags.Instance | BindingFlags.Public
              | BindingFlags.FlattenHierarchy).SingleOrDefault(x =>
                {
                  
                  return (x.Name.Equals(methodName));
                });
            if (methodInfo != null)
            {
              theObject = GetTypeOfTheObject(theObject).BaseType;
            }
          }
        }
      }
      catch (AmbiguousMatchException)
      {
        List<Type> paramsTypes = null;
        if (args != null)
        {
          paramsTypes = new List<Type>();
          foreach (var argument in args)
          {
            if (argument != null)
            {
              if (argument.GetType().Namespace.StartsWith("Castle"))
              {
                
                paramsTypes.Add(argument.GetType().BaseType);
              }
              else
              {
                paramsTypes.Add(argument.GetType());
              }
              
            }
          }
        }


        if (paramsTypes != null)
        {
          methodInfo = GetTypeOfTheObject(theObject).GetMethod(methodName
                                                               , BindingFlags.Instance
                                                                 | BindingFlags.FlattenHierarchy
                                                                 | BindingFlags.NonPublic
                                                                 | BindingFlags.InvokeMethod
                                                                 | BindingFlags.IgnoreCase                                                                                                                                  
                                                               , Type.DefaultBinder
                                                               , paramsTypes.ToArray()
                                                               , null
            );
//          foreach (var method in GetTypeOfTheObject(theObject).GetMethods(BindingFlags.Instance | BindingFlags.FlattenHierarchy| BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly | BindingFlags.Instance))
//          {
//            bool found = true;
//
//            if (method.Name == methodName && method.GetParameters().Length == args.Length)
//            {
//
//              int i = 0;
//              
//              foreach (var arg in args)
//              {
//                if (arg.GetType() != method.GetParameters()[i].ParameterType)
//                {
//                  found = false;
//                }
//                i++;
//              }
//            }
//            if (found)
//            {
//              methodInfo = method;
//            }
//          }
        }                
      }

     

      return methodInfo;
      
    }
  }
}