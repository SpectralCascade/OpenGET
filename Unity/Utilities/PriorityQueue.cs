using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime.CompilerServices;

namespace OpenGET
{

    /// <summary>
    /// Queue of items sorted by custom priority.
    /// </summary>
    public class PriorityQueue<T> : IEnumerable<PriorityQueue<T>.Item>
    {
        public class Item
        {
            public Item(PriorityQueue<T> queue, T data, double priority)
            {
                this.queue = queue;
                this.data = data;
                this.priority = priority;
            }

            /// <summary>
            /// Associated queue.
            /// </summary>
            public readonly PriorityQueue<T> queue;

            /// <summary>
            /// Item data in the queue.
            /// </summary>
            public T data;

            /// <summary>
            /// The priority of this data item.
            /// </summary>
            public double priority {
                get {
                    return _priority;
                }
                set {
                    if (_priority != value)
                    {
                        _priority = value;
                        queue.hasChanges = true;
                    }
                }
            }
            private double _priority;
        }

        /// <summary>
        /// Underlying array storing the items priorities.
        /// </summary>
        private List<Item> items = new List<Item>();

        /// <summary>
        /// Returns true if the items in this queue have been modified since last being sorted.
        /// </summary>
        public bool hasChanges { get; private set; }

        /// <summary>
        /// How many items are in this priority queue.
        /// </summary>
        public int Count => items.Count;
        
        /// <summary>
        /// Sort the queue by lowest priority to be dequeued first, or else highest priority is dequeued first.
        /// Defaults to highest priority dequeued first.
        /// </summary>
        public bool lowestFirst {
            get { return _lowestFirst; }
            set {
                if (_lowestFirst != value)
                {
                    _lowestFirst = value;
                    hasChanges = true;
                }
            }
        }
        private bool _lowestFirst = false;

        public PriorityQueue() { }

        public PriorityQueue(IEnumerable<Item> data)
        {
            items = data.ToList();
        }

        public PriorityQueue(IEnumerable<T> data)
        {
            items = data.Select(x => new Item(this, x, 0)).ToList();
        }

        /// <summary>
        /// Add an element to the queue.
        /// </summary>
        public Item Enqueue(T data, double priority)
        {
            Item added = new Item(this, data, priority);
            items.Add(added);
            hasChanges = true;
            return added;
        }

        /// <summary>
        /// Remove an element from the queue.
        /// Triggers a sort if changes are detected
        /// </summary>
        public Item DequeueItem()
        {
            if (Count <= 0)
            {
                throw new System.InvalidOperationException("No items available to dequeue in PriorityQueue object.");
            }
            if (hasChanges)
            {
                SortByPriority();
            }
            int last = items.Count - 1;
            Item item = items[last];
            items.RemoveAt(last);
            return item;
        }

        /// <summary>
        /// Remove the highest priority item from the queue.
        /// </summary>
        public T Dequeue()
        {
            return DequeueItem().data;
        }

        /// <summary>
        /// Peek at the top priority item.
        /// </summary>
        public Item PeekItem()
        {
            return items.Count > 0 ? items[items.Count - 1] : null;
        }
        
        /// <summary>
        /// Peek at the top priority data.
        /// </summary>
        public T Peek()
        {
            return PeekItem().data;
        }

        /// <summary>
        /// Sort the underlying data by priority. This is done automatically on dequeue if it hasn't been done since enqueue,
        /// or priority values have been modified.
        /// </summary>
        public void SortByPriority()
        {
            if (lowestFirst)
            {
                items.Sort((a, b) => b.priority.CompareTo(a.priority));
            }
            else
            {
                items.Sort((a, b) => a.priority.CompareTo(b.priority));
            }
            hasChanges = false;
        }

        public IEnumerator<Item> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        [IndexerName("ItemIndexer")]
        public Item this[int i] => items[i];

    }

}
