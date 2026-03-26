using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Implementations.BST;

namespace TreeDataStructures.Implementations.Splay;

public class SplayTree<TKey, TValue> : BinarySearchTree<TKey, TValue>
    where TKey : IComparable<TKey>
{
    protected override BstNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);
    
    protected override void OnNodeAdded(BstNode<TKey, TValue> newNode)
    {
        BstNode<TKey, TValue> node = newNode;
        
        while (node.Parent != null)
        {
            BstNode<TKey, TValue> parent = node.Parent;
            BstNode<TKey, TValue>? grandparent = parent.Parent;
            
            if (grandparent == null)
            {
                if (node.IsLeftChild)
                    RotateRight(parent);
                else
                    RotateLeft(parent);
            }
            else if (node.IsLeftChild && parent.IsLeftChild)
            {
                // (LL)
                RotateRight(grandparent);
                RotateRight(parent);
            }
            else if (node.IsRightChild && parent.IsRightChild)
            {
                // (RR)
                RotateLeft(grandparent);
                RotateLeft(parent);
            }
            else if (node.IsRightChild && parent.IsLeftChild)
            {
                // (LR)
                RotateLeft(parent);
                RotateRight(grandparent);
            }
            else if (node.IsLeftChild && parent.IsRightChild)
            {
                // (RL)
                RotateRight(parent);
                RotateLeft(grandparent);
            }
        }
        
        Root = node;
    }
    
    protected override void OnNodeRemoved(BstNode<TKey, TValue>? parent, BstNode<TKey, TValue>? child)
    {
        if (child != null)
        {
            BstNode<TKey, TValue> node = child;
            
            while (node.Parent != null)
            {
                BstNode<TKey, TValue> currentParent = node.Parent;
                BstNode<TKey, TValue>? grandparent = currentParent.Parent;
                
                if (grandparent == null)
                {
                    if (node.IsLeftChild)
                        RotateRight(currentParent);
                    else
                        RotateLeft(currentParent);
                }
                else if (node.IsLeftChild && currentParent.IsLeftChild)
                {
                    //(LL)
                    RotateRight(grandparent);
                    RotateRight(currentParent);
                }
                else if (node.IsRightChild && currentParent.IsRightChild)
                {
                    //(RR)
                    RotateLeft(grandparent);
                    RotateLeft(currentParent);
                }
                else if (node.IsRightChild && currentParent.IsLeftChild)
                {
                    //(LR)
                    RotateLeft(currentParent);
                    RotateRight(grandparent);
                }
                else if (node.IsLeftChild && currentParent.IsRightChild)
                {
                    //(RL)
                    RotateRight(currentParent);
                    RotateLeft(grandparent);
                }
            }
            
            Root = node;
        }
        else if (parent != null)
        {
            BstNode<TKey, TValue> node = parent;
            
            while (node.Parent != null)
            {
                BstNode<TKey, TValue> currentParent = node.Parent;
                BstNode<TKey, TValue>? grandparent = currentParent.Parent;
                
                if (grandparent == null)
                {
                    if (node.IsLeftChild)
                        RotateRight(currentParent);
                    else
                        RotateLeft(currentParent);
                }
                else if (node.IsLeftChild && currentParent.IsLeftChild)
                {
                    // (LL)
                    RotateRight(grandparent);
                    RotateRight(currentParent);
                }
                else if (node.IsRightChild && currentParent.IsRightChild)
                {
                    // (RR)
                    RotateLeft(grandparent);
                    RotateLeft(currentParent);
                }
                else if (node.IsRightChild && currentParent.IsLeftChild)
                {
                    // (LR)
                    RotateLeft(currentParent);
                    RotateRight(grandparent);
                }
                else if (node.IsLeftChild && currentParent.IsRightChild)
                {
                    //(RL)
                    RotateRight(currentParent);
                    RotateLeft(grandparent);
                }
            }
            
            Root = node;
        }
    }
    
    public override bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var node = FindNode(key);
        if (node != null)
        {
            value = node.Value;

            BstNode<TKey, TValue> currentNode = node;
            
            while (currentNode.Parent != null)
            {
                BstNode<TKey, TValue> parent = currentNode.Parent;
                BstNode<TKey, TValue>? grandparent = parent.Parent;
                
                if (grandparent == null)
                {

                    if (currentNode.IsLeftChild)
                        RotateRight(parent);
                    else
                        RotateLeft(parent);
                }
                else if (currentNode.IsLeftChild && parent.IsLeftChild)
                {
                    //(LL)
                    RotateRight(grandparent);
                    RotateRight(parent);
                }
                else if (currentNode.IsRightChild && parent.IsRightChild)
                {
                    //(RR)
                    RotateLeft(grandparent);
                    RotateLeft(parent);
                }
                else if (currentNode.IsRightChild && parent.IsLeftChild)
                {
                    //(LR)
                    RotateLeft(parent);
                    RotateRight(grandparent);
                }
                else if (currentNode.IsLeftChild && parent.IsRightChild)
                {
                    //(RL)
                    RotateRight(parent);
                    RotateLeft(grandparent);
                }
            }
            
            Root = currentNode;
            return true;
        }
        value = default;
        return false;
    }
    
}
