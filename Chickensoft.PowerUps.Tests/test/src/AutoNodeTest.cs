namespace Chickensoft.PowerUps.Tests;

using System.Threading.Tasks;
using Chickensoft.GoDotTest;
using Chickensoft.PowerUps.Tests.Fixtures;
using Godot;
using GodotTestDriver;
using Shouldly;

public class AutoNodeTest : TestClass {
  private Fixture _fixture = default!;
  private AutoNodeTestScene _scene = default!;

  public AutoNodeTest(Node testScene) : base(testScene) { }

  [Setup]
  public async Task Setup() {
    _fixture = new Fixture(TestScene.GetTree());
    _scene = await _fixture.LoadAndAddScene<AutoNodeTestScene>();
  }

  [Cleanup]
  public async Task Cleanup() => await _fixture.Cleanup();

  [Test]
  public void ConnectsNodesCorrectlyWhenInstantiated() {
    _scene.MyNode.ShouldNotBeNull();
    _scene.MyNodeOriginal.ShouldNotBeNull();
    _scene.MyUniqueNode.ShouldNotBeNull();
    _scene.DifferentName.ShouldNotBeNull();
    _scene._my_unique_node.ShouldNotBeNull();
    _scene.SomeOtherNodeReference.ShouldBeNull();
  }
}
