using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RD
{
	[CustomEditor(typeof(UISpritesAnimation))]
	public class UISpritesAnimationEditor: UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			UISpritesAnimation _base = target as UISpritesAnimation;
			DrawDefaultInspector();
			GUILayout.BeginHorizontal();
			GUILayout.Space(20);
			//GUILayout.Label("SpriteCount: " + (_base._sprites!=null?_base._sprites.Count.ToString():"null"));
			if (GUILayout.Button("RetrieveSprites",GUILayout.Width(100)))
			{
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
				List<Sprite> sprites = GetSpritesFromClip(_base._animationClip);
				_base._sprites = sprites;
			}
			GUILayout.EndHorizontal();
		}

		public List<Sprite> GetSpritesFromClip(AnimationClip _animationClip)
		{
			var sprites = new List<Sprite>();
			if (_animationClip != null)
			{
				foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(_animationClip))
				{
					var keyframes = AnimationUtility.GetObjectReferenceCurve(_animationClip, binding);
					foreach (var frame in keyframes)
					{
						sprites.Add((Sprite)frame.value);
					}
				}
			}
			return sprites;
		}

	}
}
