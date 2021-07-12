using System.Collections.Generic;

namespace Jypeli.Effects
{
    internal class LightningLayer
    {
        public List<LightningNode> Nodes
        {
            get { return nodes; }
            set { nodes = value; }
        }

        private List<LightningNode> nodes;

        private double y;
        public double Y { get { return y; } set { y = value; } }

        public LightningLayer()
        {
            nodes = new List<LightningNode>();
        }

        public void Add(LightningNode n)
        {
            nodes.Add(n);
        }
    }

    internal class LightningNode
    {
        private LightningNode parentNode;
        private List<LightningNode> childNodes;
        public List<LightningNode> ChildNodes { get { return childNodes; } }

        private double splitChance = 0;
        public double SplitChance { get { return splitChance; } }

        private Vector position;
        public Vector Position { get { return position; } set { position = value; } }
        public LightningNode()
        {
            childNodes = new List<LightningNode>();
        }

        public LightningNode(Vector position)
        {
            this.position = position;
            childNodes = new List<LightningNode>();
        }

        public void AddChild(Vector position)
        {
            AddChild(new LightningNode(position));
        }

        public void AddChild(LightningNode node)
        {
            node.parentNode = this;
            this.childNodes.Add(node);
        }
    }

    /// <summary>
    /// Salama
    /// </summary>
    public class Lightning : ParticleSystem
    {
        private LightningLayer[] layers;
        //public LightningLayer[] Layers { get { return layers; } }
        //private List<LightningNode> nodes;
        private int layerAmount = 7;

        private int currentLayer;

        //private double height;
        //private double width;

        private bool striking = false;
        private double strikeSpeed = 0.0;

        //private Color levelColor;

        private int particlesPerLayer;

        /// <summary>
        /// Salama
        /// </summary>
        /// <param name="particleImage">Partikkelien kuva</param>
        /// <param name="maxAmountOfParticles">Kuinka monta partikkelia enimmillään luodaan</param>
        public Lightning(Image particleImage, int maxAmountOfParticles)
            : base(particleImage, maxAmountOfParticles)
        {
            InitializeLayers();
            particlesPerLayer = maxAmountOfParticles / layers.Length;
        }

        /// <inheritdoc/>
        protected override void InitializeParticle(Particle p, Vector position)
        {
            p.Initialize(position, 2.0, 0.0, 0.0, Vector.Zero, Vector.Zero, 0.4);
        }

        /// <inheritdoc/>
        public override void Update(Time time)
        {
            double t = time.SinceLastUpdate.TotalSeconds;
            strikeSpeed += t;
            if (striking)
            {
                if (strikeSpeed > 0.01)
                {
                    foreach (LightningNode node in layers[currentLayer].Nodes)
                    {
                        foreach (LightningNode child in node.ChildNodes)
                        {
                            Vector v = child.Position - node.Position;
                            int particlesPerBranch = particlesPerLayer / node.ChildNodes.Count;
                            for (int i = 0; i < particlesPerBranch; i++)
                            {
                                base.AddEffect(node.Position + v*(i/(double)particlesPerBranch), 1);
                            }
                        }
                        //base.AddEffect(node.Position, 1);
                    }
                    currentLayer++;
                    strikeSpeed = 0.0;
                    if (currentLayer >= layers.Length)
                    {
                        striking = false;
                        currentLayer = 0;
                    }
                }
            }
            base.Update(time);
        }

        private void InitializeLayers()
        {
            layers = new LightningLayer[layerAmount];
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i] = new LightningLayer();
            }
        }

        /// <summary>
        /// Luo salamaniskun lähtien annetusta pisteestä
        /// </summary>
        /// <param name="startPoint">Lähtöpiste</param>
        /// <param name="width">Leveys</param>
        /// <param name="height">Korkeus</param>
        public void Strike(Vector startPoint, double width, double height)
        {
            double branchHeight = height / layerAmount;
            InitializeLayers();
            LightningNode startNode = new LightningNode(startPoint);
            layers[0].Add(startNode);
            layers[0].Y = startNode.Position.Y;
            //startNode.AddChild(startPoint);

            for (int i = 1; i < layers.Length - 1; i++)
            {
                for (int j = 0; j < layers[i - 1].Nodes.Count; j++)
                {
                    LightningNode currentNode = layers[i - 1].Nodes[j];
                    layers[i].Y = layers[i - 1].Y - branchHeight;
                    if (RandomGen.NextDouble(0, 100) < layers[i - 1].Nodes[j].SplitChance)
                    {
                        Vector pLeft = new Vector(RandomGen.NextDouble(currentNode.Position.X - width / 6, currentNode.Position.X), layers[i].Y);
                        Vector pRight = new Vector(RandomGen.NextDouble(currentNode.Position.X, currentNode.Position.X + width / 6), layers[i].Y);
                        LightningNode nLeft = new LightningNode(pLeft);
                        LightningNode nRight = new LightningNode(pRight);
                        layers[i].Nodes.Add(nLeft);
                        currentNode.AddChild(nLeft);
                        layers[i].Nodes.Add(nRight);
                        currentNode.AddChild(nRight);

                    }
                    Vector p = new Vector(RandomGen.NextDouble(startPoint.X - width / 4, startPoint.X + width / 4), layers[i].Y);
                    LightningNode n = new LightningNode(p);
                    layers[i].Nodes.Add(n);
                    currentNode.AddChild(n);
                }
            }
            striking = true;
        }
    }
}
