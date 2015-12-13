using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


using NavMeshEditor;


//!
/*!
 * 
 */
class NavMeshEditorWindow : EditorWindow
{
	private float dotSize = 1;
	private EditorNavMesh mesh;
	public bool editable { get { return this.mesh != null; } }

	public static NavMeshEditorWindow me = null;

	Object navMeshFile;


	private bool showAdvanced = false;

	private LayerMask raycastMask;

	Vector2 mousePos = Vector2.zero;


	void OnSelectionChage()
	{

		SceneView.RepaintAll();

	}

	#region Commands (Shortcuts)
	[MenuItem("Navmesh Editor Shortcuts/New Vertex (SHIFT + V) #v")]
	static void Command_NewVertex()
	{
		if (me != null && me.editable)
			me.NewVertex();
		else
			Debug.LogError("Navmesh editor must be initialized!");
	}

	[MenuItem("Navmesh Editor Shortcuts/Delete Vertex (CTRL + V) &v")]
	static void Command_RemoveVertexes()
	{
		if (me != null && me.editable)
			me.RemoveVertexes();
		else
			Debug.LogError("Navmesh editor must be initialized!");
	}
	[MenuItem("Navmesh Editor Shortcuts/New Faces (SHIFT + F) #f")]
	static void Command_NewFace()
	{
		if (me != null && me.editable)
			me.NewFace();
		else
			Debug.LogError("Navmesh editor must be initialized!");
	}

	[MenuItem("Navmesh Editor Shortcuts/Delete Faces (CTRL + F) &f")]
	static void Command_RemoveFaces()
	{
		if (me != null && me.editable)
			me.RemoveFaces();
		else
			Debug.LogError("Navmesh editor must be initialized!");
	}
	#endregion

	// Add menu item named "NavMeshEditor" to the Window menu
	[MenuItem("Window/Nav-Mesh Editor")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(NavMeshEditorWindow));
	}

	void OnEnable()
	{
		// Listen to scene events
		SceneView.onSceneGUIDelegate += SceneGUI;
		Repaint();
		me = this;
	}
	void OnDisable()
	{
		SceneView.onSceneGUIDelegate -= SceneGUI;
	}

	void SceneGUI(SceneView sceneView)
	{
		if (Event.current.type == EventType.mouseMove)
		{
			this.mousePos = Event.current.mousePosition;
			this.mousePos.y = UnityEditor.SceneView.lastActiveSceneView.camera.pixelHeight - this.mousePos.y;
		}

	}

	void OnDestroy()
	{
		this.StopEdit();
		me = null;
	}


	// The window GUI
	void OnGUI()
	{

		// Start/Stop button
		if (this.mesh == null)
		{
			if (GUILayout.Button("Start"))
				this.StartEdit();
		}
		else
		{
			if (GUILayout.Button("Stop"))
				this.StopEdit();
		}

		// If started
		if (this.mesh != null)
		{
			// Buttons
			GUILayout.BeginHorizontal();
			{
				// Labels
				GUILayout.BeginVertical();
				{
					GUILayout.Label("Vertex: ");
					GUILayout.Label("Face: ");
					GUILayout.Label("Off-Mesh Link: ");
				}
				GUILayout.EndVertical();
				// New
				GUILayout.BeginVertical();
				{
					if (GUILayout.Button("New"))
						NewVertex();
					if (GUILayout.Button("New"))
						NewFace();
					if (GUILayout.Button("New"))
						NewOffMeshLink();
				}
				GUILayout.EndVertical();
				// Delete
				GUILayout.BeginVertical();
				{
					if (GUILayout.Button("Delete"))
						RemoveVertexes();
					if (GUILayout.Button("Delete"))
						RemoveFaces();
					if (GUILayout.Button("Delete"))
						RemoveOffMeshLinks();
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();

			// Save/Load
			if (GUILayout.Button("Save"))
				Save();
		}
		else
		{
			if (navMeshFile != null && GUILayout.Button("Load"))
				Load();
		}

		// Asset
		Object previous = navMeshFile;
		navMeshFile = EditorGUILayout.ObjectField("Nav Mesh File: ", navMeshFile, typeof(Object));
		if (navMeshFile != null && !AssetDatabase.GetAssetPath(navMeshFile).EndsWith(".nvm"))
			navMeshFile = previous;

		// Advanced options
		this.showAdvanced = GUILayout.Toggle(this.showAdvanced, "  Show Advanced Options");
		if (this.showAdvanced)
		{
			EditorNavMesh.GizmosSizeMultiplyer = EditorGUILayout.FloatField("Gizmos Size: ", EditorNavMesh.GizmosSizeMultiplyer);
			string[] options = { "CanJump", "CanShoot", "CanSwim" };
			this.raycastMask = EditorGUILayout.LayerField("Raycast Mask (ignore): ", this.raycastMask);
		}
	}

	void OnInspectorUpdate()
	{
		me = this;

		// TODO [CUIDADO!!! Preciso deixar isso mais eficiente ainda]
		/*
		Salvar posição original dos vertidies
		a cada frame ver se os selecionados se mecheram
		Se se mecheu jogar as faces dele em uma lista [sem  repetir] de faces sujas
		a cada frame verificar uma face suja [Possívelmente deixar o cara configurar]
		*/
		Object[] selected = GetSelectedWithComponet(typeof(Vertex));
		if (selected == null)
			return;
		List<Face> relevantFaces = new List<Face>();

		foreach (Vertex v in selected)
			foreach (Face f in v.faces)
				if (!relevantFaces.Contains(f))
				{
					relevantFaces.Add(f);
					if (!f.IsConvex())
						f.markConvex = false;
					else
						f.markConvex = true;
				}
	}

	#region Button Functions

	#region Faces
	void NewFace()
	{
		// Get selected vertexes
		Object[] selected = GetSelectedWithComponet(typeof(Vertex));
		Vertex[] sel = new Vertex[selected.Length];
		for (int i = 0; i < sel.Length; i++)
			sel[i] = selected[i] as Vertex;

		// Create face
		Face newFace = Face.NewFace(this.mesh, sel);
	}

	void RemoveFaces()
	{
		// Get selected vertexes
		Object[] selected = GetSelectedWithComponet(typeof(Face));

		// Removing
		for (int i = 0; i < selected.Length; i++)
			(selected[i] as Face).Delete();
	}
	#endregion

	#region Vertexes
	void NewVertex()
	{
		// Create Vertex
		Vertex newVertex = Vertex.NewVertex(this.mesh);

		RaycastHit hit;
		Physics.Raycast(UnityEditor.SceneView.lastActiveSceneView.camera.ScreenPointToRay(mousePos),
							out hit, 1000f, raycastMask);
		if (hit.point != null)
			newVertex.transform.position = hit.point;
	}

	void RemoveVertexes()
	{
		// Get selected vertexes
		Object[] selected = GetSelectedWithComponet(typeof(Vertex));

		// Removing
		for (int i = 0; i < selected.Length; i++)
			(selected[i] as Vertex).Delete();
	}
	#endregion

	#region Off-Mesh Link
	void NewOffMeshLink()
	{
		// Get selected faces
		Object[] selected = GetSelectedWithComponet(typeof(Face));
		if (selected.Length != 2)
		{
			Debug.LogError("Will only create a link if axactly 2 Faces are selected!");
			return;
		}

		Face[] sel = new Face[selected.Length];
		for (int i = 0; i < sel.Length; i++)
			sel[i] = selected[i] as Face;

		// Create face
		NavMeshEditor.OffMeshLink newLink = NavMeshEditor.OffMeshLink.NewOffMeshLink(this.mesh, sel[0], sel[1]);
	}
	void RemoveOffMeshLinks()
	{
		Object[] selected = GetSelectedWithComponet(typeof(NavMeshEditor.OffMeshLink));

		for (int i = 0; i < selected.Length; i++)
			if (selected[i] != null)
				(selected[i] as NavMeshEditor.OffMeshLink).Delete();
	}
	#endregion

	#region Start/Stop
	void StartEdit()
	{
		this.mesh = EditorNavMesh.NewNavMesh();
		mesh.name = "Nav Mesh Editor";
	}

	void StopEdit()
	{
		if (this.mesh != null)
			DestroyImmediate(this.mesh.gameObject);
		this.mesh = null;
	}
	#endregion

	#region Save/Load
	void Save()
	{
		bool vertexesGood = true;
		bool facessGood = true;
		bool linksGood = true;
		bool isNew = true;
		string fileName;
		// Getting name
		if (navMeshFile == null)
			fileName = "Assets/" + System.IO.Path.GetRandomFileName() + ".nvm";
		else
		{
			fileName = AssetDatabase.GetAssetPath(navMeshFile);
			isNew = false;
		}

		// Check if faces good
		foreach (Face f in this.mesh.faces)
			facessGood = facessGood && f.isGood;
		if (!facessGood)
		{
			Debug.LogError("Mesh has non-convex faces! Proceding [May cause errors when loading!]");
		}

		// Check if vertices are good
		foreach (Vertex v in this.mesh.vertexes)
			vertexesGood = vertexesGood && v.isGood;
		if (!vertexesGood)
		{
			Debug.LogError("There are unused vertexes! Proceding");
		}

		// Check if Off-Mesh Links are good
		foreach (NavMeshEditor.OffMeshLink l in this.mesh.linkList)
			linksGood = linksGood && l.isGood;
		if (!linksGood)
		{
			Debug.LogError("Mesh has invalid off-mesh links! Proceding");
		}

		// Write
		System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.Open(fileName, System.IO.FileMode.Create));
		// Vertexes
		writer.Write(this.mesh.vertexes.Count);
		foreach (Vertex v in this.mesh.vertexes)
		{
			writer.Write(v.transform.position.x);
			writer.Write(v.transform.position.y);
			writer.Write(v.transform.position.z);
			vertexesGood = vertexesGood && v.isGood;
		}
		// Faces
		writer.Write(this.mesh.faces.Count);
		foreach (Face f in this.mesh.faces)
		{
			writer.Write(f.vertexes.Length);
			foreach (Vertex v in f.vertexes)
			{
				int pos = 0;
				foreach (Vertex x in this.mesh.vertexes)
				{
					if (v == x)
						break;
					pos++;
				}
				writer.Write(pos);
			}
		}
		// Off-Mesh Links
		writer.Write(this.mesh.linkList.Count);
		foreach (NavMeshEditor.OffMeshLink l in this.mesh.linkList)
		{
			writer.Write(l.transform.position.x);
			writer.Write(l.transform.position.y);
			writer.Write(l.transform.position.z);


			writer.Write(l.brother.transform.position.x);
			writer.Write(l.brother.transform.position.y);
			writer.Write(l.brother.transform.position.z);

			int pos = 0;
			foreach (Face f in this.mesh.faces)
			{
				if (f == l.linkedFace)
					break;
				pos++;
			}
			writer.Write(pos);

			pos = 0;
			foreach (Face f in this.mesh.faces)
			{
				if (f == l.brother.linkedFace)
					break;
				pos++;
			}
			writer.Write(pos);

			writer.Write((int)l.direction);
		}

		writer.Close();

		// Import if new
		if (isNew)
			AssetDatabase.ImportAsset(fileName);

		// Feedback if not good
		Debug.Log("Saved successfully as " + fileName);
	}

	void Load()
	{
		if (navMeshFile == null)
			return;

		this.StartEdit();


		// Getting name
		string fileName = AssetDatabase.GetAssetPath(navMeshFile);

		// Read
		System.IO.BinaryReader reader = new System.IO.BinaryReader(System.IO.File.Open(fileName, System.IO.FileMode.Open));
		// Vertexes
		int vertexes = reader.ReadInt32();
		Vector3 pos = Vector3.zero;
		for (int i = 0; i < vertexes; i++)
		{
			float x = reader.ReadSingle();
			float y = reader.ReadSingle();
			float z = reader.ReadSingle();
			Vertex newV = Vertex.NewVertex(this.mesh);
			newV.transform.position = new Vector3(x, y, z);
		}

		// Faces
		int faces = reader.ReadInt32();
		for (int i = 0; i < faces; i++)
		{
			vertexes = reader.ReadInt32();
			int[] vertexIndexes = new int[vertexes];
			Vertex[] v = new Vertex[vertexes];

			// Read vertexe indexes
			for (int j = 0; j < vertexes; j++)
			{
				int vertexIndex = reader.ReadInt32();
				v[j] = mesh.vertexes[vertexIndex];
			}

			// Adding face
			Face.NewFace(this.mesh, v);
		}

		// Off-Mesh Links
		int links = reader.ReadInt32();
		for (int i = 0; i < links; i++)
		{
			float x_A = reader.ReadSingle();
			float y_A = reader.ReadSingle();
			float z_A = reader.ReadSingle();

			float x_B = reader.ReadSingle();
			float y_B = reader.ReadSingle();
			float z_B = reader.ReadSingle();

			int faceIndex_A = reader.ReadInt32();

			int faceIndex_B = reader.ReadInt32();

			int direction = reader.ReadInt32();


			NavMeshEditor.OffMeshLink newLink = NavMeshEditor.OffMeshLink.NewOffMeshLink(this.mesh, this.mesh.faces[faceIndex_A], this.mesh.faces[faceIndex_B]);
			newLink.transform.position = new Vector3(x_A, y_A, z_A);
			newLink.brother.transform.position = new Vector3(x_B, y_B, z_B);
		}

		reader.Close();

		SceneView.RepaintAll();
	}
	#endregion

	#endregion


	Object[] GetSelectedWithComponet(System.Type component)
	{
		// Get selected vertexes
		Transform[] selected = Selection.transforms;
		System.Collections.Generic.List<Object> selectedWithComponent = new System.Collections.Generic.List<Object>();

		foreach (Transform tr in selected)
		{
			Object o = tr.GetComponent(component);
			if (o != null)
				selectedWithComponent.Add(o);
		}

		return selectedWithComponent.ToArray();
	}
}