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
        private static Step CurrentStep;
        public static Dictionary<string, Step> Steps;

        [Conditional("PROFILE")]
        public static void Init()
        {
            sw = new Stopwatch();
            Steps = new Dictionary<string, Step>();
        }

        [Conditional("PROFILE")]
        public static void Start(string name)
        {
            sw.Restart();
            
            if(!Steps.TryGetValue(name, out var val) )
            {
                val = new Step(name, null);
                Steps[name] = val;
            }
            val.Start();
            CurrentStep = val;
        }

        [Conditional("PROFILE")]
        public static void End()
        {
            CurrentStep.End();
            CurrentStep = null;
        }

        [Conditional("PROFILE")]
        public static void BeginStep(string name)
        {
            if (!CurrentStep.SubSteps.TryGetValue(name, out var val))
            {
                val = new Step(name, CurrentStep);
                CurrentStep.SubSteps[name] = val;
            }
            val.Start();
            CurrentStep = val;
        }

        [Conditional("PROFILE")]
        public static void EndStep()
        {
            CurrentStep.End();
            CurrentStep = CurrentStep.Parent;
        }
    }

    public class Step
    {
        public string Name { get; set; }
        public Step Parent { get; set; }
        public Dictionary<string, Step> SubSteps { get; set; }
        public CyclicArray<Timing> Timings { get; set; }

        private TimeSpan start;

        public Step(string name, Step parent)
        {
            Parent = parent;
            Name = name;

            Timings = new CyclicArray<Timing>(500);
            SubSteps = new Dictionary<string, Step>();
        }

        public void Start()
        {
            start = Profiler.sw.Elapsed;
        }

        public void End()
        {
            Timings.Add(new Timing()
            {
                StartTime = start,
                EndTime = Profiler.sw.Elapsed
            });
        }
    }

    public struct Timing
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public double Duration { get => (EndTime - StartTime).TotalMilliseconds; }
        
        public override string ToString()
        {
            return $"{Duration}";
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

