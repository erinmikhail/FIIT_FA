using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Interfaces;
using TreeDataStructures.Implementations.AVL;

namespace TreeDataStructures.Core;

public enum TraversalStrategy 
{ 
    InOrder, 
    PreOrder, 
    PostOrder, 
    InOrderReverse, 
    PreOrderReverse, 
    PostOrderReverse 
}

public abstract class BinarySearchTreeBase<TKey, TValue, TNode>(IComparer<TKey>? comparer = null) 
    : ITree<TKey, TValue>
    where TNode : Node<TKey, TValue, TNode>
{
    protected TNode? Root;
    public IComparer<TKey> Comparer { get; protected set; } = comparer ?? Comparer<TKey>.Default;

    public int Count { get; protected set; }
    
    public bool IsReadOnly => false;

    public ICollection<TKey> Keys
    {
        get
        { 
            var keys = new List<TKey>(Count);
            foreach (var entry in InOrder())
            {
                keys.Add(entry.Key);
            }
            return keys;
        }
    }
    
    public ICollection<TValue> Values
    { 
        get
        {
            var values = new List<TValue>(Count);
            foreach (var entry in InOrder())
            {
                values.Add(entry.Value);
            }
            return values;
        }
    }
    
    public virtual void Add(TKey key, TValue value)
    {
        if (Root == null)
        {
            Root = CreateNode(key, value);
            Count++;
            OnNodeAdded(Root);
            return;
        }

        TNode current = Root;
        TNode? parent = null;
        int cmp = 0;

        while (current != null)
        {
            parent = current;
            cmp = Comparer.Compare(key, current.Key);
            
            if (cmp == 0)
            {
                current.Value = value;
                return;
            }
            
            current = cmp < 0 ? current.Left : current.Right;
        }

        TNode newNode = CreateNode(key, value);
        newNode.Parent = parent;
        
        if (cmp < 0)
            parent!.Left = newNode;
        else
            parent!.Right = newNode;
        
        Count++;
        OnNodeAdded(newNode);
    }

    public virtual bool Remove(TKey key)
    {
        TNode? node = FindNode(key);
        if (node == null) return false;

        RemoveNode(node);
        Count--;
        return true;
    }
    
    protected virtual void RemoveNode(TNode node)
    {
        TNode physicallyRemovedNode = node;
        TNode? x = null;
        TNode? xParent = null;

        if (node.Left == null)
        {
            x = node.Right;
            xParent = node.Parent;
            Transplant(node, node.Right);
        }
        else if (node.Right == null)
        {
            x = node.Left;
            xParent = node.Parent;
            Transplant(node, node.Left);
        }
        else
        {
            TNode successor = node.Right;
            while (successor.Left != null)
            {
                successor = successor.Left;
            }

            physicallyRemovedNode = successor;
            x = successor.Right;

            if (successor.Parent == node)
            {
                xParent = successor;
            }
            else
            {
                xParent = successor.Parent;
                Transplant(successor, successor.Right);
                successor.Right = node.Right;
                if (successor.Right != null) successor.Right.Parent = successor;
            }

            Transplant(node, successor);
            successor.Left = node.Left;
            if (successor.Left != null) successor.Left.Parent = successor;
        }
        OnNodeRemoved(node, physicallyRemovedNode, x, xParent);
    }

    public virtual bool ContainsKey(TKey key) => FindNode(key) != null;
    
    public virtual bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        TNode? node = FindNode(key);
        if (node != null)
        {
            value = node.Value;
            return true;
        }
        value = default;
        return false;
    }

    public TValue this[TKey key]
    {
        get => TryGetValue(key, out TValue? val) ? val!
         : throw new KeyNotFoundException();
        set => Add(key, value);
    }

    #region Hooks
    
    protected virtual void OnNodeAdded(TNode newNode) { }
    protected virtual void OnNodesSwapped(TNode oldNode, TNode newNode) { }
    protected virtual void OnNodeRemoved( TNode logicallyRemovedNode, TNode physicallyRemovedNode, TNode? replacementNode, TNode? replacementParent) { }
    
    
    #endregion

    #region Helpers

    protected abstract TNode CreateNode(TKey key, TValue value);
    
    protected TNode? FindNode(TKey key)
    {
        TNode? current = Root;
        while (current != null)
        {
            int cmp = Comparer.Compare(key, current.Key);
            if (cmp == 0) return current;
            current = cmp < 0 ? current.Left : current.Right;
        }
        return null;
    }

    protected void RotateLeft(TNode x)
    {
        TNode? y = x.Right;
        if (y == null) return;
        
        x.Right = y.Left;
        if (y.Left != null) y.Left.Parent = x;
        
        y.Parent = x.Parent;
        
        if (x.Parent == null) Root = y;
        else if (x.IsLeftChild) x.Parent.Left = y;
        else x.Parent.Right = y;
        
        y.Left = x;
        x.Parent = y;
    }

    protected void RotateRight(TNode y)
    {
        TNode? x = y.Left;
        if (x == null) return;
        
        y.Left = x.Right;
        if (x.Right != null) x.Right.Parent = y;
        
        x.Parent = y.Parent;
        
        if (y.Parent == null) Root = x;
        else if (y.IsLeftChild) y.Parent.Left = x;
        else y.Parent.Right = x;
        
        x.Right = y;
        y.Parent = x;
    }
    
    protected void RotateBigLeft(TNode x)
    {
        RotateRight(x.Right!);
        RotateLeft(x);
    }
    
    protected void RotateBigRight(TNode y)
    {
        RotateLeft(y.Left!);
        RotateRight(y);
    }
    
    protected void RotateDoubleLeft(TNode x) => RotateBigLeft(x);
    protected void RotateDoubleRight(TNode y) => RotateBigRight(y);
    
    protected void Transplant(TNode u, TNode? v)
    {
        if (u.Parent == null) Root = v;
        else if (u.IsLeftChild) u.Parent.Left = v;
        else u.Parent.Right = v;
        
        if (v != null) v.Parent = u.Parent;
    }
    
    #endregion

    #region Traversals & Iterators

    public IEnumerable<TreeEntry<TKey, TValue>> InOrder() => new TreeEnumerable(Root, TraversalStrategy.InOrder);
    public IEnumerable<TreeEntry<TKey, TValue>> PreOrder() => new TreeEnumerable(Root, TraversalStrategy.PreOrder);
    public IEnumerable<TreeEntry<TKey, TValue>> PostOrder() => new TreeEnumerable(Root, TraversalStrategy.PostOrder);
    public IEnumerable<TreeEntry<TKey, TValue>> InOrderReverse() => new TreeEnumerable(Root, TraversalStrategy.InOrderReverse);
    public IEnumerable<TreeEntry<TKey, TValue>> PreOrderReverse() => new TreeEnumerable(Root, TraversalStrategy.PreOrderReverse);
    public IEnumerable<TreeEntry<TKey, TValue>> PostOrderReverse() => new TreeEnumerable(Root, TraversalStrategy.PostOrderReverse);

    private class TreeEnumerable(TNode? root, TraversalStrategy strategy) : IEnumerable<TreeEntry<TKey, TValue>>
    {
        public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => new TreeIterator(root, strategy);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private class TreeIterator : IEnumerator<TreeEntry<TKey, TValue>>
    {
        private readonly TraversalStrategy _strategy;
        private readonly TNode? _root;
        private readonly Stack<TNode> _stack;
        
        private TNode? _current;
        private TNode? _lastVisited;
        private TreeEntry<TKey, TValue> _currentEntry;

        public TreeIterator(TNode? root, TraversalStrategy strategy)
        {
            _strategy = strategy;
            _root = root;
            _stack = new Stack<TNode>();
            Reset();
        }

        public TreeEntry<TKey, TValue> Current => _currentEntry;
        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (_root == null) return false;

            return _strategy switch
            {
                TraversalStrategy.InOrder => MoveNextInOrder(),
                TraversalStrategy.PreOrder => MoveNextPreOrder(),
                TraversalStrategy.PostOrder => MoveNextPostOrder(),
                TraversalStrategy.InOrderReverse => MoveNextInOrderReverse(),
                TraversalStrategy.PreOrderReverse => MoveNextPreOrderReverse(),
                TraversalStrategy.PostOrderReverse => MoveNextPostOrderReverse(),
                _ => throw new NotImplementedException()
            };
        }

        private bool MoveNextInOrder()
        {
            if (_current != null || _stack.Count > 0)
            {
                while (_current != null)
                {
                    _stack.Push(_current);
                    _current = _current.Left;
                }

                _current = _stack.Pop();
                _currentEntry = CreateEntry(_current);
                _current = _current.Right;
                return true;
            }
            return false;
        }

        private bool MoveNextPreOrder()
        {
            if (_stack.Count > 0)
            {
                TNode node = _stack.Pop();
                _currentEntry = CreateEntry(node);

                if (node.Right != null) _stack.Push(node.Right);
                if (node.Left != null) _stack.Push(node.Left);
                
                return true;
            }
            return false;
        }

        private bool MoveNextPostOrder()
        {
            while (_current != null || _stack.Count > 0)
            {
                if (_current != null)
                {
                    _stack.Push(_current);
                    _current = _current.Left;
                }
                else
                {
                    TNode peekNode = _stack.Peek();
                    if (peekNode.Right != null && _lastVisited != peekNode.Right)
                    {
                        _current = peekNode.Right;
                    }
                    else
                    {
                        _currentEntry = CreateEntry(peekNode);
                        _lastVisited = _stack.Pop();
                        return true;
                    }
                }
            }
            return false;
        }

        private bool MoveNextInOrderReverse()
        {
            if (_current != null || _stack.Count > 0)
            {
                while (_current != null)
                {
                    _stack.Push(_current);
                    _current = _current.Right;
                }

                _current = _stack.Pop();
                _currentEntry = CreateEntry(_current);
                _current = _current.Left;
                return true;
            }
            return false;
        }

        private bool MoveNextPreOrderReverse()
        {
            while (_current != null || _stack.Count > 0)
            {
                if (_current != null)
                {
                    _stack.Push(_current);
                    _current = _current.Right;
                }
                else
                {
                    TNode peekNode = _stack.Peek();
                    if (peekNode.Left != null && _lastVisited != peekNode.Left)
                    {
                        _current = peekNode.Left;
                    }
                    else
                    {
                        _currentEntry = CreateEntry(peekNode);
                        _lastVisited = _stack.Pop();
                        return true;
                    }
                }
            }
            return false;
        }

        private bool MoveNextPostOrderReverse()
        {
            if (_stack.Count > 0)
            {
                TNode node = _stack.Pop();
                _currentEntry = CreateEntry(node);

                if (node.Left != null) _stack.Push(node.Left);
                if (node.Right != null) _stack.Push(node.Right);
                
                return true;
            }
            return false;
        }

        private TreeEntry<TKey, TValue> CreateEntry(TNode node)
        {
            int height = node is AvlNode<TKey, TValue> avlNode ? avlNode.Height : 0;
            return new TreeEntry<TKey, TValue>(node.Key, node.Value, height);
        }

        public void Reset()
        {
            _stack.Clear();
            _current = _root;
            _lastVisited = null;
            _currentEntry = default!;

            if (_root != null && (_strategy == TraversalStrategy.PreOrder || _strategy == TraversalStrategy.PostOrderReverse))
            {
                _stack.Push(_root);
                _current = null; 
            }
        }

        public void Dispose()
        {
            _stack.Clear();
        }
    }
    #endregion

    #region ICollection / IDictionary Implementations

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new DictEnumerator(new TreeIterator(Root, TraversalStrategy.InOrder));
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private class DictEnumerator(TreeIterator iterator) : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        public bool MoveNext() => iterator.MoveNext();
        public KeyValuePair<TKey, TValue> Current => new(iterator.Current.Key, iterator.Current.Value);
        object IEnumerator.Current => Current;
        public void Reset() => iterator.Reset();
        public void Dispose() => iterator.Dispose();
    }

    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public void Clear() { Root = null; Count = 0; }
    public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    { 
        if (array == null) throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < Count) throw new ArgumentException("Недостаточно места в массиве");
        
        int index = arrayIndex;
        var iterator = new TreeIterator(Root, TraversalStrategy.InOrder);
        while (iterator.MoveNext())
        {
            array[index++] = new KeyValuePair<TKey, TValue>(iterator.Current.Key, iterator.Current.Value);
        }
    }
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
    
    #endregion
}