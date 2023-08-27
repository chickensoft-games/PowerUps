namespace Chickensoft.PowerUps.Tests.Fixtures;

using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class OtherAttribute : System.Attribute {
  public OtherAttribute() { }
}
