using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Jypeli
{
    /// <summary>
    /// Synkroninen lista, eli lista joka päivittyy vasta kun sen Update-metodia kutsutaan.
    /// Jos listalle lisätään IUpdatable-rajapinnan toteuttavia olioita, kutsutaan myös niiden
    /// Update-metodeja samalla.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SynchronousList<T> : IEnumerable<T>, Updatable
    {
        #region Item actions
        
        protected abstract class ListAction
        {
            protected SynchronousList<T> list;

            public ListAction( SynchronousList<T> list )
            {
                this.list = list;
            }

            public abstract void Execute();
        }

        protected sealed class AddItemAction : ListAction
        {
            internal T newItem;

            public AddItemAction( SynchronousList<T> list, T itemToAdd )
                : base( list )
            {
                this.newItem = itemToAdd;
            }

            public override void Execute()
            {
                if ( list.Contains( newItem ) )
                    //throw new ArgumentException( "The object has already been added." );
                    return;

                list.items.Add( newItem );
                list.OnItemAdded( newItem );
            }
        }

        protected sealed class RemoveItemAction : ListAction
        {
            internal T removeItem;

            public RemoveItemAction( SynchronousList<T> items, T itemToRemove )
                : base( items )
            {
                this.removeItem = itemToRemove;
            }

            public override void Execute()
            {
                if ( list.items.Remove( removeItem ) )
                    list.OnItemRemoved( removeItem );
            }
        }

        protected sealed class ClearAction : ListAction
        {
            public ClearAction( SynchronousList<T> items )
                : base( items )
            {
            }

            public override void Execute()
            {
                foreach ( var item in list )
                {
                    list.OnItemRemoved( item );
                }

                list.items.Clear();
            }
        }

        #endregion

        internal List<T> items = new List<T>();
        Queue<ListAction> actions = new Queue<ListAction>();

        /// <summary>
        /// Indeksointioperaattori.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get { return items[index - FirstIndex]; }
            set { items[index - FirstIndex] = value; }
        }

        /// <summary>
        /// Ensimmäisen elementin indeksi. Muutettavissa.
        /// </summary>
        public int FirstIndex { get; set; }

        /// <summary>
        /// Viimeisen elementin indeksi.
        /// </summary>
        public int LastIndex
        {
            get { return FirstIndex + items.Count - 1; }
        }

        /// <summary>
        /// Kuinka monta elementtiä listassa nyt on.
        /// Ei laske mukaan samalla päivityskierroksella tehtyjä muutoksia.
        /// </summary>
        public int Count
        {
            get { return items.Count; }
        }

        public bool IsUpdated
        {
            get { return true; }
        }

        /// <summary>
        /// Tapahtuu kun uusi elementti on lisätty listaan.
        /// </summary>
        public event Action<T> ItemAdded;

        /// <summary>
        /// Tapahtuu kun elementti on poistettu listasta.
        /// </summary>
        public event Action<T> ItemRemoved;

        /// <summary>
        /// Luo uuden synkronisen listan.
        /// </summary>
        /// <param name="firstIndex">Ensimmäisen elementin indeksi.</param>
        public SynchronousList( int firstIndex )
        {
            FirstIndex = firstIndex;
        }

        /// <summary>
        /// Luo uuden synkronisen listan.
        /// </summary>
        public SynchronousList()
            : this( 0 )
        {
        }

        private void OnItemAdded( T item )
        {
            if ( ItemAdded != null )
                ItemAdded( item );
        }

        private void OnItemRemoved( T item )
        {
            if ( ItemRemoved != null )
                ItemRemoved( item );
        }

        #region INotifyList<T> Members

        /// <summary>
        /// Tapahtuu kun lista on muuttunut.
        /// </summary>
        public event Action Changed;

        private void OnChanged()
        {
            if ( Changed != null )
                Changed();
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        #endregion

        public void Add( T item )
        {
            actions.Enqueue( new AddItemAction( this, item ) );
        }

        public void Remove( T item )
        {
            actions.Enqueue( new RemoveItemAction( this, item ) );
        }

        public void Clear()
        {
            actions.Enqueue( new ClearAction( this ) );
        }

        public bool Contains( T item )
        {
            return items.Contains( item );
        }

        public bool WillContain( T item )
        {
            ListAction[] actionArray = actions.ToArray();
            bool exists = Contains(item);

            for (int i = 0; i < actionArray.Length; i++)
            {
                if (actionArray[i] is AddItemAction && ((AddItemAction)actionArray[i]).newItem.Equals( item ))
                    exists = true;

                else if (actionArray[i] is RemoveItemAction && ((RemoveItemAction)actionArray[i]).removeItem.Equals( item ))
                    exists = false;

                else if (actionArray[i] is ClearAction)
                    exists = false;
            }

            return exists;
        }

        public int IndexOf( T item )
        {
            return FirstIndex + items.IndexOf( item );
        }

        public T Find( Predicate<T> pred )
        {
            return items.Find( pred );
        }

        public List<T> FindAll( Predicate<T> pred )
        {
            return items.FindAll( pred );
        }

        /// <summary>
        /// Lisää ja poistaa jonossa olevat elementit, mutta ei kutsu
        /// elementtien Update-metodia.
        /// </summary>
        /// <returns>Muutettiinko listaa</returns>
        public bool UpdateChanges()
        {
            if ( actions.Count == 0 ) return false;

            while ( actions.Count > 0 )
                actions.Dequeue().Execute();

            return true;
        }

        /// <summary>
        /// Lisää ja poistaa jonossa olevat elementit sekä kutsuu niiden
        /// Update-metodia.
        /// </summary>
        /// <param name="time"></param>
        public void Update( Time time )
        {
            bool changed = UpdateChanges();

            foreach ( var item in items )
            {
                var DestroyableItem = item as Destroyable;
                var UpdatableItem = item as Updatable;

                if ( DestroyableItem != null && DestroyableItem.IsDestroyed )
                    Remove( item );
                if ( UpdatableItem != null && UpdatableItem.IsUpdated )
                    UpdatableItem.Update( time );
            }

            changed |= UpdateChanges();
            if ( changed ) OnChanged();
        }

        /// <summary>
        /// Lisää ja poistaa jonossa olevat elementit sekä kutsuu niiden
        /// Update-metodia tietyllä ehdolla.
        /// </summary>
        /// <param name="time"></param>
		/// <param name="isUpdated"></param>
        public void Update( Time time, Predicate<T> isUpdated )
        {
            bool changed = UpdateChanges();

            foreach ( var item in items )
            {
                var DestroyableItem = item as Destroyable;
                var UpdatableItem = item as Updatable;

                if ( DestroyableItem != null && DestroyableItem.IsDestroyed )
                    Remove( item );
                else if ( UpdatableItem != null && UpdatableItem.IsUpdated && isUpdated(item) )
                    UpdatableItem.Update( time );
            }

            changed |= UpdateChanges();
            if ( changed ) OnChanged();
        }

        /// <summary>
        /// Suorittaa annetun toimenpiteen kaikille (nykyisille) listan alkioille.
        /// </summary>
        /// <param name="action">Toiminto</param>
        public void ForEach( Action<T> action )
        {
            for ( int i = 0; i < items.Count; i++ )
            {
                action( items[i] );
            }
        }

        /// <summary>
        /// Suorittaa annetun toimenpiteen kaikille (nykyisille) listan alkioille.
        /// </summary>
        /// <typeparam name="T1">Toisen parametrin tyyppi</typeparam>
        /// <param name="action">Toiminto</param>
        /// <param name="p1">Toinen parametri</param>
        public void ForEach<T1>( Action<T, T1> action, T1 p1 )
        {
            for ( int i = 0; i < items.Count; i++ )
            {
                action( items[i], p1 );
            }
        }

        internal IEnumerable<T> GetObjectsAboutToBeAdded()
        {
            return from a in actions
                   where a is AddItemAction
                   select (a as AddItemAction).newItem;
        }
    }
}
