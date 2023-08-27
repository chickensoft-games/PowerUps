namespace Chickensoft.PowerUps;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Godot;
using SuperNodes.Types;

#pragma warning disable CS8019
using Chickensoft.PowerUps;
#pragma warning restore CS8019

/// <summary>
/// Interface for a PowerUp that automatically disposes of any writeable
/// properties or fields on your node that are references to IDisposable (but
/// not nodes — Godot cleans those up for you).
/// </summary>
public interface IAutoDispose { }

/// <summary>
/// Add this PowerUp to a SuperNode to automatically dispose of any writeable
/// properties or fields on your node that are references to IDisposable (but
/// not nodes — Godot cleans those up for you).
/// </summary>
[PowerUp]
public abstract partial class AutoDispose : Node, IAutoDispose {
  #region ISuperNode
  // These don't need to be copied over since we will be copied into an
  // ISuperNode.

  [PowerUpIgnore]
  public ImmutableDictionary<string, ScriptPropertyOrField> PropertiesAndFields
      => throw new NotImplementedException();
  [PowerUpIgnore]
  public TResult GetScriptPropertyOrFieldType<TResult>(
      string scriptProperty, ITypeReceiver<TResult> receiver
    ) => throw new NotImplementedException();
  [PowerUpIgnore]
  public dynamic GetScriptPropertyOrField(string scriptProperty) =>
      throw new NotImplementedException();

  #endregion ISuperNode

  #region StatefulMixinAdditions
  public HashSet<IDisposable> Disposables { get; } = new();
  #endregion StatefulMixinAdditions

  public void OnAutoDispose(int what) {
    if (what == NotificationReady) {
      // After the node is ready, register all the disposables it has setup.
      ToSignal(this, Node.SignalName.Ready).OnCompleted(
        () => Disposer.RegisterDisposables(
          PropertiesAndFields, Disposables, (ISuperNode)this
        )
      );
    }
    else if (what == NotificationExitTree) {
      // After the node has exited, cleanup the disposables it registered.
      ToSignal(this, Node.SignalName.TreeExited).OnCompleted(
        () => Disposer.DisposeDisposables(Disposables)
      );
    }
  }
}

public static class Disposer {
  public class IsDisposableChecker : ITypeReceiver<IDisposable> {
    /// <summary>Object to check to see if it is disposable.</summary>
    public object? PotentialDisposable { get; set; }

    public IsDisposableChecker() {
      PotentialDisposable = default;
    }

#nullable disable
    public IDisposable Receive<T>() =>
      PotentialDisposable is IDisposable disposable and not Node
        ? disposable
        : default;
#nullable restore
  }

  [ThreadStatic]
  private static readonly IsDisposableChecker _checker = new();

  public static void RegisterDisposables(
    ImmutableDictionary<string, ScriptPropertyOrField> propertiesAndFields,
    HashSet<IDisposable> disposables,
    ISuperNode node
  ) {
    foreach (var (name, propertyOrField) in propertiesAndFields) {
      if (!propertyOrField.IsReadable || !propertyOrField.IsMutable) {
        continue;
      }
      // Only auto-dispose readable and writeable properties
      _checker.PotentialDisposable = node.GetScriptPropertyOrField(name);
      if (
        node.GetScriptPropertyOrFieldType(name, _checker) is
          IDisposable disposable
      ) {
        disposables.Add(disposable);
      }
    }
  }

  public static void DisposeDisposables(HashSet<IDisposable> disposables) {
    lock (disposables) {
      foreach (var obj in disposables) {
        if (obj is IDisposable disposable) { disposable.Dispose(); }
      }
      disposables.Clear();
    }
  }
}
