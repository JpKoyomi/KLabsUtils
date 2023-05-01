﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using KLabs.Utils.Internal;

namespace KLabs.Utils
{
	public static class SkinnedMeshRendererExtensions
	{
		public static bool Replace(this SkinnedMeshRenderer self, Transform before, Transform after, out Transform[] dst)
		{
			return self.bones.Replace(before, after, out dst);
		}

		public static bool Replace(this Transform[] self, Transform before, Transform after, out Transform[] dst)
		{
			var dirty = false;
			dst = new Transform[self.Length];
			for (int i = 0; i < self.Length; i++)
			{
				if (self[i] == before)
				{
					dst[i] = after;
					dirty = true;
				}
				else
				{
					dst[i] = self[i];
				}
			}
			return dirty;
		}

		public static Mesh DeleteMesh(this SkinnedMeshRenderer src, BoxCollider area, bool inverse = false)
		{
			var srcMesh = src.sharedMesh;

			NarrowDownVertices(area, srcMesh.vertices, out var newVertices, out var newIndices, inverse);

			var dst = new MeshPrimitive(newVertices.Length);
			var actions = SetupCopyAttributes(srcMesh, dst);
			for (int i = 0; i < newIndices.Length; i++)
			{
				if (newIndices[i] != -1)
				{
					actions.Invoke(i);
				}
			}

			var newTriangles = new List<int>(newVertices.Length);
			NarrowDownTriangles(newIndices, srcMesh.triangles, newTriangles, out var triangleFlags);

			var mesh = Object.Instantiate(srcMesh);
			mesh.Clear(true);

			mesh.SetVertices(newVertices);
			if (dst.BoneWeights.Count == mesh.vertexCount) mesh.boneWeights = dst.BoneWeights.ToArray();
			if (dst.Normals.Count == mesh.vertexCount) mesh.SetNormals(dst.Normals);
			if (dst.Tangents.Count == mesh.vertexCount) mesh.SetTangents(dst.Tangents);
			for (int i = 0; i < dst.Uvs.Length; i++)
			{
				if (dst.Uvs[i].Count == mesh.vertexCount) mesh.SetUVs(i, dst.Uvs[i]);
			}

			NarrowDownCopyBlendShapes(srcMesh, mesh, newIndices);

			mesh.SetTriangles(newTriangles, 0);

			for (int i = 0, count = 0; i < srcMesh.subMeshCount; i++)
			{
				var subMesh = srcMesh.GetSubMesh(i);
				var shiftCount = 0;
				var badCount = 0;

				foreach (var item in triangleFlags.Skip(subMesh.indexStart).Take(subMesh.indexCount))
				{
					badCount += item ? 0 : 1;
				}
				if (subMesh.indexCount == badCount)
				{
					continue;
				}
				for (int j = 0; j < subMesh.indexStart; j++)
				{
					if (!triangleFlags[j])
					{
						shiftCount++;
					}
				}
				subMesh.indexStart -= shiftCount;
				subMesh.indexCount -= badCount;
				mesh.subMeshCount = count + 1;
				mesh.SetSubMesh(count++, subMesh);
			}

			return mesh;
		}

		private static void NarrowDownVertices(BoxCollider area, Vector3[] srcVertices, out Vector3[] dstVertices, out int[] dstIndices, bool inverse)
		{
			var vertexList = new List<Vector3>(srcVertices.Length);
			dstIndices = new int[srcVertices.Length];
			System.Func<Vector3, Vector3, bool> compare = (v1, v2) => v1 == v2;
			if (inverse) compare = (v1, v2) => v1 != v2;
			for (int i = 0; i < dstIndices.Length; i++)
			{
				if (compare(srcVertices[i], area.ClosestPoint(srcVertices[i])))
				{
					dstIndices[i] = vertexList.Count;
					vertexList.Add(srcVertices[i]);
				}
				else
				{
					dstIndices[i] = -1;
				}
			}
			dstVertices = vertexList.ToArray();
		}

		private static void NarrowDownTriangles(int[] newIndices, int[] triangles, List<int> newTriangles, out bool[] triangleFlags)
		{
			var tempTriangles = new int[triangles.Length];
			var flags = new bool[triangles.Length];
			for (int i = 0; i < triangles.Length / 3; i++)
			{
				var a = triangles[i * 3 + 0];
				var b = triangles[i * 3 + 1];
				var c = triangles[i * 3 + 2];
				if (newIndices[a] != -1 && newIndices[b] != -1 && newIndices[c] != -1)
				{
					tempTriangles[i * 3 + 0] = newIndices[a];
					tempTriangles[i * 3 + 1] = newIndices[b];
					tempTriangles[i * 3 + 2] = newIndices[c];
					flags[i * 3 + 0] = true;
					flags[i * 3 + 1] = true;
					flags[i * 3 + 2] = true;
				}
				else
				{
					tempTriangles[i * 3 + 0] = -1;
					tempTriangles[i * 3 + 1] = -1;
					tempTriangles[i * 3 + 2] = -1;
				}
			}
			for (int i = 0; i < tempTriangles.Length; i++)
			{
				if (tempTriangles[i] != -1)
				{
					newTriangles.Add(tempTriangles[i]);
				}
			}
			triangleFlags = flags;
		}

		private static void NarrowDownCopyBlendShapes(Mesh src, Mesh dst, int[] directions)
		{
			for (int i = 0; i < src.blendShapeCount; i++)
			{
				var frameCount = src.GetBlendShapeFrameCount(i);
				var name = src.GetBlendShapeName(i);
				for (int j = 0; j < frameCount; j++)
				{
					var weight = src.GetBlendShapeFrameWeight(i, j);
					var deltaVertices = new Vector3[src.vertexCount];
					var deltaNormals = new Vector3[src.vertexCount];
					var deltaTangents = new Vector3[src.vertexCount];
					src.GetBlendShapeFrameVertices(i, j, deltaVertices, deltaNormals, deltaTangents);
					deltaVertices = deltaVertices.Where((v, index) => directions[index] != -1).ToArray();
					deltaNormals = deltaNormals.Where((v, index) => directions[index] != -1).ToArray();
					deltaTangents = deltaTangents.Where((v, index) => directions[index] != -1).ToArray();
					dst.AddBlendShapeFrame(name, weight, deltaVertices, deltaNormals, deltaTangents);
				}
			}
		}

		private static System.Action<int> SetupCopyAttributes(Mesh src, MeshPrimitive dst)
		{
			var srcVertexCount = src.vertexCount;
			System.Action<int> actions = (i) => dst.BoneWeights.Add(src.boneWeights[i]);
			if (srcVertexCount == src.colors.Length)
			{
				actions += (index) => dst.Colors.Add(src.colors[index]);
			}
			if (srcVertexCount == src.normals.Length)
			{
				actions += (index) => dst.Normals.Add(src.normals[index]);
			}
			if (srcVertexCount == src.tangents.Length)
			{
				actions += (index) => dst.Tangents.Add(src.tangents[index]);
			}
			for (int i = 0; i < dst.Uvs.Length; i++)
			{
				if (srcVertexCount == src.GetUvs(i).Length)
				{
					actions += new UvsAccessHelper(src.GetUvs(i), dst.Uvs[i]).Action;
				}
			}
			return actions;
		}


		private struct UvsAccessHelper
		{
			public Vector2[] src;
			public List<Vector2> dst;

			public UvsAccessHelper(Vector2[] src, List<Vector2> dst)
			{
				this.src = src;
				this.dst = dst;
			}

			public void Action(int index) => dst.Add(src[index]);
		}

		private struct MeshPrimitive
		{
			public List<BoneWeight> BoneWeights { get; set; }
			public List<Color> Colors { get; set; }
			public List<Color32> Colors32 { get; set; }
			public List<Vector3> Normals { get; set; }
			public List<Vector4> Tangents { get; set; }
			public List<Vector2>[] Uvs { get; }

			public MeshPrimitive(int vertexCount)
			{
				BoneWeights = new List<BoneWeight>(vertexCount);
				Colors = new List<Color>(vertexCount);
				Colors32 = new List<Color32>(vertexCount);
				Normals = new List<Vector3>(vertexCount);
				Tangents = new List<Vector4>(vertexCount);
				Uvs = new List<Vector2>[8];
				for (int i = 0; i < Uvs.Length; i++)
				{
					Uvs[i] = new List<Vector2>(vertexCount);
				}
			}
		}

	}
}