namespace Chickensoft.PowerUps;

using Godot;
using SuperNodes.Types;

#pragma warning disable CS8019, IDE0005
using Chickensoft.PowerUps;
#pragma warning restore CS8019, IDE0005

/// <summary>
/// A node which can implement a Setup callback to be invoked just before
/// Ready.
/// </summary>
public interface IAutoSetup {
  /// <summary>
  /// True if the node is being unit-tested. When unit-tested, setup callbacks
  /// will not be invoked.
  /// </summary>
  bool IsTesting { get; set; }
  /// <summary>
  /// Method invoked before Ready — perform any non-node related setup and
  /// initialization here.
  /// </summary>
  void Setup() { }
}

/// <summary>
/// PowerUp which invokes a Setup method just before Ready is received. The
/// setup method is provided as a convenient place to initialize non-node
/// related values that may be needed by the node's Ready method. Separating
/// the initialization into two steps facilitates unit testing.
/// </summary>
[PowerUp]
public partial class AutoSetup : Node, IAutoSetup {
  public bool IsTesting { get; set; }
  public void OnAutoSetup(int what) {
    if (what == NotificationReady && !IsTesting) {
      ((IAutoSetup)this).Setup();
    }
  }
}
