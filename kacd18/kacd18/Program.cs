using System;
using System.Collections;
using System.Collections.Generic;

namespace MyTreeMapNamespace
{
    /// Узел дерева для MyTreeMap
    internal class Node<K, V>
    {
        public K Key { get; set; }
        public V Value { get; set; }
        public Node<K, V> Left { get; set; }
        public Node<K, V> Right { get; set; }
        public Node<K, V> Parent { get; set; }

        public Node(K key, V value, Node<K, V> parent = null)
        {
            Key = key;
            Value = value;
            Left = null;
            Right = null;
            Parent = parent;
        }
    }

    /// Обобщённый класс MyTreeMap - реализация отображения на основе дерева поиска
    public class MyTreeMap<K, V> : IEnumerable<KeyValuePair<K, V>>
    {
        private Node<K, V> root;
        private int size;
        private readonly IComparer<K> comparator;

        /// 1) Конструктор с естественным порядком сортировки
        public MyTreeMap() : this(Comparer<K>.Default)
        {
        }

        /// 2) Конструктор с указанным компаратором
        public MyTreeMap(IComparer<K> comp)
        {
            comparator = comp ?? throw new ArgumentNullException(nameof(comp));
            root = null;
            size = 0;
        }

        /// 3) Удаление всех пар
        public void Clear()
        {
            root = null;
            size = 0;
        }

        /// 4) Проверка наличия ключа
        public bool ContainsKey(object key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            return FindNode((K)key) != null;
        }

        /// 5) Проверка наличия значения
        public bool ContainsValue(object value)
        {
            return ContainsValue(root, value);
        }

        private bool ContainsValue(Node<K, V> node, object value)
        {
            if (node == null) return false;
            if (Equals(node.Value, value)) return true;
            return ContainsValue(node.Left, value) || ContainsValue(node.Right, value);
        }

        /// 6) Возврат множества всех пар
        public ISet<KeyValuePair<K, V>> EntrySet()
        {
            HashSet<KeyValuePair<K, V>> set = new HashSet<KeyValuePair<K, V>>();
            AddEntries(root, set);
            return set;
        }

        private void AddEntries(Node<K, V> node, HashSet<KeyValuePair<K, V>> set)
        {
            if (node == null) return;
            AddEntries(node.Left, set);
            set.Add(new KeyValuePair<K, V>(node.Key, node.Value));
            AddEntries(node.Right, set);
        }

        /// 7) Получение значения по ключу
        public V Get(object key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            Node<K, V> node = FindNode((K)key);
            return node != null ? node.Value : default(V);
        }

        /// 8) Проверка на пустоту
        public bool IsEmpty()
        {
            return size == 0;
        }

        /// 9) Возврат множества всех ключей
        public ISet<K> KeySet()
        {
            HashSet<K> set = new HashSet<K>();
            AddKeys(root, set);
            return set;
        }

        private void AddKeys(Node<K, V> node, HashSet<K> set)
        {
            if (node == null) return;
            AddKeys(node.Left, set);
            set.Add(node.Key);
            AddKeys(node.Right, set);
        }

        /// 10) Добавление пары
        public void Put(K key, V value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (root == null)
            {
                root = new Node<K, V>(key, value);
                size++;
                return;
            }

            Node<K, V> current = root;
            Node<K, V> parent = null;
            int cmp = 0;

            while (current != null)
            {
                parent = current;
                cmp = comparator.Compare(key, current.Key);

                if (cmp < 0)
                    current = current.Left;
                else if (cmp > 0)
                    current = current.Right;
                else
                {
                    current.Value = value;
                    return;
                }
            }

            Node<K, V> newNode = new Node<K, V>(key, value, parent);
            if (cmp < 0)
                parent.Left = newNode;
            else
                parent.Right = newNode;

            size++;
        }

        /// 11) Удаление пары по ключу
        public void Remove(object key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            root = Remove(root, (K)key);
        }

        private Node<K, V> Remove(Node<K, V> node, K key)
        {
            if (node == null) return null;

            int cmp = comparator.Compare(key, node.Key);

            if (cmp < 0)
                node.Left = Remove(node.Left, key);
            else if (cmp > 0)
                node.Right = Remove(node.Right, key);
            else
            {
                size--;

                if (node.Left == null) return node.Right;
                if (node.Right == null) return node.Left;

                Node<K, V> minNode = FindMin(node.Right);
                node.Key = minNode.Key;
                node.Value = minNode.Value;
                node.Right = Remove(node.Right, minNode.Key);
            }
            return node;
        }

        /// 12) Размер отображения
        public int Size()
        {
            return size;
        }

        /// 13) Первый ключ
        public K FirstKey()
        {
            if (IsEmpty())
                throw new InvalidOperationException("TreeMap is empty");
            return FindMin(root).Key;
        }

        /// 14) Последний ключ
        public K LastKey()
        {
            if (IsEmpty())
                throw new InvalidOperationException("TreeMap is empty");
            return FindMax(root).Key;
        }

        /// 15) Отображение с ключами меньше end
        public MyTreeMap<K, V> HeadMap(K end)
        {
            MyTreeMap<K, V> result = new MyTreeMap<K, V>(comparator);
            AddHeadMap(root, end, result);
            return result;
        }

        private void AddHeadMap(Node<K, V> node, K end, MyTreeMap<K, V> result)
        {
            if (node == null) return;
            if (comparator.Compare(node.Key, end) < 0)
            {
                AddHeadMap(node.Left, end, result);
                result.Put(node.Key, node.Value);
                AddHeadMap(node.Right, end, result);
            }
            else
                AddHeadMap(node.Left, end, result);
        }

        /// 16) Отображение с ключами от start до end
        public MyTreeMap<K, V> SubMap(K start, K end)
        {
            if (comparator.Compare(start, end) > 0)
                throw new ArgumentException("start key must be less than end key");

            MyTreeMap<K, V> result = new MyTreeMap<K, V>(comparator);
            AddSubMap(root, start, end, result);
            return result;
        }

        private void AddSubMap(Node<K, V> node, K start, K end, MyTreeMap<K, V> result)
        {
            if (node == null) return;

            if (comparator.Compare(node.Key, start) >= 0 && comparator.Compare(node.Key, end) < 0)
            {
                AddSubMap(node.Left, start, end, result);
                result.Put(node.Key, node.Value);
                AddSubMap(node.Right, start, end, result);
            }
            else if (comparator.Compare(node.Key, start) < 0)
                AddSubMap(node.Right, start, end, result);
            else
                AddSubMap(node.Left, start, end, result);
        }

        /// 17) Отображение с ключами больше start
        public MyTreeMap<K, V> TailMap(K start)
        {
            MyTreeMap<K, V> result = new MyTreeMap<K, V>(comparator);
            AddTailMap(root, start, result);
            return result;
        }

        private void AddTailMap(Node<K, V> node, K start, MyTreeMap<K, V> result)
        {
            if (node == null) return;

            if (comparator.Compare(node.Key, start) >= 0)
            {
                AddTailMap(node.Left, start, result);
                result.Put(node.Key, node.Value);
                AddTailMap(node.Right, start, result);
            }
            else
                AddTailMap(node.Right, start, result);
        }

        /// 18) Пара с ключом меньше заданного
        public KeyValuePair<K, V>? LowerEntry(K key)
        {
            Node<K, V> node = FindLower(root, key);
            return node != null ? new KeyValuePair<K, V>(node.Key, node.Value) : (KeyValuePair<K, V>?)null;
        }

        private Node<K, V> FindLower(Node<K, V> node, K key)
        {
            Node<K, V> result = null;
            while (node != null)
            {
                if (comparator.Compare(node.Key, key) < 0)
                {
                    result = node;
                    node = node.Right;
                }
                else
                    node = node.Left;
            }
            return result;
        }

        /// 19) Пара с ключом меньше или равным
        public KeyValuePair<K, V>? FloorEntry(K key)
        {
            Node<K, V> node = FindFloor(root, key);
            return node != null ? new KeyValuePair<K, V>(node.Key, node.Value) : (KeyValuePair<K, V>?)null;
        }

        private Node<K, V> FindFloor(Node<K, V> node, K key)
        {
            Node<K, V> result = null;
            while (node != null)
            {
                if (comparator.Compare(node.Key, key) <= 0)
                {
                    result = node;
                    node = node.Right;
                }
                else
                    node = node.Left;
            }
            return result;
        }

        /// 20) Пара с ключом больше заданного
        public KeyValuePair<K, V>? HigherEntry(K key)
        {
            Node<K, V> node = FindHigher(root, key);
            return node != null ? new KeyValuePair<K, V>(node.Key, node.Value) : (KeyValuePair<K, V>?)null;
        }

        private Node<K, V> FindHigher(Node<K, V> node, K key)
        {
            Node<K, V> result = null;
            while (node != null)
            {
                if (comparator.Compare(node.Key, key) > 0)
                {
                    result = node;
                    node = node.Left;
                }
                else
                    node = node.Right;
            }
            return result;
        }

        /// 21) Пара с ключом больше или равным
        public KeyValuePair<K, V>? CeilingEntry(K key)
        {
            Node<K, V> node = FindCeiling(root, key);
            return node != null ? new KeyValuePair<K, V>(node.Key, node.Value) : (KeyValuePair<K, V>?)null;
        }

        private Node<K, V> FindCeiling(Node<K, V> node, K key)
        {
            Node<K, V> result = null;
            while (node != null)
            {
                if (comparator.Compare(node.Key, key) >= 0)
                {
                    result = node;
                    node = node.Left;
                }
                else
                    node = node.Right;
            }
            return result;
        }

        /// 22) Ключ, который меньше заданного
        public K LowerKey(K key)
        {
            Node<K, V> node = FindLower(root, key);
            return node != null ? node.Key : default(K);
        }

        /// 23) Ключ, который меньше или равен
        public K FloorKey(K key)
        {
            Node<K, V> node = FindFloor(root, key);
            return node != null ? node.Key : default(K);
        }

        /// 24) Ключ, который больше заданного
        public K HigherKey(K key)
        {
            Node<K, V> node = FindHigher(root, key);
            return node != null ? node.Key : default(K);
        }

        /// 25) Ключ, который больше или равен
        public K CeilingKey(K key)
        {
            Node<K, V> node = FindCeiling(root, key);
            return node != null ? node.Key : default(K);
        }

        /// 26) Удаление и возврат первого элемента
        public KeyValuePair<K, V>? PollFirstEntry()
        {
            if (IsEmpty()) return null;

            Node<K, V> first = FindMin(root);
            KeyValuePair<K, V> result = new KeyValuePair<K, V>(first.Key, first.Value);
            Remove(first.Key);
            return result;
        }

        /// 27) Удаление и возврат последнего элемента
        public KeyValuePair<K, V>? PollLastEntry()
        {
            if (IsEmpty()) return null;

            Node<K, V> last = FindMax(root);
            KeyValuePair<K, V> result = new KeyValuePair<K, V>(last.Key, last.Value);
            Remove(last.Key);
            return result;
        }

        /// 28) Возврат первого элемента без удаления
        public KeyValuePair<K, V>? FirstEntry()
        {
            if (IsEmpty()) return null;
            Node<K, V> node = FindMin(root);
            return new KeyValuePair<K, V>(node.Key, node.Value);
        }

        /// 29) Возврат последнего элемента без удаления
        public KeyValuePair<K, V>? LastEntry()
        {
            if (IsEmpty()) return null;
            Node<K, V> node = FindMax(root);
            return new KeyValuePair<K, V>(node.Key, node.Value);
        }

        // Вспомогательные методы
        private Node<K, V> FindNode(K key)
        {
            Node<K, V> current = root;
            while (current != null)
            {
                int cmp = comparator.Compare(key, current.Key);
                if (cmp == 0) return current;
                current = cmp < 0 ? current.Left : current.Right;
            }
            return null;
        }

        private Node<K, V> FindMin(Node<K, V> node)
        {
            if (node == null) return null;
            while (node.Left != null)
                node = node.Left;
            return node;
        }

        private Node<K, V> FindMax(Node<K, V> node)
        {
            if (node == null) return null;
            while (node.Right != null)
                node = node.Right;
            return node;
        }

        // Реализация IEnumerable для возможности итерации по коллекции
        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return InOrderTraversal().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<K, V>> InOrderTraversal()
        {
            Stack<Node<K, V>> stack = new Stack<Node<K, V>>();
            Node<K, V> current = root;

            while (current != null || stack.Count > 0)
            {
                while (current != null)
                {
                    stack.Push(current);
                    current = current.Left;
                }

                current = stack.Pop();
                yield return new KeyValuePair<K, V>(current.Key, current.Value);
                current = current.Right;
            }
        }
    }
}
namespace MyTreeMapNamespace
{
    class Program
    {
        static void Main(string[] args)
        {
            MyTreeMap<string, int> map = new MyTreeMap<string, int>();

            Console.WriteLine("Добавление элементов:");
            map.Put("Ivan", 25);
            map.Put("Petr", 30);
            map.Put("Anna", 22);
            map.Put("Maria", 28);
            map.Put("Sidor", 35);

            Console.WriteLine($"Размер после добавления: {map.Size()}");
            Console.WriteLine($"Пусто? {map.IsEmpty()}");

            // Проверка наличия ключей и значений
            Console.WriteLine("\nПроверка наличия:");
            Console.WriteLine($"Содержит ключ 'Anna'? {map.ContainsKey("Anna")}");
            Console.WriteLine($"Содержит ключ 'John'? {map.ContainsKey("John")}");
            Console.WriteLine($"Содержит значение 30? {map.ContainsValue(30)}");
            Console.WriteLine($"Содержит значение 100? {map.ContainsValue(100)}");

            // Получение значений по ключу
            Console.WriteLine("\nПолучение значений:");
            Console.WriteLine($"Возраст Anna: {map.Get("Anna")}");
            Console.WriteLine($"Возраст Petr: {map.Get("Petr")}");
            Console.WriteLine($"Возраст John (не существует): {map.Get("John")}");

            Console.WriteLine("\nВсе элементы:");
            foreach (var pair in map)
            {
                Console.WriteLine($"  {pair.Key} -> {pair.Value}");
            }
        }
    }
}