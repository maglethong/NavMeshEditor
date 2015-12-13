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
	public static string xmlVersionString { get { return "v" + xmlVersion + "." + xmlSubversion; } }
	public const int xmlVersion = 1;
	public const int xmlSubversion = 0;

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
		navMeshFile = EditorGUILayout.ObjectField("Nav Mesh File: ", navMeshFile, typeof(Object), false);
		if (navMeshFile != null && !AssetDatabase.GetAssetPath(navMeshFile).EndsWith(".xml"))
			navMeshFile = previous;

		// Advanced options
		this.showAdvanced = GUILayout.Toggle(this.showAdvanced, "  Show Advanced Options");
		if (this.showAdvanced)
		{
			EditorNavMesh.GizmosSizeMultiplyer = EditorGUILayout.FloatField("Gizmos Size: ", EditorNavMesh.GizmosSizeMultiplyer);
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
		// Get selected vertices
		Object[] selected = GetSelectedWithComponet(typeof(Vertex));
		Vertex[] sel = new Vertex[selected.Length];
		for (int i = 0; i < sel.Length; i++)
			sel[i] = selected[i] as Vertex;

		// Create face
		Face.NewFace(this.mesh, sel);
	}

	void RemoveFaces()
	{
		// Get selected vertices
		Object[] selected = GetSelectedWithComponet(typeof(Face));

		// Removing
		for (int i = 0; i < selected.Length; i++)
			(selected[i] as Face).Delete();
	}
	#endregion

	#region Vertices
	void NewVertex()
	{
		// Create Vertex
		Vertex newVertex = Vertex.NewVertex(this.mesh);

		RaycastHit hit;
		if (Physics.Raycast(UnityEditor.SceneView.lastActiveSceneView.camera.ScreenPointToRay(mousePos), out hit, 1000f, raycastMask))
			newVertex.transform.position = hit.point;
	}

	void RemoveVertexes()
	{
		// Get selected vertices
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
		NavMeshEditor.OffMeshLink.NewOffMeshLink(this.mesh, sel[0], sel[1]);
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
		bool isNew = true;
		string fileName;

		// Getting name
		if (navMeshFile == null)
			fileName = "Assets/" + System.IO.Path.GetRandomFileName() + ".xml";
		else
		{
			fileName = AssetDatabase.GetAssetPath(navMeshFile);
			isNew = false;
		}

		// Check if faces good
		foreach (Face f in this.mesh.faces)
			if (!f.isGood)
				Debug.LogWarning("Mesh has non-convex faces! Proceding [May cause errors when loading!]", f);

		// Check if vertices are good
		foreach (Vertex v in this.mesh.vertexes)
			if(!v.isGood)
				Debug.LogWarning("There are unused vertices! Proceding", v);

		// Check if Off-Mesh Links are good
		foreach (NavMeshEditor.OffMeshLink l in this.mesh.linkList)
			if (!l.isGood)
				Debug.LogWarning("Mesh has invalid off-mesh links! Proceding", l);

		// Creating Xml and start writing
		System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
		settings.Indent = true;
		settings.IndentChars = "\t";
		using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(fileName, settings))
		{
			writer.WriteStartDocument();

			writer.WriteStartElement("Version");
			writer.WriteElementString("Version", "" + NavMeshEditorWindow.xmlVersion);
			writer.WriteElementString("Subversion", "" + NavMeshEditorWindow.xmlSubversion);
			writer.WriteEndElement();


			writer.WriteStartElement("NavMesh");

			// Vertices
			writer.WriteStartElement("Vertices");
			foreach (Vertex v in this.mesh.vertexes)
			{
				writer.WriteStartElement("Vertex");

				writer.WriteStartElement("Position");
				writer.WriteElementString("x", "" + v.transform.position.x);
				writer.WriteElementString("y", "" + v.transform.position.y);
				writer.WriteElementString("z", "" + v.transform.position.z);
				writer.WriteEndElement();

				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			// Faces
			writer.WriteStartElement("Faces");
			foreach (Face f in this.mesh.faces)
			{
				writer.WriteStartElement("Face");
				foreach (Vertex v in f.vertexes)
				{
					int pos = 0;
					foreach (Vertex x in this.mesh.vertexes)
					{
						if (v == x)
							break;
						pos++;
					}
					writer.WriteElementString("Vertex", "" + pos);
				}
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			// Off-Mesh Links
			writer.WriteStartElement("OffMeshLinks");
			foreach (NavMeshEditor.OffMeshLink l in this.mesh.linkList)
			{
				writer.WriteStartElement("OffMeshLink");


				writer.WriteStartElement("Point-A");

				writer.WriteStartElement("Position");
				writer.WriteElementString("x", "" + l.transform.position.x);
				writer.WriteElementString("y", "" + l.transform.position.y);
				writer.WriteElementString("z", "" + l.transform.position.z);
				writer.WriteEndElement();

				int pos = 0;
				foreach (Face f in this.mesh.faces)
				{
					if (f == l.linkedFace)
						break;
					pos++;
				}
				writer.WriteElementString("Face", "" + pos);

				writer.WriteEndElement(); // End Point A


				writer.WriteStartElement("Point-B");

				writer.WriteStartElement("Position");
				writer.WriteElementString("x", "" + l.brother.transform.position.x);
				writer.WriteElementString("y", "" + l.brother.transform.position.y);
				writer.WriteElementString("z", "" + l.brother.transform.position.z);
				writer.WriteEndElement();

				pos = 0;
				foreach (Face f in this.mesh.faces)
				{
					if (f == l.brother.linkedFace)
						break;
					pos++;
				}
				writer.WriteElementString("Face", "" + pos);

				writer.WriteEndElement(); // End Point B

				writer.WriteEndElement();

				switch (l.direction)
				{
					case NavMeshEditor.OffMeshLinkDirection.none:
						writer.WriteElementString("Direction", "none");
						break;
					case NavMeshEditor.OffMeshLinkDirection.going:
						writer.WriteElementString("Direction", "A to B");
						break;
					case NavMeshEditor.OffMeshLinkDirection.returning:
						writer.WriteElementString("Direction", "B to A");
						break;
					case NavMeshEditor.OffMeshLinkDirection.both:
						writer.WriteElementString("Direction", "both");
						break;
				}
			}

			// Import if new
			if (isNew)
				AssetDatabase.ImportAsset(fileName);

			writer.WriteEndElement();
			writer.WriteEndDocument();
			writer.Close();
		}
	}

	void Load()
	{
		if (navMeshFile == null)
			return;

		this.StartEdit();

		// Getting name
		string fileName = AssetDatabase.GetAssetPath(navMeshFile);

		// Creating Xml and start writing
		System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();
		using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(fileName, settings))
		{
			System.Xml.Linq.XDocument doc = System.Xml.Linq.XDocument.Load(reader);
			System.Xml.Linq.XElement rootElement = doc.Element("NavMesh");
			System.Xml.Linq.XElement curElement;

			// Check version compatibility
			int version = int.Parse(doc.Element("Version").Element("Version").Value);
			if (version != NavMeshEditorWindow.xmlVersion)
			{
				Debug.LogError("Xml Vile incompatible [file version = " + version + ", current version = " + NavMeshEditorWindow.xmlVersion + "]");
				reader.Close();
				return;
			}

			// Vertices
			curElement = rootElement.Element("Vertices");
			foreach (System.Xml.Linq.XElement vertexElement in curElement.Elements("Vertex"))
			{
				System.Xml.Linq.XElement position = vertexElement.Element("Position");
				float x = float.Parse(position.Element("x").Value);
				float y = float.Parse(position.Element("y").Value);
				float z = float.Parse(position.Element("z").Value);

				// Create
				Vertex newVertex = Vertex.NewVertex(this.mesh);
				newVertex.transform.position = new Vector3(x, y, z);
			}

			// Faces
			curElement = rootElement.Element("Faces");
			foreach (System.Xml.Linq.XElement faceElement in curElement.Elements("Face"))
			{
				List<Vertex> vertices = new List<Vertex>();

				foreach (System.Xml.Linq.XElement vertexElement in faceElement.Elements("Vertex"))
				{
					int index = int.Parse(vertexElement.Value);
					vertices.Add(this.mesh.vertexes[index]);
				}

				// Create
				Face.NewFace(this.mesh, vertices.ToArray());
			}

			// Off-Mesh Links
			curElement = rootElement.Element("OffMeshLinks");
			foreach (System.Xml.Linq.XElement linkElement in curElement.Elements("OffMeshLink"))
			{
				// Point A
				System.Xml.Linq.XElement pointElement = linkElement.Element("Point-A");
				System.Xml.Linq.XElement position = pointElement.Element("Position");
				float x_A = float.Parse(position.Element("x").Value);
				float y_A = float.Parse(position.Element("y").Value);
				float z_A = float.Parse(position.Element("z").Value);

				int faceIndex_A = int.Parse(pointElement.Element("Face").Value);

				// Point B
				pointElement = linkElement.Element("Point-B");
				position = pointElement.Element("Position");
				float x_B = float.Parse(position.Element("x").Value);
				float y_B = float.Parse(position.Element("y").Value);
				float z_B = float.Parse(position.Element("z").Value);

				int faceIndex_B = int.Parse(pointElement.Element("Face").Value);

				// Direction
				string directionString = linkElement.Element("Direction").Value;
				NavMeshEditor.OffMeshLinkDirection direction = NavMeshEditor.OffMeshLinkDirection.none;
				if (directionString.Equals("none"))
					direction = NavMeshEditor.OffMeshLinkDirection.none;
				else if (directionString.Equals("A to B"))
					direction = NavMeshEditor.OffMeshLinkDirection.going;
				else if (directionString.Equals("B to A"))
					direction = NavMeshEditor.OffMeshLinkDirection.returning;
				else if (directionString.Equals("both"))
					direction = NavMeshEditor.OffMeshLinkDirection.both;

				// Create
				NavMeshEditor.OffMeshLink newLink = NavMeshEditor.OffMeshLink.NewOffMeshLink(this.mesh, this.mesh.faces[faceIndex_A], this.mesh.faces[faceIndex_B]);
				newLink.direction = direction;
				newLink.transform.position = new Vector3(x_A, y_A, z_A);
				newLink.brother.transform.position = new Vector3(x_B, y_B, z_B);
			}

			reader.Close();
		}

		SceneView.RepaintAll();
	}
	#endregion

	#endregion


	Object[] GetSelectedWithComponet(System.Type component)
	{
		// Get selected vertices
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