namespace Chickensoft.PowerUps.Tests.Fixtures;

using Chickensoft.PowerUps;
using Godot;
using SuperNodes.Types;

[SuperNode(typeof(AutoDispose))]
public partial class AutoDisposeTestScene : Node2D {
  public override partial void _Notification(int what);

  // Won't get disposed since it's read-only
  public DisposableObject MyReadonlyDisposable { get; } =
    new DisposableObject();

  // Also won't get disposed since it's read-only
  public DisposableObject ReadonlyDisposable => MyReadonlyDisposable;

  // Will get disposed since it's read-write
  public DisposableObject MyDisposable { get; set; } = default!;

  // This never gets set, so it will be null. AutoDispose should be smart enough
  // to leave it alone to avoid null reference errors.
  public DisposableObject MyNullDisposable { get; set; } = default!;

  public void OnReady() => MyDisposable = new DisposableObject();
}
