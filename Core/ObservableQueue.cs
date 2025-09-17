using System.Collections.ObjectModel;

namespace Riffle.Core;

public class ObservableQueue<T> : ObservableCollection<T>
{
    public ObservableQueue()
    {
    }
    
    public ObservableQueue(IEnumerable<T> collection) : base(new List<T>(collection ?? throw new ArgumentNullException(nameof(collection))))
    {
    }
    
    public ObservableQueue(List<T> list) : base(new List<T>(list ?? throw new ArgumentNullException(nameof(list))))
    {
    }
    
    public void Enqueue(T item)
    {
        Add(item); // Add to the end
    }

    public T Dequeue()
    {
        if (Count == 0) throw new InvalidOperationException("Queue is empty");
        T removedItem = this[0];
        RemoveAt(0); // Remove from the front
        return removedItem;
    }

    public T Peek()
    {
        if (Count == 0) throw new InvalidOperationException("Queue is empty");
        return this[0];
    }
}