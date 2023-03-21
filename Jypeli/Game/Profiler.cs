using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Jypeli
{
    public static class Profiler
    {
        internal static Stopwatch sw;
        private static Timing Substep;
        public static CyclicArray<Timing> Steps;

        public static void Init()
        {
            sw = new Stopwatch();
            Steps = new CyclicArray<Timing>(500);
        }

        public static void Start(string name)
        {
            sw.Restart();
            Substep = new Timing(name, Substep);
            Steps.Add(Substep);
        }

        public static void End()
        {
            Substep.End();
            Substep = null;
        }
        
        public static void BeginStep(string name)
        {
            var t = new Timing(name, Substep);
            Substep.SubTimings.Add(t);
            Substep = t;
        }

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
            return data.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

