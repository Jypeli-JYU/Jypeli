using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Collections.ObjectModel;

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
        
        private enum ListOperation
        {
            Add,
            Remove,
            Clear
        }

        private struct ListAction
        {
            internal T Item;
            internal ListOperation Operation;
            public ListAction(ListOperation op, T item)
            {
                this.Item = item;
                this.Operation = op;
            }

            public ListAction(ListOperation op)
            {
                this.Item = default;
                this.Operation = op;
            }
        }

        #endregion

        internal List<T> items = new List<T>();
        private Queue<ListAction> actions = new Queue<ListAction>();
        private List<T> toBeAdded = new List<T>();

        /// <summary>
        /// Kuinka monta oliota ollaan lisäämässä tähän listaan seuraavalla päivityksellä.
        /// </summary>
        public int AmountToBeAdded { get; set; }

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

        /// <inheritdoc/>
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
            ItemAdded?.Invoke(item);
        }

        private void OnItemRemoved( T item )
        {
            ItemRemoved?.Invoke(item);
        }

        #region INotifyList<T> Members

        /// <summary>
        /// Tapahtuu kun lista on muuttunut.
        /// </summary>
        public event Action Changed;

        private void OnChanged()
        {
            Changed?.Invoke();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Listan numeraattori
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Lisää alkio listaan.
        /// Huom! Alkio todellisuudessa lisätään vasta kun listan <see cref="Update(Time)">Update</see> metodia on kutsuttu.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            actions.Enqueue(new ListAction(ListOperation.Add, item));
            AmountToBeAdded++;
            toBeAdded.Add(item);
        }

        /// <summary>
        /// Poistaa alkion listasta.
        /// Huom! Alkio todellisuudessa poistetaan vasta kun listan <see cref="Update(Time)">Update</see> metodia on kutsuttu.
        /// </summary>
        /// <param name="item"></param>
        public void Remove(T item)
        {
            actions.Enqueue(new ListAction(ListOperation.Remove, item));
        }

        /// <summary>
        /// Tyhjentää listan.
        /// Huom! Lista todellisuudessa tyhjennetään vasta kun listan <see cref="Update(Time)">Update</see> metodia on kutsuttu.
        /// </summary>
        public void Clear()
        {
            actions.Enqueue(new ListAction(ListOperation.Clear));
        }

        /// <summary>
        /// Sisältääkö lista alkion.
        /// Katso myös <see cref="WillContain(T)">WillContain</see>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains( T item )
        {
            return items.Contains( item );
        }

        /// <summary>
        /// Tuleeko lista sisältämään alkion sen seuraavan päivityksen jälkeen.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool WillContain( T item )
        {
            ListAction[] actionArray = actions.ToArray();
            bool exists = Contains(item);

            for (int i = 0; i < actionArray.Length; i++)
            {
                if (actionArray[i].Operation == ListOperation.Add && actionArray[i].Equals(item))
                    exists = true;

                else if (actionArray[i].Operation == ListOperation.Remove && actionArray[i].Equals(item))
                    exists = false;

                else if (actionArray[i].Operation == ListOperation.Clear)
                    exists = false;
            }

            return exists;
        }

        /// <summary>
        /// Alkion indeksi listassa.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf( T item )
        {
            return FirstIndex + items.IndexOf( item );
        }

        /// <summary>
        /// Antaa ehdon toteuttavan alkion.
        /// </summary>
        /// <param name="pred"></param>
        /// <returns></returns>
        public T Find( Predicate<T> pred )
        {
            return items.Find( pred );
        }

        /// <summary>
        /// Antaa kaikki alkiot jotka toteuttavat ehdon.
        /// </summary>
        /// <param name="pred"></param>
        /// <returns></returns>
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

            while (actions.Count > 0)
            {
                ListAction action = actions.Dequeue();
                switch (action.Operation)
                {
                    case ListOperation.Add:
                        if (Contains(action.Item))
                            break;

                        items.Add(action.Item);
                        OnItemAdded(action.Item);
                        break;

                    case ListOperation.Remove:
                        if (items.Remove(action.Item))
                            OnItemRemoved(action.Item);
                        break;

                    case ListOperation.Clear:
                        foreach (var item in this)
                        {
                            OnItemRemoved(item);
                        }

                        items.Clear();
                        break;
                }
            }

            toBeAdded.Clear();
            AmountToBeAdded = 0;
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
                if (item is Destroyable DestroyableItem && DestroyableItem.IsDestroyed)
                    Remove(item);
                if ( item is Updatable UpdatableItem && UpdatableItem.IsUpdated )
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
                if (item is Destroyable DestroyableItem && DestroyableItem.IsDestroyed)
                    Remove(item);
                else if (item is Updatable UpdatableItem && UpdatableItem.IsUpdated && isUpdated(item))
                    UpdatableItem.Update(time);
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
            return toBeAdded.AsReadOnly();
        }
    }
}
