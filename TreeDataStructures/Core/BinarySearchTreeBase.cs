using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Interfaces;

namespace TreeDataStructures.Core;

public abstract class BinarySearchTreeBase<TKey, TValue, TNode>(IComparer<TKey>? comparer = null) 
    : ITree<TKey, TValue>
    where TNode : Node<TKey, TValue, TNode>
{
    protected TNode? Root;
    public IComparer<TKey> Comparer { get; protected set; } = comparer ?? Comparer<TKey>.Default; // use it to compare Keys

    public int Count { get; protected set; }
    
    public bool IsReadOnly => false;

    public ICollection<TKey> Keys
    {
        get
        { 
            var keys = new List<TKey>(Count);
            var iterator = new TreeIterator(Root, TraversalStrategy.InOrder);
            foreach (var entry in iterator)
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
            var iterator = new TreeIterator(Root, TraversalStrategy.InOrder);
            foreach (var entry in iterator)
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
        if (node == null) { return false; }

        RemoveNode(node);
        this.Count--;
        return true;
    }
    
    
    protected virtual void RemoveNode(TNode node)
    {
        if (node.Left == null)
        {
            Transplant(node, node.Right);
            OnNodeRemoved(node.Parent, node.Right);
        }
        else if (node.Right == null)
        {
            Transplant(node, node.Left);
            OnNodeRemoved(node.Parent, node.Left);
        }
        else
        {
            //мин узел в правом поддереве
            TNode successor = node.Right;
            while (successor.Left != null)
            {
                successor = successor.Left;
            }
            
            if (successor.Parent != node)
            {
                Transplant(successor, successor.Right);
                successor.Right = node.Right;
                successor.Right.Parent = successor;
            }
            
            Transplant(node, successor);
            successor.Left = node.Left;
            successor.Left.Parent = successor;
            
            OnNodeRemoved(successor.Parent, successor);
        }
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
        get => TryGetValue(key, out TValue? val) ? val : throw new KeyNotFoundException();
        set => Add(key, value);
    }

    
    #region Hooks
    
    /// <summary>
    /// Вызывается после успешной вставки
    /// </summary>
    /// <param name="newNode">Узел, который встал на место</param>
    protected virtual void OnNodeAdded(TNode newNode) { }
    
    /// <summary>
    /// Вызывается после удаления. 
    /// </summary>
    /// <param name="parent">Узел, чей ребенок изменился</param>
    /// <param name="child">Узел, который встал на место удаленного</param>
    protected virtual void OnNodeRemoved(TNode? parent, TNode? child) { }
    
    #endregion
    
    
    #region Helpers
    protected abstract TNode CreateNode(TKey key, TValue value);
    
    
    protected TNode? FindNode(TKey key)
    {
        TNode? current = Root;
        while (current != null)
        {
            int cmp = Comparer.Compare(key, current.Key);
            if (cmp == 0) { return current; }
            current = cmp < 0 ? current.Left : current.Right;
        }
        return null;
    }

    protected void RotateLeft(TNode x)
    {
        TNode? y = x.Right;
        if (y == null) return;
        
        x.Right = y.Left;
        if (y.Left != null)
            y.Left.Parent = x;
        
        y.Parent = x.Parent;
        
        if (x.Parent == null)
            Root = y;
        else if (x.IsLeftChild)
            x.Parent.Left = y;
        else
            x.Parent.Right = y;
        
        y.Left = x;
        x.Parent = y;
    }

    protected void RotateRight(TNode y)
    {
        TNode? x = y.Left;
        if (x == null) return;
        
        y.Left = x.Right;
        if (x.Right != null)
            x.Right.Parent = y;
        
        x.Parent = y.Parent;
        
        if (y.Parent == null)
            Root = x;
        else if (y.IsLeftChild)
            y.Parent.Left = x;
        else
            y.Parent.Right = x;
        
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
    
    protected void RotateDoubleLeft(TNode x)
    {
        RotateLeft(x);
        RotateLeft(x);
    }
    
    protected void RotateDoubleRight(TNode y)
    {
        RotateRight(y);
        RotateRight(y);
    }
    
    protected void Transplant(TNode u, TNode? v)
    {
        if (u.Parent == null)
        {
            Root = v;
        }
        else if (u.IsLeftChild)
        {
            u.Parent.Left = v;
        }
        else
        {
            u.Parent.Right = v;
        }
        v?.Parent = u.Parent;
    }
    #endregion
    
    public IEnumerable<TreeEntry<TKey, TValue>>  InOrder() => InOrderTraversal(Root);
    
    private IEnumerable<TreeEntry<TKey, TValue>>  InOrderTraversal(TNode? node)
    {
        if (node == null) {  yield break; }
        var stack = new Stack<TNode>();
        TNode? current = node;
        
        while (current != null || stack.Count > 0)
        {
            while (current != null)
            {
                stack.Push(current);
                current = current.Left;
            }
            
            current = stack.Pop();
            yield return new TreeEntry<TKey, TValue>(current.Key, current.Value, GetNodeHeight(current));
            current = current.Right;
        }
    }
    
    public IEnumerable<TreeEntry<TKey, TValue>>  PreOrder() => PreOrderTraversal(Root);
    private IEnumerable<TreeEntry<TKey, TValue>> PreOrderTraversal(TNode? node)
    {
        if (node == null) yield break;
        
        var stack = new Stack<TNode>();
        stack.Push(node);
        
        while (stack.Count > 0)
        {
            TNode current = stack.Pop();
            yield return new TreeEntry<TKey, TValue>(current.Key, current.Value, GetNodeHeight(current));
            
            if (current.Right != null)
                stack.Push(current.Right);
            if (current.Left != null)
                stack.Push(current.Left);
        }
    }
    public IEnumerable<TreeEntry<TKey, TValue>> PostOrder() => PostOrderTraversal(Root);
    private IEnumerable<TreeEntry<TKey, TValue>> PostOrderTraversal(TNode? node)
    {
        if (node == null) yield break;
        
        var stack1 = new Stack<TNode>();
        var stack2 = new Stack<TNode>();
        stack1.Push(node);
        
        while (stack1.Count > 0)
        {
            TNode current = stack1.Pop();
            stack2.Push(current);
            
            if (current.Left != null)
                stack1.Push(current.Left);
            if (current.Right != null)
                stack1.Push(current.Right);
        }
        
        while (stack2.Count > 0)
        {
            TNode current = stack2.Pop();
            yield return new TreeEntry<TKey, TValue>(current.Key, current.Value, GetNodeHeight(current));
        }
    }
    public IEnumerable<TreeEntry<TKey, TValue>> InOrderReverse() => InOrderReverseTraversal(Root);
    private IEnumerable<TreeEntry<TKey, TValue>> InOrderReverseTraversal(TNode? node)
    {
        if (node == null) yield break;
        
        var stack = new Stack<TNode>();
        TNode? current = node;
        
        while (current != null || stack.Count > 0)
        {
            while (current != null)
            {
                stack.Push(current);
                current = current.Right;
            }
            
            current = stack.Pop();
            yield return new TreeEntry<TKey, TValue>(current.Key, current.Value, GetNodeHeight(current));
            current = current.Left;
        }
    }
    public IEnumerable<TreeEntry<TKey, TValue>> PreOrderReverse() => PreOrderReverseTraversal(Root);
    private IEnumerable<TreeEntry<TKey, TValue>> PreOrderReverseTraversal(TNode? node)
    {
        if (node == null) yield break;
        
        var stack = new Stack<TNode>();
        stack.Push(node);
        
        while (stack.Count > 0)
        {
            TNode current = stack.Pop();
            yield return new TreeEntry<TKey, TValue>(current.Key, current.Value, GetNodeHeight(current));
            
            if (current.Left != null)
                stack.Push(current.Left);
            if (current.Right != null)
                stack.Push(current.Right);
        }
    }
    public IEnumerable<TreeEntry<TKey, TValue>> PostOrderReverse() => PostOrderReverseTraversal(Root);
    private IEnumerable<TreeEntry<TKey, TValue>> PostOrderReverseTraversal(TNode? node)
    {
        if (node == null) yield break;
        
        var stack1 = new Stack<TNode>();
        var stack2 = new Stack<TNode>();
        stack1.Push(node);
        
        while (stack1.Count > 0)
        {
            TNode current = stack1.Pop();
            stack2.Push(current);
            
            if (current.Right != null)
                stack1.Push(current.Right);
            if (current.Left != null)
                stack1.Push(current.Left);
        }
        
        while (stack2.Count > 0)
        {
            TNode current = stack2.Pop();
            yield return new TreeEntry<TKey, TValue>(current.Key, current.Value, GetNodeHeight(current));
        }
    }
    
    private int GetNodeHeight(TNode node)
    {
        if (node is AvlNode<TKey, TValue> avlNode)
            return avlNode.Height;
        return 0;
    }
    
    /// <summary>
    /// Внутренний класс-итератор. 
    /// Реализует паттерн Iterator вручную, без yield return (ban).
    /// </summary>
    private struct TreeIterator :
        IEnumerable<TreeEntry<TKey, TValue>>,
        IEnumerator<TreeEntry<TKey, TValue>>
    {
        // probably add something here
        private readonly TraversalStrategy _strategy; // or make it template parameter?
        private TNode? _root;
        private Stack<TNode> _stack;
        private TNode? _current;
        private TreeEntry<TKey, TValue> _currentEntry;

        public TreeIterator(TNode? root, TraversalStrategy strategy)
        {
            _strategy = strategy;
            _root = root;
            _stack = new Stack<TNode>();
            _current = null;
            _currentEntry = default!;
            Reset();
        }

        public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;

        public TreeEntry<TKey, TValue> Current => _currentEntry;
        object IEnumerator.Current => Current;


        public bool MoveNext()
        {
            if (_strategy == TraversalStrategy.InOrder)
            {
                if (_stack.Count == 0 && _current == null)
                {
                    _current = _root;
                    while (_current != null)
                    {
                        _stack.Push(_current);
                        _current = _current.Left;
                    }
                }
                
                if (_stack.Count == 0)
                    return false;
                
                _current = _stack.Pop();
                _currentEntry = new TreeEntry<TKey, TValue>(_current!.Key, _current.Value, 0);
                
                _current = _current.Right;
                while (_current != null)
                {
                    _stack.Push(_current);
                    _current = _current.Left;
                }
                
                return true;
            }
            else if (_strategy == TraversalStrategy.PreOrder)
            {
                if (_stack.Count == 0 && _current == null)
                {
                    if (_root != null)
                        _stack.Push(_root);
                }
                
                if (_stack.Count == 0)
                    return false;
                
                _current = _stack.Pop();
                _currentEntry = new TreeEntry<TKey, TValue>(_current!.Key, _current.Value, 0);
                
                if (_current.Right != null)
                    _stack.Push(_current.Right);
                if (_current.Left != null)
                    _stack.Push(_current.Left);
                
                return true;
            }
            else if (_strategy == TraversalStrategy.PostOrder)
            {
                if (_stack.Count == 0 && _current == null)
                {
                    var tempStack = new Stack<TNode>();
                    if (_root != null)
                        tempStack.Push(_root);
                    
                    while (tempStack.Count > 0)
                    {
                        var node = tempStack.Pop();
                        _stack.Push(node);
                        if (node.Left != null)
                            tempStack.Push(node.Left);
                        if (node.Right != null)
                            tempStack.Push(node.Right);
                    }
                }
                
                if (_stack.Count == 0)
                    return false;
                
                _current = _stack.Pop();
                _currentEntry = new TreeEntry<TKey, TValue>(_current!.Key, _current.Value, 0);
                
                return true;
            }
            else if (_strategy == TraversalStrategy.InOrderReverse)
            {
                if (_stack.Count == 0 && _current == null)
                {
                    _current = _root;
                    while (_current != null)
                    {
                        _stack.Push(_current);
                        _current = _current.Right;
                    }
                }
                
                if (_stack.Count == 0)
                    return false;
                
                _current = _stack.Pop();
                _currentEntry = new TreeEntry<TKey, TValue>(_current!.Key, _current.Value, 0);
                
                _current = _current.Left;
                while (_current != null)
                {
                    _stack.Push(_current);
                    _current = _current.Right;
                }
                
                return true;
            }
            else if (_strategy == TraversalStrategy.PreOrderReverse)
            {
                if (_stack.Count == 0 && _current == null)
                {
                    if (_root != null)
                        _stack.Push(_root);
                }
                
                if (_stack.Count == 0)
                    return false;
                
                _current = _stack.Pop();
                _currentEntry = new TreeEntry<TKey, TValue>(_current!.Key, _current.Value, 0);
                
                if (_current.Left != null)
                    _stack.Push(_current.Left);
                if (_current.Right != null)
                    _stack.Push(_current.Right);
                
                return true;
            }
            else if (_strategy == TraversalStrategy.PostOrderReverse)
            {
                if (_stack.Count == 0 && _current == null)
                {
                    var tempStack = new Stack<TNode>();
                    if (_root != null)
                        tempStack.Push(_root);
                    
                    while (tempStack.Count > 0)
                    {
                        var node = tempStack.Pop();
                        _stack.Push(node);
                        if (node.Right != null)
                            tempStack.Push(node.Right);
                        if (node.Left != null)
                            tempStack.Push(node.Left);
                    }
                }
                
                if (_stack.Count == 0)
                    return false;
                
                _current = _stack.Pop();
                _currentEntry = new TreeEntry<TKey, TValue>(_current!.Key, _current.Value, 0);
                
                return true;
            }
            
            throw new NotImplementedException("Strategy not implemented");
        }

        public void Reset()
        {
            _stack.Clear();
            _current = null;
            _currentEntry = default!;
        }


        public void Dispose()
        {
            // TODO release managed resources here
            _stack.Clear();
        }
    }
    
    
    private enum TraversalStrategy { InOrder, PreOrder, PostOrder, InOrderReverse, PreOrderReverse, PostOrderReverse }
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        var iterator = new TreeIterator(Root, TraversalStrategy.InOrder);
        foreach (var entry in iterator)
        {
            yield return new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
        }
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public void Clear() { Root = null; Count = 0; }
    public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    { 
        if (array == null)
            throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < Count)
            throw new ArgumentException("Недостаточно места в массиве");
        
        int index = arrayIndex;
        var iterator = new TreeIterator(Root, TraversalStrategy.InOrder);
        foreach (var entry in iterator)
        {
            array[index++] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
        }
    }
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
}