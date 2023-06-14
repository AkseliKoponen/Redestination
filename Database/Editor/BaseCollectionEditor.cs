using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace RD.DB
{
	[CustomEditor(typeof(BaseCollection))]
	public class BaseCollectionEditor : BaseEditor
	{
		enum BaseType { card, effect, aura, requirement, FX,FXAudio, Hyperlink, item, talent, DialogCharacter, weapon, equipment, consumable, SceneConstructor, combatCharacter, fight}
		BaseType _baseType = BaseType.card;
		Type _type;
		bool showList = false;
		BaseCollection _base;
		List<BaseObject> _displayList = new List<BaseObject>();
		bool debug = false;
		string _searchString;
		public void OnEnable()
		{
			_base = target as BaseCollection;
		
		}
		public override void OnInspectorGUI()
		{
			
			if (!showList) {
				GUILayout.BeginVertical();
				#region Combat
				GUILayout.Label("COMBAT");
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Cards", GUILayout.Width(150)))
					ChangeType(BaseType.card);
				if (GUILayout.Button("Effects", GUILayout.Width(150)))
					ChangeType(BaseType.effect);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Auras", GUILayout.Width(150)))
					ChangeType(BaseType.aura);
				if (GUILayout.Button("Requirements", GUILayout.Width(150)))
					ChangeType(BaseType.requirement);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Combat Characters", GUILayout.Width(150)))
					ChangeType(BaseType.combatCharacter);
				if (GUILayout.Button("Talents", GUILayout.Width(150)))
					ChangeType(BaseType.talent);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Fights", GUILayout.Width(150)))
					ChangeType(BaseType.fight);
				GUILayout.EndHorizontal();
				#endregion
				#region Items
				GUILayout.Label("ITEMS");
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Items", GUILayout.Width(150)))
					ChangeType(BaseType.item);
				if (GUILayout.Button("Consumables", GUILayout.Width(150)))
					ChangeType(BaseType.consumable);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Equipments", GUILayout.Width(150)))
					ChangeType(BaseType.equipment);
				if (GUILayout.Button("Weapons", GUILayout.Width(150)))
					ChangeType(BaseType.weapon);
				GUILayout.EndHorizontal();
				#endregion
				#region Text
				GUILayout.Label("TEXT");
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Hyperlinks", GUILayout.Width(150)))
					ChangeType(BaseType.Hyperlink);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Scene Constructors", GUILayout.Width(150)))
					ChangeType(BaseType.SceneConstructor);
				if (GUILayout.Button("Dialog Characters", GUILayout.Width(150)))
					ChangeType(BaseType.DialogCharacter);
				GUILayout.EndHorizontal();
				#endregion
				#region Art
				GUILayout.Label("ART");
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("FX", GUILayout.Width(150)))
					ChangeType(BaseType.FX);
				if (GUILayout.Button("FX Audio", GUILayout.Width(150)))
					ChangeType(BaseType.FXAudio);
				GUILayout.EndHorizontal();
				GUILayout.Space(50);
				#endregion
				#region DatabaseTools
				GUILayout.BeginHorizontal();
				_searchString = GUILayout.TextField(_searchString, GUILayout.Width(150));
				if (GUILayout.Button("Search", GUILayout.Width(150)))
				{
					_base.Search(_searchString);
				}
				GUILayout.EndHorizontal();
				#endregion
				//GUILayout.Label("");
				//if(GUILayout.Button("Release IDs", GUILayout.Width(150))) IDScrubWindow.Init(this);
				GUILayout.EndVertical();
			}
			else
			{
				Color defaultColor = GUI.color;
				GUILayout.BeginVertical();
				GUI.color = new Color(0.75f, 0.75f,0.25f);
				GUILayout.Label(_baseType.ToString().ToUpper());
				GUILayout.BeginHorizontal();
				GUI.color = new Color(0.75f,0.25f,0.25f);
				if (GUILayout.Button("Back", GUILayout.Width(100)))
					Back();
				GUI.color = defaultColor;
				if (_baseType == BaseType.card)
				{
					GUILayout.Space(50);
					if (GUILayout.Button("Update Descriptions", GUILayout.Width(200)))
						_base.UpdateDescriptionsCards();
					GUILayout.Space(50);
				}
				else if (_baseType == BaseType.Hyperlink)
				{
					GUILayout.Space(50);
					if (GUILayout.Button("Update Descriptions", GUILayout.Width(200)))
						_base.UpdateDescriptionsHyperlinks();
					GUILayout.Space(50);
				}
				else
					GUILayout.Space(100);
				GUI.color = new Color(0.25f, 0.75f, 0.25f);
				if (GUILayout.Button("SAVE", GUILayout.Width(100)))
					Back(true);
				GUI.color = defaultColor;
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				if(GUILayout.Button("id", GUILayout.Width(50)))
					Sort("id");
				if (GUILayout.Button("Name", GUILayout.Width(150)))
					Sort("name");
				if (GUILayout.Button("New "+ _baseType.ToString(), GUILayout.Width(150)))
				{
					_displayList.Add(null);
				}
				GUILayout.EndHorizontal();

				for (int i = 0;i< _displayList.Count;i++)
				{
					BaseObject bo = _displayList[i];
					GUILayout.BeginHorizontal();
					GUILayout.Label(new GUIContent(bo != null ? bo.GetID().ToString() : "-", bo != null ? bo._description : "-"), GUILayout.Width(50));
					GUILayout.Label(new GUIContent(bo != null ? bo._name : "-", bo != null?bo._description:"-"), GUILayout.Width(150));
					switch (_baseType)
					{
						case BaseType.card:
							bo = (BaseCard)_displayList[i];
							bo = (BaseCard)EditorGUILayout.ObjectField(bo, typeof(BaseCard), false);
							break;
						case BaseType.effect:
							bo = (BaseEffect)_displayList[i];
							bo = (BaseEffect)EditorGUILayout.ObjectField(bo, typeof(BaseEffect), false);
							break;
						case BaseType.aura:
							bo = (BaseAura)_displayList[i];
							bo = (BaseAura)EditorGUILayout.ObjectField(bo, typeof(BaseAura), false);
							break;
						case BaseType.requirement:
							bo = (BaseRequirement)_displayList[i];
							bo = (BaseRequirement)EditorGUILayout.ObjectField(bo, typeof(BaseRequirement), false);
							break;
						case BaseType.FX:
							bo = (BaseFX)_displayList[i];
							bo = (BaseFX)EditorGUILayout.ObjectField(bo, typeof(BaseFX), false);
							break;
						case BaseType.FXAudio:
							bo = (BaseFXAudio)_displayList[i];
							bo = (BaseFXAudio)EditorGUILayout.ObjectField(bo, typeof(BaseFXAudio), false);
							break;
						case BaseType.Hyperlink:
							bo = (Hyperlink)_displayList[i];
							bo = (Hyperlink)EditorGUILayout.ObjectField(bo, typeof(Hyperlink), false);
							break;
						case BaseType.item:
							bo = (BaseItem)_displayList[i];
							bo = (BaseItem)EditorGUILayout.ObjectField(bo, typeof(BaseItem), false);
							break;
						case BaseType.consumable:
							bo = (BaseConsumable)_displayList[i];
							bo = (BaseConsumable)EditorGUILayout.ObjectField(bo, typeof(BaseConsumable), false);
							break;
						case BaseType.equipment:
							bo = (BaseEquipment)_displayList[i];
							bo = (BaseEquipment)EditorGUILayout.ObjectField(bo, typeof(BaseEquipment), false);
							break;
						case BaseType.weapon:
							bo = (BaseWeapon)_displayList[i];
							bo = (BaseWeapon)EditorGUILayout.ObjectField(bo, typeof(BaseWeapon), false);
							break;
						case BaseType.talent:
							bo = (BaseTalent)_displayList[i];
							bo = (BaseTalent)EditorGUILayout.ObjectField(bo, typeof(BaseTalent), false);
							break;
						case BaseType.DialogCharacter:
							bo = (DialogCharacter)_displayList[i];
							bo = (DialogCharacter)EditorGUILayout.ObjectField(bo, typeof(DialogCharacter), false);
							break;
						case BaseType.SceneConstructor:
							bo = (SceneConstructor)_displayList[i];
							bo = (SceneConstructor)EditorGUILayout.ObjectField(bo, typeof(SceneConstructor), false);
							break;
						case BaseType.fight:
							bo = (BaseFight)_displayList[i];
							bo = (BaseFight)EditorGUILayout.ObjectField(bo, typeof(BaseFight), false);
							break;
						case BaseType.combatCharacter:
							bo = (BaseCombatCharacter)_displayList[i];
							bo = (BaseCombatCharacter)EditorGUILayout.ObjectField(bo, typeof(BaseCombatCharacter), false);
							break;
						default:break;
					}
					_displayList[i] = bo;
					if (GUILayout.Button("-", GUILayout.Width(50))){
						_displayList.RemoveAt(i);
						i--;
					}
					if (GUILayout.Button("COPY", GUILayout.Width(50)))
					{
						switch (_baseType)
						{
							case BaseType.card:
								_displayList.Add(Copy<BaseCard>(bo));
								break;
							case BaseType.effect:
								_displayList.Add(Copy<BaseEffect>(bo));
								break;
							case BaseType.aura:
								_displayList.Add(Copy<BaseAura>(bo));
								break;
							case BaseType.requirement:
								_displayList.Add(Copy<BaseRequirement>(bo));
								break;
							case BaseType.FX:
								_displayList.Add(Copy<BaseFX>(bo));
								break;
							case BaseType.FXAudio:
								_displayList.Add(Copy<BaseFXAudio>(bo));
								break;
							case BaseType.Hyperlink:
								_displayList.Add(Copy<Hyperlink>(bo));
								break;
							case BaseType.item:
								_displayList.Add(Copy<BaseItem>(bo));
								break;
							case BaseType.consumable:
								_displayList.Add(Copy<BaseConsumable>(bo));
								break;
							case BaseType.equipment:
								_displayList.Add(Copy<BaseEquipment>(bo));
								break;
							case BaseType.weapon:
								_displayList.Add(Copy<BaseWeapon>(bo));
								break;
							case BaseType.talent:
								_displayList.Add(Copy<BaseTalent>(bo));
								break;
							case BaseType.DialogCharacter:
								_displayList.Add(Copy<DialogCharacter>(bo));
								break;
							case BaseType.SceneConstructor:
								_displayList.Add(Copy<SceneConstructor>(bo));
								break;
							case BaseType.fight:
								_displayList.Add(Copy<BaseFight>(bo));
								break;
							case BaseType.combatCharacter:
								_displayList.Add(Copy<BaseCombatCharacter>(bo));
								break;
							default: break;
						}
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();

			}

		}
		void Back(bool save = false)
		{
			showList = false;
			if (save)
			{
				_base.initList();
				Sort();
				switch (_baseType)
				{
					case BaseType.card:
						_base._cards.Clear();
						foreach(BaseObject bo in _displayList)
							_base._cards.Add((BaseCard)bo);
						break;
					case BaseType.aura:
						_base._auras.Clear();
						foreach (BaseObject bo in _displayList)
							_base._auras.Add((BaseAura)bo);
						break;
					case BaseType.effect:
						_base._effects.Clear();
						foreach (BaseObject bo in _displayList)
							_base._effects.Add((BaseEffect)bo);
						break;
					case BaseType.FX:
						_base._fXs.Clear();
						foreach (BaseObject bo in _displayList)
							_base._fXs.Add((BaseFX)bo);
						break;
					case BaseType.FXAudio:
						_base._fXAudios.Clear();
						foreach (BaseObject bo in _displayList)
							_base._fXAudios.Add((BaseFXAudio)bo);
						break;
					case BaseType.requirement:
						_base._requirements.Clear();
						foreach (BaseObject bo in _displayList)
							_base._requirements.Add((BaseRequirement)bo);
						break;
					case BaseType.Hyperlink:
						_base._hyperlinks.Clear();
						foreach (BaseObject bo in _displayList)
							_base._hyperlinks.Add((Hyperlink)bo);
						break;
					case BaseType.DialogCharacter:
						_base._dialogCharacters.Clear();
						foreach (BaseObject bo in _displayList)
							_base._dialogCharacters.Add((DialogCharacter)bo);
						break;
					case BaseType.item:
						_base._items.Clear();
						foreach (BaseObject bo in _displayList)
							_base._items.Add((BaseItem)bo);
						break;
					case BaseType.equipment:
						_base._equipments.Clear();
						foreach (BaseObject bo in _displayList)
							_base._equipments.Add((BaseEquipment)bo);
						break;
					case BaseType.weapon:
						_base._weapons.Clear();
						foreach (BaseObject bo in _displayList)
							_base._weapons.Add((BaseWeapon)bo);
						break;
					case BaseType.consumable:
						_base._consumables.Clear();
						foreach (BaseObject bo in _displayList)
							_base._consumables.Add((BaseConsumable)bo);
						break;
					case BaseType.SceneConstructor:
						_base._sceneConstructors.Clear();
						foreach (BaseObject bo in _displayList)
							_base._sceneConstructors.Add((SceneConstructor)bo);
						break;
					case BaseType.combatCharacter:
						_base._combatCharacters.Clear();
						foreach (BaseObject bo in _displayList)
							_base._combatCharacters.Add((BaseCombatCharacter)bo);
						break;
					case BaseType.fight:
						_base._fights.Clear();
						foreach (BaseObject bo in _displayList)
							_base._fights.Add((BaseFight)bo);
						break;
					default:
						break;
				}
				EditorUtility.SetDirty(_base);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
		}

		void ChangeType(BaseType bt)
		{
			showList = true;
			_baseType = bt;
			_displayList.Clear();
			_displayList = new List<BaseObject>();
			switch (_baseType)
			{
				case BaseType.card:
					_displayList.AddRange(_base._cards);
					break;
				case BaseType.effect:
					_displayList.AddRange(_base._effects);
					break;
				case BaseType.aura:
					_displayList.AddRange(_base._auras);
					break;
				case BaseType.requirement:
					_displayList.AddRange(_base._requirements);
					break;
				case BaseType.FX:
					_displayList.AddRange(_base._fXs);
					break;
				case BaseType.FXAudio:
					_displayList.AddRange(_base._fXAudios);
					break;
				case BaseType.Hyperlink:
					_displayList.AddRange(_base._hyperlinks);
					break;
				case BaseType.item:
					_displayList.AddRange(_base._items);
					break;
				case BaseType.equipment:
					_displayList.AddRange(_base._equipments);
					break;
				case BaseType.weapon:
					_displayList.AddRange(_base._weapons);
					break;
				case BaseType.consumable:
					_displayList.AddRange(_base._consumables);
					break;
				case BaseType.talent:
					_displayList.AddRange(_base._talents);
					break;
				case BaseType.DialogCharacter:
					_displayList.AddRange(_base._dialogCharacters);
					break;
				case BaseType.SceneConstructor:
					_displayList.AddRange(_base._sceneConstructors);
					break;
				case BaseType.fight:
					_displayList.AddRange(_base._fights);
					break;
				case BaseType.combatCharacter:
					_displayList.AddRange(_base._combatCharacters);
					break;
			}
		}

		public List<BaseObject> _visualObjects;
		public void Sort(string sort = "id")
		{
			for (int i = 0; i < _displayList.Count; i++)
			{
				if (_displayList[i] == null)
				{
					_displayList.RemoveAt(i);
					i--;
					continue;
				}
				else
				{
					BaseObject bo = _displayList[i];
					for (int subi = 0; subi < i; subi++)
					{
						if (_displayList[subi] == null)
						{
							_displayList.RemoveAt(subi);
							subi--;
							continue;

						}
						else
						{
							BaseObject subBo = _displayList[subi];
							if (subBo == bo && subi != i)
							{
								_displayList.RemoveAt(subi);
								subi--;
								continue;
							}
							else
							{
								switch (sort)
								{
									case "id":
									default:
										if (bo.GetID() < subBo.GetID())
										{
											_displayList.RemoveAt(i);
											_displayList.Insert(subi, bo);
											subi = i;
										}
										break;
									case "name":
										if (String.Compare(bo._name, subBo._name) < 0)
										{
											_displayList.RemoveAt(i);
											_displayList.Insert(subi, bo);
											subi = i;
										}
										break;
								}
							}
						}
					}
				}
			}
		}
		public void IDScrub()
		{
			ScrubList<BaseCard>();
			ScrubList<BaseEffect>();
			ScrubList<BaseAura>();
			ScrubList<BaseRequirement>();
			ScrubList<BaseFX>();
			ScrubList<BaseFXAudio>();
			ScrubList<Hyperlink>();
			ScrubList<BaseItem>();
			ScrubList<BaseTalent>();
			ScrubList<DialogCharacter>();
			ScrubList<BaseWeapon>();
			ScrubList<BaseEquipment>();
			ScrubList<BaseConsumable>();
			ScrubList<SceneConstructor>();
			ScrubList<BaseFight>();
			ScrubList<BaseCombatCharacter>();
			EditorUtility.SetDirty(_base);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			void ScrubList<T>()
			{
				FieldInfo fi = _base.GetType().GetField(_base.GetListNameFromType(typeof(T)));
				if (fi == null)
					return;
				List<T> list = (List<T>)fi.GetValue(_base);
				for (int i = 0; i < list.Count; i++)
				{
					object osd = list[i];
					BaseObject bob = (BaseObject)osd;
					bob.SetID(i);
					EditorGuiTools.AutoRename(bob);
				}
				string idName = _base.GetIDNameFromType(typeof(T));
				FieldInfo fid = _base.GetType().GetField(idName);
				if (fid == null || fid.FieldType != typeof(int))
					return;
				fid.SetValue(_base, list.Count);
			}
		}


		public static BaseObject Copy<T>(BaseObject obj)
		{
			string path = AssetDatabase.GetAssetPath(obj);
			string newPath = path.Substring(0, path.LastIndexOf("/")+1) +obj.GetName() + ".asset";
			Debug.Log(path);
			AssetDatabase.CopyAsset(path, newPath);
			BaseObject bob = (BaseObject)(AssetDatabase.LoadAssetAtPath(newPath, typeof(T)));
			//BaseCollection bc = GetDatabase();
			GetDatabase().Add<T>(AssetDatabase.LoadAssetAtPath(newPath, typeof(T)));
			EditorGuiTools.AutoRename(bob);
			return bob;

		}
		public static BaseCollection GetDatabase()
		{
			return (BaseCollection)AssetDatabase.LoadAssetAtPath("Assets/Resources/Database/Collection.asset", typeof(BaseCollection));
		}
		protected override void Dirtify()
		{
			BaseCollection _base = target as BaseCollection;
			EditorUtility.SetDirty(_base);
		}
	}

	public class IDScrubWindow : EditorWindow
	{
		
		public static void Init(BaseCollectionEditor bce)
		{
			IDScrubWindow window = ScriptableObject.CreateInstance<IDScrubWindow>();
			edit = bce;
			window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
			window.ShowPopup();
		}

		static BaseCollectionEditor edit;
		void OnGUI()
		{
			EditorGUILayout.LabelField("Are you sure you want to normalize IDs?\nThis could take a while...", EditorStyles.wordWrappedLabel);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Yes")) edit.IDScrub();
			if (GUILayout.Button("Cancel")) this.Close();
			GUILayout.EndHorizontal();
		}
	}
}

