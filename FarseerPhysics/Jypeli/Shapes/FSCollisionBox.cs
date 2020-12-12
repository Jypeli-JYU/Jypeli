using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;

namespace Jypeli.Farseer
{
	public class FSCollisionBox : FSCollisionPolygon
	{
		float _width = 0.1f;
		float _height = 0.1f;


		public FSCollisionBox()
		{
		}


		public FSCollisionBox(float width, float height)
		{
			_width = width;
			_height = height;
			_verts = PolygonTools.CreateRectangle(FSConvert.DisplayToSim * _width / 2,
				FSConvert.DisplayToSim * _height / 2);
		}


		#region Configuration

		public FSCollisionBox SetSize(float width, float height, Body body)
		{
			_width = width;
			_height = height;
			_verts = PolygonTools.CreateRectangle(FSConvert.DisplayToSim * _width / 2,
				FSConvert.DisplayToSim * _height / 2);
			_areVertsDirty = true;
			RecreateFixture(body);
			return this;
		}

		#endregion
	}
}