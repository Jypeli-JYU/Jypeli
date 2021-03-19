#region MIT License
/*
 * Copyright (c) 2005-2008 Jonathan Mark Porter. http://physics2d.googlepages.com/
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to deal 
 * in the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of 
 * the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be 
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion



#if UseDouble
using Scalar = System.Double;
#else
using Scalar = System.Single;
#endif
using System;
using System.Collections.ObjectModel;

using AdvanceMath;

namespace Physics2DDotNet
{
    public interface IDuplicateable<T>
        where T : IDuplicateable<T>
    {
        T Duplicate();
    }
    public interface IPendable
    {
        event EventHandler Pending;
        event EventHandler Added;
        event EventHandler<RemovedEventArgs> Removed;
        event EventHandler LifetimeChanged;
        bool IsPending { get;}
        bool IsAdded { get;}
        Lifespan Lifetime { get; set;}
        object Tag { get; set;}
    }
    public interface IPhysicsEntity : IPendable
    {
        PhysicsEngine Engine { get;}
    }
    public interface IJoint : IPhysicsEntity
    {
        void CheckFrozen();
        ReadOnlyCollection<Body> Bodies { get;}
    }
    /// <summary>
    /// Describes a Contact in a collision.
    /// </summary>
    public interface IContactPointInfo
    {
        /// <summary>
        /// Gets The world coordinates of the contact.
        /// </summary>
        Vector2D Position { get;}
        /// <summary>
        /// Gets a Direction Vector Pointing away from the Edge.
        /// </summary>
        Vector2D Normal { get;}
        /// <summary>
        /// Gets The distance the contact is inside the other object.
        /// </summary>
        Scalar Distance { get;}
    }

    public enum ContactState
    {
        New,
        Old,
        Ended
    }

    public interface IContact
    {
         event EventHandler Updated;
         event EventHandler Ended;

        /// <summary>
        /// Gets The First Body that is part of the Contact.
        /// (The Normal belongs to this Body.)
        /// </summary>
        Body Body1 { get;}
        /// <summary>
        /// Gets The Second Body that is part of the Contact.
        /// (The Position of the Vertex belongs to this Body.)
        /// </summary>
        Body Body2 { get;}
        bool IgnoresCollisionResponse { get;}
        ContactState State { get;}
        ReadOnlyCollection<IContactPointInfo> Points { get;}
    }
}