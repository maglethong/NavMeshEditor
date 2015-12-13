using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace NavMeshEditor
{
	public class Face : MonoBehaviour
	{

		public Vertex[] vertexes;

		private static int count = 0;

		EditorNavMesh navMesh;

		public bool markConvex = true;

		public Vector3 center
		{
			get
			{
				Vector3 middle = Vector2.zero;
				foreach (Vertex v in this.vertexes)
					middle += v.transform.position;
				return middle / this.vertexes.Length;
			}
		}

		public bool isGood { get { return this.markConvex; } }


		public Material NormalMaterial;
		public Material SelectedMaterial;
		public Material BadMaterial;
		private bool RefreshScreen = false;


		public static Face NewFace(EditorNavMesh navMesh, Vertex[] vertexes)
		{
			if (vertexes.Length < 3)
				return null;

			// Check face exists
			if (CheckExists(vertexes))
			{
				Debug.LogError("Selected vertexes already form a face");
				return null;
			}


			int size = vertexes.Length;

			// Create object with component
			GameObject obj = new GameObject();
			Face face = obj.AddComponent<Face>();
			obj.AddComponent<MeshFilter>();
			obj.AddComponent<MeshRenderer>();
			face.name = "Face (" + count++ + ")";
			obj.transform.parent = navMesh.transform;
			face.vertexes = new Vertex[size];




			////
			face.NormalMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/NavMeshEditor/NavmeshNormal_MAT.mat");
			face.SelectedMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/NavMeshEditor/NavmeshSelected_MAT.mat");
			face.BadMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/NavMeshEditor/NavmeshBad_MAT.mat");
			////



			// Projection
			Vector2[] projection = new Vector2[size];
			for (int i = 0; i < size; i++)
				projection[i] = new Vector2(vertexes[i].transform.position.x, vertexes[i].transform.position.z);

			// Mid position
			Vector2 middle = Vector2.zero;
			foreach (Vector2 v in projection)
				middle += v;
			middle /= size;

			// Translating projection to center
			for (int i = 0; i < size; i++)
				projection[i] -= middle;

			// Get Vertex angle
			float[] angles = new float[size];
			for (int i = 0; i < size; i++)
			{
				angles[i] = Vector2.Angle(Vector2.left, projection[i]);
				if (projection[i].y < 0)
					angles[i] = 360 - angles[i];
			}

			// Oredering vertexes by angle
			int cur = 0;
			while (true)
			{
				int lowest = -1;
				for (int i = 0; i < size; i++)
					if ((angles[i] != -1) &&
						(lowest == -1 || angles[i] < angles[lowest]))
						lowest = i;
				if (lowest == -1)
					break;
				angles[lowest] = -1;
				face.vertexes[cur++] = vertexes[lowest];
			}

			// Check face is convex
			if (face.IsConvex())
				navMesh.faces.Add(face);
			else
			{
				Debug.LogError("Selected vertexes do not form a convex poligon in XY");
				GameObject.DestroyImmediate(face.gameObject);
				return null;
			}

			// Add reference in all vertexes
			foreach (Vertex v in face.vertexes)
				v.faces.Add(face);
			face.navMesh = navMesh;

			//Calculate midpoint
			Vector3 pos = Vector3.zero;
			foreach (Vertex v in face.vertexes)
				pos += v.transform.position;
			pos /= face.vertexes.Length;

			//Position
			obj.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			obj.transform.position = Vector3.zero;

			return face;
		}

		public bool IsConvex()
		{
			bool got_negative = false;
			bool got_positive = false;
			int num_points = this.vertexes.Length;
			int B, C;
			for (int A = 0; A < num_points; A++)
			{
				B = (A + 1) % num_points;
				C = (B + 1) % num_points;

				float cross_product =
					CrossProductLength(
						this.vertexes[A].transform.position.x, this.vertexes[A].transform.position.z,
						this.vertexes[B].transform.position.x, this.vertexes[B].transform.position.z,
						this.vertexes[C].transform.position.x, this.vertexes[C].transform.position.z);
				if (cross_product < 0)
				{
					got_negative = true;
				}
				else if (cross_product > 0)
				{
					got_positive = true;
				}
				if (got_negative && got_positive) return false;
			}

			return true;
		}

		private static float CrossProductLength(float Ax, float Ay, float Bx, float By, float Cx, float Cy)
		{
			// Get the vectors' coordinates.
			float BAx = Ax - Bx;
			float BAy = Ay - By;
			float BCx = Cx - Bx;
			float BCy = Cy - By;

			// Calculate the Z coordinate of the cross product.
			return (BAx * BCy - BAy * BCx);
		}

		public static bool CheckExists(Vertex[] vertexes)
		{
			List<Face> possible = new List<Face>();

			// Feeding possible iwth vertex[0]
			foreach (Face f in vertexes[0].faces)
				if (f.vertexes.Length == vertexes.Length)
					possible.Add(f);

			// Search if other vertexes share one face
			for (int i = 1; i < vertexes.Length; i++)
			{
				// No match
				if (possible.Count == 0)
					break;

				// Remove Faces not found in the other vrtexes
				for (int j = 0; j < possible.Count; j++)
					if (!vertexes[i].faces.Contains(possible[j]))
						possible.RemoveAt(j--);
			}

			return possible.Count != 0;
		}



		public void Delete()
		{
			foreach (Vertex v in this.vertexes)
				v.faces.Remove(this);
			this.navMesh.faces.Remove(this);
			GameObject.DestroyImmediate(this.gameObject);
		}

		void OnDrawGizmos()
		{

			if (RefreshScreen)
			{

				RefreshScreen = false;
				//SceneView.RepaintAll();

			}

			Draw(NormalMaterial, BadMaterial);

		}

		void OnDrawGizmosSelected()
		{

			if (!RefreshScreen)
			{

				RefreshScreen = true;
				//SceneView.RepaintAll();
			}

			Draw(SelectedMaterial, SelectedMaterial);

		}



		public void Draw(Material ConvexMaterial, Material ConcaveMaterial)
		{
			int size = this.vertexes.Length;

			// Invalid?
			if (size < 3)
				return;

			// Draw edge lines
			//Gizmos.color = Color.green;
			//Gizmos.DrawLine(this.vertexes[0].transform.position, this.vertexes[size - 1].transform.position);
			//for (int i = 1; i < size; i++)
			//	Gizmos.DrawLine(this.vertexes[i - 1].transform.position, this.vertexes[i].transform.position);

			// Calc position to draw cube
			Vector3 pos = Vector3.zero;
			foreach (Vertex v in this.vertexes)
				pos += v.transform.position;
			pos /= size;

			// Draw
			//if (this.markConvex)
			///	Gizmos.color = convexColor;
			//else
			//	Gizmos.color = concaveColor;
			//Gizmos.DrawCube(pos, Vector3.one * Vector3.Distance(UnityEditor.SceneView.lastActiveSceneView.camera.transform.position, pos) * EditorNavMesh.GizmosSizeMultiplyer *2);

			// Draw connections
			//foreach (Vertex v in this.vertexes)
			//Gizmos.DrawLine(pos, v.transform.position);

			/////////////////Bagunça do Yvan///////////////////

			if (this.markConvex)
			{


				GetComponent<MeshRenderer>().material = ConvexMaterial;

			}
			else
			{

				GetComponent<MeshRenderer>().material = ConcaveMaterial;
			}


			//Create mesh vertices 
			Vector3[] MeshVertices = new Vector3[vertexes.Length + 1];
			MeshVertices[0] = pos;

			for (int i = 1; i < vertexes.Length + 1; i++)
			{

				MeshVertices[i] = vertexes[i - 1].transform.position;
			}

			//Create mesh uvs
			Vector2[] MeshUVs = new Vector2[vertexes.Length + 1];
			MeshUVs[0] = Vector2.zero;

			for (int i = 1; i < vertexes.Length + 1; i++)
			{

				MeshUVs[i] = Vector2.up;
			}

			//Create mesh faces
			int[] MeshFaces = new int[vertexes.Length * 3];

			MeshFaces[0] = 0;
			MeshFaces[1] = 1;
			MeshFaces[2] = 2;
			MeshFaces[vertexes.Length * 3 - 3] = 0;
			MeshFaces[vertexes.Length * 3 - 2] = MeshVertices.Length - 1;
			MeshFaces[vertexes.Length * 3 - 1] = 1;

			for (int i = 3; i < (vertexes.Length - 1) * 3; i = i + 3)
			{

				MeshFaces[i] = 0;
				MeshFaces[i + 1] = 1 + (i) / 3;
				MeshFaces[i + 2] = 2 + i / 3;



			}

			//Create and render mesh
			Mesh DrawMesh = new Mesh();
			DrawMesh.vertices = MeshVertices;
			DrawMesh.uv = MeshUVs;
			DrawMesh.triangles = MeshFaces;
			DrawMesh.RecalculateNormals();
			//Gizmos.DrawMesh(DrawMesh);
			GetComponent<MeshFilter>().mesh = DrawMesh;
			//RenderEdges
			Gizmos.color = Gizmos.color + new Color(0.1f, 0.1f, 0.1f);
			//Gizmos.DrawLine(this.vertexes[0].transform.position, this.vertexes[size - 1].transform.position);
			//for (int i = 1; i < size; i++)
			//Gizmos.DrawLine(this.vertexes[i - 1].transform.position, this.vertexes[i].transform.position);

			/////////////////Bagunça do Yvan///////////////////


		}
	}
}