namespace Pokemon.Project.World.Spherical.Geography
{
    /// <summary>
    /// Indexed binary min-heap with one live entry per graph node and bounded O(node count) memory.
    /// </summary>
    internal sealed class SphericalIndexedMinHeap
    {
        private readonly int[] _heap;
        private readonly int[] _positions;
        private readonly float[] _keys;
        public int Count { get; private set; }

        /// <summary>
        /// Initializes heap storage and marks every graph node as not inserted.
        /// </summary>
        public SphericalIndexedMinHeap(int capacity, float[] keys)
        {
            _heap = new int[capacity];
            _positions = new int[capacity];
            _keys = keys;
            for (int i = 0; i < _positions.Length; i++) _positions[i] = -1;
        }

        /// <summary>
        /// Reports whether a node currently exists in the heap.
        /// </summary>
        public bool Contains(int node) => _positions[node] >= 0;

        /// <summary>
        /// Reports whether a node has already been removed from the heap.
        /// </summary>
        public bool IsSettled(int node) => _positions[node] == -2;

        /// <summary>
        /// Inserts a node unless it is already queued.
        /// </summary>
        public void Insert(int node)
        {
            if (_positions[node] >= 0) return;
            int index = Count++;
            _heap[index] = node;
            _positions[node] = index;
            SiftUp(index);
        }

        /// <summary>
        /// Restores heap ordering after a queued node receives a smaller distance.
        /// </summary>
        public void DecreaseKey(int node) => SiftUp(_positions[node]);

        /// <summary>
        /// Removes and returns the node with the smallest current distance.
        /// </summary>
        public int PopMinimum()
        {
            int result = _heap[0];
            _positions[result] = -2;
            Count--;
            if (Count > 0)
            {
                int last = _heap[Count];
                _heap[0] = last;
                _positions[last] = 0;
                SiftDown(0);
            }
            return result;
        }

        /// <summary>
        /// Moves a node toward the heap root while its key is smaller than its parent.
        /// </summary>
        private void SiftUp(int index)
        {
            int node = _heap[index];
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (Compare(_heap[parent], node) <= 0) break;
                _heap[index] = _heap[parent];
                _positions[_heap[index]] = index;
                index = parent;
            }
            _heap[index] = node;
            _positions[node] = index;
        }

        /// <summary>
        /// Moves a node toward the heap leaves while either child has a smaller key.
        /// </summary>
        private void SiftDown(int index)
        {
            int node = _heap[index];
            while (true)
            {
                int left = index * 2 + 1;
                if (left >= Count) break;
                int right = left + 1;
                int child = right < Count && Compare(_heap[right], _heap[left]) < 0 ? right : left;
                if (Compare(node, _heap[child]) <= 0) break;
                _heap[index] = _heap[child];
                _positions[_heap[index]] = index;
                index = child;
            }
            _heap[index] = node;
            _positions[node] = index;
        }

        /// <summary>
        /// Compares nodes by distance and then node identity for deterministic tie-breaking.
        /// </summary>
        private int Compare(int a, int b)
        {
            int result = _keys[a].CompareTo(_keys[b]);
            return result != 0 ? result : a.CompareTo(b);
        }
    }
}
