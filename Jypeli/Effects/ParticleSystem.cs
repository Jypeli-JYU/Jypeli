using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Jypeli.Rendering;
using Matrix = System.Numerics.Matrix4x4;

namespace Jypeli.Effects
{
    /// <summary>
    /// Sekoitusmoodi, kuinka päällekkäin olevat osittain läpinäkyvät kappaleet näkyvät
    /// </summary>
    public enum BlendMode
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        Alpha, // TODO: Näiden käyttö.
        Additive
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }


    /// <summary>
    /// Järjestelmä partikkelien käsittelyyn
    /// </summary>
    public unsafe class ParticleSystem : GameObject
    {
        private unsafe struct Particledata
        {
            public Vector2 Position; // Vector2 koska float
            public float Rotation;
            public float Scale;
            public fixed float Color[4];
        }

        private Random random = new Random();

        // Lista efektin partikkeleille
        private LinkedList<Particle> particles;

        // Jono efektin vapaille partikkeleille
        private Queue<Particle> freeParticles;

        private int maxParticles = 20;

        private Particledata[] particledata;// x, y, rot, scale, alpha

        /// <summary>
        /// Yksittäisen partikkelin kuva.
        /// </summary>
        public Image ParticleImage { get; set; }

        /// <summary>
        /// Partikkelin toissijainen kuva. Jos <c>null</c> niin käytetään vain yhtä kuvaa.
        /// </summary>
        public Image OuterParticleImage { get; set; }

        // Perivien luokkien täytyy määrittää nämä muuttujat
        #region Subclass variables

        /// <summary>
        /// Pienin skaalaus joka efektin partikkeleilla voi olla
        /// </summary>
        public double MinScale { get; set; }

        /// <summary>
        /// Suurin skaalaus joka efektin partikkeleilla voi olla
        /// </summary>
        public double MaxScale { get; set; }

        /// <summary>
        /// Määrä jonka partikkeli skaalautuu päivityksessä
        /// </summary>
        public double ScaleAmount { get; set; }

        /// <summary>
        /// Pienin nopeus joka efektin partikkelilla voi olla
        /// </summary>
        public double MinVelocity { get; set; }
        /// <summary>
        /// Suurin nopeus joka efektin partikkelilla voi olla
        /// </summary>
        public double MaxVelocity { get; set; }

        /// <summary>
        /// Efektin partikkelin lyhin mahdollinen elinaika
        /// </summary>
        public double MinLifetime { get; set; }

        /// <summary>
        /// Efektin partikkelin pisin mahdollinen elinaika
        /// </summary>
        public double MaxLifetime { get; set; }

        /// <summary>
        /// Pienin kiihtyvyys joka efektin partikkelilla voi olla
        /// </summary>
        public double MinAcceleration { get; set; }

        /// <summary>
        /// Suurin kiihtyvyys joka efektin partikkelilla voi olla
        /// </summary>
        public double MaxAcceleration { get; set; }

        /// <summary>
        /// Pienin pyörimisnopeus joka efektin partikkelilla voi olla
        /// </summary>
        public double MinRotationSpeed { get; set; }

        /// <summary>
        /// Suurin pyörimisnopeus joka efektin partikkelilla voi olla
        /// </summary>
        public double MaxRotationSpeed { get; set; }

        /// <summary>
        /// Pienin kierre mikä efektin partikkelilla voi olla
        /// </summary>
        public double MinRotation { get; set; }

        /// <summary>
        /// Suurin kierre mikä efektin partikkelilla voi olla
        /// </summary>
        public double MaxRotation { get; set; }

        /// <summary>
        /// Efektin läpinäkyvyyskerroin (0.0-1.0)
        /// </summary>
        public double AlphaAmount { get; set; }

        /// <summary>
        /// Suurin sallittu suunnan poikkeama asteina
        /// </summary>
        public double MaxAngleChange { get; set; }

        #endregion

        /// <summary>
        /// Sekoitusmoodi
        /// </summary>
        public BlendMode BlendMode { get; set; }

        /// <summary>
        /// Vaikuttaako efektiin tuuli
        /// </summary>
        public bool IgnoreWind { get; set; }

        private bool visible = true;
        private bool fadeIn = false;
        private bool fadeOut = false;
        private double originalAlpha = 1.0;
        private double fadeTime = 0.0;
        private double fadeTimePassed = 0.0;

        /// <summary>
        /// Efekti tulee näkyviin tietyn sekuntimäärän aikana
        /// </summary>
        /// <param name="timeInSeconds">Aika joka kuluu että efekti on näkyvä</param>
        public void FadeIn(double timeInSeconds)
        {
            if (!visible)
            {
                fadeTimePassed = 0.0;
                fadeIn = true;
                fadeTime = timeInSeconds;
            }
        }

        /// <summary>
        /// Efekti hiipuu näkyvistä tietyn sekuntimäärän aikana
        /// </summary>
        /// <param name="TimeInSeconds">Aika joka kuluu että efekti katoaa</param>
        public void FadeOut(double TimeInSeconds)
        {
            if (visible)
            {
                originalAlpha = AlphaAmount;
                fadeTimePassed = 0.0;
                fadeOut = true;
                fadeTime = TimeInSeconds;
            }
        }
        /// <summary>
        /// Muodostaja
        /// </summary>
        /// <param name="particleImage">Partikkelin kuva.</param>
        /// <param name="maxAmountOfParticles">Suurin määrä partikkeleita mitä efektillä voi olla kerralla</param>
        public ParticleSystem(Image particleImage, int maxAmountOfParticles)
            : base(0, 0)
        {
            this.ParticleImage = particleImage;
            AlphaAmount = 1.0;
            IgnoreWind = false;
            BlendMode = BlendMode.Alpha;

            IsUpdated = true;
            InitializeParticles();

            particles = new LinkedList<Particle>();
            freeParticles = new Queue<Particle>(maxAmountOfParticles);
            for (int i = 0; i < maxAmountOfParticles; i++)
            {
                freeParticles.Enqueue(new Particle());
            }

            gl = ((Rendering.OpenGl.GraphicsDevice)Game.GraphicsDevice).Gl;
#if ANDROID
            shader = new Rendering.OpenGl.Shader(gl, Game.ResourceContent.LoadInternalText("Shaders.OpenGLES.ParticleVertexShader.glsl"), Game.ResourceContent.LoadInternalText("Shaders.OpenGLES.DefaultTextureShader.glsl"));
#elif DESKTOP
            shader = new Rendering.OpenGl.Shader(gl, Game.ResourceContent.LoadInternalText("Shaders.OpenGL.ParticleVertexShader.glsl"), Game.ResourceContent.LoadInternalText("Shaders.OpenGL.DefaultTextureShader.glsl"));
#endif
            particledata = new Particledata[maxAmountOfParticles]; // Varataan näytönohjaimelta muistia jokaiselle partikkelille.

            vertexbuffer = new Rendering.OpenGl.BufferObject<VertexPositionColorTexture>(gl, vertices, Silk.NET.OpenGL.BufferTargetARB.ArrayBuffer);
            Vao = new Rendering.OpenGl.VertexArrayObject<VertexPositionColorTexture, Vector4>(gl, vertexbuffer, null);

            // TODO: Tämä on hieman ruma ja kaipaisi jonkinlaista abstraktiota.
            Vao.VertexAttributePointer(0, 3, Silk.NET.OpenGL.VertexAttribPointerType.Float, (uint)sizeof(VertexPositionColorTexture), 0);
            Vao.VertexAttributePointer(1, 4, Silk.NET.OpenGL.VertexAttribPointerType.Float, (uint)sizeof(VertexPositionColorTexture), 12);
            Vao.VertexAttributePointer(2, 2, Silk.NET.OpenGL.VertexAttribPointerType.Float, (uint)sizeof(VertexPositionColorTexture), 28);

            databuffer = new Rendering.OpenGl.BufferObject<Particledata>(gl, particledata, Silk.NET.OpenGL.BufferTargetARB.ArrayBuffer); // x, y, kulma, koko, läpinäkyvyys

            databuffer.Bind();
            gl.EnableVertexAttribArray(3);
            gl.VertexAttribPointer(3, 2, Silk.NET.OpenGL.VertexAttribPointerType.Float, false, (uint)sizeof(Particledata), (void*)0);
            gl.VertexAttribDivisor(3, 1);

            gl.EnableVertexAttribArray(4);
            gl.VertexAttribPointer(4, 1, Silk.NET.OpenGL.VertexAttribPointerType.Float, false, (uint)sizeof(Particledata), (void*)8); // Vika parametri on offsetin määrä tavuissa.
            gl.VertexAttribDivisor(4, 1);

            gl.EnableVertexAttribArray(5);
            gl.VertexAttribPointer(5, 1, Silk.NET.OpenGL.VertexAttribPointerType.Float, false, (uint)sizeof(Particledata), (void*)12);
            gl.VertexAttribDivisor(5, 1);

            gl.EnableVertexAttribArray(6);
            gl.VertexAttribPointer(6, 4, Silk.NET.OpenGL.VertexAttribPointerType.Float, false, (uint)sizeof(Particledata), (void*)16);
            gl.VertexAttribDivisor(6, 1);

            gl.BindBuffer(Silk.NET.OpenGL.BufferTargetARB.ArrayBuffer, 0);
        }
        Rendering.OpenGl.VertexArrayObject<VertexPositionColorTexture, Vector4> Vao;
        Rendering.OpenGl.BufferObject<VertexPositionColorTexture> vertexbuffer;
        Rendering.OpenGl.BufferObject<Particledata> databuffer;

        Silk.NET.OpenGL.GL gl;
        IShader shader;

        /// <summary>
        /// Metodi joka asettaa partikkeleille attribuutit
        /// Täytyy kutsua perityistä luokista
        /// </summary>
        protected virtual void InitializeParticles()
        {
        }

        /// <summary>
        /// Lisää efektin kentälle
        /// </summary>
        /// <param name="x">Efektin x-koordinaatti</param>
        /// <param name="y">Efektin y-koordinaatti</param>
        /// <param name="numberOfParticles">Partikkeleiden määrä efektissä</param>
        public void AddEffect(double x, double y, int numberOfParticles)
        {
            AddEffect(new Vector(x, y), numberOfParticles);
        }

        /// <summary>
        /// Lisää efektin kentälle
        /// </summary>
        /// <param name="position"></param>
        /// <param name="angle"></param>
        /// <param name="numberOfParticles"></param>
        public void AddEffect(Vector position, Angle angle, int numberOfParticles)
        {
            for (int i = 0; i < numberOfParticles && freeParticles.Count > 0; i++)
            {
                Particle p = freeParticles.Dequeue();
                particles.AddLast(p);
                InitializeParticle(p, position, angle);
            }
        }

        /// <summary>
        /// Lisää efektin kentälle
        /// </summary>
        /// <param name="position">Paikka.</param>
        /// <param name="numberOfParticles">Partikkeleiden määrä efektissä</param>
        public void AddEffect(Vector position, int numberOfParticles)
        {
            for (int i = 0; i < numberOfParticles && freeParticles.Count > 0; i++)
            {
                Particle p = freeParticles.Dequeue();
                particles.AddLast(p);
                InitializeParticle(p, position);
            }
        }

        /// <summary>
        /// Antaa satunnaisen suunnan
        /// </summary>
        /// <returns>Satunnainen suunta</returns>
        protected virtual Vector GiveRandomDirection()
        {
            double angle = random.NextDouble() * MathHelper.TwoPi;
            return new Vector(Math.Cos(angle), Math.Sin(angle));
        }

        /// <summary>
        /// Alustaa yhden partikkelin
        /// </summary>
        /// <param name="p">Partikkeli joka alustetaan</param>
        /// <param name="position">Sijainti johon alustetaan</param>
        protected virtual void InitializeParticle(Particle p, Vector position)
        {
            Vector direction = GiveRandomDirection();

            double scale = RandomGen.NextDouble(MinScale, MaxScale);
            double rotation = RandomGen.NextDouble(MinRotation, MaxRotation);
            double velocity = RandomGen.NextDouble(MinVelocity, MaxVelocity);
            double lifetime = RandomGen.NextDouble(MinLifetime, MaxLifetime);
            double acceleration = RandomGen.NextDouble(MinAcceleration, MaxAcceleration);
            double rotationSpeed = RandomGen.NextDouble(MinRotationSpeed, MaxRotationSpeed);

            p.Initialize(position, scale, rotation, rotationSpeed, velocity * direction, acceleration * direction, lifetime);
        }

        /// <summary>
        /// Alustaa yhden partikkelin
        /// </summary>
        /// <param name="p">Partikkeli joka alustetaan</param>
        /// <param name="position">Sijainti johon alustetaan</param>
        /// <param name="angle">Partikkelin alkukulma</param>
        protected virtual void InitializeParticle(Particle p, Vector position, Angle angle)
        {
            Vector direction = GiveRandomDirection();

            double rotation = angle.Degrees;
            double scale = RandomGen.NextDouble(MinScale, MaxScale);
            double velocity = RandomGen.NextDouble(MinVelocity, MaxVelocity);
            double lifetime = RandomGen.NextDouble(MinLifetime, MaxLifetime);
            double acceleration = RandomGen.NextDouble(MinAcceleration, MaxAcceleration);
            double rotationSpeed = RandomGen.NextDouble(MinRotationSpeed, MaxRotationSpeed);

            p.Initialize(position, scale, rotation, rotationSpeed, velocity * direction, acceleration * direction, lifetime);
        }

        /// <summary>
        /// Kutsutaan kun luokka päivitetään
        /// </summary>
        /// <param name="time">Gametime</param>
        public override void Update(Time time)
        {
            double t = time.SinceLastUpdate.TotalSeconds;

            LinkedListNode<Particle> node = particles.First;
            while (node != null)
            {
                Particle p = node.Value;
                if (p.Alive)
                {
                    p.Update(t);
                    node = node.Next;
                }
                else
                {
                    freeParticles.Enqueue(p);
                    LinkedListNode<Particle> previous = node;
                    node = node.Next;
                    particles.Remove(previous);
                }
            }
            if (fadeOut)
            {
                AlphaAmount = Math.Min(1 - fadeTimePassed / fadeTime, originalAlpha);
                fadeTimePassed += t;
                if (fadeTime <= fadeTimePassed)
                {
                    AlphaAmount = 0.0;
                    fadeOut = false;
                    visible = false;
                }
            }
            if (fadeIn)
            {
                AlphaAmount = Math.Min(fadeTimePassed / fadeTime, originalAlpha);
                fadeTimePassed += t;
                if (fadeTime <= fadeTimePassed)
                {
                    AlphaAmount = originalAlpha;
                    fadeIn = false;
                    visible = true;
                }
            }

            base.Update(time);
        }


        private static readonly Vector textureTopLeft = new Vector(0.0f, 0.0f);
        private static readonly Vector textureBottomLeft = new Vector(0.0f, 1.0f);
        private static readonly Vector textureTopRight = new Vector(1.0f, 0.0f);
        private static readonly Vector textureBottomRight = new Vector(1.0f, 1.0f);

        private static readonly Vector3 topLeft = new Vector3(-0.5f, 0.5f, 0);
        private static readonly Vector3 bottomLeft = new Vector3(-0.5f, -0.5f, 0);
        private static readonly Vector3 topRight = new Vector3(0.5f, 0.5f, 0);
        private static readonly Vector3 bottomRight = new Vector3(0.5f, -0.5f, 0);

        private static readonly VertexPositionColorTexture[] vertices =
        {
            // Triangle 1
            new VertexPositionColorTexture(bottomLeft, Color.Transparent, textureBottomLeft),
            new VertexPositionColorTexture(topLeft, Color.Transparent, textureTopLeft),
            new VertexPositionColorTexture(bottomRight, Color.Transparent, textureBottomRight),

            // Triangle 2
            new VertexPositionColorTexture(bottomRight, Color.Transparent, textureBottomRight),
            new VertexPositionColorTexture(topLeft, Color.Transparent, textureTopLeft),
            new VertexPositionColorTexture(topRight, Color.Transparent, textureTopRight),
        };

        internal unsafe void Draw(Matrix worldMatrix)
        {
            if (particles.Count == 0)
                return;
            // TODO: Pitäisikö particleilla muuttaa kaikki floateiksi, ei ole mitään syytä miksi niiden pitäisi olla doubleja.
            // Tai eikö particlen voisi suoraan muuttaa structiksi jolloin koko tätä kopiointia ei tarvitsisi tehdä?
            int i = 0;
            foreach (Particle p in particles)
            {
                double nTime = p.Lifetime.TotalMilliseconds / p.MaxLifetime.TotalMilliseconds;

                Particledata pdata = new Particledata();
                pdata.Position = p.Position;
                pdata.Rotation = (float)p.Rotation;
                pdata.Scale = (float)p.Scale;
                pdata.Color[0] = 1;
                pdata.Color[1] = 1;
                pdata.Color[2] = 1;
                pdata.Color[3] = (float)(4 * nTime * (1 - nTime) * AlphaAmount);
                particledata[i++] = pdata;
            }

            databuffer.UpdateBuffer(0, particledata);

            var device = Game.GraphicsDevice;

            shader.Use();
            shader.SetUniform("world", worldMatrix * device.View * device.Projection);

            if (OuterParticleImage is not null)
            {
                device.BindTexture(OuterParticleImage);

                shader.SetUniform("scale", 1f);

                Vao.Bind();

                gl.DrawArraysInstanced(Silk.NET.OpenGL.GLEnum.Triangles, 0, 6, (uint)particles.Count);
            }

            device.BindTexture(ParticleImage);
            shader.SetUniform("scale", 0.75f);
            Vao.Bind();

            gl.DrawArraysInstanced(Silk.NET.OpenGL.GLEnum.Triangles, 0, 6, (uint)particles.Count);
        }
    }
}