namespace Chickensoft.PowerUps.Tests;

using System.Reflection;
using Godot;
using GoDotTest;

public partial class Tests : Node2D {
  public override void _Ready() {
    var testEnv = TestEnvironment.From(OS.GetCmdlineArgs());
    testEnv = testEnv with { ShouldRunTests = true };
    GoTest.RunTests(Assembly.GetExecutingAssembly(), this, testEnv);
  }
}
