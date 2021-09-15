#region MIT License
/*
 * Copyright (c) 2009 University of Jyväskylä, Department of Mathematical
 * Information Technology.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

/*
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen, Mikko Röyskö
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Jypeli
{
    public partial class GameObject : GameObjectContainer
    {
        internal SynchronousList<GameObject> _childObjects = null;
        internal protected bool autoResizeChildObjects = true;

        /// <summary>
        /// Olion lapsioliot. Saa muuttaa.
        /// </summary>
        public SynchronousList<GameObject> Objects
        {
            get { InitChildren(); return _childObjects; }
        }

        /// <summary>
        /// Olion lapsiolioiden lukumäärä.
        /// Kuten Objects.Count, mutta optimoitu.
        /// </summary>
        public int ObjectCount
        {
            get { return _childObjects == null ? 0 : _childObjects.Count; }
        }

        /// <summary>
        /// Palauttaa olion lapsioliot.
        /// </summary>
        /// <typeparam name="T">Olion tyyppi rakenteessa (esim. PhysicsObject)</typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetChildObjects<T>() where T : IGameObject
        {
            foreach ( IGameObject o in Objects )
            {
                if ( o is T )
                    yield return (T)o;
            }
        }

        /// <summary>
        /// Palauttaa olion lapsioliot.
        /// </summary>
        /// <typeparam name="T">Olion tyyppi rakenteessa (esim. PhysicsObject)</typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetChildObjects<T>( Predicate<T> predicate ) where T : IGameObject
        {
            foreach ( IGameObject o in Objects )
            {
                if ( o is T && predicate( (T)o ) )
                    yield return (T)o;
            }
        }

        /// <summary>
        /// Palauttaa olion lapsioliot.
        /// </summary>
        public SynchronousList<GameObject> GetChildObjectList => Objects;

        /// <summary>
        /// Lisää annetun peliolion tämän olion lapseksi. Lapsiolio liikkuu tämän olion mukana.
        /// </summary>
        /// <remarks>
        /// <c>PhysicsObject</c>-tyyppiset oliot voi lisätä lapsiolioksi ainoastaan jos käytössä on Farseer-fysiikkamoottori.
        /// </remarks>
        public void Add(IGameObject childObject)
        {
            if (childObject == this)
                throw new InvalidOperationException("Child cannot be same as parent");

            if (this is PhysicsObject && childObject is PhysicsObject && PhysicsGameBase.Instance != null && PhysicsGameBase.Instance.FarseerGame)
            {
                PhysicsGameBase.Instance.Engine.ConnectBodies((PhysicsObject)this, (PhysicsObject)childObject);
            }
            
            if ( !( childObject is GameObject ) )
                throw new ArgumentException( "Child object can not be a non-GameObject" );

            Objects.Add( (GameObject)childObject );
            childObject.Parent = this;
            ((GameObject)childObject).InitialRelativePosition = childObject.RelativePositionToMainParent;
            ((GameObject)childObject).InitialRelativeAngle = childObject.RelativeAngleToMainParent;
        }

        /// <summary> 
        /// Poistaa lapsiolion. Jos haluat tuhota olion, 
        /// kutsu mielummin olion <c>Destroy</c>-metodia. 
        /// </summary> 
        /// <remarks> 
        /// Oliota ei poisteta välittömästi, vaan viimeistään seuraavan 
        /// päivityksen jälkeen. 
        /// </remarks> 
        public void Remove( IGameObject childObject )
        {
            if ( !( childObject is GameObject ) )
                throw new ArgumentException( "Child object can not be a non-GameObject" );

            Objects.Remove( (GameObject)childObject );
            childObject.Parent = null;
        }

        /// <summary>
        /// Alustaa lapsioliot
        /// </summary>
        protected virtual void InitChildren()
        {
            if ( _childObjects != null ) return;
            _childObjects = new SynchronousList<GameObject>();
            _childObjects.ItemAdded += this.OnChildAdded;
            _childObjects.ItemRemoved += this.OnChildRemoved;
            _childObjects.Changed += this.NotifyParentAboutChangedSizingAttributes;

            this.AddedToGame += () => _childObjects.ForEach(c => Game.OnAddObject(c));
            this.Removed += () => _childObjects.ForEach(c => Game.OnRemoveObject(c));

            // Objects list needs updating
            IsUpdated = true;
        }

        private void OnChildAdded( GameObject child )
        {
            child.Parent = this; 

            // It is possible to add children to objects which themselves are not yet (or not at the moment)
            // added to the game. This might not be obvious, since the _childObjects SynchronousList is
            // naturally not updated when the parent is not in game ? but there exist functions that
            // flush _childObjects' queued operations right away, and they may get called.
            // Therefore we need to ensure that Game.OnAddObject is only called if the parent is in game.
            if (this.IsAddedToGame)
                Game.OnAddObject(child);
        }

        private void OnChildRemoved( GameObject child )
        {
            // In the same vein as in OnChildAdded, it is possible that a child is removed from
            // _childObjects while not in game. This avoids multiple removal.
            if (child.IsAddedToGame)
                Game.OnRemoveObject(child);

            // This 'if' ensures that nothing is broken if a child is transferred
            // to another parent first and removed afterwards
            if ( child.Parent == this )
                child.Parent = null;
        }

        private void DestroyChildren()
        {
            if ( _childObjects == null ) return;

            foreach ( GameObject child in _childObjects )
            {
                child.Destroy();
            }
        }

        private void UpdateChildren( Time time )
        {
            Objects.Update( time );

            Objects.ForEach(o => {
                o.RelativePositionToMainParent = o.InitialRelativePosition;
                o.RelativeAngleToMainParent = o.InitialRelativeAngle;
            });
        }

        /// <summary>
        /// Antaa olion korkeimman tason isäntäolion.
        /// Eli vanhemman vanhemman...
        /// </summary>
        /// <returns></returns>
        public GameObject GetMainParent()
        {
            if (this.Parent is null) return this;
            else return ((GameObject)Parent).GetMainParent();
        }

        private void UpdateChildSizes( Vector oldSize, Vector newSize )
        {
            if ( _childObjects == null ) return;

            double xFactor = newSize.X / oldSize.X;
            double yFactor = newSize.Y / oldSize.Y;

            foreach ( var o in _childObjects )
            {
                Vector oldChildSize = o.Size;
                o.Size = new Vector( oldChildSize.X * xFactor, oldChildSize.Y * yFactor );

                //                    Vector direction = o.Position.Normalize();
                //                    double distance = o.Position.Magnitude;
                Vector oldChildPosition = o.Position;
                o.Position = new Vector( oldChildPosition.X * xFactor, oldChildPosition.Y * yFactor );
            }
        }

        private bool IsInsideChildren( Vector point )
        {
            if ( _childObjects == null ) return false;

            for ( int i = 0; i < _childObjects.Count; i++ )
            {
                if ( _childObjects[i].IsInside( point ) ) return true;
            }

            return false;
        }

        /// <summary>
        /// Poistaa kaikki lapsioliot.
        /// </summary>
        public virtual void Clear()
        {
            _childObjects.Clear();
        }
    }
}
