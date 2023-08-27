namespace Chickensoft.PowerUps.Tests;

using System.Threading.Tasks;
using Chickensoft.GoDotTest;
using Chickensoft.PowerUps.Tests.Fixtures;
using Godot;
using GodotTestDriver;
using Shouldly;

public class AutoDisposeTest : TestClass {
  private Fixture _fixture = default!;
  private AutoDisposeTestScene _scene = default!;

  public AutoDisposeTest(Node testScene) : base(testScene) { }

  [Setup]
  public async Task Setup() {
    _fixture = new Fixture(TestScene.GetTree());
    _scene = await _fixture.LoadAndAddScene<AutoDisposeTestScene>(
      autoFree: false,
      autoRemoveFromRoot: false
    );
  }

  [Test]
  public async Task DisposesWriteableProperties() {
    _scene.MyReadonlyDisposable.IsDisposed.ShouldBeFalse();
    _scene.ReadonlyDisposable.IsDisposed.ShouldBeFalse();
    _scene.MyDisposable.IsDisposed.ShouldBeFalse();
    _scene.MyNullDisposable.ShouldBeNull();

    _scene.GetParent().RemoveChild(_scene);

    _scene.MyReadonlyDisposable.IsDisposed.ShouldBeFalse();
    _scene.ReadonlyDisposable.IsDisposed.ShouldBeFalse();
    _scene.MyDisposable.IsDisposed.ShouldBeTrue();
    _scene.MyNullDisposable.ShouldBeNull();

    await _fixture.Cleanup();
  }
}
