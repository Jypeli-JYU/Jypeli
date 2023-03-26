using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Jypeli.Profiling
{
    
    public static class Profiler
    {
        internal static Stopwatch sw;
        private static Timing Substep;
        public static Dictionary<string, CyclicArray<Timing>> Steps;

        private static int maxSteps = 200;

        [Conditional("PROFILE")]
        public static void Init()
        {
            sw = new Stopwatch();
            Steps = new Dictionary<string, CyclicArray<Timing>>();
        }

        [Conditional("PROFILE")]
        public static void Start(string name)
        {
            sw.Restart();
            Substep = new Timing(name, Substep);
            
            if(!Steps.TryGetValue(name, out var val) )
            {
                val = new CyclicArray<Timing>(maxSteps);
                Steps[name] = val;
            }
            val.Add(Substep);
        }

        [Conditional("PROFILE")]
        public static void End()
        {
            Substep.End();
            Substep = null;
        }

        [Conditional("PROFILE")]
        public static void BeginStep(string name)
        {
            var t = new Timing(name, Substep);
            Substep.SubTimings.Add(t);
            Substep = t;
        }

        [Conditional("PROFILE")]
        public static void EndStep()
        {
            Substep.End(); 
            Substep = Substep.Parent;
        }
    }

    public class Timing
    {
        public Timing Parent { get; set; }
        public string Name { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public List<Timing> SubTimings { get; set; }

        public Timing(string name, Timing parent)
        {
            Name = name;
            Parent = parent;
            
            StartTime = Profiler.sw.Elapsed;
            SubTimings = new List<Timing>();
        }

        public void End()
        {
            EndTime = Profiler.sw.Elapsed;
        }
        
        public override string ToString()
        {
            return $"{Name}: {(EndTime - StartTime).TotalMilliseconds}";
        }
    }

    public class CyclicArray<T> : ICollection<T>
    {
        private T[] data;
        private int index = 0;
        private int _capacity;

        public CyclicArray(int capacity)
        {
            data = new T[capacity];
            _capacity = capacity;
        }

        public void Add(T item)
        {
            data[index++] = item;
            if (index == _capacity) index = 0;
        }

        public void Clear()
        {
            data = new T[_capacity];
            index = 0;
        }

        public bool Contains(T item)
        {
            return data.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }


        public IEnumerator<T> GetEnumerator()
        {
            return new CyclicArrayEnumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public class CyclicArrayEnumerator<T> : IEnumerator<T>
        {
            public CyclicArray<T> _carr;

            // Enumerators are positioned before the first element
            // until the first MoveNext() call.
            int position = -1;

            public CyclicArrayEnumerator(CyclicArray<T> list)
            {
                _carr = list;
                position = list.index;
            }

            public bool MoveNext()
            {
                position++;
                if (position == _carr._capacity)
                    position = 0;
                return (position != _carr.index);
            }

            public void Reset()
            {
                position = _carr.index;
            }

            void IDisposable.Dispose() { }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public T Current
            {
                get
                {
                    try
                    {
                        return _carr.data[position];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
        }
    }


}

