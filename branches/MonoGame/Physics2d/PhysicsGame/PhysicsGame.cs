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
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 */

using System;
using Physics2DDotNet;
using Physics2DDotNet.PhysicsLogics;
using Physics2DDotNet.Joints;
using Physics2DDotNet.Solvers;
using AdvanceMath;
using System.Collections.Generic;
using Physics2DDotNet.Ignorers;

namespace Jypeli
{
    /// <summary>
    /// Peli, jossa on fysiikan laskenta mukana. Peliin lisätyt <code>PhysicsObject</code>-oliot
    /// käyttäytyvät fysiikan lakien mukaan.
    /// </summary>
    public class PhysicsGame : PhysicsGameBase
    {
        private GravityField gravityfield;
        private Vector gravity = Vector.Zero;

        static internal Dictionary<int, ObjectIgnorer> IgnoreGroups = null;

        /// <summary>
        /// Painovoima. Voimavektori, joka vaikuttaa kaikkiin ei-staattisiin kappaleisiin.
        /// </summary>
        public Vector Gravity
        {
            get
            {
                return gravity;
            }
            set
            {
                gravity = value;
                updatePhysicsConstants();
            }
        }

        /// <summary>
        /// Alustaa uuden fysiikkapelin.
        /// </summary>
        public PhysicsGame()
            : base()
        {
            phsEngine.BroadPhase = new Physics2DDotNet.Detectors.SelectiveSweepDetector();
            //phsEngine.BroadPhase = new Physics2DDotNet.Detectors.SpatialHashDetector();

            SequentialImpulsesSolver phsSolver = new SequentialImpulsesSolver();
            phsSolver.Iterations = 12;
            phsSolver.SplitImpulse = true;
            //phsSolver.BiasFactor = 0.7;
            phsSolver.BiasFactor = 0.0;
            //phsSolver.AllowedPenetration = 0.1;
            phsSolver.AllowedPenetration = 0.01;
            phsEngine.Solver = (CollisionSolver)phsSolver;
        }

        private void updatePhysicsConstants()
        {
            if ( gravityfield != null ) gravityfield.Lifetime.IsExpired = true;

            if ( gravity != Vector.Zero )
            {
                gravityfield = new GravityField( new Vector2D( gravity.X, gravity.Y ), new Lifespan() );
                phsEngine.AddLogic( gravityfield );
            }
        }       
    }
}
