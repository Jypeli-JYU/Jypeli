namespace Jypeli
{
    public partial class PhysicsObject
    {
        /// <summary>
        /// Olion massa (paino).
        /// Mitä enemmän massaa, sitä enemmän voimaa tarvitaan saamaan olio liikkeelle / pysähtymään.
        /// </summary>
        /// <value>The mass.</value>
        public double Mass
        {
            get { return Body.Mass; }
            set
            {
                Body.Mass = value;
            }
        }

        /// <summary>
        /// Olion hitausmomentti eli massa/paino kääntyessä.
        /// Mitä suurempi, sitä hitaampi olio on kääntymään / sitä enemmän vääntöä tarvitaan.
        /// Äärettömällä hitausmomentilla olio ei käänny lainkaan (paitsi suoraan kulmaa muuttamalla).
        /// </summary>
        /// <value>The moment of inertia.</value>
        public double MomentOfInertia
        {
            get { return Body.MomentOfInertia; }
            set
            {
                Body.MomentOfInertia = value;
            }
        }

        /// <summary>
        /// Jos <c>false</c>, olio ei voi pyöriä.
        /// </summary>
        public bool CanRotate
        {
            get { return Body.CanRotate; }
            set
            {
                Body.CanRotate = value;
            }
        }

        /// <summary>
        /// Nopeuskerroin.
        /// Pienempi arvo kuin 1 (esim. 0.998) toimii kuten kitka / ilmanvastus.
        /// </summary>
        /// <value>The linear damping.</value>
        public double LinearDamping
        {
            get { return Body.LinearDamping; }
            set { Body.LinearDamping = value; }
        }

        /// <summary>
        /// Kulmanopeuskerroin.
        /// Pienempi arvo kuin 1 (esim. 0.998) toimii kuten kitka / ilmanvastus.
        /// </summary>
        /// <value>The angular damping.</value>
        public double AngularDamping
        {
            get { return Body.AngularDamping; }
            set { Body.AngularDamping = value; }
        }
    }
}
