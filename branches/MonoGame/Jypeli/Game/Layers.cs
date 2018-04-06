﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jypeli.Controls;

namespace Jypeli
{
    public partial class Game : GameObjectContainer
    {
        /// <summary>
        /// Kerrokset, joilla pelioliot viihtyvät.
        /// </summary>
        public SynchronousList<Layer> Layers { get; private set; }

        /// <summary>
        /// Kerrokset, joilla olevat pelioliot eivät liiku kameran mukana.
        /// </summary>
        public IList<Layer> StaticLayers
        {
            get
            {
#if WINDOWS_STOREAPP
                return Layers.FindAll( l => l.IgnoresZoom && l.RelativeTransition == Vector.Zero );
#else
                return Layers.FindAll( l => l.IgnoresZoom && l.RelativeTransition == Vector.Zero ).AsReadOnly();
#endif
            }
        }

        /// <summary>
        /// Kerrokset, joilla olevat pelioliot liikkuvat kameran mukana.
        /// </summary>
        public IList<Layer> DynamicLayers
        {
#if WINDOWS_STOREAPP
            get { return Layers.FindAll( l => !l.IgnoresZoom || l.RelativeTransition != Vector.Zero ); }
#else
            get { return Layers.FindAll( l => !l.IgnoresZoom || l.RelativeTransition != Vector.Zero ).AsReadOnly(); }
#endif
        }

        /// <summary>
        /// Pienin mahdollinen kerros.
        /// </summary>
        public int MinLayer
        {
            get { return Layers.FirstIndex; }
        }

        /// <summary>
        /// Suurin mahdollinen kerros.
        /// </summary>
        public int MaxLayer
        {
            get { return Layers.LastIndex; }
        }

        /// <summary>
        /// Kerrosten määrä.
        /// </summary>
        public int LayerCount
        {
            get { return Layers.Count; }
        }

        /// <summary>
        /// Kuinka monta pelioliota pelissä on
        /// </summary>
        internal int ObjectCount
        {
            get
            {
                return Layers.Sum<Layer>( l => l.Objects.Count );
            }
        }

        private void InitLayers()
        {
            Layers = new SynchronousList<Layer>( -3 );
            Layers.ItemAdded += OnLayerAdded;
            Layers.ItemRemoved += OnLayerRemoved;

            for ( int i = 0; i < 7; i++ )
            {
                Layers.Add( new Layer() );
            }

            // This is the widget layer
            Layers.Add( Layer.CreateStaticLayer() );

            Layers.UpdateChanges();
        }

        protected virtual void OnObjectAdded( IGameObject obj )
        {
            IGameObjectInternal iObj = obj as IGameObjectInternal;
            if ( iObj == null ) return;
            iObj.IsAddedToGame = true;
            iObj.OnAddedToGame();

            ControlContexted cObj = obj as ControlContexted;
            if ( cObj != null ) ActivateObject( cObj );
        }

        protected virtual void OnObjectRemoved( IGameObject obj )
        {
            IGameObjectInternal iObj = obj as IGameObjectInternal;
            if ( iObj == null ) return;
            iObj.IsAddedToGame = false;
            iObj.OnRemoved();

            ControlContexted cObj = obj as ControlContexted;
            if ( cObj != null ) DeactivateObject( cObj );
        }

        internal static void OnAddObject( IGameObject obj )
        {
            Debug.Assert( Instance != null );
            Instance.OnObjectAdded( obj );
        }

        internal static void OnRemoveObject( IGameObject obj )
        {
            Debug.Assert( Instance != null );
            Instance.OnObjectRemoved( obj );
        }

        private void OnLayerAdded( Layer l )
        {
            l.Objects.ItemAdded += this.OnObjectAdded;
            l.Objects.ItemRemoved += this.OnObjectRemoved;
        }

        private void OnLayerRemoved( Layer l )
        {
            l.Objects.ItemAdded -= this.OnObjectAdded;
            l.Objects.ItemRemoved -= this.OnObjectRemoved;
        }

        /// <summary>
        /// Lisää olion peliin.
        /// Tavalliset oliot tulevat automaattisesti kerrokselle 0
        /// ja ruutuoliot päällimmäiselle kerrokselle.
        /// </summary>
        public void Add( IGameObject o )
        {
            if ( o.Layer != null && o.Layer.Objects.WillContain( o ) )
            {
                if ( o.Layer == Layers[0] )
                {
                    throw new NotSupportedException( "Object cannot be added twice" );
                }
                else
                    throw new NotSupportedException( "Object cannot be added to multiple layers" );
            }

            if ( o is Widget ) Add( o, MaxLayer );
            else Add( o, 0 );
        }

        /// <summary>
        /// Lisää peliolion peliin, tiettyyn kerrokseen.
        /// </summary>
        /// <param name="o">Lisättävä olio.</param>
        /// <param name="layer">Kerros, luku väliltä [-3, 3].</param>
        public virtual void Add( IGameObject o, int layer )
        {
            if ( o == null ) throw new NullReferenceException( "Tried to add a null object to game" );
            Layers[layer].Add( o );
        }

        internal static IList<IGameObject> GetObjectsAboutToBeAdded()
        {
            List<IGameObject> result = new List<IGameObject>();

            foreach ( Layer layer in Game.Instance.Layers )
            {
                layer.GetObjectsAboutToBeAdded( result );
            }

            return result;
        }

        /// <summary>
        /// Lisää oliokerroksen peliin.
        /// </summary>
        /// <param name="l"></param>
        public void Add( Layer l )
        {
            Layers.Add( l );
            Layers.UpdateChanges();
        }

        /// <summary> 
        /// Poistaa olion pelistä. Jos haluat tuhota olion, 
        /// kutsu mielummin olion <c>Destroy</c>-metodia. 
        /// </summary> 
        /// <remarks> 
        /// Oliota ei poisteta välittömästi, vaan viimeistään seuraavan 
        /// päivityksen jälkeen. 
        /// </remarks> 
        public void Remove( IGameObject o )
        {
            if ( !o.IsAddedToGame )
                return;

            foreach ( Layer l in Layers )
                l.Remove( o );
        }

        /// <summary>
        /// Poistaa oliokerroksen pelistä.
        /// </summary>
        /// <param name="l"></param>
        public void Remove( Layer l )
        {
            Layers.Remove( l );
            Layers.UpdateChanges();
        }

        /// <summary>
        /// Tuhoaa ja poistaa pelistä kaikki pelioliot (ml. fysiikkaoliot).
        /// </summary>
        public void ClearGameObjects()
        {
            foreach (var layer in Layers)
            {
                layer.Clear();
            }

            addMessageDisplay();
        }

        /// <summary>
        /// Nollaa oliokerrokset. Huom. tuhoaa kaikki pelioliot!
        /// </summary>
        /// <param name="l"></param>
        public void ResetLayers()
        {
            ClearGameObjects();

            foreach (var layer in Layers)
            {
                // Jos muutoksia ei päivitetä, niin taso ei ehdi oikeasti hävittää
                // olioitaan, koska InitLayers luo uudet tasot ja sen jälkeen
                // vanhat tasot muuttuvat roskaksi - siten esim. peliolioiden
                // OnRemoved-metodia ei kutsuta ilman tätä ApplyChanges()-kutsua
                layer.ApplyChanges();
            }

            InitLayers();
        }

        /// <summary>
        /// Poistaa kaikki oliokerrokset. Huom. tuhoaa kaikki pelioliot!
        /// </summary>
        /// <param name="l"></param>
        public void RemoveAllLayers()
        {
            ClearGameObjects();
            Layers.Clear();
        }

#region GetObject methods
        /// <summary>
        /// Palauttaa listan kaikista peliolioista jotka toteuttavat ehdon.
        /// Lista on järjestetty päällimmäisestä alimmaiseen.
        /// </summary>
        /// <param name="condition">Ehto</param>
        /// <returns>Lista olioista</returns>
        public List<GameObject> GetObjects( Predicate<GameObject> condition )
        {
            List<GameObject> objs = new List<GameObject>();

            for ( int i = MaxLayer; i >= MinLayer; i-- )
            {
                foreach ( var obj in Layers[i].Objects )
                {
                    GameObject gobj = obj as GameObject;

                    if ( gobj != null && condition( gobj ) )
                        objs.Add( gobj );
                }
            }

            return objs;
        }

         /// <summary>
        /// Palauttaa listan kaikista peliolioista.
        /// Lista on järjestetty päällimmäisestä alimmaiseen.
        /// </summary>
        /// <returns>Listan olioista.</returns>
        public List<GameObject> GetAllObjects()
        {
            return GetObjects(g => true);
        }
        
        /// <summary>
        /// Palauttaa listan kaikista peliolioista joilla on tietty tagi.
        /// Lista on järjestetty päällimmäisestä alimmaiseen.
        /// </summary>
        /// <param name="tags">Tagi(t)</param>
        /// <returns>Lista olioista</returns>
        public List<GameObject> GetObjectsWithTag( params string[] tags )
        {
            return GetObjects( o => tags.Contains<string>( o.Tag as string ) );
        }

        /// <summary>
        /// Palauttaa ensimmäisen peliolion joka toteuttaa ehdon (null jos mikään ei toteuta).
        /// </summary>
        /// <param name="condition">Ehto</param>
        /// <returns>Olio</returns>
        public GameObject GetFirstObject( Predicate<GameObject> condition )
        {
            for ( int i = MaxLayer; i >= MinLayer; i-- )
            {
                foreach ( var obj in Layers[i].Objects )
                {
                    GameObject gobj = obj as GameObject;

                    if ( gobj != null && condition( gobj ) )
                        return gobj;
                }
            }

            return null;
        }

        /// <summary>
        /// Palauttaa listan peliolioista, jotka ovat annetussa paikassa.
        /// Jos paikassa ei ole mitään pelioliota, palautetaan tyhjä lista.
        /// Lista on järjestetty päällimmäisestä alimmaiseen.
        /// </summary>
        /// <param name="position">Paikkakoordinaatit</param>
        /// <returns>Lista olioista</returns>
        public List<GameObject> GetObjectsAt( Vector position )
        {
            return GetObjects( obj => obj.IsInside( position ) );
        }

        /// <summary>
        /// Palauttaa listan peliolioista, jotka ovat annetussa paikassa tietyllä säteellä.
        /// Jos paikassa ei ole mitään pelioliota, palautetaan tyhjä lista.
        /// Lista on järjestetty päällimmäisestä alimmaiseen.
        /// </summary>
        /// <param name="position">Paikkakoordinaatit</param>
        /// <param name="radius">Säde jolla etsitään</param>
        /// <returns>Lista olioista</returns>
        public List<GameObject> GetObjectsAt( Vector position, double radius )
        {
            Predicate<GameObject> isInsideRadius = delegate( GameObject obj )
            {
                if ( IsJypeliWidget<GameObject>( obj ) ) return false;

                Vector positionUp = new Vector( position.X, position.Y + radius );
                Vector positionDown = new Vector( position.X, position.Y - radius );
                Vector positionLeft = new Vector( position.X - radius, position.Y );
                Vector positionRight = new Vector( position.X + radius, position.Y );

                if ( obj.IsInside( position ) ) return true;
                if ( obj.IsInside( positionUp ) ) return true;
                if ( obj.IsInside( positionDown ) ) return true;
                if ( obj.IsInside( positionLeft ) ) return true;
                if ( obj.IsInside( positionRight ) ) return true;

                return false;
            };

            return GetObjects( isInsideRadius );
        }

        /// <summary>
        /// Palauttaa peliolion, joka on annetussa paikassa.
        /// Jos paikassa ei ole mitään pelioliota, palautetaan null.
        /// Jos olioita on useampia, palautetaan päällimmäinen.
        /// </summary>
        /// <param name="position">Paikkakoordinaatit</param>
        /// <returns>Mahdollinen olio</returns>
        public GameObject GetObjectAt( Vector position )
        {
            return GetFirstObject( obj => obj.IsInside( position ) && !IsJypeliWidget<GameObject>( obj ) );
        }

        /// <summary>
        /// Palauttaa peliolion, joka on annetussa paikassa tietyllä säteellä.
        /// Jos paikassa ei ole mitään pelioliota, palautetaan null.
        /// Jos olioita on useampia, palautetaan ensin lisätty.
        /// </summary>
        /// <param name="position">Paikkakoordinaatit</param>
        /// <param name="radius">Säde jolla etsitään</param>
        /// <returns>Mahdollinen olio</returns>
        public GameObject GetObjectAt( Vector position, double radius )
        {
            var objs = GetObjectsAt( position, radius );
            return objs.Count > 0 ? objs[0] : null;
        }

        /// <summary>
        /// Palauttaa listan peliolioista, jotka ovat annetussa paikassa tietyllä säteellä.
        /// Jos paikassa ei ole mitään pelioliota, palautetaan tyhjä lista.
        /// Lista on järjestetty päällimmäisestä alimmaiseen.
        /// Vain annetulla tagilla varustetut oliot huomioidaan.
        /// </summary>
        /// <param name="position">Paikkakoordinaatit</param>
        /// <param name="tag">Etsittävän olion tagi.</param>
        /// <returns>Lista olioista</returns>
        public List<GameObject> GetObjectsAt( Vector position, object tag )
        {
            return GetObjectsAt( position ).FindAll( obj => obj.Tag == tag );
        }

        /// <summary>
        /// Palauttaa peliolion, joka on annetussa paikassa.
        /// Vain annetulla tagilla varustetut oliot huomioidaan.
        /// Jos paikassa ei ole mitään pelioliota, palautetaan null.
        /// Jos olioita on useampia, palautetaan ensin lisätty.
        /// </summary>
        /// <param name="position">Paikkakoordinaatit</param>
        /// <param name="tag">Etsittävän olion tagi.</param>
        /// <returns>Mahdollinen olio</returns>
        public GameObject GetObjectAt( Vector position, object tag )
        {
            return GetObjectsAt( position ).Find( obj => obj.Tag == tag );
        }

        /// <summary>
        /// Palauttaa listan peliolioista, jotka ovat annetussa paikassa tietyllä säteellä.
        /// Jos paikassa ei ole mitään pelioliota, palautetaan tyhjä lista.
        /// Lista on järjestetty päällimmäisestä alimmaiseen.
        /// Vain annetulla tagilla varustetut oliot huomioidaan.
        /// </summary>
        /// <param name="position">Paikkakoordinaatit</param>
        /// <param name="tag">Etsittävän olion tagi.</param>
        /// <param name="radius">Säde jolla etsitään</param>
        /// <returns>Lista olioista</returns>
        public List<GameObject> GetObjectsAt( Vector position, object tag, double radius )
        {
            return GetObjectsAt( position, radius ).FindAll<GameObject>( obj => obj.Tag == tag );
        }

        /// <summary>
        /// Palauttaa peliolion, joka on annetussa paikassa tietyllä säteellä.
        /// Vain annetulla tagilla varustetut oliot huomioidaan.
        /// Jos paikassa ei ole mitään pelioliota, palautetaan null.
        /// Jos olioita on useampia, palautetaan ensin lisätty.
        /// </summary>
        /// <param name="position">Paikkakoordinaatit</param>
        /// <param name="tag">Etsittävän olion tagi.</param>
        /// <param name="radius">Säde jolla etsitään</param>
        /// <returns>Mahdollinen olio</returns>
        public GameObject GetObjectAt( Vector position, object tag, double radius )
        {
            return GetObjectsAt( position, radius ).Find( obj => obj.Tag == tag );
        }

        /// <summary>
        /// Palauttaa ensimmäisen ruutuolion joka toteuttaa ehdon (null jos mikään ei toteuta).
        /// </summary>
        /// <param name="condition">Ehto</param>
        /// <returns>Lista olioista</returns>
        public Widget GetFirstWidget( Predicate<Widget> condition )
        {
            return (Widget)GetFirstObject( obj => obj is Widget && condition( (Widget)obj ) );
        }

        /// <summary>
        /// Palauttaa ruutuolion, joka on annetussa paikassa.
        /// Jos paikassa ei ole mitään oliota, palautetaan null.
        /// Jos olioita on useampia, palautetaan päällimmäinen.
        /// </summary>
        /// <param name="position">Paikkakoordinaatit</param>
        /// <returns>Mahdollinen ruutuolio</returns>
        public Widget GetWidgetAt( Vector position )
        {
            return (Widget)GetFirstObject( obj => obj is Widget && obj.IsInside( position ) && !IsJypeliWidget<IGameObject>( obj ) );
        }
#endregion
    }
}
