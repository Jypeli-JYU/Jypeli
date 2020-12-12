using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;


namespace Jypeli.Farseer
{
	public class FSCollisionChain : FSCollisionShape
	{
		List<Vector2> _verts;
		bool _loop;


		public FSCollisionChain()
		{
			_fixtureDef.Shape = new ChainShape();
		}


		public FSCollisionChain(List<Vector2> verts) : this()
		{
			_verts = verts;
		}


		public FSCollisionChain(Vector2[] verts) : this()
		{
			_verts = new List<Vector2>(verts);
		}


		#region Configuration

		public FSCollisionChain SetLoop(bool loop, Body body)
		{
			_loop = loop;
			RecreateFixture(body);
			return this;
		}


		public FSCollisionChain SetVertices(Vertices verts, Body body)
		{
			_verts = verts;
			RecreateFixture(body);
			return this;
		}


		public FSCollisionChain SetVertices(List<Vector2> verts, Body body)
		{
			_verts = verts;
			RecreateFixture(body);
			return this;
		}


		public FSCollisionChain SetVertices(Vector2[] verts, Body body)
		{
			if (_verts == null)
				_verts = new List<Vector2>();

			_verts.Clear();
			_verts.AddRange(verts);
			RecreateFixture(body);
			return this;
		}

		#endregion


		public void OnEntityTransformChanged(Transform comp, Body body)
		{
			RecreateFixture(body);
		}


		void RecreateFixture(Body body)
		{
			DestroyFixture(body);

			// scale our verts and convert them to sim units
			var verts = new Vertices(_verts);
			verts.Scale(new Vector2(1) * FSConvert.DisplayToSim);

			var chainShape = _fixtureDef.Shape as ChainShape;
			chainShape.SetVertices(verts, _loop);

			CreateFixture(body);
		}
	}
}