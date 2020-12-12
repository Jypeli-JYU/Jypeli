using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;


namespace Jypeli.Farseer
{
	public class FSCollisionCircle : FSCollisionShape
	{
		Vector2 _center;
		float _radius = 0.1f;


		public FSCollisionCircle()
		{
			_fixtureDef.Shape = new CircleShape();
		}


		public FSCollisionCircle(float radius) : this()
		{
			_radius = radius;
			_fixtureDef.Shape.Radius = _radius * FSConvert.DisplayToSim;
		}


		#region Configuration

		public FSCollisionCircle SetRadius(float radius, Body body)
		{
			_radius = radius;
			RecreateFixture(body);
			return this;
		}


		public FSCollisionCircle SetCenter(Vector2 center, Body body)
		{
			_center = center;
			RecreateFixture(body);
			return this;
		}

		#endregion

		/*
		public override void OnEntityTransformChanged(Transform.Component comp)
		{
			if (comp == Transform.Component.Scale)
				RecreateFixture();
		}
		*/

		void RecreateFixture(Body body)
		{
			_fixtureDef.Shape.Radius = _radius * FSConvert.DisplayToSim;
			(_fixtureDef.Shape as CircleShape).Position = FSConvert.DisplayToSim * _center;

			if (_fixture != null)
			{
				var circleShape = _fixture.Shape as CircleShape;
				circleShape.Radius = _fixtureDef.Shape.Radius;
				circleShape.Position = FSConvert.DisplayToSim * _center;

				// wake the body if it is asleep to update collisions
				WakeAnyContactingBodies(body);
			}
		}
	}
}