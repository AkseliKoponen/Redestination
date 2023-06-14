using System.Collections.Generic;
using RD;
using UnityEngine;

namespace RD.Combat
{
	public class CombatTerrain : MonoBehaviour
	{
		public List<TerrainEffect> _terrainEffects { get; private set; }
		public CombatSelectableObject _selObject { get; private set; }
		public CombatSelectableObject _movementGhostPrefab;
		public GameObject _ghostInstance { get; private set; }
		public bool IsEmpty()
		{
			return _selObject == null || _ghosting;
		}
		public void Empty()
		{
			_selObject = null;
		}
		public bool ContainsCharacter()
		{
			/*
		Debug.Log("----");
		Debug.Log(_selObject != null);
		Debug.Log(_selObject.GetComponentInParent<CombatCharacter>() != null);
		Debug.Log("----");*/
			if (IsEmpty())
				return false;
			if (_selObject.GetComponentInParent<CombatCharacter>() == null)
				return false;
			return true;
			//return _selObject != null && _selObject.GetComponentInParent<CombatCharacter>() != null;
		}
		public bool BlockLoS()
		{
			return ContainsCharacter() && GetCharacter()._blockLineOfSight;
		}
		public CombatCharacter GetCharacter()
		{
			return _selObject.GetComponentInParent<CombatCharacter>();
		}

		public bool SetObject(CombatSelectableObject cobject) {
			//Debug.Log("SetObject! "+cobject._cc.GetName());
			if (!_selObject)
			{
				_selObject = cobject;
				return true;
			}
			else
			{
				Debug.LogError("Attempting to Set Object to CombatTerrain while it contains another object");
				return false;
			}
		}
		public void TriggerEffects()
		{
			foreach(TerrainEffect te in _terrainEffects)
			{
				te.Trigger();
			}
		}
		bool _ghosting = false;
		public void CreateGhost(SpriteRenderer original)
		{
			if (_ghostInstance)
				DestroyImmediate(_ghostInstance);
			_ghosting = true;
			_ghostInstance = Instantiate(_movementGhostPrefab.gameObject, transform);
			_ghostInstance.GetComponent<CombatSelectableObject>()._ghost = true;
			SpriteRenderer sr = _ghostInstance.GetComponentInChildren<SpriteRenderer>();
			sr.transform.localScale = original.transform.localScale;
			CodeTools.CopyEverything(original.GetComponentInParent<BoxCollider>(), _ghostInstance.GetComponent<BoxCollider>(),typeof(BoxCollider));
			//CopyEverything(original.GetComponent<Animator>(), _ghostInstance.GetComponentInChildren<Animator>(), typeof(Animator));
			sr.sprite = original.sprite;
			sr.flipX = original.flipX;
			sr.material.SetFloat("randomSeed", Random.Range(0, 1));
			sr.transform.localPosition = original.transform.localPosition;
			if (original.GetComponentInParent<CombatCharacter>())
			{
				CombatCharacter cc = original.GetComponentInParent<CombatCharacter>();
				int targetPos = CombatManager.ArenaManager.GetPosition(cc);
				int thisPos = CombatManager.ArenaManager.GetPosition(this);
				Facing facing = CombatManager.ArenaManager.GetOrientation(targetPos, thisPos);
				if (facing != cc._facing)
				{
					sr.flipX = !sr.flipX;
					Vector3 tempos = sr.transform.localPosition;
					tempos.x *= -1;
					sr.transform.localPosition = tempos;
				}
				Vector4 direction = sr.material.GetVector("Direction");
				direction.x *= facing == Facing.Left ? -1 : 1;
				sr.material.SetVector("Direction", direction);
			}
			/*
		Material tempGhostMat = sr.material;
		CopyEverything(original, sr, typeof(SpriteRenderer));
		sr.material = tempGhostMat;*/
		}

		public void ClearGhost()
		{
			if(_ghostInstance)
				_ghostInstance.SetActive(false);
			_ghosting = false;
		}
	}

	public class TerrainEffect
	{
		public CombatTerrain _combatTerrain;

		public TerrainEffect(CombatTerrain combatTerrain)
		{
			_combatTerrain = combatTerrain;
		}
		public void Trigger()
		{

		}

	}
}