namespace Chickensoft.PowerUps;

#pragma warning disable CA1061, CS8766

using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Chickensoft.GodotNodeInterfaces;
#pragma warning disable CS8019, IDE0005
using Chickensoft.PowerUps;
#pragma warning restore CS8019, IDE0005
using Godot;
using SuperNodes.Types;

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
/// Apply this PowerUp to your SuperNode to automatically connect declared node
/// references to their corresponding instances in the scene tree.
/// </summary>
public interface IAutoNode : IFakeNodeTreeEnabled, ISuperNode { }

/// <summary>
/// Apply this PowerUp to your SuperNode to automatically connect declared node
/// references to their corresponding instances in the scene tree.
/// </summary>
[PowerUp]
public abstract partial class AutoNode : Node, IAutoNode {
  public FakeNodeTree? FakeNodes { get; set; }

  /// <summary>
  /// Initialize the fake node tree for unit testing.
  /// </summary>
  /// <param name="nodes">Map of node paths to mock nodes.</param>
  public void FakeNodeTree(
    System.Collections.Generic.Dictionary<string, INode>? nodes
  ) => FakeNodes = new FakeNodeTree(this, nodes);

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
  public void SetScriptPropertyOrField(string scriptProperty, dynamic? value) =>
      throw new NotImplementedException();
  #endregion ISuperNode

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void OnAutoNode(int what) {
    if (what == NotificationSceneInstantiated) {
      AutoNodeConnector.ConnectNodes(PropertiesAndFields, this);
    }
  }
}

public static class AutoNodeConnector {
  public class TypeChecker : ITypeReceiver<bool> {
    public object Value { get; set; } = default!;

    public bool Receive<T>() => Value is T;
  }

  private static readonly TypeChecker _checker = new();

  public static void ConnectNodes(
      ImmutableDictionary<string, ScriptPropertyOrField> propertiesAndFields,
      IAutoNode autoNode
    ) {
    var node = (Node)autoNode;
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

      Exception? e;

      // First, check to see if the node has been faked for testing.
      // Faked nodes take precedence over real nodes.
      if (autoNode.FakeNodes?.GetNode(path) is INode fakeNode) {
        // We found a faked node for this path. Make sure it's the expected
        // type.
        _checker.Value = fakeNode;
        var satisfiesFakeType =
          autoNode.GetScriptPropertyOrFieldType(name, _checker);
        if (!satisfiesFakeType) {
          e = new InvalidOperationException(
            $"Found a faked node at '{path}' of type " +
            $"'{fakeNode.GetType().Name}' that is not the expected type " +
            $"'{propertyOrField.Type.Name}' for member '{name}' on " +
            $"'{node.Name}'."
          );
          GD.PushError(e.Message);
          throw e;
        }
        // Faked node satisfies the expected type :)
        autoNode.SetScriptPropertyOrField(name, fakeNode);
        continue;
      }

      // We're dealing with what should be an actual node in the tree.
      var potentialChild = node.GetNodeOrNull(path);

      if (potentialChild is not Node child) {
        e = new InvalidOperationException(
          $"AutoNode: Node at '{path}' does not exist in either the real or " + $"fake subtree for '{node.Name}' member '{name}' of type " +
          $"'{propertyOrField.Type.Name}'."
        );
        GD.PushError(e.Message);
        throw e;
      }

      // see if the unchecked node satisfies the expected type of node from the
      // property type
      _checker.Value = child;
      var originalNodeSatisfiesType =
        autoNode.GetScriptPropertyOrFieldType(name, _checker);

      if (originalNodeSatisfiesType) {
        // Property expected a vanilla Godot node type and it matched, so we
        // set it and leave.
        autoNode.SetScriptPropertyOrField(name, child);
        continue;
      }

      // Plain Godot node type wasn't expected, so we need to check if the
      // property was expecting a Godot node interface type.
      //
      // Check to see if the node needs to be adapted to satisfy an
      // expected interface type.
      var adaptedChild = GodotInterfaces.AdaptNode(child);
      _checker.Value = adaptedChild;
      var adaptedChildSatisfiesType =
        autoNode.GetScriptPropertyOrFieldType(name, _checker);

      if (adaptedChildSatisfiesType) {
        autoNode.SetScriptPropertyOrField(name, adaptedChild);
        continue;
      }

      // Tell user we can't connect the node to the property.
      e = new InvalidOperationException(
        $"Node at '{path}' of type '{child.GetType().Name}' does not " +
        $"satisfy the expected type '{propertyOrField.Type.Name}' for " +
        $"member '{name}' on '{node.Name}'."
      );
      GD.PushError(e.Message);
      throw e;
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
    Span<char> output = stackalloc char[span.Length + 1];
    var outputIndex = 1;

    output[0] = '%';

    for (var i = 1; i < span.Length + 1; i++) {
      var c = span[i - 1];

      if (c == '_') { continue; }

      output[outputIndex++] = i == 1 || span[i - 2] == '_'
        ? (char)(c & 0xDF)
        : c;
    }

    return new string(output[..outputIndex]);
  }
}

#pragma warning restore CA1061, CS8766
