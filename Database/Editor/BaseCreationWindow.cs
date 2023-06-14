using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static RD.DB.EditorGuiTools;

namespace RD.DB
{
	[ExecuteInEditMode]
	public class BaseCreationWindow : EditorWindow
	{
		//
		/*
    MaterialChangerConfigScriptable mcConfig;
    private List<GameObject> sceneObjects = new List<GameObject>();
    private List<GameObject> modelChangerObjects = new List<GameObject>();
    private List<string> modelChangerGroupsFound = new List<string>();

    Dictionary<string, ModelOptionPackage> modelChangerDictionary = new Dictionary<string, ModelOptionPackage>();

    private string newName = "";
    private string newGroup = "";
    private ModelOptionPackage newModelPackage = null;

    private bool showModelChangers = true;
	*/
		//
		public BaseObject _baseObject;
		public BaseCard _baseCard;
		public BaseEffect _baseEffect;
		public BaseAura _baseAura;
		public BaseTalent _baseTalent;
		public BaseRequirement _baseRequirement;
		public BaseFX _baseFX;
		public BaseFXAudio _baseFXAudio;
		public Hyperlink _hyperlink;
		public DialogCharacter _dialogCharacter;
		public BaseFight _baseFight;
		public BaseItem _baseItem;
		public BaseConsumable _baseConsumable;
		public BaseEquipment _baseEquipment;
		public BaseWeapon _baseWeapon;
		public SceneConstructor _sceneConstructor;
		public BaseCombatCharacter _baseCombatCharacter;
		public ObjectType _showObject;
		public enum ObjectType { None,Card,Effect,Aura,Talent,Requirement, FX,FXAudio, Item, Equipment, Weapon, Consumable, Hyperlink, SceneConstructor, DialogCharacter, Fight, CombatCharacter }
		List<int> spaces = new List<int> { 6, 10 };
		BaseCollection _baseCollection;
		[MenuItem("Database/Create")]
		static void OpenWindow()
		{
			BaseCreationWindow window = EditorWindow.GetWindow(typeof(BaseCreationWindow),false,"Database Creation") as BaseCreationWindow;
		
			window.Show();
		}


		private void OnGUI()
		{
			if (_baseCollection == null)
			{
				_baseCollection = BaseCollectionEditor.GetDatabase();
			}
			if (_showObject != ObjectType.None)
				if (GUILayout.Button("Cancel", GUILayout.Width(150)))
					Cancel();

			GUILayout.BeginVertical();
			if (_showObject == ObjectType.None) GUILayout.Space(20);
			for (int i = 1; i < Enum.GetNames(typeof(ObjectType)).Length; i++)
			{
				if (i % 2 > 0)
					GUILayout.BeginHorizontal();
				ObjectType obj = (ObjectType)Enum.ToObject(typeof(ObjectType), i);
				DrawObject(obj);
				if (i % 2 == 0)
				{
					GUILayout.EndHorizontal();
					if (spaces.Contains(i) && _showObject == ObjectType.None)
					{
						GUILayout.Space(5);
						GuiLine(3);
						GUILayout.Space(5);
					}
				}
			}
			#region Methods

			void DrawObject(ObjectType oType)
			{
				if (_showObject == ObjectType.None)
				{
					GUILayout.Space(20);
					if (_showObject != oType)
					{
						if (GUILayout.Button("New " + oType.ToString(), GUILayout.Width(150)))
						{
							_showObject = oType;
							NewObject(_showObject);
							Repaint();
						}
					}
				}
				else if (_showObject == oType)
				{
					GUILayout.BeginVertical();
					DrawClassFields(GetObject(oType), 0, false, true);
					GUILayout.BeginHorizontal();
					if (GUILayout.Button("Save " + oType.ToString(), GUILayout.Width(150)))
					{
						BaseObject bob = SaveObject(_showObject);
						AutoRename(bob);
						SaveDatabase(_baseCollection);
						AssetDatabase.SaveAssets();
						_showObject = ObjectType.None;
						Repaint();
						Selection.activeObject =  bob;
					}
					if (GUILayout.Button("Save " + oType.ToString()+" and keep open.", GUILayout.Width(250)))
					{
						BaseObject bob = SaveObject(_showObject);
						AutoRename(bob);
						SaveDatabase(_baseCollection);
						AssetDatabase.SaveAssets();
						Selection.activeObject = bob;
					}
					GUILayout.EndHorizontal();
					GUILayout.EndVertical();
				}
				Repaint();
			}
			string GetFolder()
			{
				string s = "";
				switch (_showObject)
				{
					default:
						s = _showObject.ToString();
						break;
				}
				return s + "/";
			}
			object GetObject(ObjectType ot)
			{
				switch (ot)
				{
					case ObjectType.Aura:
						return _baseAura;
					case ObjectType.Card:
						return _baseCard;
					case ObjectType.Consumable:
						return _baseConsumable;
					case ObjectType.DialogCharacter:
						return _dialogCharacter;
					case ObjectType.Effect:
						return _baseEffect;
					case ObjectType.Equipment:
						return _baseEquipment;
					case ObjectType.Fight:
						return _baseFight;
					case ObjectType.Talent:
						return _baseTalent;
					case ObjectType.FX:
						return _baseFX;
					case ObjectType.Hyperlink:
						return _hyperlink;
					case ObjectType.Item:
						return _baseItem;
					case ObjectType.None:
					default:
						return null;
					case ObjectType.Requirement:
						return _baseRequirement;
					case ObjectType.Weapon:
						return _baseWeapon;
					case ObjectType.SceneConstructor:
						return _sceneConstructor;
					case ObjectType.FXAudio:
						return _baseFXAudio;
					case ObjectType.CombatCharacter:
						return _baseCombatCharacter;
				}
			}
			void NewObject(ObjectType ot)
			{
				switch (ot)
				{
					case ObjectType.Aura:
						_baseAura = (BaseAura)CreateInstance(typeof(BaseAura));
						break;
					case ObjectType.Card:
						_baseCard = (BaseCard)CreateInstance(typeof(BaseCard));
						break;
					case ObjectType.Consumable:
						_baseConsumable = (BaseConsumable)CreateInstance(typeof(BaseConsumable));
						break;
					case ObjectType.DialogCharacter:
						_dialogCharacter = (DialogCharacter)CreateInstance(typeof(DialogCharacter));
						break;
					case ObjectType.Effect:
						_baseEffect = (BaseEffect)CreateInstance(typeof(BaseEffect));
						break;
					case ObjectType.Equipment:
						_baseEquipment = (BaseEquipment)CreateInstance(typeof(BaseEquipment));
						break;
					case ObjectType.Fight:
						_baseFight = (BaseFight)CreateInstance(typeof(BaseFight));
						break;
					case ObjectType.Talent:
						_baseTalent = (BaseTalent)CreateInstance(typeof(BaseTalent));
						break;
					case ObjectType.FX:
						_baseFX = (BaseFX)CreateInstance(typeof(BaseFX));
						break;
					case ObjectType.Hyperlink:
						_hyperlink = (Hyperlink)CreateInstance(typeof(Hyperlink));
						break;
					case ObjectType.Item:
						_baseItem = (BaseItem)CreateInstance(typeof(BaseItem));
						break;
					case ObjectType.Requirement:
						_baseRequirement = (BaseRequirement)CreateInstance(typeof(BaseRequirement));
						break;
					case ObjectType.Weapon:
						_baseWeapon = (BaseWeapon)CreateInstance(typeof(BaseWeapon));
						break;
					case ObjectType.SceneConstructor:
						_sceneConstructor = (SceneConstructor)CreateInstance(typeof(SceneConstructor));
						break;
					case ObjectType.None:
						break;
					case ObjectType.FXAudio:
						_baseFXAudio = (BaseFXAudio)CreateInstance(typeof(BaseFXAudio));
						break;
					case ObjectType.CombatCharacter:
						_baseCombatCharacter = (BaseCombatCharacter)CreateInstance(typeof(BaseCombatCharacter));
						break;
				}
			}
			BaseObject SaveObject(ObjectType ot)
			{
			
				switch (ot)
				{
					case ObjectType.Aura:
						AssetDatabase.CreateAsset(_baseAura, "Assets/Resources/Database/" + GetFolder() + _baseAura.GetName() + ".asset");
						//_baseCollection.AddToAuraDatabase(_baseAura);
						_baseCollection.Add<BaseAura>(_baseAura);
						return _baseAura;
					case ObjectType.Card:
						AssetDatabase.CreateAsset(_baseCard, "Assets/Resources/Database/" + GetFolder() + _baseCard.GetName() + ".asset");
						_baseCollection.Add<BaseCard>(_baseCard);
						return _baseCard;
					case ObjectType.Consumable:
						AssetDatabase.CreateAsset(_baseConsumable, "Assets/Resources/Database/" + GetFolder() + _baseConsumable.GetName() + ".asset");
						_baseCollection.Add<BaseConsumable>(_baseConsumable);
						return _baseConsumable;
					case ObjectType.DialogCharacter:
						AssetDatabase.CreateAsset(_dialogCharacter, "Assets/Resources/Database/" + GetFolder() + _dialogCharacter.GetName() + ".asset");
						_baseCollection.Add<DialogCharacter>(_dialogCharacter);
						return _dialogCharacter;
					case ObjectType.Effect:
						AssetDatabase.CreateAsset(_baseEffect, "Assets/Resources/Database/" + GetFolder() + _baseEffect.GetName() + ".asset");
						_baseCollection.Add<BaseEffect>(_baseEffect);
						return _baseEffect;
					case ObjectType.Equipment:
						AssetDatabase.CreateAsset(_baseEquipment, "Assets/Resources/Database/" + GetFolder() + _baseEquipment.GetName() + ".asset");
						_baseCollection.Add<BaseEquipment>(_baseEquipment);
						return _baseEquipment;
					case ObjectType.Fight:
						AssetDatabase.CreateAsset(_baseFight, "Assets/Resources/Database/" + GetFolder() + _baseFight.GetName() + ".asset");
						_baseCollection.Add<BaseFight>(_baseFight);
						return _baseFight;
					case ObjectType.Talent:
						AssetDatabase.CreateAsset(_baseTalent, "Assets/Resources/Database/" + GetFolder() + _baseTalent.GetName() + ".asset");
						_baseCollection.Add<BaseTalent>(_baseTalent);
						return _baseTalent;
					case ObjectType.FX:
						AssetDatabase.CreateAsset(_baseFX, "Assets/Resources/Database/" + GetFolder() + _baseFX.GetName() + ".asset");
						_baseCollection.Add<BaseFX>(_baseFX);
						return _baseFX;
					case ObjectType.FXAudio:
						AssetDatabase.CreateAsset(_baseFXAudio, "Assets/Resources/Database/" + GetFolder() + _baseFXAudio.GetName() + ".asset");
						_baseCollection.Add<BaseFXAudio>(_baseFXAudio);
						return _baseFXAudio;
					case ObjectType.Hyperlink:
						AssetDatabase.CreateAsset(_hyperlink, "Assets/Resources/Database/" + GetFolder() + _hyperlink.GetName() + ".asset");
						_baseCollection.Add<Hyperlink>(_hyperlink);
						return _hyperlink;
					case ObjectType.Item:
						AssetDatabase.CreateAsset(_baseItem, "Assets/Resources/Database/" + GetFolder() + _baseItem.GetName() + ".asset");
						_baseCollection.Add<BaseItem>(_baseItem);
						return _baseItem;
					case ObjectType.Requirement:
						AssetDatabase.CreateAsset(_baseRequirement, "Assets/Resources/Database/" + GetFolder() + _baseRequirement.GetName() + ".asset");
						_baseCollection.Add<BaseRequirement>(_baseRequirement);
						return _baseRequirement;
					case ObjectType.Weapon:
						AssetDatabase.CreateAsset(_baseWeapon, "Assets/Resources/Database/" + GetFolder() + _baseWeapon.GetName() + ".asset");
						_baseCollection.Add<BaseWeapon>(_baseWeapon);
						return _baseWeapon;
					case ObjectType.SceneConstructor:
						AssetDatabase.CreateAsset(_sceneConstructor, "Assets/Resources/Database/" + GetFolder() + _sceneConstructor.GetName() + ".asset");
						_baseCollection.Add<SceneConstructor>(_sceneConstructor);
						return _sceneConstructor;
					case ObjectType.CombatCharacter:
						if (_baseCombatCharacter._visualObject == null)
						{
							Debug.LogError("Visual Object Required, assigning default value!");
							_baseCombatCharacter._visualObject = ((BaseCombatCharacter)_baseCollection.Get<BaseCombatCharacter>(1))._visualObject;
						}
						AssetDatabase.CreateAsset(_baseCombatCharacter, "Assets/Resources/Database/" + GetFolder() + _baseCombatCharacter.GetName() + ".asset");
						_baseCollection.Add<BaseCombatCharacter>(_baseCombatCharacter);
						return _baseCombatCharacter;
					case ObjectType.None:
					default:
						Debug.LogError("unknown ShowObject type");
						return null;
				}
			}
			#endregion
		}


		void Cancel()
		{
			_showObject = ObjectType.None;
			Repaint();
		}
	}
}