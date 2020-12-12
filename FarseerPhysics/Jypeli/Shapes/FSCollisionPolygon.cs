using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;


namespace Jypeli.Farseer
{
	public class FSCollisionPolygon : FSCollisionShape
	{
		/// <summary>
		/// verts are stored in sim units
		/// </summary>
		protected Vertices _verts;

		Vector2 _center;
		protected bool _areVertsDirty = true;


		public FSCollisionPolygon()
		{
			_fixtureDef.Shape = new PolygonShape();
		}


		public FSCollisionPolygon(List<Vector2> vertices) : this()
		{
			_verts = new Vertices(vertices);
			_verts.Scale(new Vector2(FSConvert.DisplayToSim));
		}


		public FSCollisionPolygon(Vector2[] vertices) : this()
		{
			_verts = new Vertices(vertices);
			_verts.Scale(new Vector2(FSConvert.DisplayToSim));
		}


		#region Configuration

		public FSCollisionPolygon SetVertices(Vertices vertices, Body body)
		{
			_verts = new Vertices(vertices);
			_areVertsDirty = true;
			RecreateFixture(body);
			return this;
		}


		public FSCollisionPolygon SetVertices(List<Vector2> vertices, Body body)
		{
			_verts = new Vertices(vertices);
			_areVertsDirty = true;
			RecreateFixture(body);
			return this;
		}


		public FSCollisionPolygon SetCenter(Vector2 center, Body body)
		{
			_center = center;
			_areVertsDirty = true;
			RecreateFixture(body);
			return this;
		}

		#endregion


		public void OnAddedToEntity(Body body)
		{
			UpdateVerts();
			CreateFixture(body);
		}

		/* TODO:
		public override void OnEntityTransformChanged(Transform.Component comp)
		{
			if (comp == Transform.Component.Scale)
				RecreateFixture();
		}
		*/


		internal void CreateFixture(Body body)
		{
			UpdateVerts();
			base.CreateFixture(body);
		}


		protected void RecreateFixture(Body body)
		{
			DestroyFixture(body);
			UpdateVerts();
			CreateFixture(body);
		}


		protected void UpdateVerts()
		{
			if (!_areVertsDirty)
				return;

			_areVertsDirty = false;

			var shapeVerts = (_fixtureDef.Shape as PolygonShape).Vertices;
			shapeVerts.attachedToBody = false;

			shapeVerts.Clear();
			shapeVerts.AddRange(_verts);
			shapeVerts.Scale(new Vector2(1));
			shapeVerts.Translate(ref _center);

			(_fixtureDef.Shape as PolygonShape).SetVerticesNoCopy(shapeVerts);
		}
	}
}