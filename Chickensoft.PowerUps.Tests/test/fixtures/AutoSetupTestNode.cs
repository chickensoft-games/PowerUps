namespace Chickensoft.PowerUps.Tests.Fixtures;

using Chickensoft.PowerUps;
using Godot;
using SuperNodes.Types;

[SuperNode(typeof(AutoSetup))]
public partial class AutoSetupTestNode : Node2D {
  public override partial void _Notification(int what);

  public bool SetupCalled { get; set; }

  public void Setup() => SetupCalled = true;
}

[SuperNode(typeof(AutoSetup))]
public partial class AutoSetupTestNodeNoImplementation : Node2D {
  public override partial void _Notification(int what);
}
