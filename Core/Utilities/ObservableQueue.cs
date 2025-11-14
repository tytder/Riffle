using System.Collections.ObjectModel;

namespace Riffle.Core.Utilities;

public class ObservableQueue<T> : ObservableCollection<T>
{
    private readonly bool _isReverseQueue;
    private readonly int _maxCapacity;

    public ObservableQueue(int maxCapacity = int.MaxValue, bool isReverseQueue = false)
    {
        _maxCapacity = maxCapacity;
        _isReverseQueue = isReverseQueue;
    }
    
    public ObservableQueue(IEnumerable<T> collection, int maxCapacity = int.MaxValue, bool isReverseQueue = false) : base(new List<T>(collection ?? throw new ArgumentNullException(nameof(collection))))
    {
        _maxCapacity = maxCapacity;
        _isReverseQueue = isReverseQueue;
    }
    
    public ObservableQueue(List<T> list, int maxCapacity = int.MaxValue, bool isReverseQueue = false) : base(new List<T>(list ?? throw new ArgumentNullException(nameof(list))))
    {
        _maxCapacity = maxCapacity;
        _isReverseQueue = isReverseQueue;
    }
    
    public void Enqueue(T item)
    {
        // if adding one would go over the max capacity (so we currently would be at the max capacity), remove one.
        if (Count >= _maxCapacity)
        {
            Dequeue();
        }
        if (_isReverseQueue) Insert(0, item);
        else Add(item);

    }

    public T Dequeue()
    {
        if (Count == 0) throw new InvalidOperationException("Queue is empty");
        int index = _isReverseQueue ? Count - 1 : 0; // if reversed, grab last item, else grab first item
        T removedItem = this[index];
        RemoveAt(index); // Remove from the front
        return removedItem;
    }

    public T Peek(bool peekOldest = false)
    {
        if (Count == 0) throw new InvalidOperationException("Queue is empty");
        int index = peekOldest ? Count - 1 : 0;
        return this[index];
    }
}