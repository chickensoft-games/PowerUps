namespace Chickensoft.PowerUps.Tests;

using System;
using Chickensoft.GoDotTest;
using Chickensoft.PowerUps.Tests.Fixtures;
using Godot;
using Shouldly;

public class AutoNodeInvalidCastTest : TestClass {
  public AutoNodeInvalidCastTest(Node testScene) : base(testScene) { }

  [Test]
  public void ThrowsOnIncorrectNodeType() => Should.Throw<Exception>(() =>
    GD.Load<PackedScene>(
      "res://test/fixtures/AutoNodeInvalidCastTestScene.tscn"
    ).Instantiate<AutoNodeTestScene>()
  );
}
