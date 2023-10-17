#pragma warning disable
namespace Chickensoft.PowerUps;


using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.PowerUps;
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

  public void AddChild(
    object node, bool forceReadableName = false, InternalMode @internal = InternalMode.Disabled
  ) {
    if (node is INodeAdapter adapter) {
      // If it's an adapter, we can add the underlying node directly.
      base.AddChild(adapter.Object, forceReadableName, @internal);
      return;
    }

    if (node is Node godotNode) {
      // If it's a Godot node, we can add it directly.
      base.AddChild(godotNode, forceReadableName, @internal);
      return;
    }

    if (node is INode iNode) {
      // We can only add nodes by interface only when we are in a test
      // environment, so check to see if that's been setup.
      if (FakeNodes is not FakeNodeTree fakeNodeTree) {
        throw new InvalidOperationException(
          "Fake node tree has not been initialized. If you are attempting to " +
          "unit test a node scene, make sure that you have called " +
          "node.FakeNodeTree() to initialize the fake node tree."
        );
      }

      // We are running in a test environment.
      fakeNodeTree.AddChild(iNode);
      return;
    }

    throw new InvalidOperationException(
      $"Cannot add child of type {node.GetType()} to {GetType()}."
    );
  }

  public new INode? FindChild(string pattern, bool recursive, bool owned) {
    if (FakeNodes is FakeNodeTree fakeNodeTree) {
      return fakeNodeTree.FindChild(pattern);
    }
    var node = base.FindChild(pattern, recursive, owned);
    return node is null ? null : (GodotInterfaces.AdaptNode(node));
  }

  public new INode[] FindChildren(
    string pattern, string type = "", bool recursive = true, bool owned = true
  ) {
    if (FakeNodes is FakeNodeTree fakeNodeTree) {
      return fakeNodeTree.FindChildren(pattern);
    }
    var nodes = base.FindChildren(pattern, type, recursive, owned);
    var adaptedNodes = new System.Collections.Generic.List<INode>(nodes.Count);
    foreach (var node in nodes) {
      adaptedNodes.Add(GodotInterfaces.AdaptNode(node));
    }
    return adaptedNodes.ToArray();
  }

  public new T GetChild<T>(int idx, bool includeInternal = false) where T : class, INode {
    if (FakeNodes is FakeNodeTree fakeNodeTree) {
      return fakeNodeTree.GetChild<T>(idx);
    }
    var node = GetChild(idx, includeInternal);
    return GodotInterfaces.Adapt<T>(node);
  }

  public new int GetChildCount(bool includeInternal = false) =>
    FakeNodes is FakeNodeTree fakeNodeTree
      ? fakeNodeTree.GetChildCount()
      : GetChildCount(includeInternal);

  public new INode[] GetChildren(bool includeInternal = false) {
    if (FakeNodes is FakeNodeTree fakeNodeTree) {
      return fakeNodeTree.GetChildren();
    }
    var nodes = base.GetChildren(includeInternal);
    var adaptedNodes = new System.Collections.Generic.List<INode>(nodes.Count);
    foreach (var node in nodes) {
      adaptedNodes.Add(GodotInterfaces.AdaptNode(node));
    }
    return adaptedNodes.ToArray();
  }

  public new INode GetNode(NodePath path) {
    if (FakeNodes is FakeNodeTree fakeNodeTree) {
      return fakeNodeTree.GetNode(path);
    }
    var node = base.GetNode(path);
    return node is null ? default! : GodotInterfaces.AdaptNode(node);
  }

  public new INode? GetNodeOrNull(NodePath path) {
    if (FakeNodes is FakeNodeTree fakeNodeTree) {
      return fakeNodeTree.GetNode(path);
    }
    var node = base.GetNodeOrNull(path);
    return node is null
      ? null
      : (INode)GodotInterfaces.AdaptNode(node);
  }

  public new bool HasNode(NodePath path) =>
    FakeNodes is FakeNodeTree fakeNodeTree
      ? fakeNodeTree.HasNode(path)
      : base.HasNode(path);

  public void RemoveChild(object node) {
    if (node is INodeAdapter adapter) {
      // If it's an adapter, we can remove the underlying node directly.
      base.RemoveChild(adapter.Object);
      return;
    }

    if (node is Node godotNode) {
      // If it's a Godot node, we can remove it directly.
      base.RemoveChild(godotNode);
      return;
    }

    if (node is INode iNode) {
      // We can remove nodes by interface only when we are in a test
      // environment, so check to see if that's been setup.
      if (FakeNodes is not FakeNodeTree fakeNodeTree) {
        throw new InvalidOperationException(
          "Fake node tree has not been initialized. If you are attempting to " +
          "unit test a node scene, make sure that you have called " +
          "node.FakeNodeTree() to initialize the fake node tree."
        );
      }

      // We are running in a test environment.
      fakeNodeTree.RemoveChild(iNode);
      return;
    }

    throw new InvalidOperationException(
      $"Cannot remove child of type {node.GetType()} from {GetType()}."
    );
  }

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

  private static TypeChecker _checker = new();

  public static void ConnectNodes(
      ImmutableDictionary<string, ScriptPropertyOrField> propertiesAndFields,
      IAutoNode autoNode
    ) {
    var godotNode = (Node)autoNode;
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

      var originalNode = godotNode.GetNode(path);

      // see if the unchecked node satisfies the expected type of node from the property type
      _checker.Value = originalNode;
      var originalNodeSatisfiesType =
        autoNode.GetScriptPropertyOrFieldType(name, _checker);

      if (originalNode is null) {
        throw new InvalidOperationException(
          $"Node {path} does not exist in scene tree for property {name} of " +
          $"type {propertyOrField.Type} on {godotNode.Name}."
        );
      }

      if (originalNodeSatisfiesType) {
        // Property expected a vanilla Godot node type and it matched, so we
        // set it and leave.
        autoNode.SetScriptPropertyOrField(name, originalNode);
        continue;
      }

      // Plain Godot node type wasn't expected, so we need to check if the
      // property was expecting a Godot node interface type.

      // check to see if the node needs to be adapted to satisfy an
      // expected interface type.
      var adaptedNode = GodotInterfaces.AdaptNode(originalNode);
      _checker.Value = adaptedNode;
      var adaptedNodeSatisfiesType =
        autoNode.GetScriptPropertyOrFieldType(name, _checker);

      // If the adapted node does not satisfy the expected interface/adapter
      // node type, then we can't connect the node to the property.
      if (!adaptedNodeSatisfiesType) {
        // Tell user we can't connect the node to the property.
        throw new InvalidOperationException(
          $"Node {path} of type {originalNode.GetType()} does not satisfy " +
          $"the expected type {propertyOrField.Type} for property {name} on " +
          $"{godotNode.Name}."
        );
      }

      // Otherwise, the adapted node satisfies the expected adapter or interface
      // type, so we can be done.
      autoNode.SetScriptPropertyOrField(name, adaptedNode);
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

#pragma warning restore
