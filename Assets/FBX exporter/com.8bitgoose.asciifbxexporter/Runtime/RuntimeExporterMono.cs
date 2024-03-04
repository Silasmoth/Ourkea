using UnityEditor;
using UnityEngine;

namespace AsciiFBXExporter
{
	/// <summary>
	/// Simple mono component that shows how to export an object at runtime.
	/// Attach this to any game object and assign RootObjectToExport
	/// </summary>
	public class RuntimeExporterMono : MonoBehaviour
	{
		public GameObject rootObjectToExport;
		public string Fullpath = "C:/Users/clusi/Documents/GitHub/Ourkea/Model Epxorts/test.fbx";
		public string AbsolutePath = "C:/Users/clusi/Documents/GitHub/Ourkea/Model Epxorts";
		public string FileName = "TestFBXExport.fbx";
		public string TextureFolderName = "FBXTextures/";
		public bool usePlaceholderGUI = true;

		private string exportedFileName = null;

		void OnGUI()
		{
			if(usePlaceholderGUI == false)
				return;

			if(rootObjectToExport != null && GUI.Button(new Rect(10, 10, 150, 50), "Export FBX"))
			{
				this.ExportGameObject();
			}

			if(exportedFileName == null && AbsolutePath.Contains("kellan"))
			{
				GUI.Label(new Rect(10, 60, 400, 50), "You have not changed the absolte file name from the default absolute path. Please change.");
			}

			if(exportedFileName != null)
				GUI.Label(new Rect(10, 60, 400, 50), "Exported FBX File: `" + exportedFileName + "`");
		}

		public bool ExportGameObject()
		{
			
			return ExportGameObject(rootObjectToExport, Fullpath, TextureFolderName, ref exportedFileName);
		}

		/// <summary>
		/// Will export to whatever folder path is provided within the Assets folder
		/// </summary>
		/// <param name="rootGameObject"></param>
		/// <param name="folderPath"></param>
		/// <param name="fileName"></param>
		/// <param name="textureFolderName"></param>
		/// <returns></returns>
		public static bool ExportGameObject(GameObject rootGameObject, string fullpath, string textureFolderName, ref string exportedFileName)
		{
			if(rootGameObject == null)
			{
				Debug.Log("Root game object is null, please assign it");
				return false;
			}

			// forces use of forward slash for directory names
			fullpath = fullpath.Replace('\\', '/');
			

			bool exported = FBXExporter.ExportGameObjAtRuntime(rootGameObject, fullpath, textureFolderName, true);

#if UNITY_EDITOR
			UnityEditor.AssetDatabase.Refresh();
#endif
			return exported;
		}
	}
}