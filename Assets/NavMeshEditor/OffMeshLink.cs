using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace NavMeshEditor
{

	public enum OffMeshLinkDirection 
	{
		none = 0,
		going = 1, 
		returning = 2, 
		both = 3
	}

	public class OffMeshLink : MonoBehaviour
	{
		public Face linkedFace;
		public OffMeshLink brother;
		public bool isMain;
		private static int count = 0;
		EditorNavMesh navMesh;

		public bool isGood { get { return this.brother != null; } }

		public OffMeshLinkDirection direction = OffMeshLinkDirection.both;


		public static OffMeshLink NewOffMeshLink(EditorNavMesh navMesh, Face face1, Face face2)
		{
			int num = count++;

			// Create A
			GameObject obj1 = new GameObject();
			obj1.name = "Link (" + num + ") A";
			obj1.transform.parent = navMesh.transform;
			OffMeshLink link1 = obj1.AddComponent<OffMeshLink>();
			link1.isMain = true;
			link1.navMesh = navMesh;
			link1.transform.position = face1.center;

			// Create B
			GameObject obj2 = new GameObject();
			obj2.name = "Link (" + num + ") B";
			obj2.transform.parent = navMesh.transform;
			OffMeshLink link2 = obj2.AddComponent<OffMeshLink>();
			link2.isMain = false;
			link2.navMesh = navMesh;
			link2.transform.position = face2.center;

			// Create Link
			link1.brother = link2;
			link2.brother = link1;
			link1.linkedFace = face1;
			link2.linkedFace = face2;
			navMesh.linkList.Add(link1);

			UnityEditor.Selection.activeGameObject = obj1;

			return link1;
		}

		public void Delete()
		{
			if ( !this.isMain && this.brother.isMain)
				this.brother.Delete();
			else
			{
				this.navMesh.linkList.Remove(this);
				GameObject.DestroyImmediate(this.gameObject);
				GameObject.DestroyImmediate(this.brother.gameObject);
			}
		}

		void OnDrawGizmos()
		{
			Draw(Color.magenta, Color.grey, Color.magenta, Color.red);
		}
		void OnDrawGizmosSelected()
		{
			Draw(Color.cyan, Color.grey, Color.magenta, Color.red);
		}


		public void Draw(Color color, Color linkedFaceColor, Color linkColor, Color conflictColor)
		{
			// Draw Point
			float size = EditorNavMesh.GizmosSizeMultiplyer;
			if (!this.isGood)
				Gizmos.color = conflictColor;
			else
				Gizmos.color = color;
			Gizmos.DrawSphere(this.transform.position, size);

			// Draw Link with face
			Gizmos.color = linkedFaceColor;
			Gizmos.DrawLine(this.transform.position, this.linkedFace.center);

			// Draw Link
			if(this.isMain)
			{
				Gizmos.color = linkColor;
				Gizmos.DrawLine(this.transform.position, this.brother.transform.position);
			}
		}
	}
}
