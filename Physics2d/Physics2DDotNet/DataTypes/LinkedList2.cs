using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Physics2DDotNet.DataTypes
{
    /// <summary>
    /// Custom linked list that allows more liberal insertion of nodes
    /// unlike the one provided by .net framework 4.5
    /// </summary>
    public class LinkedList2<T>
    {
        public LinkedListNode2<T> First { get; private set; }
        public LinkedListNode2<T> Last { get; private set; }

        public LinkedList2()
        {
            First = null;
            Last = null;
        }

        private void AddRoot( LinkedListNode2<T> node )
        {
            if ( node == null ) throw new ArgumentNullException();

            First = node;
            Last = node;
            node.Next = null;
            node.Previous = null;
        }

        public void AddLast( LinkedListNode2<T> node )
        {
            if ( node == null ) throw new ArgumentNullException();

            if ( First == null )
            {
                AddRoot( node );
            }
            else
            {
                Last.Next = node;
                node.Next = null;
                node.Previous = Last;
                Last = node;
            }
        }

        public void AddFirst( LinkedListNode2<T> node )
        {
            if ( node == null ) throw new ArgumentNullException();

            if ( First == null )
            {
                AddRoot( node );
            }
            else
            {
                node.Previous = null;
                node.Next = First;
                First.Previous = node;
                First = node;
            }
        }

        public void Remove( LinkedListNode2<T> remove )
        {
            if ( remove == null )
                return;

            for ( LinkedListNode2<T> node = First; node != null; node = node.Next )
            {
                if ( node == remove )
                {
                    if ( node == First )
                        First = node.Next;
                    if ( node == Last )
                        Last = node.Previous;

                    if ( node.Previous != null )
                        node.Previous.Next = node.Next;
                    if ( node.Next != null )
                        node.Next.Previous = node.Previous;
                }
            }
        }
    }

    public class LinkedListNode2<T>
    {
        public T Value { get; set; }
        public LinkedListNode2<T> Previous { get; set; }
        public LinkedListNode2<T> Next { get; set; }

        public LinkedListNode2( T val )
        {
            this.Value = val;
        }
    }
}
