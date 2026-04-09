using System;
using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.AVL;

public class AvlTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, AvlNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override AvlNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);

    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode)
    {
        AvlNode<TKey, TValue>? current = newNode;
        while (current != null)
        {
            current = Balance(current);
            current = current.Parent;
        }
    }

    protected override void OnNodeRemoved( AvlNode<TKey, TValue> logicallyRemovedNode, AvlNode<TKey, TValue> physicallyRemovedNode, AvlNode<TKey, TValue>? replacementNode, AvlNode<TKey, TValue>? replacementParent)
    {
        AvlNode<TKey, TValue>? current = replacementParent;
        while (current != null)
        {
            current = Balance(current);
            current = current.Parent;
        }
    }

    private int GetHeight(AvlNode<TKey, TValue>? node) => node?.Height ?? 0;

    private int GetBalance(AvlNode<TKey, TValue>? node) 
        => node == null ? 0 : GetHeight(node.Left) - GetHeight(node.Right);

    private void UpdateHeight(AvlNode<TKey, TValue> node)
    {
        node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));
    }

    private AvlNode<TKey, TValue> Balance(AvlNode<TKey, TValue> node)
    {
        UpdateHeight(node);
        int balance = GetBalance(node);

        if (balance > 1)
        {
            if (GetBalance(node.Left) < 0)
            {
                var left = node.Left!;
                RotateLeft(left);
                UpdateHeight(left);
                UpdateHeight(left.Parent!);
            }
            RotateRight(node);
            UpdateHeight(node);
            UpdateHeight(node.Parent!);
            
            return node.Parent!;
        }
        else if (balance < -1)
        {
            if (GetBalance(node.Right) > 0)
            {
                var right = node.Right!;
                RotateRight(right);
                UpdateHeight(right);
                UpdateHeight(right.Parent!);
            }
            RotateLeft(node);
            UpdateHeight(node);
            UpdateHeight(node.Parent!);
            
            return node.Parent!;
        }

        return node;
    }
}