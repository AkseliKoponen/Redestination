using System;
using System.Collections.Generic;
using System.Reflection;
using RD.Combat;
using RD;
using UnityEditor;
using UnityEngine;
using static RD.CodeTools;
using TMPro;

namespace RD.DB
{
	public static class EditorGuiTools
	{
		public static bool _simpleMove = true;
		static List<string> hiddenFields = new List<string> { "_actualDescription", "_hostileArena","_friendlyArena","_linkToPast" };
		public static System.Object DrawClassFields(System.Object _class, float indent = 0, bool sameRow = false, bool useLayoutSpace = false)
		{
			if (_class.GetType().IsSubclassOf(typeof(BaseObject)))
			{
				GUILayout.Space(5);
				GUILayout.BeginHorizontal();
				GUILayout.Space(50);
				GUIStyle titleStyle = new GUIStyle();
				titleStyle.fontSize = 18;
				titleStyle.fontStyle = FontStyle.Bold;
				EditorGUILayout.LabelField(_class.GetType().Name.ToString(), titleStyle);
				GUILayout.EndHorizontal();
				GUILayout.Space(5);
				GuiLine();
				GUILayout.Space(5);
			}
			if (sameRow)
				GUILayout.BeginHorizontal();
			
			List<int> tempSpaces = new List<int>();
			LayOutSpace los = null;
			if (useLayoutSpace)
			{
				MethodInfo method = _class.GetType().GetMethod("GetLayout");
				if (method != null)
				{
					los = (LayOutSpace)method.Invoke(_class, null);
					tempSpaces.AddRange(los._spaces);
				}
			}
			int index = -1;
			foreach (FieldInfo field in _class.GetType().GetFields())
			{
				if (tempSpaces != null && tempSpaces.Count > 0)
				{
					tempSpaces[0] -= 1;
					if (tempSpaces[0] <= 0)
					{
						index += 1;
						EditorGUILayout.EndFoldoutHeaderGroup();
						los.layOuts[index] = EditorGUILayout.BeginFoldoutHeaderGroup(los.layOuts[index], los._names[index]);
						tempSpaces.RemoveAt(0);
						//EditorGUILayout.LabelField("------------------------------------------------");//EditorGUILayout.Space(30);
					}

				}
				if(field.Name == "_linkToPast")
				{
					GUILayout.Space(-40);
					bool b = (bool)field.GetValue(_class);
					if (GUILayout.Button(b ? "AND" : ".", GUILayout.Height(20), GUILayout.Width(40)))
					{
						field.SetValue(_class, !b);
					}
				}

				bool bannedName = hiddenFields.Contains(field.Name) || (_class.GetType()!=typeof(BaseCard) && field.Name.Equals("_links"));
				if (index >= 0 && !los.layOuts[index] || field.IsNotSerialized || bannedName)
					continue;
				string name = RenameVariable(field.Name);
				#region Exceptions
				if (name == "NameCombinationNoun" || name == "NameCombinationAdjective")
				{
					if (_class.GetType() == typeof(BaseCard))
					{
						BaseCard bc = (BaseCard)_class;
						if (!bc._cardClass.HasFlag(BaseCard.CardClass.Inventor))
						{
							continue;
						}
					}
				}
				if (_class.GetType() == typeof(BaseTalent))
				{
					if (name == "Trigger")
					{
						EditorGUILayout.BeginVertical();
						EditorGUILayout.Space(5);
						EditorGUILayout.EndVertical();
					}
					BaseTalent bt = (BaseTalent)_class;
					if (name == "TriggerEffect" && bt._trigger.HasFlag(BaseTalent.TalentTrigger.ActivateEffect) == false)
						continue;
					if (name == "TriggerAura" && (bt._trigger.HasFlag(BaseTalent.TalentTrigger.ApplyAura) == false && bt._trigger.HasFlag(BaseTalent.TalentTrigger.GainAura) == false))
						continue;
					if ((name == "ChargesPerCombat_base" || name == "ChargesPerDay_base") && bt._charges == false)
						continue;
					if (name == "AuraIcon" && bt._visibleAura == false)
						continue;
				}
				#endregion
				Type type = field.FieldType;
				if (field.Name == "hideFlags" || field.IsPrivate) continue;       //If is private, skip

				if (sameRow)
					GUILayout.BeginVertical();
				else
					GUILayout.BeginHorizontal();

				
				GUILayout.Space(indent);
				FieldAttributes fab = field.Attributes;
				TooltipAttribute tooltip = GetTooltip(field, true);
				if (type.Name != "Vector2")
				{
					string nameText = name;
					if(type.Name == "List`1")
					{
						System.Collections.ICollection list = field.GetValue(_class) as System.Collections.ICollection;
						if(list.Count > 1) nameText += " ("+list.Count+")";
					}
					if (tooltip != null)
						GUILayout.Label(new GUIContent(nameText, tooltip.tooltip), GUILayout.Width(sameRow ? 50 : 150));
					else
						EditorGUILayout.LabelField(nameText, GUILayout.Width(sameRow ? 50 : 150));
				}
				//Debug.Log(field.Name + type.Name);
				switch (type.Name)
				{

					default:
						Debug.Log("Unimplemented data type: " + type.Name);
						break;
					#region Common Fields
					case "Int32":
					case "Int64":
						switch (name)
						{
							case "Id":
								int val = (int)(field.GetValue(_class));
								EditorGUILayout.LabelField(val.ToString(), GUILayout.Width(50));
								break;
							case "Hp":
								field.SetValue(_class, EditorGUILayout.IntField((int)field.GetValue(_class), GUILayout.Width(50)));
								if (_class.GetType() == typeof(BaseCombatCharacter))
								{
									EditorGUILayout.Space(20);
									EditorGUILayout.LabelField("Total: " + ((BaseCombatCharacter)_class).CalculateMaxHP());
								}
								break;
							case "Initiative":
								field.SetValue(_class, EditorGUILayout.IntField((int)field.GetValue(_class), GUILayout.Width(50)));
								if (_class.GetType() == typeof(BaseCombatCharacter))
								{
									EditorGUILayout.Space(20);
									EditorGUILayout.LabelField("Total: " + ((BaseCombatCharacter)_class)._attributes.GetInitiative((int)field.GetValue(_class)));
								}
								break;
							default:
								field.SetValue(_class, EditorGUILayout.IntField((int)field.GetValue(_class), GUILayout.Width(50)));
								break;
						}
						break;
					case "Single":
					case "Float":
						switch (name)
						{
							case "PowerMultiplier":
								field.SetValue(_class, EditorGUILayout.FloatField((float)field.GetValue(_class), GUILayout.Width(40)));
								break;
							default:
								field.SetValue(_class, EditorGUILayout.FloatField((float)field.GetValue(_class), GUILayout.Width(70)));
								break;
						}
						break;
					case "String":
						switch (name)
						{
							case "FlavourText":
							case "Description":
								GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
								textAreaStyle.wordWrap = true;
								textAreaStyle.alignment = TextAnchor.UpperLeft;
								field.SetValue(_class, EditorGUILayout.TextArea((string)field.GetValue(_class), textAreaStyle, GUILayout.Width(310), GUILayout.Height(100)));
								break;
							case "IdString":
								field.SetValue(_class, EditorGUILayout.TextField((string)field.GetValue(_class), GUILayout.Width(150)));
								GUILayout.Space(20);
								CheckStringAvailability(_class.GetType(), (string)field.GetValue(_class), (BaseObject)_class);
								break;
							case "NameAbbreviation":
								string s = ((BaseObject)_class)._name;
								bool abbrev = s!=null?s.Length < 7:true;
								if (abbrev) EditorGUI.BeginDisabledGroup(true);
								field.SetValue(_class, EditorGUILayout.TextField(!abbrev?(string)field.GetValue(_class):s, GUILayout.Width(150)));
								if (GUILayout.Button("Auto", GUILayout.Width(50)))
									field.SetValue(_class, Combat.CombatCharacter.Abbreviate(s));
								if (abbrev) EditorGUI.EndDisabledGroup();
								break;
							default:
								field.SetValue(_class, EditorGUILayout.TextField((string)field.GetValue(_class), GUILayout.Width(150)));
								break;
						}
						if (_class.GetType() == typeof(Hyperlink) || _class.GetType() == typeof(DialogCharacter))
						{
							foreach (string n in new List<string> { "IdString", "Name" })
								if (n.Equals(name))
								{
									CheckStringAvailability(_class.GetType(), (string)field.GetValue(_class), (BaseObject)_class);
									break;
								}
						}
						break;
					case "Boolean":
						field.SetValue(_class, EditorGUILayout.Toggle((bool)field.GetValue(_class)));
						break;
					case "Bint":
						Bint _bint = (Bint)field.GetValue(_class);
						field.SetValue(_class, (Bint)DrawClassFields(_bint, 0, true));
						break;
					case "Bfloat":
						Bfloat _bfloat = (Bfloat)field.GetValue(_class);
						field.SetValue(_class, (Bfloat)DrawClassFields(_bfloat, 0, true));
						break;
					case "GameObject":
						field.SetValue(_class, (GameObject)EditorGUILayout.ObjectField((GameObject)field.GetValue(_class), type, false));
						break;
					case "DeckType":
						field.SetValue(_class, EditorGUILayout.EnumPopup((Deck.DeckType)field.GetValue(_class)));
						break;
					case "Vector2":
						field.SetValue(_class, (Vector2)EditorGUILayout.Vector2Field(name, (Vector2)field.GetValue(_class)));
						break;
					case "Sprite":
						field.SetValue(_class, EditorGUILayout.ObjectField((Sprite)field.GetValue(_class), typeof(Sprite), false));
						Sprite spr = (Sprite)field.GetValue(_class);
						if (spr != null)
						{
							Vector2 spriteSize = GetSpritePreviewSize(name);
							GUILayout.Box(CodeTools.textureFromSprite(spr), GUILayout.Width(spriteSize.x), GUILayout.Height(spriteSize.y));
						}
						break;
					case "Color":
						if (_class.GetType() == typeof(DialogCharacter))
						{
							DialogCharacter dc = (DialogCharacter)_class;
							if (name == "Color" && dc._useFactionColor)
							{
								EditorGUI.BeginDisabledGroup(true);
								EditorGUILayout.ColorField((Color)field.GetValue(_class), GUILayout.Width(125));
								EditorGUI.EndDisabledGroup();
								break;
							}
							else if(name == "DarknessColor" && dc._useDefaultSplashColors)
							{
								field.SetValue(_class, Color.white);
								EditorGUI.BeginDisabledGroup(true);
								EditorGUILayout.ColorField((Color)field.GetValue(_class), GUILayout.Width(125));
								EditorGUI.EndDisabledGroup();
								break;
							}
							else if(name == "WhitenessColor" && dc._useDefaultSplashColors)
							{
								field.SetValue(_class, Color.black);
								EditorGUI.BeginDisabledGroup(true);
								EditorGUILayout.ColorField((Color)field.GetValue(_class), GUILayout.Width(125));
								EditorGUI.EndDisabledGroup();
								break;
							}
						}
						field.SetValue(_class, EditorGUILayout.ColorField((Color)field.GetValue(_class), GUILayout.Width(125)));
						if (GUILayout.Button("New Color",GUILayout.Width(100)))
						{
							field.SetValue(_class, GetRandomColor());
						}
						break;
					case "TextAsset":
						field.SetValue(_class, (TextAsset)EditorGUILayout.ObjectField((TextAsset)field.GetValue(_class), type, false));
						break;
					case "TMP_FontAsset":
						field.SetValue(_class, (TMP_FontAsset)EditorGUILayout.ObjectField((TMP_FontAsset)field.GetValue(_class), type, false));
						break;
					case "Material":
						field.SetValue(_class, (Material)EditorGUILayout.ObjectField((Material)field.GetValue(_class), type, false));
						break;
					case "AudioClip":
						field.SetValue(_class, (AudioClip)EditorGUILayout.ObjectField((AudioClip)field.GetValue(_class), type, false));
						break;
					case "Polarity":
						field.SetValue(_class, EditorGUILayout.EnumPopup((CodeTools.Polarity)field.GetValue(_class)));
						break;
					#endregion
					#region BaseEffect Fields
					case "BaseEffect":
						field.SetValue(_class, (BaseEffect)EditorGUILayout.ObjectField((BaseEffect)field.GetValue(_class), type, false));
						break;
					case "SpellPowerType":
						field.SetValue(_class, EditorGUILayout.EnumPopup((BaseEffect.SpellPowerType)field.GetValue(_class)));
						break;
					case "MovementType":
						field.SetValue(_class, EditorGUILayout.EnumPopup((BaseEffect.MovementType)field.GetValue(_class)));
						break;
					case "CardTarget":
						field.SetValue(_class, EditorGUILayout.EnumPopup((BaseEffect.CardTargeting.CardTarget)field.GetValue(_class)));
						break;
					case "CardModification":
						var _cardModification = (BaseEffect.CardModification)field.GetValue(_class);
						GUILayout.EndHorizontal();
						GUILayout.BeginVertical();
						field.SetValue(_class, (BaseEffect.CardModification)DrawClassFields(_cardModification, 0, false));
						break;
					case "CardTargeting":
						var _cardTargeting = (BaseEffect.CardTargeting)field.GetValue(_class);
						GUILayout.EndHorizontal();
						GUILayout.BeginVertical();
						field.SetValue(_class, (BaseEffect.CardTargeting)DrawClassFields(_cardTargeting, 0, false));
						break;
					case "CardManagement":
						var _cardManagement = (BaseEffect.CardManagement)field.GetValue(_class);
						GUILayout.EndHorizontal();
						GUILayout.BeginVertical();
						field.SetValue(_class, (BaseEffect.CardManagement)DrawClassFields(_cardManagement, 1, false));
						break;
					case "CardRecovery":
						var _cardRecovery = (BaseEffect.CardRecovery)field.GetValue(_class);
						GUILayout.EndHorizontal();
						GUILayout.BeginVertical();
						field.SetValue(_class, (BaseEffect.CardRecovery)DrawClassFields(_cardRecovery, 0, false));
						break;
					case "AddCard":
						var _addCard = (BaseEffect.AddCard)field.GetValue(_class);
						GUILayout.EndHorizontal();
						GUILayout.BeginVertical();
						field.SetValue(_class, (BaseEffect.AddCard)DrawClassFields(_addCard, 0, false));
						break;
					case "ApplyAura":
						var _applyAura = (BaseEffect.ApplyAura)field.GetValue(_class);
						field.SetValue(_class, (BaseEffect.ApplyAura)DrawClassFields(_applyAura, 0, true));
						break;
					case "HealthData":
						var _healthData = (BaseEffect.HealthData)field.GetValue(_class);
						field.SetValue(_class, (BaseEffect.HealthData)DrawClassFields(_healthData, 0, true));
						break;
					case "DamageType":
						field.SetValue(_class, EditorGUILayout.EnumPopup((BaseEffect.DamageType)field.GetValue(_class)));
						break;
					#endregion
					#region BaseCard Fields
					case "BaseCard":
						field.SetValue(_class, (BaseCard)EditorGUILayout.ObjectField((BaseCard)field.GetValue(_class), type, false));
						break;
					case "Effects":
						BaseCard.Effects effects = (BaseCard.Effects)field.GetValue(_class);
						GUILayout.EndHorizontal();
						GUILayout.BeginVertical();
						field.SetValue(_class, (BaseCard.Effects)DrawClassFields(effects, 0, false));
						break;
					case "Features":
						BaseCard.Features features = (BaseCard.Features)field.GetValue(_class);
						GUILayout.EndHorizontal();
						GUILayout.BeginVertical();
						field.SetValue(_class, (BaseCard.Features)DrawClassFields(features, 0, false));
						break;
					case "CardType":
						field.SetValue(_class, EditorGUILayout.EnumPopup((BaseCard.CardType)field.GetValue(_class)));
						break;
					case "MultiTargetType":
						field.SetValue(_class, EditorGUILayout.EnumPopup((BaseCard.MultiTargetType)field.GetValue(_class)));
						break;
					case "TargetType":
						field.SetValue(_class, EditorGUILayout.EnumPopup((BaseCard.TargetType)field.GetValue(_class), GUILayout.Width(60)));
						break;
					case "Target":
						field.SetValue(_class, EditorGUILayout.EnumFlagsField((BaseCard.Target)field.GetValue(_class)));
						break;
					case "CardClass":
						field.SetValue(_class, EditorGUILayout.EnumFlagsField((BaseCard.CardClass)field.GetValue(_class)));
						break;
					case "RequirementTarget":
						var _reqTarget = (BaseCard.RequirementTarget)field.GetValue(_class);
						field.SetValue(_class, (BaseCard.RequirementTarget)DrawClassFields(_reqTarget, 0, true));
						break;
					case "EffectTarget":
						var _effectTarget = (BaseCard.EffectTarget)field.GetValue(_class);
						field.SetValue(_class, (BaseCard.EffectTarget)DrawClassFields(_effectTarget, 0, true));
						break;
					#endregion
					#region BaseAura Fields
					case "BaseAura":
						field.SetValue(_class, (BaseAura)EditorGUILayout.ObjectField((BaseAura)field.GetValue(_class), type, false));
						break;
					case "StackingType":
						field.SetValue(_class, EditorGUILayout.EnumPopup((BaseAura.StackingType)field.GetValue(_class)));
						break;
					case "TickType":
						field.SetValue(_class, EditorGUILayout.EnumPopup((BaseAura.TickType)field.GetValue(_class)));
						break;
					case "PowerEffect":
						BaseAura.PowerEffect _powerEffect = (BaseAura.PowerEffect)field.GetValue(_class);
						field.SetValue(_class, (BaseAura.PowerEffect)DrawClassFields(_powerEffect, 0, true));
						break;
					#endregion
					#region BaseTalent Fields
					case "BaseTalent":
						field.SetValue(_class, (BaseTalent)EditorGUILayout.ObjectField((BaseTalent)field.GetValue(_class), type, false));
						break;
					case "TalentTrigger":
						field.SetValue(_class, (BaseTalent.TalentTrigger)EditorGUILayout.EnumPopup((BaseTalent.TalentTrigger)field.GetValue(_class)));
						//field.SetValue(_class, (BaseTalent.TalentTrigger)EditorGUILayout.EnumFlagsField((BaseTalent.TalentTrigger)field.GetValue(_class)));
						break;
					#endregion
					#region BaseRequirement Fields
					case "BaseRequirement":
						field.SetValue(_class, (BaseRequirement)EditorGUILayout.ObjectField((BaseRequirement)field.GetValue(_class), type, false));
						break;
					#endregion
					#region BaseFX Fields
					case "BaseFX":
						field.SetValue(_class, (BaseFX)EditorGUILayout.ObjectField((BaseFX)field.GetValue(_class), type, false));
						break;
					case "BaseFXAnimation":
						BaseFX.BaseFXAnimation baseFXAnimation = (BaseFX.BaseFXAnimation)field.GetValue(_class);
						GUILayout.EndHorizontal();
						GUILayout.BeginVertical();
						field.SetValue(_class, (BaseFX.BaseFXAnimation)DrawClassFields(baseFXAnimation, 0, false));
						break;
					case "ParticleEffect":
						BaseFX.ParticleEffect particleEffect = (BaseFX.ParticleEffect)field.GetValue(_class);
						GUILayout.EndHorizontal();
						GUILayout.BeginVertical();
						field.SetValue(_class, (BaseFX.ParticleEffect)DrawClassFields(particleEffect, 0, false));
						break;
					case "BaseFXAudio":
						field.SetValue(_class, (BaseFXAudio)EditorGUILayout.ObjectField((BaseFXAudio)field.GetValue(_class), type, false));
						break;
					case "Animation":
						field.SetValue(_class, (Animation)EditorGUILayout.ObjectField((Animation)field.GetValue(_class), type, false));
						break;
					case "AnimationName":
						field.SetValue(_class, EditorGUILayout.EnumPopup((BaseFX.AnimationName)field.GetValue(_class)));
						break;
					case "Clip":
						BaseFXAudio.Clip _clip = (BaseFXAudio.Clip)field.GetValue(_class);
						field.SetValue(_class, (BaseFXAudio.Clip)DrawClassFields(_clip, 0, true));
						break;
					#endregion
					#region DialogCharacter Fields
					case "Faction":
						field.SetValue(_class, EditorGUILayout.EnumPopup((DialogCharacter.Faction)field.GetValue(_class)));
						break;
					case "DialogCharacter":
						field.SetValue(_class, (DialogCharacter)EditorGUILayout.ObjectField((DialogCharacter)field.GetValue(_class), type, false));
						break;
					#endregion
					#region BaseFight Fields
					case "Arena":
						//field.SetValue(_class, (BaseFight.Arena)BaseFightEditor.DrawArena(_class, field));
						break;
					case "BaseFight":
						field.SetValue(_class, (BaseFight)EditorGUILayout.ObjectField((BaseFight)field.GetValue(_class), type, false));
						break;
					#endregion
					#region BaseItems Fields
					case "ItemFlags":
						field.SetValue(_class, EditorGUILayout.EnumFlagsField((BaseItem.ItemFlags)field.GetValue(_class)));
						break;
					case "EquipmentType":
						field.SetValue(_class, EditorGUILayout.EnumPopup((BaseEquipment.EquipmentType)field.GetValue(_class)));
						break;
					case "BaseWeapon":
						field.SetValue(_class, (BaseWeapon)EditorGUILayout.ObjectField((BaseWeapon)field.GetValue(_class), type, false));
						break;
					case "BaseItem":
						field.SetValue(_class, (BaseItem)EditorGUILayout.ObjectField((BaseItem)field.GetValue(_class), type, false));
						break;
					case "BaseConsumable":
						field.SetValue(_class, (BaseConsumable)EditorGUILayout.ObjectField((BaseConsumable)field.GetValue(_class), type, false));
						break;
					case "BaseEquipment":
						field.SetValue(_class, (BaseEquipment)EditorGUILayout.ObjectField((BaseEquipment)field.GetValue(_class), type, false));
						break;
					#endregion
					#region BaseCombatCharacter Fields
					case "Alliance":
						field.SetValue(_class, EditorGUILayout.EnumPopup((Combat.CombatCharacter.Alliance)field.GetValue(_class)));
						break;
					case "Attributes":
						var _attributes = (BaseCombatCharacter.Attributes)field.GetValue(_class);
						field.SetValue(_class, (BaseCombatCharacter.Attributes)DrawClassFields(_attributes, 0, true));
						break;
					case "Facing":
						if(_class.GetType() == typeof(BaseFight))
						{
							Facing fac = (Facing)field.GetValue(_class);
							field.SetValue(_class, EditorGUILayout.EnumPopup((Combat.Facing)field.GetValue(_class)));
							if (fac != (Facing)field.GetValue(_class))
								((BaseFight)_class).ChangeSides();
						}
						else
							field.SetValue(_class, EditorGUILayout.EnumPopup((Combat.Facing)field.GetValue(_class)));
						break;
					case "BaseCombatCharacter":
						BaseCombatCharacter bcc = (BaseCombatCharacter)field.GetValue(_class);
						field.SetValue(_class, EditorGUILayout.ObjectField(bcc,type,false));
						if (bcc != null && bcc._visualObject._turnIcon!=null)
						{
							Vector2 spriteSize = GetSpritePreviewSize("TurnIcon");
							GUILayout.Box(CodeTools.textureFromSprite(bcc._visualObject._turnIcon), GUILayout.Width(spriteSize.x), GUILayout.Height(spriteSize.y));
						}
						break;
					case "CombatCharacter":
						field.SetValue(_class, (CombatCharacter)EditorGUILayout.ObjectField((CombatCharacter)field.GetValue(_class), type, false));
						break;
					case "TalentTree":
						field.SetValue(_class, (UI.TalentTree)EditorGUILayout.ObjectField((UI.TalentTree)field.GetValue(_class), type, false));
						break;
					#endregion
					case "List`1":
						if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
						{
							Type itemType = type.GetGenericArguments()[0];
							switch (itemType.Name)
							{
								case "BaseCard":
									DrawList((List<BaseCard>)field.GetValue(_class), typeof(BaseCard), _class, field);
									break;
								case "BaseEffect":
									DrawList((List<BaseEffect>)field.GetValue(_class), typeof(BaseEffect), _class, field);
									break;
								case "BaseRequirement":
									DrawList((List<BaseRequirement>)field.GetValue(_class), typeof(BaseRequirement), _class, field);
									break;
								case "BaseAura":
									DrawList((List<BaseAura>)field.GetValue(_class), typeof(BaseAura), _class, field);
									break;
								case "BaseFX":
									DrawList((List<BaseFX>)field.GetValue(_class), typeof(BaseFX), _class, field);
									break;
								case "Single":
								case "Float":
									DrawList((List<float>)field.GetValue(_class), typeof(float), _class, field);
									break;
								case "ApplyAura":
									DrawList((List<BaseEffect.ApplyAura>)field.GetValue(_class), typeof(BaseEffect.ApplyAura), _class, field);
									break;
								case "EffectTarget":
									DrawList((List<BaseCard.EffectTarget>)field.GetValue(_class), typeof(BaseCard.EffectTarget), _class, field);
									break;
								case "RequirementTarget":
									DrawList((List<BaseCard.RequirementTarget>)field.GetValue(_class), typeof(BaseCard.RequirementTarget), _class, field);
									break;
								case "ParticleEffect":
									DrawList((List<BaseFX.ParticleEffect>)field.GetValue(_class), typeof(BaseFX.ParticleEffect), _class, field);
									break;
								case "String":
									DrawList((List<string>)field.GetValue(_class), typeof(string), _class, field);
									break;
								case "Clip":
									DrawList((List<BaseFXAudio.Clip>)field.GetValue(_class), typeof(BaseFXAudio.Clip), _class, field);
									break;
								case "BaseTalent":
									DrawList((List<BaseTalent>)field.GetValue(_class), typeof(BaseTalent), _class, field);
									break;
								case "PowerEffect":
									DrawList((List<BaseAura.PowerEffect>)field.GetValue(_class), typeof(BaseAura.PowerEffect), _class, field);
									break;
								case "BaseObject":
									DrawList((List<BaseObject>)field.GetValue(_class), typeof(BaseObject), _class, field);
									break;
								case "TalentTrigger":
									DrawList((List<BaseTalent.TalentTrigger>)field.GetValue(_class), typeof(BaseTalent.TalentTrigger), _class, field);
									break;
								default:
									Debug.Log("Unimplemented -LIST- type: " + itemType.Name);
									break;
							}

						}
						break;

				}
				if (sameRow)
					GUILayout.EndVertical();
				else
					GUILayout.EndHorizontal();
			}
			if (sameRow)
				GUILayout.EndHorizontal();
			return _class;
		}
		static void CheckStringAvailability(Type stringType, string str, BaseObject current)
		{
			if (GUILayout.Button("Check Availability", GUILayout.Width(120)))
			{
				BaseCollection bc = (BaseCollection)Resources.Load("Database/Collection");
				BaseObject bo=null;
				if (stringType == typeof(Hyperlink))
					bo = (Hyperlink)bc.Get<Hyperlink>(str);
				else if (stringType == typeof(DialogCharacter))
					bo = (DialogCharacter)bc.Get < DialogCharacter>(str);
				bool available = (bo == null || bo == current);
				if (available)
					Debug.Log("'<color=green>" + str + "</color>'" + " is available.");
				else
					Debug.Log("'<color=red>" + str + "</color>'" + " is NOT available. Currently used by "+bo._name+"["+bo._id+"]");
			}
		}
		static string RenameVariable(string s)
		{
			if (s.StartsWith("_"))
			{
				s = s.Substring(1);
			}
			s = s.Substring(0, 1).ToUpper() + s.Substring(1);
			return s;
		}
		static int NameLength(string name)
		{
			return name.Length * 10;
		}
		static Vector2 GetSpritePreviewSize(string fieldName)
		{
			switch (fieldName)
			{
				default:
					return new Vector2(100, 100);
				case "TurnIcon":
					return new Vector2(62, 18);
			}
		}
		#region DrawLists
		static void DrawList(List<BaseObject> _list, Type type, System.Object _class, FieldInfo field)
		{

			if (GUILayout.Button("+", GUILayout.Width(30)))
			{
				_list.Add(null);//(BaseObject)ScriptableObject.CreateInstance(type));
			}

			GUILayout.BeginVertical();
			for (int i = 0; i < _list.Count; i++)
			{

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("-", GUILayout.Width(30)))
				{
					_list.RemoveAt(i);
					GUILayout.EndHorizontal();
					break;
				}
				_list[i] = (BaseObject)EditorGUILayout.ObjectField(_list[i], type, false);
				field.SetValue(_class, _list);
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

		}
		static void DrawList(List<BaseCard> _list, Type type, System.Object _class, FieldInfo field)
		{
			if (GUILayout.Button("+", GUILayout.Width(30)))
			{
				_list.Add(null);
			}

			GUILayout.BeginVertical();
			for (int i = 0; i < _list.Count; i++)
			{

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("-", GUILayout.Width(30)))
				{
					_list.RemoveAt(i);
					GUILayout.EndHorizontal();
					break;
				}
				_list[i] = (BaseCard)EditorGUILayout.ObjectField(_list[i], type, false);
				field.SetValue(_class, _list);
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

		}
		static void DrawList(List<BaseEffect> _list, Type type, System.Object _class, FieldInfo field)
		{

			if (GUILayout.Button("+", GUILayout.Width(30)))
			{
				_list.Add(null);//(BaseEffect)ScriptableObject.CreateInstance(type));
			}

			GUILayout.BeginVertical();
			for (int i = 0; i < _list.Count; i++)
			{

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("-", GUILayout.Width(30)))
				{
					_list.RemoveAt(i);
					GUILayout.EndHorizontal();
					break;
				}
				_list[i] = (BaseEffect)EditorGUILayout.ObjectField(_list[i], type, false);
				field.SetValue(_class, _list);
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

		}
		static void DrawList(List<BaseRequirement> _list, Type type, System.Object _class, FieldInfo field)
		{

			if (GUILayout.Button("+", GUILayout.Width(30)))
			{
				_list.Add(null);
			}

			GUILayout.BeginVertical();
			for (int i = 0; i < _list.Count; i++)
			{

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("-", GUILayout.Width(30)))
				{
					_list.RemoveAt(i);
					GUILayout.EndHorizontal();
					break;
				}
				_list[i] = (BaseRequirement)EditorGUILayout.ObjectField(_list[i], type, false);
				field.SetValue(_class, _list);
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

		}
		static void DrawList(List<BaseAura> _list, Type type, System.Object _class, FieldInfo field)
		{

			if (GUILayout.Button("+", GUILayout.Width(30)))
			{
				_list.Add(null);
			}

			GUILayout.BeginVertical();
			for (int i = 0; i < _list.Count; i++)
			{

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("-", GUILayout.Width(30)))
				{
					_list.RemoveAt(i);
					GUILayout.EndHorizontal();
					break;
				}
				_list[i] = (BaseAura)EditorGUILayout.ObjectField(_list[i], type, false);
				field.SetValue(_class, _list);
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

		}
		static void DrawList(List<BaseTalent.TalentTrigger> _list, Type type, System.Object _class, FieldInfo field)
		{

			if (GUILayout.Button("+", GUILayout.Width(30)))
			{
				_list.Add(new BaseTalent.TalentTrigger());
			}

			GUILayout.BeginVertical();
			for (int i = 0; i < _list.Count; i++)
			{

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("-", GUILayout.Width(30)))
				{
					_list.RemoveAt(i);
					GUILayout.EndHorizontal();
					break;
				}
				field.SetValue(_class, (BaseTalent.TalentTrigger)EditorGUILayout.EnumFlagsField((BaseTalent.TalentTrigger)field.GetValue(_class)));
				field.SetValue(_class, _list);
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

		}
		static void DrawList(List<BaseFX> _list, Type type, System.Object _class, FieldInfo field)
		{

			if (GUILayout.Button("+", GUILayout.Width(30)))
			{
				_list.Add(null);
			}

			GUILayout.BeginVertical();
			for (int i = 0; i < _list.Count; i++)
			{

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("-", GUILayout.Width(30)))
				{
					_list.RemoveAt(i);
					GUILayout.EndHorizontal();
					break;
				}
				_list[i] = (BaseFX)EditorGUILayout.ObjectField(_list[i], type, false);
				field.SetValue(_class, _list);
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

		}
		static void DrawList(List<float> _list, Type type, System.Object _class, FieldInfo field)
		{

			if (GUILayout.Button("+", GUILayout.Width(30)))
			{
				_list.Add(0);
			}

			GUILayout.BeginVertical();
			for (int i = 0; i < _list.Count; i++)
			{
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(NameLength(i.ToString())));
				if (GUILayout.Button("-", GUILayout.Width(30)))
				{
					_list.RemoveAt(i);
					GUILayout.EndHorizontal();
					break;
				}
				_list[i] = EditorGUILayout.FloatField(_list[i], GUILayout.Width(150));
				//_list[i] = f;
				field.SetValue(_class, _list);
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

		}
		static void DrawList(List<BaseEffect.ApplyAura> _list, Type type, System.Object _class, FieldInfo field)
		{
			if (GUILayout.Button("+", GUILayout.Width(30)))
			{
				_list.Add(new BaseEffect.ApplyAura());
			}

			GUILayout.BeginVertical();
			for (int i = 0; i < _list.Count; i++)
			{

				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(NameLength(i.ToString())));
				if (GUILayout.Button("-", GUILayout.Width(30)))
				{
					_list.RemoveAt(i);
					GUILayout.EndHorizontal();
					break;
				}
				_list[i] = (BaseEffect.ApplyAura)DrawClassFields(_list[i], 0, true);
				//_list[i] = EditorGUILayout.FloatField((BaseEffect.ApplyAura)field.GetValue(_class), GUILayout.Width(150));
				field.SetValue(_class, _list);
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

		}
		static void DrawList(List<BaseCard.EffectTarget> _list, Type type, System.Object _class, FieldInfo field)
		{
			GUILayout.Space(-50);
			if (GUILayout.Button("+", GUILayout.Width(30)))
			{
				_list.Add(new BaseCard.EffectTarget(true));
			}
			GUILayout.BeginVertical();
			for (int i = 0; i < _list.Count; i++)
			{
				GUILayout.BeginHorizontal();
				if (i > 0)
				{
					GUILayout.Space(-30);
					if (GUILayout.Button("^^", GUILayout.Width(30)))
					{
						CodeTools.ListMove(_list, i, i - 1);
						break;
					}
				}
				EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(NameLength(i.ToString())));
				if (GUILayout.Button("-", GUILayout.Width(30)))
				{
					_list.RemoveAt(i);
					GUILayout.EndHorizontal();
					break;
				}
				_list[i] = (BaseCard.EffectTarget)DrawClassFields(_list[i], 0, true);
				field.SetValue(_class, _list);
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

		}
		static void DrawList(List<BaseCard.RequirementTarget> _list, Type type, System.Object _class, FieldInfo field)
		{
			if (GUILayout.Button("+", GUILayout.Width(30)))
			{
				_list.Add(new BaseCard.RequirementTarget(true));
			}

			GUILayout.BeginVertical();
			for (int i = 0; i < _list.Count; i++)
			{

				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(NameLength(i.ToString())));
				if (GUILayout.Button("-", GUILayout.Width(30)))
				{
					_list.RemoveAt(i);
					GUILayout.EndHorizontal();
					break;
				}
				_list[i] = (BaseCard.RequirementTarget)DrawClassFields(_list[i], 0, true);
				field.SetValue(_class, _list);
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

		}
		static void DrawList(List<BaseFX.ParticleEffect> _list, Type type, System.Object _class, FieldInfo field)
		{
			if (GUILayout.Button("+", GUILayout.Width(30)))
			{
				_list.Add(new BaseFX.ParticleEffect(true));
			}

			GUILayout.BeginVertical();
			for (int i = 0; i < _list.Count; i++)
			{

				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(NameLength(i.ToString())));
				if (GUILayout.Button("-", GUILayout.Width(30)))
				{
					_list.RemoveAt(i);
					GUILayout.EndHorizontal();
					break;
				}
				_list[i] = (BaseFX.ParticleEffect)DrawClassFields(_list[i], 0, true);
				field.SetValue(_class, _list);
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

		}
		static void DrawList(List<string> _list, Type type, System.Object _class, FieldInfo field)
		{
			if (GUILayout.Button("+", GUILayout.Width(30)))
			{
				_list.Add("");
			}
			GUILayout.BeginVertical();
			for (int i = 0; i < _list.Count; i++)
			{

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("-", GUILayout.Width(30)))
				{
					_list.RemoveAt(i);
					GUILayout.EndHorizontal();
					break;
				}
				_list[i] = EditorGUILayout.TextField(_list[i], GUILayout.Width(150));
				if(_class.GetType() == typeof(Hyperlink) || _class.GetType() == typeof(DialogCharacter))
				{
					string name = RenameVariable(field.Name);
					List<string> CheckNames = new List<string> {"Names","OptionalStrings"};
					foreach (string n in CheckNames)
						if (n.Equals(name))
						{
							CheckStringAvailability(_class.GetType(), _list[i], (BaseObject)_class);
							break;
						}
				}
				GUILayout.EndHorizontal();
			}
			field.SetValue(_class, _list);
			GUILayout.EndVertical();

		}
		static void DrawList(List<BaseAura.PowerEffect> _list, Type type, System.Object _class, FieldInfo field)
		{

			if (GUILayout.Button("+", GUILayout.Width(30)))
			{
				_list.Add(new BaseAura.PowerEffect());
			}

			GUILayout.BeginVertical();
			for (int i = 0; i < _list.Count; i++)
			{

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("-", GUILayout.Width(30)))
				{
					_list.RemoveAt(i);
					GUILayout.EndHorizontal();
					break;
				}
				_list[i] = (BaseAura.PowerEffect)DrawClassFields(_list[i], 0, true);
				field.SetValue(_class, _list);
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

		}
		static void DrawList(List<BaseFXAudio.Clip> _list, Type type, System.Object _class, FieldInfo field)
		{
			if (GUILayout.Button("+", GUILayout.Width(30)))
			{
				_list.Add(new BaseFXAudio.Clip(true));
			}
			GUILayout.BeginVertical();
			if (GUILayout.Button("Add selected objects to _clips", GUILayout.Width(250)))
			{
				BaseFXAudio bfa = (BaseFXAudio)_class;
				bfa.AddClips(Selection.GetFiltered(typeof(AudioClip), SelectionMode.Assets));
			}
			for (int i = 0; i < _list.Count; i++)
			{

				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(NameLength(i.ToString())));
				if (GUILayout.Button("-", GUILayout.Width(30)))
				{
					_list.RemoveAt(i);
					GUILayout.EndHorizontal();
					break;
				}
				_list[i] = (BaseFXAudio.Clip)DrawClassFields(_list[i], 0, true);
				field.SetValue(_class, _list);
				if (GUILayout.Button(">") && _list[i].clip!=null)
				{
					//Debug.Log("_list[" + i + "] = "+_list[i].clip.name);
					AudioUtil.PlayClip(_list[i].clip);
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

		}
		static void DrawList(List<BaseTalent> _list, Type type, System.Object _class, FieldInfo field)
		{

			if (GUILayout.Button("+", GUILayout.Width(30)))
			{
				_list.Add(null);
			}

			GUILayout.BeginVertical();
			for (int i = 0; i < _list.Count; i++)
			{

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("-", GUILayout.Width(30)))
				{
					_list.RemoveAt(i);
					GUILayout.EndHorizontal();
					break;
				}
				_list[i] = (BaseTalent)EditorGUILayout.ObjectField(_list[i], type, false);
				field.SetValue(_class, _list);
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

		}
		#endregion
		public static void GuiLine(int i_height = 1)
		{
			Rect rect = EditorGUILayout.GetControlRect(false, i_height);
			rect.height = i_height;
			EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
		}
		private static TooltipAttribute GetTooltip(FieldInfo field, bool inherit)
		{
			TooltipAttribute[] attributes = field.GetCustomAttributes(typeof(TooltipAttribute), inherit) as TooltipAttribute[];

			return attributes.Length > 0 ? attributes[0] : null;
		}

		
		public static void SetTextureColor(this Texture2D tex2, Color32 color)
		{


			var fillColorArray = tex2.GetPixels32();

			for (var i = 0; i < fillColorArray.Length; ++i)
			{
				fillColorArray[i] = color;
			}

			tex2.SetPixels32(fillColorArray);

			tex2.Apply();
		}

		public static void AutoRename(BaseObject bo)
		{
			EditorUtility.SetDirty(bo);
			bo._actualDescription = bo._description;
			string idstr = bo._id > 99 ? bo._id.ToString() : bo._id > 9 ? "0" + bo._id : "00" + bo._id;
			AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(bo), idstr + " - " + bo._name);
			AssetDatabase.SaveAssets();
			Debug.Log("<color=green> Saved: </color>" + bo._name);
		}
		public static void SaveDatabase(UnityEngine.Object obj)
		{
			EditorUtility.SetDirty(obj);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
		/*
	static void ToggleLayOutSpace(LayOutSpace los,int index)
	{
		los.layOuts[index] = !los.layOuts[index];
	}*/
		public static void FindReferences(BaseObject bob)
		{
			string thisPath = AssetDatabase.GetAssetPath(bob);
			int referencesCount = 0;
			int cutlength = Application.dataPath.IndexOf("/Assets") + 1;
			//return;
			List<string> references = new List<string>();
			foreach (string dir in System.IO.Directory.GetDirectories(Application.dataPath + "/Resources/Database/"))
			{
				if (dir.EndsWith("Fight") || dir.EndsWith("CombatCharacter") || dir.EndsWith("SceneConstructor"))
					continue;
				foreach (string fil in System.IO.Directory.GetFiles(dir + "/"))
				{
					if (fil.EndsWith(".asset"))
					{
						string cut = fil.Substring(cutlength);
						if (cut != thisPath)
						{
							foreach (string dep in AssetDatabase.GetDependencies(cut))
							{
								if (dep.Equals(thisPath))
								{
									references.Add(cut);
									referencesCount++;
								}
							}
						}
					}
				}
			}
			Debug.Log("<color=orange> " + bob.GetFileName() + "</color> has <color=orange>" + referencesCount + "</color> references \n (When excluding Fight, CombatCharacter and SceneConstructor)");
			foreach (string rfr in references)
			{
				Debug.Log(StylizePath(rfr));
			}

			string StylizePath(string path)
			{
				path = path.Substring(0, path.IndexOf(".asset"));
				path = path.Substring(path.Substring(0, path.LastIndexOf("/")).LastIndexOf("/") + 1);
				path = "<color=cyan>"+path.Substring(0, path.IndexOf("/")) + "</color>/<color=orange>" + path.Substring(path.IndexOf("/") + 1) +"</color>";
				return path;
			}
		}
		public static class AudioUtil
		{
			public static void PlayClip(AudioClip clip, int startSample, bool loop)
			{
				Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
				Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
				MethodInfo method = audioUtilClass.GetMethod(
					"PlayPreviewClip",
					BindingFlags.Static | BindingFlags.Public,
					null,
					new System.Type[] {
						typeof(AudioClip),
						typeof(Int32),
						typeof(Boolean)
					},
					null
				);
				method.Invoke(
					null,
					new object[] {
						clip,
						startSample,
						loop
					}
				);

				SetClipSamplePosition(clip, startSample);
			}

			public static void PlayClip(AudioClip clip, int startSample)
			{
				PlayClip(clip, startSample, false);
			}

			public static void PlayClip(AudioClip clip)
			{
				PlayClip(clip, 0, false);
			}
			public static void SetClipSamplePosition(AudioClip clip, int iSamplePosition)
			{
				Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
				Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
				MethodInfo method = audioUtilClass.GetMethod(
					"SetPreviewClipSamplePosition",
					BindingFlags.Static | BindingFlags.Public
				);

				method.Invoke(
					null,
					new object[] {
						clip,
						iSamplePosition
					}
				);
			}
		}
	}
}