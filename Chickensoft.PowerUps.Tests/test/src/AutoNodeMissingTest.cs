namespace Chickensoft.PowerUps.Tests;

using System;
using Chickensoft.GoDotTest;
using Chickensoft.PowerUps.Tests.Fixtures;
using Godot;
using Shouldly;

public class AutoNodeMissingTest : TestClass {
  public AutoNodeMissingTest(Node testScene) : base(testScene) { }

  [Test]
  public void ThrowsOnMissingNode() => Should.Throw<Exception>(() =>
    GD.Load<PackedScene>(
      "res://test/fixtures/AutoNodeMissingTestScene.tscn"
    ).Instantiate<AutoNodeTestScene>()
  );
}
