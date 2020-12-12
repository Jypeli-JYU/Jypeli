using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;


namespace Jypeli.Farseer
{
	public abstract class FSCollisionShape
	{
		internal FSFixtureDef _fixtureDef = new FSFixtureDef();
		protected Fixture _fixture;

		/// <summary>
		/// encapsulates the Entity's position/rotation/scale and allows setting up a hieararchy
		/// </summary>
		public readonly Transform Transform;

		#region Configuration

		public FSCollisionShape SetFriction(float friction, Body body)
		{
			_fixtureDef.Friction = friction;
			if (_fixture != null)
			{
				_fixture.Friction = friction;

				var contactEdge = body.ContactList;
				while (contactEdge != null)
				{
					var contact = contactEdge.Contact;
					if (contact.FixtureA == _fixture || contact.FixtureB == _fixture)
						contact.ResetFriction();
					contactEdge = contactEdge.Next;
				}
			}

			return this;
		}


		public FSCollisionShape SetRestitution(float restitution, Body body)
		{
			_fixtureDef.Restitution = restitution;
			if (_fixture != null)
			{
				_fixture.Restitution = restitution;
				var contactEdge = body.ContactList;
				while (contactEdge != null)
				{
					var contact = contactEdge.Contact;
					if (contact.FixtureA == _fixture || contact.FixtureB == _fixture)
						contact.ResetRestitution();
					contactEdge = contactEdge.Next;
				}
			}

			return this;
		}


		public FSCollisionShape SetDensity(float density)
		{
			_fixtureDef.Density = density;
			if (_fixture != null)
				_fixture.Shape.Density = density;
			return this;
		}


		public FSCollisionShape SetIsSensor(bool isSensor)
		{
			_fixtureDef.IsSensor = isSensor;
			if (_fixture != null)
				_fixture.IsSensor = isSensor;
			return this;
		}


		public FSCollisionShape SetCollidesWith(Category collidesWith)
		{
			_fixtureDef.CollidesWith = collidesWith;
			if (_fixture != null)
				_fixture.CollidesWith = collidesWith;
			return this;
		}


		public FSCollisionShape SetCollisionCategories(Category collisionCategories)
		{
			_fixtureDef.CollisionCategories = collisionCategories;
			if (_fixture != null)
				_fixture.CollisionCategories = collisionCategories;
			return this;
		}


		public FSCollisionShape SetIgnoreCCDWith(Category ignoreCCDWith)
		{
			_fixtureDef.IgnoreCCDWith = ignoreCCDWith;
			if (_fixture != null)
				_fixture.IgnoreCCDWith = ignoreCCDWith;
			return this;
		}


		public FSCollisionShape SetCollisionGroup(short collisionGroup)
		{
			_fixtureDef.CollisionGroup = collisionGroup;
			if (_fixture != null)
				_fixture.CollisionGroup = collisionGroup;
			return this;
		}

		#endregion


		#region Component lifecycle

		public void OnAddedToEntity(Body body)
		{
			CreateFixture(body);
		}


		public void OnRemovedFromEntity(Body body)
		{
			DestroyFixture(body);
		}


		public void OnEnabled(Body body)
		{
			CreateFixture(body);
		}


		public void OnDisabled(Body body)
		{
			DestroyFixture(body);
		}

		#endregion


		/// <summary>
		/// wakes any contacting bodies. Useful when creating a fixture or changing something that won't trigger the bodies to wake themselves
		/// such as Circle.center.
		/// </summary>
		protected void WakeAnyContactingBodies(Body body)
		{
			var contactEdge = body.ContactList;
			while (contactEdge != null)
			{
				var contact = contactEdge.Contact;
				if (contact.FixtureA == _fixture || contact.FixtureB == _fixture)
				{
					contact.FixtureA.Body.IsAwake = true;
					contact.FixtureB.Body.IsAwake = true;
				}

				contactEdge = contactEdge.Next;
			}
		}


		internal virtual void CreateFixture(Body body)
		{
			if (_fixture != null)
				return;

			_fixtureDef.Shape.Density = _fixtureDef.Density;
			_fixture = body.CreateFixture(_fixtureDef.Shape, this);
			_fixture.Friction = _fixtureDef.Friction;
			_fixture.Restitution = _fixtureDef.Restitution;
			_fixture.IsSensor = _fixtureDef.IsSensor;
			_fixture.CollidesWith = _fixtureDef.CollidesWith;
			_fixture.CollisionCategories = _fixtureDef.CollisionCategories;
			_fixture.IgnoreCCDWith = _fixtureDef.IgnoreCCDWith;
			_fixture.CollisionGroup = _fixtureDef.CollisionGroup;
		}


		internal virtual void DestroyFixture(Body body)
		{
			if (_fixture == null)
				return;

			body.DestroyFixture(_fixture);
			_fixture = null;
		}
	}
}