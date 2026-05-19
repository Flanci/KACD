using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MyCollections
{
    // Вспомогательный класс для хранения фиктивных значений в MyTreeMap
    internal class DummyObject
    {
        public static readonly DummyObject Instance = new DummyObject();
        private DummyObject() { }
    }

    internal enum Color
    {
        Red,
        Black
    }

    // Узел красно-чёрного дерева
    internal class RBTreeNode<K>
    {
        public K Key { get; set; }
        public Color Color { get; set; }
        public RBTreeNode<K> Left { get; set; }
        public RBTreeNode<K> Right { get; set; }
        public RBTreeNode<K> Parent { get; set; }

        public RBTreeNode(K key, Color color)
        {
            Key = key;
            Color = color;
            Left = null;
            Right = null;
            Parent = null;
        }

        public bool IsRed => Color == Color.Red;
        public bool IsBlack => Color == Color.Black;
    }

    public class MyTreeMap<K, V> where K : IComparable<K>
    {
        private RBTreeNode<K> _root;
        private int _size;
        private readonly IComparer<K> _comparer;

        public MyTreeMap() : this(Comparer<K>.Default) { }

        public MyTreeMap(IComparer<K> comparer)
        {
            _comparer = comparer ?? Comparer<K>.Default;
            _root = null;
            _size = 0;
        }

        public int Size => _size;
        public bool IsEmpty => _size == 0;

        // Вспомогательные методы для работы с деревом
        private int Compare(K key1, K key2) => _comparer.Compare(key1, key2);

        private bool IsRed(RBTreeNode<K> node) => node != null && node.IsRed;

        private RBTreeNode<K> RotateLeft(RBTreeNode<K> node)
        {
            var right = node.Right;
            node.Right = right.Left;
            if (right.Left != null)
                right.Left.Parent = node;
            right.Parent = node.Parent;
            if (node.Parent == null)
                _root = right;
            else if (node == node.Parent.Left)
                node.Parent.Left = right;
            else
                node.Parent.Right = right;
            right.Left = node;
            node.Parent = right;
            return right;
        }

        private RBTreeNode<K> RotateRight(RBTreeNode<K> node)
        {
            var left = node.Left;
            node.Left = left.Right;
            if (left.Right != null)
                left.Right.Parent = node;
            left.Parent = node.Parent;
            if (node.Parent == null)
                _root = left;
            else if (node == node.Parent.Left)
                node.Parent.Left = left;
            else
                node.Parent.Right = left;
            left.Right = node;
            node.Parent = left;
            return left;
        }

        private void FixInsert(RBTreeNode<K> node)
        {
            while (node != _root && IsRed(node.Parent))
            {
                if (node.Parent == node.Parent.Parent?.Left)
                {
                    var uncle = node.Parent.Parent.Right;
                    if (IsRed(uncle))
                    {
                        node.Parent.Color = Color.Black;
                        uncle.Color = Color.Black;
                        node.Parent.Parent.Color = Color.Red;
                        node = node.Parent.Parent;
                    }
                    else
                    {
                        if (node == node.Parent.Right)
                        {
                            node = node.Parent;
                            RotateLeft(node);
                        }
                        node.Parent.Color = Color.Black;
                        node.Parent.Parent.Color = Color.Red;
                        RotateRight(node.Parent.Parent);
                    }
                }
                else
                {
                    var uncle = node.Parent.Parent?.Left;
                    if (IsRed(uncle))
                    {
                        node.Parent.Color = Color.Black;
                        uncle.Color = Color.Black;
                        node.Parent.Parent.Color = Color.Red;
                        node = node.Parent.Parent;
                    }
                    else
                    {
                        if (node == node.Parent.Left)
                        {
                            node = node.Parent;
                            RotateRight(node);
                        }
                        node.Parent.Color = Color.Black;
                        node.Parent.Parent.Color = Color.Red;
                        RotateLeft(node.Parent.Parent);
                    }
                }
            }
            _root.Color = Color.Black;
        }

        public void Put(K key, V value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            RBTreeNode<K> parent = null;
            var current = _root;

            while (current != null)
            {
                parent = current;
                int cmp = Compare(key, current.Key);
                if (cmp < 0)
                    current = current.Left;
                else if (cmp > 0)
                    current = current.Right;
                else
                    return;
            }

            var newNode = new RBTreeNode<K>(key, Color.Red);
            newNode.Parent = parent;

            if (parent == null)
                _root = newNode;
            else if (Compare(key, parent.Key) < 0)
                parent.Left = newNode;
            else
                parent.Right = newNode;

            FixInsert(newNode);
            _size++;
        }

        public V Get(K key)
        {
            var node = FindNode(key);
            if (node == null)
                throw new KeyNotFoundException();
            return default(V);
        }

        public bool ContainsKey(K key)
        {
            return FindNode(key) != null;
        }

        private RBTreeNode<K> FindNode(K key)
        {
            var current = _root;
            while (current != null)
            {
                int cmp = Compare(key, current.Key);
                if (cmp < 0)
                    current = current.Left;
                else if (cmp > 0)
                    current = current.Right;
                else
                    return current;
            }
            return null;
        }

        private RBTreeNode<K> FindMin(RBTreeNode<K> node)
        {
            while (node.Left != null)
                node = node.Left;
            return node;
        }

        private RBTreeNode<K> FindMax(RBTreeNode<K> node)
        {
            while (node.Right != null)
                node = node.Right;
            return node;
        }

        private void Transplant(RBTreeNode<K> u, RBTreeNode<K> v)
        {
            if (u.Parent == null)
                _root = v;
            else if (u == u.Parent.Left)
                u.Parent.Left = v;
            else
                u.Parent.Right = v;
            if (v != null)
                v.Parent = u.Parent;
        }

        private void FixDelete(RBTreeNode<K> node, RBTreeNode<K> parent)
        {
            while (node != _root && (node == null || node.IsBlack))
            {
                if (node == parent?.Left)
                {
                    var sibling = parent.Right;
                    if (IsRed(sibling))
                    {
                        sibling.Color = Color.Black;
                        parent.Color = Color.Red;
                        RotateLeft(parent);
                        sibling = parent.Right;
                    }
                    if ((sibling.Left == null || sibling.Left.IsBlack) &&
                        (sibling.Right == null || sibling.Right.IsBlack))
                    {
                        sibling.Color = Color.Red;
                        node = parent;
                        parent = node.Parent;
                    }
                    else
                    {
                        if (sibling.Right == null || sibling.Right.IsBlack)
                        {
                            if (sibling.Left != null)
                                sibling.Left.Color = Color.Black;
                            sibling.Color = Color.Red;
                            RotateRight(sibling);
                            sibling = parent.Right;
                        }
                        sibling.Color = parent.Color;
                        parent.Color = Color.Black;
                        if (sibling.Right != null)
                            sibling.Right.Color = Color.Black;
                        RotateLeft(parent);
                        node = _root;
                        break;
                    }
                }
                else
                {
                    var sibling = parent?.Left;
                    if (IsRed(sibling))
                    {
                        sibling.Color = Color.Black;
                        parent.Color = Color.Red;
                        RotateRight(parent);
                        sibling = parent.Left;
                    }
                    if ((sibling.Right == null || sibling.Right.IsBlack) &&
                        (sibling.Left == null || sibling.Left.IsBlack))
                    {
                        sibling.Color = Color.Red;
                        node = parent;
                        parent = node.Parent;
                    }
                    else
                    {
                        if (sibling.Left == null || sibling.Left.IsBlack)
                        {
                            if (sibling.Right != null)
                                sibling.Right.Color = Color.Black;
                            sibling.Color = Color.Red;
                            RotateLeft(sibling);
                            sibling = parent.Left;
                        }
                        sibling.Color = parent.Color;
                        parent.Color = Color.Black;
                        if (sibling.Left != null)
                            sibling.Left.Color = Color.Black;
                        RotateRight(parent);
                        node = _root;
                        break;
                    }
                }
            }
            if (node != null)
                node.Color = Color.Black;
        }

        public void Remove(K key)
        {
            var z = FindNode(key);
            if (z == null)
                return;

            var y = z;
            var yOriginalColor = y.Color;
            RBTreeNode<K> x = null;
            RBTreeNode<K> xParent = null;

            if (z.Left == null)
            {
                x = z.Right;
                xParent = z.Parent;
                Transplant(z, z.Right);
            }
            else if (z.Right == null)
            {
                x = z.Left;
                xParent = z.Parent;
                Transplant(z, z.Left);
            }
            else
            {
                y = FindMin(z.Right);
                yOriginalColor = y.Color;
                x = y.Right;
                if (y.Parent == z)
                {
                    if (x != null)
                        x.Parent = y;
                    xParent = y;
                }
                else
                {
                    Transplant(y, y.Right);
                    y.Right = z.Right;
                    y.Right.Parent = y;
                    xParent = y.Parent;
                }
                Transplant(z, y);
                y.Left = z.Left;
                y.Left.Parent = y;
                y.Color = z.Color;
            }

            if (yOriginalColor == Color.Black)
                FixDelete(x, xParent);

            _size--;
        }

        public IEnumerable<K> KeysInOrder()
        {
            return InOrderTraversal(_root);
        }

        private IEnumerable<K> InOrderTraversal(RBTreeNode<K> node)
        {
            if (node != null)
            {
                foreach (var key in InOrderTraversal(node.Left))
                    yield return key;
                yield return node.Key;
                foreach (var key in InOrderTraversal(node.Right))
                    yield return key;
            }
        }

        public K FirstKey()
        {
            if (_root == null)
                throw new InvalidOperationException("Map is empty");
            return FindMin(_root).Key;
        }

        public K LastKey()
        {
            if (_root == null)
                throw new InvalidOperationException("Map is empty");
            return FindMax(_root).Key;
        }

        public K CeilingKey(K key)
        {
            var current = _root;
            K result = default(K);
            bool found = false;

            while (current != null)
            {
                int cmp = Compare(key, current.Key);
                if (cmp <= 0)
                {
                    result = current.Key;
                    found = true;
                    current = current.Left;
                }
                else
                {
                    current = current.Right;
                }
            }

            return found ? result : default(K);
        }

        public K FloorKey(K key)
        {
            var current = _root;
            K result = default(K);
            bool found = false;

            while (current != null)
            {
                int cmp = Compare(key, current.Key);
                if (cmp >= 0)
                {
                    result = current.Key;
                    found = true;
                    current = current.Right;
                }
                else
                {
                    current = current.Left;
                }
            }

            return found ? result : default(K);
        }

        public K HigherKey(K key)
        {
            var current = _root;
            K result = default(K);
            bool found = false;

            while (current != null)
            {
                int cmp = Compare(key, current.Key);
                if (cmp < 0)
                {
                    result = current.Key;
                    found = true;
                    current = current.Left;
                }
                else
                {
                    current = current.Right;
                }
            }

            return found ? result : default(K);
        }

        public K LowerKey(K key)
        {
            var current = _root;
            K result = default(K);
            bool found = false;

            while (current != null)
            {
                int cmp = Compare(key, current.Key);
                if (cmp > 0)
                {
                    result = current.Key;
                    found = true;
                    current = current.Right;
                }
                else
                {
                    current = current.Left;
                }
            }

            return found ? result : default(K);
        }

        public void Clear()
        {
            _root = null;
            _size = 0;
        }

        public MyTreeMap<K, V> SubMap(K fromKey, K toKey, bool fromInclusive, bool toInclusive)
        {
            var result = new MyTreeMap<K, V>(_comparer);
            foreach (var key in KeysInOrder())
            {
                bool fromCondition = fromInclusive ? Compare(key, fromKey) >= 0 : Compare(key, fromKey) > 0;
                bool toCondition = toInclusive ? Compare(key, toKey) <= 0 : Compare(key, toKey) < 0;
                if (fromCondition && toCondition)
                    result.Put(key, default(V));
            }
            return result;
        }

        public MyTreeMap<K, V> HeadMap(K toKey, bool inclusive)
        {
            var result = new MyTreeMap<K, V>(_comparer);
            foreach (var key in KeysInOrder())
            {
                if (inclusive ? Compare(key, toKey) <= 0 : Compare(key, toKey) < 0)
                    result.Put(key, default(V));
                else
                    break;
            }
            return result;
        }

        public MyTreeMap<K, V> TailMap(K fromKey, bool inclusive)
        {
            var result = new MyTreeMap<K, V>(_comparer);
            bool startAdding = false;
            foreach (var key in KeysInOrder())
            {
                if (!startAdding)
                {
                    if (inclusive ? Compare(key, fromKey) >= 0 : Compare(key, fromKey) > 0)
                        startAdding = true;
                }
                if (startAdding)
                    result.Put(key, default(V));
            }
            return result;
        }

        public K PollFirst()
        {
            if (_root == null)
                return default(K);
            var first = FindMin(_root);
            var key = first.Key;
            Remove(key);
            return key;
        }

        public K PollLast()
        {
            if (_root == null)
                return default(K);
            var last = FindMax(_root);
            var key = last.Key;
            Remove(key);
            return key;
        }
    }

    // Основной класс MyTreeSet
    public class MyTreeSet<E> : IEnumerable<E> where E : IComparable<E>
    {
        private MyTreeMap<E, object> _m;

        // 1) Конструктор MyTreeSet() для создания пустого множества
        public MyTreeSet()
        {
            _m = new MyTreeMap<E, object>();
        }

        // 2) Конструктор MyTreeSet(MyTreeMap<E, Object> m)
        public MyTreeSet(MyTreeMap<E, object> m)
        {
            _m = m ?? throw new ArgumentNullException(nameof(m));
        }

        // 3) Конструктор MyTreeSet(T[] a)
        public MyTreeSet(E[] a)
        {
            _m = new MyTreeMap<E, object>();
            if (a != null)
                AddAll(a);
        }

        // 4) Конструктор MyTreeSet(SortedSet<E> s)
        public MyTreeSet(SortedSet<E> s)
        {
            _m = new MyTreeMap<E, object>();
            if (s != null)
            {
                foreach (var item in s)
                    Add(item);
            }
        }

        // 5) Метод add(T e)
        public bool Add(E e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (_m.ContainsKey(e))
                return false;

            _m.Put(e, DummyObject.Instance);
            return true;
        }

        // 6) Метод addAll(T[] a)
        public void AddAll(E[] a)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));

            foreach (var item in a)
                Add(item);
        }

        // 7) Метод clear()
        public void Clear()
        {
            _m.Clear();
        }

        // 8) Метод contains(Object o)
        public bool Contains(object o)
        {
            if (o == null)
                return false;

            try
            {
                var key = (E)o;
                return _m.ContainsKey(key);
            }
            catch
            {
                return false;
            }
        }

        // 9) Метод containsAll(T[] a)
        public bool ContainsAll(E[] a)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));

            foreach (var item in a)
            {
                if (!Contains(item))
                    return false;
            }
            return true;
        }

        // 10) Метод isEmpty()
        public bool IsEmpty() => _m.IsEmpty;

        // 12) Метод remove(object o)
        public bool Remove(object o)
        {
            if (o == null)
                return false;

            try
            {
                var key = (E)o;
                if (!_m.ContainsKey(key))
                    return false;

                _m.Remove(key);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 13) Метод removeAll(T[] a)
        public void RemoveAll(E[] a)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));

            foreach (var item in a)
                Remove(item);
        }

        // 14) Метод retainAll(T[] a)
        public void RetainAll(E[] a)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));

            var toRetain = new HashSet<E>(a);
            var toRemove = new List<E>();

            foreach (var item in this)
            {
                if (!toRetain.Contains(item))
                    toRemove.Add(item);
            }

            foreach (var item in toRemove)
                Remove(item);
        }

        // 15) Метод size()
        public int Size() => _m.Size;

        // 16) Метод toArray()
        public object[] ToArray()
        {
            var result = new object[_m.Size];
            int index = 0;
            foreach (var key in _m.KeysInOrder())
                result[index++] = key;
            return result;
        }

        // 17) Метод toArray(T[] a)
        public E[] ToArray(E[] a)
        {
            if (a == null)
                a = new E[_m.Size];

            if (a.Length < _m.Size)
                a = new E[_m.Size];

            int index = 0;
            foreach (var key in _m.KeysInOrder())
                a[index++] = key;

            if (a.Length > _m.Size)
                a[_m.Size] = default(E);

            return a;
        }

        // 18) Метод first()
        public E First()
        {
            if (IsEmpty())
                throw new InvalidOperationException("Set is empty");
            return _m.FirstKey();
        }

        // 19) Метод last()
        public E Last()
        {
            if (IsEmpty())
                throw new InvalidOperationException("Set is empty");
            return _m.LastKey();
        }

        // 20) Метод subSet(E fromElement, E toElement)
        public MyTreeSet<E> SubSet(E fromElement, E toElement)
        {
            if (fromElement == null || toElement == null)
                throw new ArgumentNullException();

            var subMap = _m.SubMap(fromElement, toElement, true, false);
            return new MyTreeSet<E>(subMap);
        }

        // 21) Метод headSet(E toElement)
        public MyTreeSet<E> HeadSet(E toElement)
        {
            if (toElement == null)
                throw new ArgumentNullException(nameof(toElement));

            var headMap = _m.HeadMap(toElement, false);
            return new MyTreeSet<E>(headMap);
        }

        // 22) Метод tailSet(E fromElement)
        public MyTreeSet<E> TailSet(E fromElement)
        {
            if (fromElement == null)
                throw new ArgumentNullException(nameof(fromElement));

            var tailMap = _m.TailMap(fromElement, true);
            return new MyTreeSet<E>(tailMap);
        }

        // 23) Метод ceiling(E obj)
        public E Ceiling(E obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var result = _m.CeilingKey(obj);
            if (result == null || (result.Equals(default(E)) && !_m.ContainsKey(result)))
                return default(E);
            return result;
        }

        // 24) Метод floor(E obj)
        public E Floor(E obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var result = _m.FloorKey(obj);
            if (result == null || (result.Equals(default(E)) && !_m.ContainsKey(result)))
                return default(E);
            return result;
        }

        // 25) Метод higher(E obj)
        public E Higher(E obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var result = _m.HigherKey(obj);
            if (result == null || (result.Equals(default(E)) && !_m.ContainsKey(result)))
                return default(E);
            return result;
        }

        // 26) Метод lower(E obj)
        public E Lower(E obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var result = _m.LowerKey(obj);
            if (result == null || (result.Equals(default(E)) && !_m.ContainsKey(result)))
                return default(E);
            return result;
        }

        // 27) Метод headSet(E upperBound, bool incl)
        public MyTreeSet<E> HeadSet(E upperBound, bool incl)
        {
            if (upperBound == null)
                throw new ArgumentNullException(nameof(upperBound));

            var headMap = _m.HeadMap(upperBound, incl);
            return new MyTreeSet<E>(headMap);
        }

        // 28) Метод subSet(E lowerBound, bool lowIncl, E upperBound, bool highIncl)
        public MyTreeSet<E> SubSet(E lowerBound, bool lowIncl, E upperBound, bool highIncl)
        {
            if (lowerBound == null || upperBound == null)
                throw new ArgumentNullException();

            var subMap = _m.SubMap(lowerBound, upperBound, lowIncl, highIncl);
            return new MyTreeSet<E>(subMap);
        }

        // 29) Метод tailSet(E fromElement, bool inclusive)
        public MyTreeSet<E> TailSet(E fromElement, bool inclusive)
        {
            if (fromElement == null)
                throw new ArgumentNullException(nameof(fromElement));

            var tailMap = _m.TailMap(fromElement, inclusive);
            return new MyTreeSet<E>(tailMap);
        }

        // 30) Метод pollLast()
        public E PollLast()
        {
            if (IsEmpty())
                return default(E);
            return _m.PollLast();
        }

        // 31) Метод pollFirst()
        public E PollFirst()
        {
            if (IsEmpty())
                return default(E);
            return _m.PollFirst();
        }

        // 32) Метод descendingIterator()
        public IEnumerator<E> DescendingIterator()
        {
            var list = new List<E>();
            foreach (var key in _m.KeysInOrder())
                list.Add(key);
            list.Reverse();
            foreach (var item in list)
                yield return item;
        }

        // 33) Метод descendingSet()
        public MyTreeSet<E> DescendingSet()
        {
            var descSet = new MyTreeSet<E>();
            var list = new List<E>();
            foreach (var key in _m.KeysInOrder())
                list.Add(key);
            list.Reverse();
            descSet.AddAll(list.ToArray());
            return descSet;
        }

        // Реализация IEnumerable<E>
        public IEnumerator<E> GetEnumerator()
        {
            foreach (var key in _m.KeysInOrder())
                yield return key;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    // Программа с примерами
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Демонстрация работы MyTreeSet ===\n");

            // Пример 1: Базовые операции - добавление, удаление, поиск
            Console.WriteLine("Пример 1: Базовые операции (add, remove, contains, size)");
            var set1 = new MyTreeSet<int>();

            set1.Add(5);
            set1.Add(2);
            set1.Add(8);
            set1.Add(1);
            set1.Add(3);

            Console.WriteLine($"Добавлены элементы: 5, 2, 8, 1, 3");
            Console.WriteLine($"Размер множества: {set1.Size()}");
            Console.WriteLine($"Элементы (отсортированы): {string.Join(", ", set1.ToArray())}");
            Console.WriteLine($"Содержит 3? {set1.Contains(3)}");
            Console.WriteLine($"Содержит 10? {set1.Contains(10)}");

            set1.Remove(2);
            Console.WriteLine($"После удаления 2, размер: {set1.Size()}");
            Console.WriteLine($"Элементы: {string.Join(", ", set1.ToArray())}\n");

            // Пример 2: Операции поиска и подмножества
            Console.WriteLine("Пример 2: Поиск элементов и работа с подмножествами");
            var set2 = new MyTreeSet<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            Console.WriteLine($"Исходное множество: {string.Join(", ", set2.ToArray())}");
            Console.WriteLine($"First (наименьший): {set2.First()}");
            Console.WriteLine($"Last (наибольший): {set2.Last()}");
            Console.WriteLine($"Ceiling(5): {set2.Ceiling(5)} (наименьший элемент >= 5)");
            Console.WriteLine($"Floor(5): {set2.Floor(5)} (наибольший элемент <= 5)");
            Console.WriteLine($"Higher(5): {set2.Higher(5)} (строго больше 5)");
            Console.WriteLine($"Lower(5): {set2.Lower(5)} (строго меньше 5)");

            var subSet = set2.SubSet(3, 7);
            Console.WriteLine($"subSet(3, 7): {string.Join(", ", subSet.ToArray())}");

            var headSet = set2.HeadSet(5);
            Console.WriteLine($"headSet(5): {string.Join(", ", headSet.ToArray())}");

            var tailSet = set2.TailSet(6);
            Console.WriteLine($"tailSet(6): {string.Join(", ", tailSet.ToArray())}\n");

            // Пример 3: pollFirst, pollLast и descendingSet
            Console.WriteLine("Пример 3: Извлечение элементов и обратный порядок");
            var set3 = new MyTreeSet<string>(new string[] { "Apple", "Banana", "Cherry", "Date", "Elderberry" });
            Console.WriteLine($"Исходное множество: {string.Join(", ", set3.ToArray())}");

            Console.WriteLine($"pollFirst(): {set3.PollFirst()} (извлекаем наименьший)");
            Console.WriteLine($"pollLast(): {set3.PollLast()} (извлекаем наибольший)");
            Console.WriteLine($"Множество после извлечения: {string.Join(", ", set3.ToArray())}");

            var descSet = set3.DescendingSet();
            Console.WriteLine($"descendingSet(): {string.Join(", ", descSet.ToArray())}");

            Console.Write("descendingIterator(): ");
            var iterator = set3.DescendingIterator();
            var items = new List<string>();
            while (iterator.MoveNext())
                items.Add(iterator.Current);
            Console.WriteLine(string.Join(", ", items));
        }
    }
}