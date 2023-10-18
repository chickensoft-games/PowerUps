namespace Chickensoft.PowerUps.Tests;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public class NodeAttributeTest : TestClass {
  public NodeAttributeTest(Node testScene) : base(testScene) { }

  [Test]
  public void Initializes() {
    var attr = new NodeAttribute("path");
    attr.Path.ShouldBe("path");
  }
}
