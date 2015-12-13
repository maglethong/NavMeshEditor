using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace NavMeshEditor
{
	public class EditorNavMesh : MonoBehaviour
	{

		public List<Vertex> vertexes = new List<Vertex>();
		public List<Face> faces = new List<Face>();
		public List<OffMeshLink> linkList = new List<OffMeshLink>();


		public static float GizmosSizeMultiplyer = 0.02f;


		public static EditorNavMesh NewNavMesh()
		{
			GameObject obj = new GameObject();
			EditorNavMesh navMesh = obj.AddComponent<EditorNavMesh>();

			return navMesh;
		}
	}
}
