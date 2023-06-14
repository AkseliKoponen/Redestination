using UnityEngine;

//using UnityEditor;

namespace RD
{
	public static class GlobalSettings
	{
		static bool started;
		public static void Start()
		{
			started = true;
			_cursors = new Cursors(true);
			ResetCursor();
		
		}


		public static Cursors _cursors;
		public struct Cursors
		{
			public Texture2D defaultCursor;
			public Texture2D dragCursor;
			public Cursors(bool b = true)
			{
				string path = "Cursors/RPG/Cursors 64/";
				defaultCursor = UnityEngine.Resources.Load<Texture2D>(path + "Cursor_Basic2_G");
				//Debug.Log("Asd");
				//Debug.Log(PlayerSettings.defaultCursor.name);
				dragCursor = UnityEngine.Resources.Load<Texture2D>(path + "Cursor_Basic2_G");
			}

		}
		public static void SetDragCursor()
		{
			Debug.Log("SetDragCursor");
			if (!started)
				Start();
			UnityEngine.Cursor.SetCursor(_cursors.dragCursor, Vector2.zero, CursorMode.Auto);
		}

		public static void ResetCursor()
		{
			if (!started)
				Start();
			UnityEngine.Cursor.SetCursor(_cursors.defaultCursor, Vector2.zero, CursorMode.Auto);

		}

	}
}
