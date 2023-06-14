using System.Collections.Generic;
using UnityEngine;

namespace RD.DB
{
	public class BaseWeapon : BaseEquipment
	{
		public int _weaponDamage;
		public int _cooldown = 0;
		[Tooltip("Trigger when attacking.")]
		public BaseCard.Effects _onHitEffect = new BaseCard.Effects();
		public BaseFXAudio _fxDraw;
		public BaseFXAudio _fxStrike;
		public BaseFXAudio _fxSheathe;
		public BaseWeapon()
		{
			_fxDraw = null;
			_fxStrike = null;
			_onHitEffect = new BaseCard.Effects(true);
			_layOutSpace = new LayOutSpace(new List<int> { 1, 6, 3, 3 }, new List<string> { "Weapon", "Equipment", "Item", "General" });
		}
	}
}
