using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class NameSpritesAutomatically : MonoBehaviour
{

	// private enum FramesCount : int { idle = 1, crouch = 3, jump = 3, run = 6, roll = 6, swing = 9, sting = 3, spear_sting = 3, spear_swing = 3, overhead_swing = 3};


	[MenuItem("Database/Sprites/Rename Card Arts")]
	static void SetCardArtName()
	{
		List<Texture2D> textures = new List<Texture2D>();
		string path = "/Resources/Card Art/";
		string datapath = Application.dataPath + path;
		string[] fileEntries = Directory.GetFiles(datapath);
		List<string> _imgs = new List<string>();
		foreach(string s in fileEntries)
		{
			if (s.EndsWith(".png"))
				_imgs.Add(s);
		}
		Debug.Log("vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv");
		//Debug.Log("<color=green>"+datapath + " contains + " + _imgs.Count + " images</color>");
		Debug.Log("--------------------------------------");
		for (int i = 0; i < _imgs.Count; i++)
		{
			_imgs[i] = _imgs[i].Substring(_imgs[i].LastIndexOf("/")+1);
			Texture2D t = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Resources/Card Art/"+_imgs[i], typeof(Texture2D));
			path = AssetDatabase.GetAssetPath(t);
			TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
			SpriteMetaData[] smd = ti.spritesheet;
			if (smd.Length > 0)
			{
				string oldName = smd[0].name;
				string nm = "Card Art "+_imgs[i].Substring(0, _imgs[i].LastIndexOf("."));
				if (smd[0].name != nm)
				{
					smd[0].name = nm;
					Debug.Log(_imgs[i] + " name was <color=orange>" + oldName + "</color> and is now <color=orange>" + nm+"</color>");
					ti.spritesheet = smd;
					//Debug.Log(ti.spritesheet[0].name);
					EditorUtility.SetDirty(ti);
					ti.SaveAndReimport();
					//Debug.Log(ti.spritesheet[0].name);
					//AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
				}
			}
		}
		Debug.Log("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
		AssetDatabase.SaveAssets();

		return;
	}
	
	static void SetSpriteNames()
	{
		int[] FramesCount = new int[] { 1, 3, 3, 6, 6, 9, 3, 3, 3, 3 };
		string[] FramesName = new string[] { "idle", "crouch", "jump", "run", "roll", "swing", "sting", "spear_sting", "spear_swing", "overhead_swing" };
		int animAmount = 10;

		Texture2D myTexture = (Texture2D)AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Sprites/Player_Sprites/player_spritesheet_new.png");

		string path = AssetDatabase.GetAssetPath(myTexture);
		TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;

		ti.isReadable = true;


		List<SpriteMetaData> newData = new List<SpriteMetaData>();

		int SliceWidth = 40;
		int SliceHeight = 32;

		for (int y = 0; y < animAmount; y++)
		{
			for (int x = 0; x < FramesCount[y]; x++)
			{
				SpriteMetaData smd = new SpriteMetaData();
				smd.pivot = new Vector2(0.5f, 0.5f);
				smd.name = FramesName[y] + "_" + (x + 1);
				smd.rect = new Rect(x * SliceWidth, myTexture.height - y * SliceHeight - SliceHeight, SliceWidth, SliceHeight);

				newData.Add(smd);
			}
		}

		ti.spritesheet = newData.ToArray();
		AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
	}
}
