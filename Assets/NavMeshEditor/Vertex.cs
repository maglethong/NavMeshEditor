using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace NavMeshEditor
{
	public class Vertex : MonoBehaviour
	{
		public List<Face> faces = new List<Face>();
		private static int count = 0;
		EditorNavMesh navMesh;

		public bool isGood { get { return this.faces.Count > 0; } }



		public static Vertex NewVertex(EditorNavMesh navMesh)
		{
			GameObject obj = new GameObject();
			Vertex vertex = obj.AddComponent<Vertex>();
			vertex.name = "Vertex (" + count++ + ")";
			obj.transform.parent = navMesh.transform;

			UnityEditor.Selection.activeGameObject = obj;

			navMesh.vertexes.Add(vertex);

			vertex.navMesh = navMesh;

			return vertex;
		}

		public void Delete()
		{
			while (this.faces.Count > 0)
				faces[0].Delete();
			this.navMesh.vertexes.Remove(this);
			GameObject.DestroyImmediate(this.gameObject);
		}

		void OnDrawGizmos()
		{
			Draw(Color.green, Color.red);
		}
		void OnDrawGizmosSelected()
		{
			Draw(Color.blue, Color.blue);
		}

		public void Draw(Color color, Color conflictColor)
		{
			float size = EditorNavMesh.GizmosSizeMultiplyer;
			if ( !this.isGood )
				Gizmos.color = conflictColor;
			else
				Gizmos.color = color;
			Gizmos.DrawSphere(transform.position, size);
		}
	}
}
