namespace Chickensoft.PowerUps.Tests;
using Chickensoft.GoDotTest;
using Chickensoft.PowerUps.Tests.Fixtures;
using Godot;
using Shouldly;

public class AutoSetupTest : TestClass {
  public AutoSetupTest(Node testScene) : base(testScene) { }

  [Test]
  public void SetsUpNode() {
    var node = new AutoSetupTestNode();

    node._Notification((int)Node.NotificationReady);

    node.SetupCalled.ShouldBeTrue();
  }

  [Test]
  public void DefaultImplementationDoesNothing() {
    var node = new AutoSetupTestNodeNoImplementation();

    node._Notification((int)Node.NotificationReady);
  }
}
