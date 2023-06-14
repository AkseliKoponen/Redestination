using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RD.DB
{
	[CreateAssetMenu(fileName = "New Collection", menuName = "Redestination/BaseCollection", order = 1)]
	public class BaseCollection : ScriptableObject
	{
		public List<BaseCard> _cards;
		public List<BaseEffect> _effects;
		public List<BaseAura> _auras;
		public List<BaseRequirement> _requirements;
		public List<BaseFX> _fXs;
		public List<BaseFXAudio> _fXAudios;
		public List<BaseFight> _fights;
		public List<BaseItem> _items;
		public List<BaseConsumable> _consumables;
		public List<BaseEquipment> _equipments;
		public List<BaseWeapon> _weapons;
		public List<BaseCombatCharacter> _combatCharacters;
		public List<Hyperlink> _hyperlinks;
		public List<DialogCharacter> _dialogCharacters;
		public List<SceneConstructor> _sceneConstructors;
		public List<BaseTalent> _talents;
		bool dbug = false;
		public BaseCollection()
		{
			_cards = new List<BaseCard>();
			_effects = new List<BaseEffect>();
			_auras = new List<BaseAura>();
			_requirements = new List<BaseRequirement>();
			_fXs = new List<BaseFX>();
			_hyperlinks = new List<Hyperlink>();
			//NullID();
		}
		public void initList()
		{
			if(_cards==null) _cards = new List<BaseCard>();
			if(_effects==null) _effects = new List<BaseEffect>();
			if(_auras==null) _auras = new List<BaseAura>();
			if(_requirements==null) _requirements = new List<BaseRequirement>();
			if(_fXs==null) _fXs = new List<BaseFX>();
			if (_hyperlinks == null) _hyperlinks = new List<Hyperlink>();
			if (_combatCharacters == null) _combatCharacters = new List<BaseCombatCharacter>();
		}
		public int _idCard;
		public int _idEffect;
		public int _idAura;
		public int _idRequirement;
		public int _idFX;
		public int _idFXAudio;
		public int _idHyperlink;
		public int _idDialogCharacter;
		public int _idFight;
		public int _idItem;
		public int _idConsumable;
		public int _idEquipment;
		public int _idWeapon;
		public int _idSceneConstructor;
		public int _idCombatCharacter;
		public int _idTalent;
		#region Add To Collection
		#region Unused
/*
		public void AddToCardDatabase(BaseCard obj)
		{
			_cards.Add(obj);
			obj.SetID(_idCard);
			_idCard++;
		}
		public void AddToEffectDatabase(BaseEffect obj)
		{
			_effects.Add(obj);
			obj.SetID(_idEffect);
			_idEffect++;
		}
		public void AddToAuraDatabase(BaseAura obj)
		{
			_auras.Add(obj);
			obj.SetID(_idAura);
			_idAura++;
		}
		public void AddToRequirementDatabase(BaseRequirement obj)
		{
			_requirements.Add(obj);
			obj.SetID(_idRequirement);
			_idRequirement++;
		}
		public void AddToFXDatabase(BaseFX obj)
		{
			_fXs.Add(obj);
			obj.SetID(_idFX);
			_idFX++;
		}
		public void AddToFXAudioDatabase(BaseFXAudio obj)
		{
			_fXAudios.Add(obj);
			obj.SetID(_idFXAudio);
			_idFXAudio++;
		}
		public void AddToHyperlinkDatabase(Hyperlink obj)
		{
			_hyperlinks.Add(obj);
			obj.SetID(_idHyperlink);
			_idHyperlink++;
		}
		public void AddToDialogCharacterDatabase(DialogCharacter obj)
		{
			_dialogCharacters.Add(obj);
			obj.SetID(_idDialogCharacter);
			_idDialogCharacter++;
		}
		public void AddToFightDatabase(BaseFight obj)
		{
			_fights.Add(obj);
			obj.SetID(_idFight);
			_idFight++;
		}
		public void AddToItemDatabase(BaseItem obj)
		{
			_items.Add(obj);
			obj.SetID(_idItem);
			_idItem++;
		}
		public void AddToConsumableDatabase(BaseConsumable obj)
		{
			_consumables.Add(obj);
			obj.SetID(_idConsumable);
			_idConsumable++;
		}
		public void AddToEquipmentDatabase(BaseEquipment obj)
		{
			_equipments.Add(obj);
			obj.SetID(_idWeapon);
			_idFight++;
		}
		public void AddToWeaponDatabase(BaseWeapon obj)
		{
			_weapons.Add(obj);
			obj.SetID(_idWeapon);
			_idWeapon++;
		}
		public void AddToSceneConstructorDatabase(SceneConstructor obj)
		{
			if (_sceneConstructors == null)
				_sceneConstructors = new List<SceneConstructor>();
			_sceneConstructors.Add(obj);
			obj.SetID(_idSceneConstructor);
			_idSceneConstructor++;
		}
*/
		#endregion
		public void Add<T>(System.Object obj)
		{
			//----Add to List---
			string listName = GetListNameFromType(typeof(T));
			FieldInfo fi = GetType().GetField(listName);
			if (fi == null)
				return;
			List<T> list = (List<T>)fi.GetValue(this);
			T value = (T)obj;
			list.Add(value);
			fi.SetValue(this, list);
			//--Update ID----
			string idName = GetIDNameFromType(typeof(T));
			FieldInfo fid = GetType().GetField(idName);
			if (fid == null || fid.FieldType != typeof(int))
				return;
			int id = (int)fid.GetValue(this) + 1;
			fid.SetValue(this, id);
			BaseObject bob = (BaseObject)obj;
			bob.SetID(id);
		}
		#endregion
		#region GetFromCollection
		public BaseObject Get<T>(int id)
		{
			string listName = GetListNameFromType(typeof(T));
			//Debug.Log(listName);
			FieldInfo fi = GetType().GetField(listName);
			if(fi!=null)
			{
				List<T> list = (List<T>)fi.GetValue(this);
				foreach(object osd in list)
				{
					BaseObject bo = (BaseObject)osd;
					if (bo._id == id)
						return bo;
				}
			}
			Debug.LogError(id + " not found in "+listName);
			return null;

		}
		public BaseObject Get<T>(string name)
		{
			string listName = GetListNameFromType(typeof(T));
			FieldInfo fi = GetType().GetField(listName);
			if (fi != null)
			{
				List<T> list = (List<T>)fi.GetValue(this);
				foreach (object osd in list)
				{
					if(osd.GetType() == typeof(DialogCharacter))
					{

						DialogCharacter dc = (DialogCharacter)osd;
						if (dc.CheckName(name))
							return dc;
					}
					else if (osd.GetType() == typeof(Hyperlink))
					{
						Hyperlink hl = (Hyperlink)osd;
						if (hl.CheckName(name))
							return hl;
					}
					else
					{
						BaseObject bo = (BaseObject)osd;
						if (bo._name.ToLower() == name.ToLower())
							return bo;
					}
				}
			}
			//Debug.LogError("'"+ name + "'" + " not found in " + listName);
			return null;
		}
		#endregion
		string GetListNameFromIDName(string idName)
		{
			string s = "_" + idName.Substring(3, 1).ToLower() + idName.Substring(4)+"s";
			return s;
		}
		public string GetListNameFromType(Type t)
		{
			string s = t.Name;
			switch (t.Name)
			{
				case "Hyperlink":
				case "DialogCharacter":
				case "SceneConstructor":
					s = "_" + s.Substring(0, 1).ToLower() + s.Substring(1) + "s";
					return s;

				default:
					//Remove "Base" from the front of the name
					s = "_" + s.Substring(4, 1).ToLower() + s.Substring(5) + "s";
					return s;

			}
		}
		public string GetIDNameFromType(Type t)
		{
			string s = t.Name;
			switch (t.Name)
			{
				case "Hyperlink":
				case "DialogCharacter":
				case "SceneConstructor":
					break;

				default:
					//Remove "Base" from the front of the name
					s = s.Substring(4);
					break;

			}
			s = "_id" + s;
			return s;
		}
		public void UpdateDescriptionsCards()
		{
			foreach (BaseCard bo in _cards)
			{
				bo.UpdateDescription(this);
			}
		}
		public void UpdateDescriptionsHyperlinks()
		{
			foreach (Hyperlink bo in _hyperlinks)
			{
				bo.UpdateDescription(this);
			}
		}

		public void Search(string searchstr)
		{
			int totalCount = 0;
			searchstr = searchstr.ToLower();
			List<BaseObject> list = new List<BaseObject>();
			list.Clear(); list.AddRange(_cards); SearchNameAndDescription(list, typeof(BaseCard));
			list.Clear(); list.AddRange(_effects); SearchNameAndDescription(list, typeof(BaseEffect));
			list.Clear(); list.AddRange(_auras); SearchNameAndDescription(list, typeof(BaseAura));
			list.Clear(); list.AddRange(_requirements); SearchNameAndDescription(list, typeof(BaseRequirement));
			list.Clear(); list.AddRange(_combatCharacters); SearchNameAndDescription(list, typeof(BaseCombatCharacter));
			//list.Clear(); list.AddRange(_talents); SearchNameAndDescription(list, typeof(BaseTalent));
			list.Clear(); list.AddRange(_fights); SearchNameAndDescription(list, typeof(BaseFight));
			list.Clear(); list.AddRange(_items); SearchNameAndDescription(list, typeof(BaseItem));
			list.Clear(); list.AddRange(_consumables); SearchNameAndDescription(list, typeof(BaseConsumable));
			list.Clear(); list.AddRange(_equipments); SearchNameAndDescription(list, typeof(BaseEquipment));
			list.Clear(); list.AddRange(_weapons); SearchNameAndDescription(list, typeof(BaseWeapon));
			list.Clear(); list.AddRange(_hyperlinks); SearchNameAndDescription(list, typeof(Hyperlink));
			list.Clear(); list.AddRange(_sceneConstructors); SearchNameAndDescription(list, typeof(SceneConstructor));
			list.Clear(); list.AddRange(_dialogCharacters); SearchNameAndDescription(list, typeof(DialogCharacter));
			list.Clear(); list.AddRange(_fXs); SearchNameAndDescription(list, typeof(BaseFX));
			list.Clear(); list.AddRange(_fXAudios); SearchNameAndDescription(list, typeof(BaseFXAudio));
			Debug.Log("<color=orange>----- " + totalCount + " total matches -----</color>");

			void SearchNameAndDescription(List<BaseObject> bobs, Type t)
			{
				bool matchfound = false;
				string typename = t.Name;
				foreach(BaseObject bob in bobs)
				{
					if (bob._name.ToLower().Contains(searchstr))
					{
						if (!matchfound)
							Debug.Log("-----" + typename + "-----");
						totalCount++;
						Debug.Log("<color=lime>NAME</color> of " + typename + " <color=orange>" + bob._name + " </color><color=cyan>#" + bob._id + "</color> contains <color=yellow>'" + searchstr + "</color>");
						totalCount++;
						matchfound = true;
					}
					if (bob._description.ToLower().Contains(searchstr))
					{
						totalCount++;
						Debug.Log("<color=lime>DESCRIPTION</color> of " + typename + " <color=orange>" + bob._name + " </color><color=cyan>#" + bob._id + "</color> contains <color=yellow>'" + searchstr + "'</color>");
						totalCount++;
						matchfound = true;
					}
				}
				if (matchfound)
				{
					Debug.Log("------ ------");
				}
				else
					Debug.Log("-----No matches in " + typename + "------");
			}
		}
	}
}
