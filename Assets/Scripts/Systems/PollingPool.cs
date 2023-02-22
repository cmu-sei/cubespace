/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System.Collections.Generic;
using UnityEngine;

namespace Systems {
    /// <summary>
    /// A pool that links generic objects together through a linked list.
    /// Adapted from https://forum.unity.com/threads/perfomant-audiosource-pool.503056/.
    /// </summary>
    /// <typeparam name="T">The component type to include within the nodes of a linked list.</typeparam>
    public abstract class PollingPool<T> where T : Component
    {
        // The base prefab to spawn in the pool
        private readonly T prefab;

        // The pool is represented via a LinkedList instantiated via a Queue of available resources
        protected readonly Queue<T> pool = new Queue<T>();
        private readonly LinkedList<T> inuse = new LinkedList<T>();
        private readonly Queue<LinkedListNode<T>> nodePool = new Queue<LinkedListNode<T>>();

        // The last time a check for if a resource was in use was performed
        private int lastCheckFrame = -1;
        // The maximum number of resources that can exist within the pool
        protected int poolLimit;

        /// <summary>
        /// Creates a new pool to draw resoruces from.
        /// </summary>
        /// <param name="prefab">The prefab of the resource to instantiate.</param>
        /// <param name="poolLimit">The maximum number of items that can exist in the pool.</param>
        protected PollingPool(T prefab, int poolLimit = int.MaxValue)
        {
            this.prefab = prefab;
            this.poolLimit = poolLimit;
        }

        /// <summary>
        /// Checks inuse list to see if each used T is still being used.
        /// Called each time Get() is called on a different frame.
        /// </summary>
        private void CheckInUse()
        {
            // Start with the first node in the in use resource pool
            var node = inuse.First;

            // Loop through all nodes
            while (node != null)
            {
                // Move to the next node
                var current = node;
                node = node.Next;

                // If the resource observed is not being used, remove it from the in use pool
                if (!IsActive(current.Value))
                {
                    Disable(current);
                }
            }
        }

        /// <summary>
        /// Gets a new or existing resource.
        /// </summary>
        /// <returns>A new instance of the desired resource.</returns>
        public virtual T Get()
        {
            // Check if a resource is still being used
            if (lastCheckFrame != Time.frameCount)
            {
                lastCheckFrame = Time.frameCount;
                CheckInUse();
            }

            // If the amount of items in use is less than the amount we can use, consider adding a new item
            if (inuse.Count < poolLimit)
            {
                // Create a blank item we will soon instantiate
                T item;

                // Instantiate a new object or get the last item on the queue
                if (pool.Count == 0) item = InstantiateObject();
                else item = pool.Dequeue();
                
                // If the node pool is empty, add the item to the end of the list of ones in use
                if (nodePool.Count == 0) inuse.AddLast(item);
                // Otherwise, do the same as above, but dequeue the most recent node in the node pool
                else
                {
                    var node = nodePool.Dequeue();
                    node.Value = item;
                    inuse.AddLast(node);
                }

                // Make the item active and return it
                item.gameObject.SetActive(true);
                return item;
            }
            // Otherwise, we have run out of space in the pool
            else
            {
                Debug.Log("Polling Pool has run out of space!");
                return null;
            }
        }

        /// <summary>
        /// Removes a resource from the resources in use and places it back in the pools of available resources.
        /// </summary>
        /// <param name="current">The observed node within the resource pool.</param>
        protected virtual void Disable(LinkedListNode<T> current) {
            pool.Enqueue(current.Value);
            inuse.Remove(current);
            nodePool.Enqueue(current);
        }

        /// <summary>
        /// Instantiates a new object for the resource pool.
        /// This method is virtual so it can allow the setting of options during instantiation.
        /// </summary>
        /// <returns>A new instance of the object to add to the pool.</returns>
        protected virtual T InstantiateObject()
        {
            T temp = GameObject.Instantiate(prefab);
            return temp;
        }

        /// <summary>
        /// Abstract method that checks if a component is active.
        /// </summary>
        /// <param name="component">The component whose state should be checked.</param>
        /// <returns>A boolean saying that the component provided is active.</returns>
        protected abstract bool IsActive(T component);
    }
}

