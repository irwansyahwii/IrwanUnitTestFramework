using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;

namespace IrwanUnitTestFramework
{
  public static class UnitTestExtensions
  {
    public static GetControlInfo<T> GetControl<T>(this Control theContainer) where T:Control
    {
      return new GetControlInfo<T>(theContainer);
    }

    public static ApplicationExceptionAssertInfo AssertApplicationExceptionMessageWhenCalling(this object theObject, Action<object> callback)
    {      
      return new ApplicationExceptionAssertInfo(theObject, callback);
    }

    public static ApplicationExceptionAssertInfo AssertApplicationExceptionMessageWhenCalling(this object theObject,
                                                                                              string methodName, params object[] parameteres)
    {
      return AssertApplicationExceptionMessageWhenCalling(theObject, x => x.Call(methodName, parameteres));
    }

    public static SpecificException<T> AssertSpecificException<T> (this object theObject, Action<object> callback) where T: Exception
    {
      return new SpecificException<T>(theObject, callback);
    }

    public static ArgumentExceptionAssertInfo AssertArgumentExceptionMessageWhenCalling(this object theObject, Action<object> callback)
    {
      return new ArgumentExceptionAssertInfo(theObject, callback);
    }

    public static ArgumentExceptionAssertInfo AssertArgumentExceptionMessageWhenCalling(this object theObject,
                                                                                        string methodName,
                                                                                        params object[] parameteres)
    {
      return AssertArgumentExceptionMessageWhenCalling(theObject, x => x.Call(methodName, parameteres));
    }
  }

  public class GetControlInfo<T> where T:Control
  {
    public T ControlType { get; set; }
    public Control TheContainer { get; set; }
//    public GetControlInfo(Type controlType, Control theControl)
    public GetControlInfo(Control theContainer)
    {
      TheContainer = theContainer;
    }
    public T WhereNameIs(string controlName)
    {
      T foundControl = default(T);
      Control[] founds = TheContainer.Controls.Find(controlName, true);
      if (founds != null)
      {
        if (founds.Length > 0)
        {
          foundControl = founds[0] as T;
        }
      }

      return foundControl;
    }
  }

  public class UnitTestAssertInfo
  {
    public Action<object> Callback;
    public object TheObject;

    public UnitTestAssertInfo(object theObject, Action<object> callback)
    {
      TheObject = theObject;
      Callback = callback;
    }
  }

  public class SpecificException<T> : UnitTestAssertInfo where T : Exception
  {
    public SpecificException(object theObject, Action<object> callback) : base(theObject, callback)
    {
    }
    public void AreEquals(string expectedMessage)
    {
      try
      {
        Callback(TheObject);
        Assert.Fail("The method must throw an exception.");
      }
      catch (T ex)
      {
        T theRightException = ex;

        if (theRightException.InnerException != null)
        {
          theRightException = theRightException.InnerException as T;
          if (theRightException == null)
          {
            Assert.Fail("Exception thrown has an InnnerException but is not type of {0}. Inner exception: {1}", typeof(T), theRightException.InnerException);
          }          
        }

        Assert.AreEqual(expectedMessage, theRightException.Message);
        throw theRightException;
      }
      catch (TargetInvocationException ex)
      {
        if (ex.InnerException == null)
        {
          Assert.Fail("SpecificException<T>.AreEquals(string expectedMessage). ex.InnerException must not null.");
        }
        T theRightException = ex.InnerException as T;

        if (theRightException == null)
        {
          Assert.Fail(
            "SpecificException<T>.AreEquals(string expectedMessage). theRightException is not type of {0}. The exception: {1}",
            typeof (T), ex.InnerException);
        }

        Assert.AreEqual(expectedMessage, theRightException.Message);
        throw theRightException;

      }
      catch (Exception ex)
      {
        T theRightException = null;
        if (ex.InnerException != null)
        {
          theRightException = ex.InnerException as T;
          if (theRightException == null)
          {
            Assert.Fail("Exception thrown has an InnnerException but is not type of {0}. Inner exception: {1}", typeof(T), theRightException.InnerException);
          }
          else
          {
            Assert.AreEqual(expectedMessage, theRightException.Message);
            throw theRightException;
          }
        }
        Assert.Fail("Exception is not type of {0}: . The Exception:{1}", typeof(T), ex);
      }
    }
  }

  public class ArgumentExceptionAssertInfo: UnitTestAssertInfo
  {
    public ArgumentExceptionAssertInfo(object theObject, Action<object> callback) : base(theObject, callback)
    {
    }
    public void AreEquals(string expectedMessage)
    {
      try
      {
        Callback(TheObject);
      }
      catch (ArgumentException ex)
      {
        ArgumentException theRightException = ex;
        Assert.AreEqual(expectedMessage, theRightException.Message);
        throw theRightException;
      }
      catch(TargetInvocationException ex)
      {
        ArgumentException theRightException = ex.InnerException as ArgumentException;
        if (theRightException == null)
        {
          throw new InvalidOperationException("ex.InnerException is not an ArgumentException");
        }
        Assert.AreEqual(expectedMessage, theRightException.Message);
        throw theRightException;
        
      }
      catch (Exception ex)
      {
        Assert.Fail("Exception is not type of ArgumentException: " + ex.ToString());        
      }
    }
  }

  public class ApplicationExceptionAssertInfo : UnitTestAssertInfo
  {
    public ApplicationExceptionAssertInfo(object theObject, Action<object> callback)
      : base(theObject, callback)
    {
    }

    public void AreEquals(string expectedMessage)
    {
      try
      {
        Callback(TheObject);
        
      }
      catch (ApplicationException ex)
      {
        ApplicationException theRightException = ex;
        ApplicationException innerAppException = ex.InnerException as ApplicationException;

        if (innerAppException != null)
        {
          theRightException = innerAppException;
        }

        Assert.AreEqual(expectedMessage, theRightException.Message);
        throw theRightException;
      }
      catch(Exception ex)
      {
        Assert.Fail("Exception is not type of ApplicationException: " + ex.ToString());
        
      }
    }
  }
}
