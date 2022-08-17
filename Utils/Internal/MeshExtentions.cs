
using UnityEngine;

namespace KLabs.Utils.Internal
{
	public static class MeshExtentions
	{
		public static Vector2[] GetUvs(this Mesh self, int id)
		{
			switch (id)
			{
				case 0: return self.uv;
				case 1: return self.uv2;
				case 2: return self.uv3;
				case 3: return self.uv4;
				case 4: return self.uv5;
				case 5: return self.uv6;
				case 6: return self.uv7;
				case 7: return self.uv8;
				default: throw new System.IndexOutOfRangeException();
			}
		}
	}
}
