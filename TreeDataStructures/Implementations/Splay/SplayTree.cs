using System;
using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Implementations.BST;

namespace TreeDataStructures.Implementations.Splay;

public class SplayTree<TKey, TValue> : BinarySearchTree<TKey, TValue>
    where TKey : IComparable<TKey>
{
    protected override BstNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);

    private void Splay(BstNode<TKey, TValue>? node)
    {
        if (node == null) return;

        while (node.Parent != null)
        {
            BstNode<TKey, TValue> parent = node.Parent;
            BstNode<TKey, TValue>? grandparent = parent.Parent;

            if (grandparent == null)
            {
                // Zig
                if (node.IsLeftChild)
                    RotateRight(parent);
                else
                    RotateLeft(parent);
            }
            else if (node.IsLeftChild && parent.IsLeftChild)
            {
                // Zig-Zig (LL)
                RotateRight(grandparent);
                RotateRight(parent);
            }
            else if (node.IsRightChild && parent.IsRightChild)
            {
                // Zig-Zig (RR)
                RotateLeft(grandparent);
                RotateLeft(parent);
            }
            else if (node.IsRightChild && parent.IsLeftChild)
            {
                // Zig-Zag (LR)
                RotateLeft(parent);
                RotateRight(grandparent);
            }
            else if (node.IsLeftChild && parent.IsRightChild)
            {
                // Zig-Zag (RL)
                RotateRight(parent);
                RotateLeft(grandparent);
            }
        }

        Root = node;
    }

    protected override void OnNodeAdded(BstNode<TKey, TValue> newNode)
    {
        Splay(newNode);
    }

    protected override void OnNodeRemoved( BstNode<TKey, TValue> logicallyRemovedNode, BstNode<TKey, TValue> physicallyRemovedNode, BstNode<TKey, TValue>? replacementNode, BstNode<TKey, TValue>? replacementParent)
    {
        if (replacementParent != null)
        {
            Splay(replacementParent);
        }
        else if (replacementNode != null)
        {
            Splay(replacementNode);
        }
    }

    public override bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var node = FindNode(key);
        if (node != null)
        {
            value = node.Value;
            Splay(node);
            return true;
        }
        value = default;
        return false;
    }

    public override bool ContainsKey(TKey key)
    {
        var node = FindNode(key);
        if (node != null)
        {
            Splay(node);
            return true;
        }
        return false;
    }
}