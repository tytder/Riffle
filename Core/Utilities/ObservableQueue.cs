using System.Collections.ObjectModel;

namespace Riffle.Core.Utilities;

public class ObservableQueue<T> : ObservableCollection<T>
{
    private readonly bool _isReverseQueue;

    public ObservableQueue(bool isReverseQueue = false)
    {
        _isReverseQueue = isReverseQueue;
    }
    
    public ObservableQueue(IEnumerable<T> collection, bool isReverseQueue = false) : base(new List<T>(collection ?? throw new ArgumentNullException(nameof(collection))))
    {
        _isReverseQueue = isReverseQueue;
    }
    
    public ObservableQueue(List<T> list, bool isReverseQueue = false) : base(new List<T>(list ?? throw new ArgumentNullException(nameof(list))))
    {
        _isReverseQueue = isReverseQueue;
    }
    
    public void Enqueue(T item)
    {
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