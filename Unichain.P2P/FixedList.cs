using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unichain.P2P; 

/// <summary>
/// A fixed size list that removes the first item when exceeding the capacity.
/// Similar to FIFO (First In First Out).
/// </summary>
/// <typeparam name="T">The type of the itens stored</typeparam>
public class FixedList<T> {
    private readonly int capacity;
    private int count;
    private LinkedList<T> items;

    /// <summary>
    /// Creates a new fixed size list.
    /// </summary>
    /// <param name="capacity">The maximum capacity of this list</param>
    public FixedList(int capacity) {
        this.capacity = capacity;
        items = new();
    }

    /// <summary>
    /// Adds a new item to the list. If exceeding the capacity, the first item will be removed
    /// </summary>
    /// <param name="item">The item to be added</param>
    public void Add(T item) {
        if (count == capacity) {
            items.RemoveFirst();
            items.AddLast(item);
        } else {
            items.AddLast(item);
            count++;
        }
    }

    /// <summary>
    /// Checks if the list contains the given item
    /// </summary>
    /// <param name="item">The item to be checked</param>
    /// <returns>The result of the operation</returns>
    public bool Contains(T item) {
        return items.Contains(item);
    }
}
