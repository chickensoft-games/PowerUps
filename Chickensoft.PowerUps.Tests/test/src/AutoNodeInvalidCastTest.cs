namespace Chickensoft.PowerUps.Tests;

using Chickensoft.GoDotTest;
using Chickensoft.PowerUps.Tests.Fixtures;
using Godot;
using Shouldly;

public class AutoNodeInvalidCastTest : TestClass {
  public AutoNodeInvalidCastTest(Node testScene) : base(testScene) { }

  [Test]
  public void ThrowsOnIncorrectNodeType() {
    var scene = GD.Load<PackedScene>(
      "res://test/fixtures/AutoNodeInvalidCastTestScene.tscn"
    );
    // AutoNode will actually throw an InvalidCastException
    // during the scene instantiation, but for whatever reason that doesn't
    // happen on our call stack. So we just make sure the node is null after :/
    var node = scene.Instantiate<AutoNodeInvalidCastTestScene>();
    node.Node.ShouldBeNull();
  }
}
