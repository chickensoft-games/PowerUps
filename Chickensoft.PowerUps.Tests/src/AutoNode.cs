namespace Chickensoft.PowerUps;

using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Godot;
using SuperNodes.Types;

#pragma warning disable CS8019
using Chickensoft.PowerUps;
#pragma warning restore CS8019

/// <summary>
/// Node attribute. Apply this to properties or fields that need to be
/// automatically connected to a corresponding node instance in the scene tree.
/// </summary>
[AttributeUsage(
  AttributeTargets.Field | AttributeTargets.Property,
  AllowMultiple = false
)]
public class NodeAttribute : Attribute {
  /// <summary>
  /// Explicit node path or unique identifier that the tagged property or field
  /// should reference. If not provided (or null), the name of the property or
  /// field itself will be converted to PascalCase (with any leading
  /// underscores removed) and used as a unique node identifier. For example,
  /// the reference `Node2D _myNode` would be connected to `%MyNode`.
  /// </summary>
  public string? Path { get; }

  public NodeAttribute(string? path = null) {
    Path = path;
  }

  public const string ID = "global::Chickensoft.PowerUps.NodeAttribute";
}

/// <summary>
/// Interface for a PowerUp that automatically connects declared node
/// references to their corresponding instances in the scene tree.
/// </summary>
public interface IAutoNode { }

/// <summary>
/// Apply this PowerUp to your SuperNode to automatically connect declared node
/// references to their corresponding instances in the scene tree.
/// </summary>
[PowerUp]
public abstract partial class AutoNode : Node, IAutoNode {
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
  [PowerUpIgnore]
  public void SetScriptPropertyOrField(string scriptProperty, dynamic value) =>
      throw new NotImplementedException();

  #endregion ISuperNode

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void OnAutoNode(int what) {
    if (what == NotificationSceneInstantiated) {
      AutoNodeConnector.ConnectNodes(PropertiesAndFields, (ISuperNode)this);
    }
  }
}

public static class AutoNodeConnector {
  public class NodeChecker : ITypeReceiver<bool> {
    public Node Node { get; set; } = default!;

    public bool Receive<T>() => Node is T;
  }

  [ThreadStatic]
  private static readonly NodeChecker _checker = new();

  public static void ConnectNodes(
      ImmutableDictionary<string, ScriptPropertyOrField> propertiesAndFields,
      ISuperNode superNode
    ) {
    var superNodeNode = (Node)superNode;
    foreach (var (name, propertyOrField) in propertiesAndFields) {
      if (
        !propertyOrField.Attributes.TryGetValue(
          NodeAttribute.ID, out var nodeAttributes
        )
      ) {
        continue;
      }
      var nodeAttribute = nodeAttributes[0];

      var path = nodeAttribute.ArgumentExpressions[0] as string ??
        AsciiToPascalCase(name);

      var node = ((Node)superNode).GetNode(path) ?? throw new InvalidOperationException(
          $"Node {path} does not exist in scene tree for property {name} of " +
          $"type {propertyOrField.Type} on {superNodeNode.Name}."
        );

      _checker.Node = node;
      var isValidAssignment =
        superNode.GetScriptPropertyOrFieldType(name, _checker);

      if (!isValidAssignment) {
        throw new InvalidCastException(
          $"Cannot assign node of type {node.GetType()} to property " +
          $"{name} of type {propertyOrField.Type} on {superNodeNode.Name}."
        );
      }

      superNode.SetScriptPropertyOrField(name, node);
    }
  }

  /// <summary>
  /// <para>
  /// Converts an ASCII string to PascalCase. This looks insane, but it is the
  /// fastest out of all the benchmarks I did.
  /// </para>
  /// <para>
  /// Since messing with strings can be slow and looking up nodes is a common
  /// operation, this is a good place to optimize. No heap allocations!
  /// </para>
  /// <para>
  /// Removes underscores, always capitalizes the first letter, and capitalizes
  /// the first letter after an underscore.
  /// </para>
  /// </summary>
  /// <param name="input">Input string.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string AsciiToPascalCase(string input) {
    var span = input.AsSpan();
    Span<char> output = stackalloc char[span.Length];
    var outputIndex = 0;

    for (var i = 0; i < span.Length; i++) {
      var c = span[i];

      if (c == '_') { continue; }

      output[outputIndex++] = i == 0 || span[i - 1] == '_'
        ? (char)(c & 0xDF)
        : c;
    }

    return new string(output[..outputIndex]);
  }
}
