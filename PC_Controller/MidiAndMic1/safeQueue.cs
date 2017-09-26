using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MidiAndMic1
{
     public class safeQueue<T>
    {
        private Queue<T> queue = new Queue<T>();
        public int count = 0;
        T temp;
        public int Count
        {
            get { return queue.Count; }
        }

        public void Enqueue(T qValue)
        {
            temp = qValue;
            Monitor.Enter(queue);
            try
            {
                queue.Enqueue(qValue);
            } 
            finally
            {
                Monitor.Exit(queue);
            }
        }
        public T Dequeue()
        {
            T retval;
            Monitor.Enter(queue);
            try
            {
                retval = queue.Dequeue();
            } catch(Exception ex)
            {
                retval = temp;
                Console.WriteLine("  ..");
            }
            finally
            {
                Monitor.Exit(queue);
            }
            return retval;
        }
        public void Clear()
        {
            Monitor.Enter(queue);
            try
            {
                queue.Clear();
            }
            finally
            {
                Monitor.Exit(queue);
            }
        }
    }
}
