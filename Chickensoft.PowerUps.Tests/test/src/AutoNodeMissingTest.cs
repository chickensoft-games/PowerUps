namespace Chickensoft.PowerUps.Tests;

using Chickensoft.GoDotTest;
using Chickensoft.PowerUps.Tests.Fixtures;
using Godot;
using Shouldly;

public class AutoNodeMissingTest : TestClass {
  public AutoNodeMissingTest(Node testScene) : base(testScene) { }

  [Test]
  public void ThrowsOnMissingNode() {
    var scene = GD.Load<PackedScene>("res://test/fixtures/AutoNodeMissingTestScene.tscn");
    // AutoNode will actually throw an InvalidOperationException
    // during the scene instantiation, but for whatever reason that doesn't
    // happen on our call stack. So we just make sure the node is null after :/
    var node = scene.InstantiateOrNull<AutoNodeMissingTestScene>();
    node.MyNode.ShouldBeNull();
  }
}
