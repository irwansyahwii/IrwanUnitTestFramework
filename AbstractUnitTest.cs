using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrwanUnitTestFramework
{
  public abstract class AbstractUnitTest
  {
    public abstract void Arrange();
    public abstract void Act();
    public abstract void Asserts();

    public virtual void Run()
    {
      Arrange();
      Act();
      Asserts();
    }
  }
}
