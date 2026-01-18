using System;
using System.Collections;
using System.Collections.Generic;
using Server.Items;
using Server.Network;
using Server.ContextMenus;
using Server.Mobiles;
using Server.Misc;
using Server.Regions;
using Server.Multis;
using Server.Engines.Plants;
using Server.Engines.Apiculture;
using Server.Engines.Mahjong;
using System.IO;
using System.Text;

namespace Server
{
	public class ItemInformation
	{
		public static ItemSalesInfo GetData( int val )
		{
			ItemSalesInfo[] list = ItemSalesInfo.m_SellingInfo;
			return list[val];
		}

		public static int GetInfo( Type itemtype )
		{
			ItemSalesInfo[] list = ItemSalesInfo.m_SellingInfo;
			int entries = list.Length;
			int val = 0;
			bool record = false;

			while ( entries > 0 )
			{
				if ( list[val].ItemsType == itemtype )
				{
					record = true;
					entries = 0;
				}
				else
					val++;

				entries--;
			}

			if ( record )
				return val;
			else
				return -1;
		}

		public static int GetValue( GemType gemType )
		{
			switch ( gemType )
			{
				case GemType.Amber: return 50;
				case GemType.Citrine: return 60;
				case GemType.Ruby: return 70;
				case GemType.Tourmaline: return 80;
				case GemType.Amethyst: return 90;
				case GemType.Emerald: return 100;
				case GemType.Sapphire: return 110;
				case GemType.StarSapphire: return 120;
				case GemType.Diamond: return 150;
				case GemType.Pearl: return 500;

				case GemType.None:
				default:
					return 0;
			}
		}

		public static int AddUpBenefits( Item item, int price, bool checkCrafted, bool resale )
		{
			if ( item.CoinPrice > 0 )
			{
				if ( item.NotIdentified )
					return ( 2 * Utility.RandomMinMax( 5, 25 ) );

				return ( item.CoinPrice * 2 );
			}
			else if ( !MySettings.S_QualityPrices && !resale )
				return price;
			else if (	( item is BaseArmor && ((BaseArmor)item).Quality == ArmorQuality.Low ) || 
					( item is BaseClothing && ((BaseClothing)item).Quality == ClothingQuality.Low ) || 
					( item is BaseQuiver && ((BaseQuiver)item).Quality == ClothingQuality.Low ) || 
					( item is BaseWeapon && ((BaseWeapon)item).Quality == WeaponQuality.Low ) || 
					( item is BaseInstrument && ((BaseInstrument)item).Quality == InstrumentQuality.Low ) 
				)
				price = (int)( price * 0.60 );
			else if (	( item is BaseArmor && ((BaseArmor)item).Quality == ArmorQuality.Exceptional ) || 
						( item is BaseClothing && ((BaseClothing)item).Quality == ClothingQuality.Exceptional ) || 
						( item is BaseQuiver && ((BaseQuiver)item).Quality == ClothingQuality.Exceptional ) || 
						( item is BaseWeapon && ((BaseWeapon)item).Quality == WeaponQuality.Exceptional ) || 
						( item is BaseInstrument && ((BaseInstrument)item).Quality == InstrumentQuality.Exceptional ) 
				)
				price = (int)( price * 1.25 );

			if ( CraftResources.GetGold( item.Resource ) > 0 )
				price = (int)( CraftResources.GetGold( item.Resource ) * price );

			if ( item.Enchanted != MagicSpell.None && item.EnchantUsesMax == 0 && item.EnchantUses > 5 )
			{
				int level = SpellItems.GetLevel( (int)(item.Enchanted) );
				price += level * 50;
			}

			if ( price < 1 )
				price = 1;

			if ( !item.Built && checkCrafted )
				return 0;
			else if ( item is BaseInstrument && ((BaseInstrument)item).UsesRemaining < 50 )
				return 0;
			else if ( item is BaseTool && ((BaseTool)item).UsesRemaining < 20 )
				return 0;
			else if ( item is BaseHarvestTool && ((BaseHarvestTool)item).UsesRemaining < 20 )
				return 0;
			else if ( item.EnchantUsesMax > 0 && item.EnchantUses < 6 && item.Enchanted == MagicSpell.None )
				return 0;

			if ( item is BaseTrinket && ((BaseTrinket)item).GemType != GemType.None )
			{
				price += GetValue(((BaseTrinket)item).GemType);
			}

			if ( item is BaseArmor )
			{
				price +=		((BaseArmor)item).ArmorAttributes.DurabilityBonus * 2;
				price +=		((BaseArmor)item).ArmorAttributes.LowerStatReq * 2;
				price +=		((BaseArmor)item).ArmorAttributes.SelfRepair * 100;
				price +=		((BaseArmor)item).ArmorAttributes.MageArmor * 200;
				price +=		((BaseArmor)item).PhysicalBonus * 2;
				price +=		((BaseArmor)item).FireBonus * 2;
				price +=		((BaseArmor)item).ColdBonus * 2;
				price +=		((BaseArmor)item).PoisonBonus * 2;
				price +=		((BaseArmor)item).EnergyBonus * 2;
				price +=		((BaseArmor)item).DexBonus * 5;
				price +=		((BaseArmor)item).IntBonus * 5;
				price +=		((BaseArmor)item).StrBonus * 5;

				price +=		(int)(((BaseArmor)item).SkillBonuses.Skill_1_Value * 2);
				price +=		(int)(((BaseArmor)item).SkillBonuses.Skill_2_Value * 2);
				price +=		(int)(((BaseArmor)item).SkillBonuses.Skill_3_Value * 2);
				price +=		(int)(((BaseArmor)item).SkillBonuses.Skill_4_Value * 2);
				price +=		(int)(((BaseArmor)item).SkillBonuses.Skill_5_Value * 2);

				price +=		((BaseArmor)item).Attributes.SpellChanneling * 200;
				price +=		((BaseArmor)item).Attributes.DefendChance * 10;
				price +=		((BaseArmor)item).Attributes.ReflectPhysical * 2;
				price +=		((BaseArmor)item).Attributes.AttackChance * 10;
				price +=		((BaseArmor)item).Attributes.RegenHits * 5;
				price +=		((BaseArmor)item).Attributes.RegenStam * 5;
				price +=		((BaseArmor)item).Attributes.RegenMana * 5;
				price +=		((BaseArmor)item).Attributes.NightSight * 6;
				price +=		((BaseArmor)item).Attributes.BonusHits * 5;
				price +=		((BaseArmor)item).Attributes.BonusStam * 5;
				price +=		((BaseArmor)item).Attributes.BonusMana * 5;

				int lmc = ((BaseArmor)item).Attributes.LowerManaCost;
					if ( ((BaseArmor)item).Attributes.LowerManaCost > MyServerSettings.LowerMana() )
						lmc = MyServerSettings.LowerMana();
							price += lmc * 5;

				int lrc = ((BaseArmor)item).Attributes.LowerRegCost;
					if ( ((BaseArmor)item).Attributes.LowerRegCost > MyServerSettings.LowerReg() )
						lrc = MyServerSettings.LowerReg();
							price += lrc * 5;

				price +=		((BaseArmor)item).Attributes.Luck * 2;
				price +=		((BaseArmor)item).Attributes.WeaponDamage * 5;
				price +=		((BaseArmor)item).Attributes.WeaponSpeed * 6;
				price +=		((BaseArmor)item).Attributes.BonusStr * 10;
				price +=		((BaseArmor)item).Attributes.BonusDex * 10;
				price +=		((BaseArmor)item).Attributes.BonusInt * 10;
				price +=		((BaseArmor)item).Attributes.EnhancePotions * 2;
				price +=		((BaseArmor)item).Attributes.CastSpeed * 4;
				price +=		((BaseArmor)item).Attributes.CastRecovery * 4;
				price +=		((BaseArmor)item).Attributes.SpellDamage * 4;

				SkillName skill;
				double bonus;

				((BaseArmor)item).SkillBonuses.GetValues( 0, out skill, out bonus ); price += (int)(bonus*10);
				((BaseArmor)item).SkillBonuses.GetValues( 1, out skill, out bonus ); price += (int)(bonus*10);
				((BaseArmor)item).SkillBonuses.GetValues( 2, out skill, out bonus ); price += (int)(bonus*10);
				((BaseArmor)item).SkillBonuses.GetValues( 3, out skill, out bonus ); price += (int)(bonus*10);
				((BaseArmor)item).SkillBonuses.GetValues( 4, out skill, out bonus ); price += (int)(bonus*10);
			}
			else if ( item is BaseWeapon )
			{
				price +=		((BaseWeapon)item).WeaponAttributes.SelfRepair * 100;
				price +=		((BaseWeapon)item).WeaponAttributes.LowerStatReq * 2;
				price +=		((BaseWeapon)item).WeaponAttributes.HitPhysicalArea * 3;
				price +=		((BaseWeapon)item).WeaponAttributes.HitFireArea * 3;
				price +=		((BaseWeapon)item).WeaponAttributes.HitColdArea * 3;
				price +=		((BaseWeapon)item).WeaponAttributes.HitPoisonArea * 3;
				price +=		((BaseWeapon)item).WeaponAttributes.HitEnergyArea * 3;
				price +=		((BaseWeapon)item).WeaponAttributes.HitMagicArrow * 3;
				price +=		((BaseWeapon)item).WeaponAttributes.HitHarm * 3;
				price +=		((BaseWeapon)item).WeaponAttributes.HitFireball * 3;
				price +=		((BaseWeapon)item).WeaponAttributes.HitLightning * 3;
				price +=		((BaseWeapon)item).WeaponAttributes.UseBestSkill * 10;
				price +=		((BaseWeapon)item).WeaponAttributes.MageWeapon * 5;
				price +=		((BaseWeapon)item).WeaponAttributes.HitDispel * 3;
				price +=		((BaseWeapon)item).WeaponAttributes.HitLeechHits * 3;
				price +=		((BaseWeapon)item).WeaponAttributes.HitLowerAttack * 3;
				price +=		((BaseWeapon)item).WeaponAttributes.HitLowerDefend * 3;
				price +=		((BaseWeapon)item).WeaponAttributes.HitLeechMana * 3;
				price +=		((BaseWeapon)item).WeaponAttributes.HitLeechStam * 3;
				price +=		((BaseWeapon)item).WeaponAttributes.ResistPhysicalBonus * 2;
				price +=		((BaseWeapon)item).WeaponAttributes.ResistFireBonus * 2;
				price +=		((BaseWeapon)item).WeaponAttributes.ResistColdBonus * 2;
				price +=		((BaseWeapon)item).WeaponAttributes.ResistPoisonBonus * 2;
				price +=		((BaseWeapon)item).WeaponAttributes.ResistEnergyBonus * 2;

				if ( ((BaseWeapon)item).Slayer != SlayerName.None )
					price +=		200;

				if ( ((BaseWeapon)item).Slayer2 != SlayerName.None )
					price +=		200;

				price +=		(int)(((BaseWeapon)item).SkillBonuses.Skill_1_Value * 2);
				price +=		(int)(((BaseWeapon)item).SkillBonuses.Skill_2_Value * 2);
				price +=		(int)(((BaseWeapon)item).SkillBonuses.Skill_3_Value * 2);
				price +=		(int)(((BaseWeapon)item).SkillBonuses.Skill_4_Value * 2);
				price +=		(int)(((BaseWeapon)item).SkillBonuses.Skill_5_Value * 2);

				if ( item is IPugilistGlove
				 || item is GiftThrowingGloves
				 || item is LevelThrowingGloves
				 || item is ThrowingGloves
				 || item is WizardWand
				 || item is WizardStaff
				 || item is WizardStick ){} else
				price +=		((BaseWeapon)item).Attributes.SpellChanneling * 200;

				price +=		((BaseWeapon)item).Attributes.DefendChance * 10;
				price +=		((BaseWeapon)item).Attributes.ReflectPhysical * 2;
				price +=		((BaseWeapon)item).Attributes.AttackChance * 10;
				price +=		((BaseWeapon)item).Attributes.RegenHits * 5;
				price +=		((BaseWeapon)item).Attributes.RegenStam * 5;
				price +=		((BaseWeapon)item).Attributes.RegenMana * 5;
				price +=		((BaseWeapon)item).Attributes.NightSight * 6;
				price +=		((BaseWeapon)item).Attributes.BonusHits * 5;
				price +=		((BaseWeapon)item).Attributes.BonusStam * 5;
				price +=		((BaseWeapon)item).Attributes.BonusMana * 5;

				int lmc = ((BaseWeapon)item).Attributes.LowerManaCost;
					if ( ((BaseWeapon)item).Attributes.LowerManaCost > MyServerSettings.LowerMana() )
						lmc = MyServerSettings.LowerMana();
							price += lmc * 5;

				int lrc = ((BaseWeapon)item).Attributes.LowerRegCost;
					if ( ((BaseWeapon)item).Attributes.LowerRegCost > MyServerSettings.LowerReg() )
						lrc = MyServerSettings.LowerReg();
							price += lrc * 5;

				price +=		((BaseWeapon)item).Attributes.Luck * 2;
				price +=		((BaseWeapon)item).Attributes.WeaponDamage * 5;
				price +=		((BaseWeapon)item).Attributes.WeaponSpeed * 6;
				price +=		((BaseWeapon)item).Attributes.BonusStr * 10;
				price +=		((BaseWeapon)item).Attributes.BonusDex * 10;
				price +=		((BaseWeapon)item).Attributes.BonusInt * 10;
				price +=		((BaseWeapon)item).Attributes.EnhancePotions * 2;
				price +=		((BaseWeapon)item).Attributes.CastSpeed * 4;
				price +=		((BaseWeapon)item).Attributes.CastRecovery * 4;
				price +=		((BaseWeapon)item).Attributes.SpellDamage * 4;

				SkillName skill;
				double bonus;

				((BaseWeapon)item).SkillBonuses.GetValues( 0, out skill, out bonus ); price += (int)(bonus*10);
				((BaseWeapon)item).SkillBonuses.GetValues( 1, out skill, out bonus ); price += (int)(bonus*10);
				((BaseWeapon)item).SkillBonuses.GetValues( 2, out skill, out bonus ); price += (int)(bonus*10);
				((BaseWeapon)item).SkillBonuses.GetValues( 3, out skill, out bonus ); price += (int)(bonus*10);
				((BaseWeapon)item).SkillBonuses.GetValues( 4, out skill, out bonus ); price += (int)(bonus*10);
			}
			else if ( item is Spellbook )
			{
				price +=		((Spellbook)item).Resistances.Physical * 2;
				price +=		((Spellbook)item).Resistances.Fire * 2;
				price +=		((Spellbook)item).Resistances.Cold * 2;
				price +=		((Spellbook)item).Resistances.Poison * 2;
				price +=		((Spellbook)item).Resistances.Energy * 2;

				if ( ((Spellbook)item).Slayer != SlayerName.None )
					price +=		200;

				if ( ((Spellbook)item).Slayer2 != SlayerName.None )
					price +=		200;

				price +=		(int)(((Spellbook)item).SkillBonuses.Skill_1_Value * 2);
				price +=		(int)(((Spellbook)item).SkillBonuses.Skill_2_Value * 2);
				price +=		(int)(((Spellbook)item).SkillBonuses.Skill_3_Value * 2);
				price +=		(int)(((Spellbook)item).SkillBonuses.Skill_4_Value * 2);
				price +=		(int)(((Spellbook)item).SkillBonuses.Skill_5_Value * 2);

				price +=		((Spellbook)item).Attributes.SpellChanneling * 200;
				price +=		((Spellbook)item).Attributes.DefendChance * 10;
				price +=		((Spellbook)item).Attributes.ReflectPhysical * 2;
				price +=		((Spellbook)item).Attributes.AttackChance * 10;
				price +=		((Spellbook)item).Attributes.RegenHits * 5;
				price +=		((Spellbook)item).Attributes.RegenStam * 5;
				price +=		((Spellbook)item).Attributes.RegenMana * 5;
				price +=		((Spellbook)item).Attributes.NightSight * 6;
				price +=		((Spellbook)item).Attributes.BonusHits * 5;
				price +=		((Spellbook)item).Attributes.BonusStam * 5;
				price +=		((Spellbook)item).Attributes.BonusMana * 5;

				int lmc = ((Spellbook)item).Attributes.LowerManaCost;
					if ( ((Spellbook)item).Attributes.LowerManaCost > MyServerSettings.LowerMana() )
						lmc = MyServerSettings.LowerMana();
							price += lmc * 5;

				int lrc = ((Spellbook)item).Attributes.LowerRegCost;
					if ( ((Spellbook)item).Attributes.LowerRegCost > MyServerSettings.LowerReg() )
						lrc = MyServerSettings.LowerReg();
							price += lrc * 5;

				price +=		((Spellbook)item).Attributes.Luck * 2;
				price +=		((Spellbook)item).Attributes.WeaponDamage * 5;
				price +=		((Spellbook)item).Attributes.WeaponSpeed * 6;
				price +=		((Spellbook)item).Attributes.BonusStr * 10;
				price +=		((Spellbook)item).Attributes.BonusDex * 10;
				price +=		((Spellbook)item).Attributes.BonusInt * 10;
				price +=		((Spellbook)item).Attributes.EnhancePotions * 2;
				price +=		((Spellbook)item).Attributes.CastSpeed * 4;
				price +=		((Spellbook)item).Attributes.CastRecovery * 4;
				price +=		((Spellbook)item).Attributes.SpellDamage * 4;

				SkillName skill;
				double bonus;

				((Spellbook)item).SkillBonuses.GetValues( 0, out skill, out bonus ); price += (int)(bonus*10);
				((Spellbook)item).SkillBonuses.GetValues( 1, out skill, out bonus ); price += (int)(bonus*10);
				((Spellbook)item).SkillBonuses.GetValues( 2, out skill, out bonus ); price += (int)(bonus*10);
				((Spellbook)item).SkillBonuses.GetValues( 3, out skill, out bonus ); price += (int)(bonus*10);
				((Spellbook)item).SkillBonuses.GetValues( 4, out skill, out bonus ); price += (int)(bonus*10);

				int spells = ((Spellbook)item).SpellCount;
				int pageCt = 0;

				if ( ((Spellbook)item).MageryBook() )
				{
					while ( spells > 0 )
					{
						spells--;
						pageCt++;

						if ( pageCt < 9 ){			price += 10; }
						else if ( pageCt < 17 ){	price += 20; }
						else if ( pageCt < 25 ){	price += 30; }
						else if ( pageCt < 33 ){	price += 40; }
						else if ( pageCt < 41 ){	price += 50; }
						else if ( pageCt < 49 ){	price += 60; }
						else if ( pageCt < 57 ){	price += 70; }
						else {						price += 80; }
					}
				}
				else if ( item is NecromancerSpellbook )
				{
					while ( spells > 0 )
					{
						spells--;
						pageCt++;

						if ( pageCt == 1 ){			price += 12; }
						else if ( pageCt == 2 ){	price += 24; }
						else if ( pageCt == 3 ){	price += 28; }
						else if ( pageCt == 4 ){	price += 52; }
						else if ( pageCt == 5 ){	price += 52; }
						else if ( pageCt == 6 ){	price += 52; }
						else if ( pageCt == 7 ){	price += 52; }
						else if ( pageCt == 8 ){	price += 54; }
						else if ( pageCt == 9 ){	price += 78; }
						else if ( pageCt == 10 ){	price += 78; }
						else if ( pageCt == 11 ){	price += 102; }
						else if ( pageCt == 12 ){	price += 128; }
						else if ( pageCt == 13 ){	price += 128; }
						else if ( pageCt == 14 ){	price += 128; }
						else if ( pageCt == 15 ){	price += 144; }
						else if ( pageCt == 16 ){	price += 202; }
						else {						price += 228; }
					}
				}
				else if ( item is ElementalSpellbook )
				{
					while ( spells > 0 )
					{
						spells--;
						pageCt++;

						if ( pageCt < 5 ){			price += 10; }
						else if ( pageCt < 9 ){		price += 16; }
						else if ( pageCt < 13 ){	price += 22; }
						else if ( pageCt < 17 ){	price += 28; }
						else if ( pageCt < 21 ){	price += 34; }
						else if ( pageCt < 25 ){	price += 40; }
						else if ( pageCt < 29 ){	price += 46; }
						else {						price += 52; }
					}
				}
				else if ( item is MysticSpellbook )
				{
					while ( spells > 0 )
					{
						spells--;
						pageCt++;

						price += pageCt * pageCt;
					}
				}
				else if ( item is SongBook )
				{
					while ( spells > 0 )
					{
						spells--;
						pageCt++;

						if ( pageCt < 3 )
							price += 12;
						else if ( pageCt < 6 )
							price += 14;
						else if ( pageCt < 9 )
							price += 16;
						else if ( pageCt < 10 )
							price += 20;
						else
							price += 32;
					}
				}
			}
			else if ( item is BaseInstrument )
			{
				price +=		((BaseInstrument)item).Resistances.Physical * 2;
				price +=		((BaseInstrument)item).Resistances.Fire * 2;
				price +=		((BaseInstrument)item).Resistances.Cold * 2;
				price +=		((BaseInstrument)item).Resistances.Poison * 2;
				price +=		((BaseInstrument)item).Resistances.Energy * 2;

				if ( ((BaseInstrument)item).Slayer != SlayerName.None )
					price +=		200;

				if ( ((BaseInstrument)item).Slayer2 != SlayerName.None )
					price +=		200;

				price +=		(int)(((BaseInstrument)item).SkillBonuses.Skill_1_Value * 2);
				price +=		(int)(((BaseInstrument)item).SkillBonuses.Skill_2_Value * 2);
				price +=		(int)(((BaseInstrument)item).SkillBonuses.Skill_3_Value * 2);
				price +=		(int)(((BaseInstrument)item).SkillBonuses.Skill_4_Value * 2);
				price +=		(int)(((BaseInstrument)item).SkillBonuses.Skill_5_Value * 2);

				price +=		((BaseInstrument)item).Attributes.SpellChanneling * 200;
				price +=		((BaseInstrument)item).Attributes.DefendChance * 10;
				price +=		((BaseInstrument)item).Attributes.ReflectPhysical * 2;
				price +=		((BaseInstrument)item).Attributes.AttackChance * 10;
				price +=		((BaseInstrument)item).Attributes.RegenHits * 5;
				price +=		((BaseInstrument)item).Attributes.RegenStam * 5;
				price +=		((BaseInstrument)item).Attributes.RegenMana * 5;
				price +=		((BaseInstrument)item).Attributes.NightSight * 6;
				price +=		((BaseInstrument)item).Attributes.BonusHits * 5;
				price +=		((BaseInstrument)item).Attributes.BonusStam * 5;
				price +=		((BaseInstrument)item).Attributes.BonusMana * 5;

				int lmc = ((BaseInstrument)item).Attributes.LowerManaCost;
					if ( ((BaseInstrument)item).Attributes.LowerManaCost > MyServerSettings.LowerMana() )
						lmc = MyServerSettings.LowerMana();
							price += lmc * 5;

				int lrc = ((BaseInstrument)item).Attributes.LowerRegCost;
					if ( ((BaseInstrument)item).Attributes.LowerRegCost > MyServerSettings.LowerReg() )
						lrc = MyServerSettings.LowerReg();
							price += lrc * 5;

				price +=		((BaseInstrument)item).Attributes.Luck * 2;
				price +=		((BaseInstrument)item).Attributes.WeaponDamage * 5;
				price +=		((BaseInstrument)item).Attributes.WeaponSpeed * 6;
				price +=		((BaseInstrument)item).Attributes.BonusStr * 10;
				price +=		((BaseInstrument)item).Attributes.BonusDex * 10;
				price +=		((BaseInstrument)item).Attributes.BonusInt * 10;
				price +=		((BaseInstrument)item).Attributes.EnhancePotions * 2;
				price +=		((BaseInstrument)item).Attributes.CastSpeed * 4;
				price +=		((BaseInstrument)item).Attributes.CastRecovery * 4;
				price +=		((BaseInstrument)item).Attributes.SpellDamage * 4;

				SkillName skill;
				double bonus;

				((BaseInstrument)item).SkillBonuses.GetValues( 0, out skill, out bonus ); price += (int)(bonus*10);
				((BaseInstrument)item).SkillBonuses.GetValues( 1, out skill, out bonus ); price += (int)(bonus*10);
				((BaseInstrument)item).SkillBonuses.GetValues( 2, out skill, out bonus ); price += (int)(bonus*10);
				((BaseInstrument)item).SkillBonuses.GetValues( 3, out skill, out bonus ); price += (int)(bonus*10);
				((BaseInstrument)item).SkillBonuses.GetValues( 4, out skill, out bonus ); price += (int)(bonus*10);
			}
			else if ( item is BaseTrinket )
			{
				price +=		((BaseTrinket)item).Resistances.Physical * 2;
				price +=		((BaseTrinket)item).Resistances.Fire * 2;
				price +=		((BaseTrinket)item).Resistances.Cold * 2;
				price +=		((BaseTrinket)item).Resistances.Poison * 2;
				price +=		((BaseTrinket)item).Resistances.Energy * 2;

				price +=		(int)(((BaseTrinket)item).SkillBonuses.Skill_1_Value * 2);
				price +=		(int)(((BaseTrinket)item).SkillBonuses.Skill_2_Value * 2);
				price +=		(int)(((BaseTrinket)item).SkillBonuses.Skill_3_Value * 2);
				price +=		(int)(((BaseTrinket)item).SkillBonuses.Skill_4_Value * 2);
				price +=		(int)(((BaseTrinket)item).SkillBonuses.Skill_5_Value * 2);

				price +=		((BaseTrinket)item).Attributes.SpellChanneling * 200;
				price +=		((BaseTrinket)item).Attributes.DefendChance * 10;
				price +=		((BaseTrinket)item).Attributes.ReflectPhysical * 2;
				price +=		((BaseTrinket)item).Attributes.AttackChance * 10;
				price +=		((BaseTrinket)item).Attributes.RegenHits * 5;
				price +=		((BaseTrinket)item).Attributes.RegenStam * 5;
				price +=		((BaseTrinket)item).Attributes.RegenMana * 5;
				price +=		((BaseTrinket)item).Attributes.NightSight * 6;
				price +=		((BaseTrinket)item).Attributes.BonusHits * 5;
				price +=		((BaseTrinket)item).Attributes.BonusStam * 5;
				price +=		((BaseTrinket)item).Attributes.BonusMana * 5;

				int lmc = ((BaseTrinket)item).Attributes.LowerManaCost;
					if ( ((BaseTrinket)item).Attributes.LowerManaCost > MyServerSettings.LowerMana() )
						lmc = MyServerSettings.LowerMana();
							price += lmc * 5;

				int lrc = ((BaseTrinket)item).Attributes.LowerRegCost;
					if ( ((BaseTrinket)item).Attributes.LowerRegCost > MyServerSettings.LowerReg() )
						lrc = MyServerSettings.LowerReg();
							price += lrc * 5;

				price +=		((BaseTrinket)item).Attributes.Luck * 2;
				price +=		((BaseTrinket)item).Attributes.WeaponDamage * 5;
				price +=		((BaseTrinket)item).Attributes.WeaponSpeed * 6;
				price +=		((BaseTrinket)item).Attributes.BonusStr * 10;
				price +=		((BaseTrinket)item).Attributes.BonusDex * 10;
				price +=		((BaseTrinket)item).Attributes.BonusInt * 10;
				price +=		((BaseTrinket)item).Attributes.EnhancePotions * 2;
				price +=		((BaseTrinket)item).Attributes.CastSpeed * 4;
				price +=		((BaseTrinket)item).Attributes.CastRecovery * 4;
				price +=		((BaseTrinket)item).Attributes.SpellDamage * 4;

				SkillName skill;
				double bonus;

				((BaseTrinket)item).SkillBonuses.GetValues( 0, out skill, out bonus ); price += (int)(bonus*10);
				((BaseTrinket)item).SkillBonuses.GetValues( 1, out skill, out bonus ); price += (int)(bonus*10);
				((BaseTrinket)item).SkillBonuses.GetValues( 2, out skill, out bonus ); price += (int)(bonus*10);
				((BaseTrinket)item).SkillBonuses.GetValues( 3, out skill, out bonus ); price += (int)(bonus*10);
				((BaseTrinket)item).SkillBonuses.GetValues( 4, out skill, out bonus ); price += (int)(bonus*10);
			}
			else if ( item is BaseClothing )
			{
				price +=		((BaseClothing)item).Resistances.Physical * 2;
				price +=		((BaseClothing)item).Resistances.Fire * 2;
				price +=		((BaseClothing)item).Resistances.Cold * 2;
				price +=		((BaseClothing)item).Resistances.Poison * 2;
				price +=		((BaseClothing)item).Resistances.Energy * 2;

				price +=		(int)(((BaseClothing)item).SkillBonuses.Skill_1_Value * 2);
				price +=		(int)(((BaseClothing)item).SkillBonuses.Skill_2_Value * 2);
				price +=		(int)(((BaseClothing)item).SkillBonuses.Skill_3_Value * 2);
				price +=		(int)(((BaseClothing)item).SkillBonuses.Skill_4_Value * 2);
				price +=		(int)(((BaseClothing)item).SkillBonuses.Skill_5_Value * 2);

				price +=		((BaseClothing)item).Attributes.SpellChanneling * 200;
				price +=		((BaseClothing)item).Attributes.DefendChance * 10;
				price +=		((BaseClothing)item).Attributes.ReflectPhysical * 2;
				price +=		((BaseClothing)item).Attributes.AttackChance * 10;
				price +=		((BaseClothing)item).Attributes.RegenHits * 5;
				price +=		((BaseClothing)item).Attributes.RegenStam * 5;
				price +=		((BaseClothing)item).Attributes.RegenMana * 5;
				price +=		((BaseClothing)item).Attributes.NightSight * 6;
				price +=		((BaseClothing)item).Attributes.BonusHits * 5;
				price +=		((BaseClothing)item).Attributes.BonusStam * 5;
				price +=		((BaseClothing)item).Attributes.BonusMana * 5;

				int lmc = ((BaseClothing)item).Attributes.LowerManaCost;
					if ( ((BaseClothing)item).Attributes.LowerManaCost > MyServerSettings.LowerMana() )
						lmc = MyServerSettings.LowerMana();
							price += lmc * 5;

				int lrc = ((BaseClothing)item).Attributes.LowerRegCost;
					if ( ((BaseClothing)item).Attributes.LowerRegCost > MyServerSettings.LowerReg() )
						lrc = MyServerSettings.LowerReg();
							price += lrc * 5;

				price +=		((BaseClothing)item).Attributes.Luck * 2;
				price +=		((BaseClothing)item).Attributes.WeaponDamage * 5;
				price +=		((BaseClothing)item).Attributes.WeaponSpeed * 6;
				price +=		((BaseClothing)item).Attributes.BonusStr * 10;
				price +=		((BaseClothing)item).Attributes.BonusDex * 10;
				price +=		((BaseClothing)item).Attributes.BonusInt * 10;
				price +=		((BaseClothing)item).Attributes.EnhancePotions * 2;
				price +=		((BaseClothing)item).Attributes.CastSpeed * 4;
				price +=		((BaseClothing)item).Attributes.CastRecovery * 4;
				price +=		((BaseClothing)item).Attributes.SpellDamage * 4;

				SkillName skill;
				double bonus;

				((BaseClothing)item).SkillBonuses.GetValues( 0, out skill, out bonus ); price += (int)(bonus*10);
				((BaseClothing)item).SkillBonuses.GetValues( 1, out skill, out bonus ); price += (int)(bonus*10);
				((BaseClothing)item).SkillBonuses.GetValues( 2, out skill, out bonus ); price += (int)(bonus*10);
				((BaseClothing)item).SkillBonuses.GetValues( 3, out skill, out bonus ); price += (int)(bonus*10);
				((BaseClothing)item).SkillBonuses.GetValues( 4, out skill, out bonus ); price += (int)(bonus*10);
			}
			else if ( item is BaseQuiver )
			{
				price +=		((BaseQuiver)item).LowerAmmoCost * 5;
				price +=		((BaseQuiver)item).WeightReduction * 2;

				price +=		((BaseQuiver)item).Attributes.SpellChanneling * 200;
				price +=		((BaseQuiver)item).Attributes.DefendChance * 10;
				price +=		((BaseQuiver)item).Attributes.ReflectPhysical * 2;
				price +=		((BaseQuiver)item).Attributes.AttackChance * 10;
				price +=		((BaseQuiver)item).Attributes.RegenHits * 5;
				price +=		((BaseQuiver)item).Attributes.RegenStam * 5;
				price +=		((BaseQuiver)item).Attributes.RegenMana * 5;
				price +=		((BaseQuiver)item).Attributes.NightSight * 6;
				price +=		((BaseQuiver)item).Attributes.BonusHits * 5;
				price +=		((BaseQuiver)item).Attributes.BonusStam * 5;
				price +=		((BaseQuiver)item).Attributes.BonusMana * 5;

				int lmc = ((BaseQuiver)item).Attributes.LowerManaCost;
					if ( ((BaseQuiver)item).Attributes.LowerManaCost > MyServerSettings.LowerMana() )
						lmc = MyServerSettings.LowerMana();
							price += lmc * 5;

				int lrc = ((BaseQuiver)item).Attributes.LowerRegCost;
					if ( ((BaseQuiver)item).Attributes.LowerRegCost > MyServerSettings.LowerReg() )
						lrc = MyServerSettings.LowerReg();
							price += lrc * 5;

				price +=		((BaseQuiver)item).Attributes.Luck * 2;
				price +=		((BaseQuiver)item).Attributes.WeaponDamage * 5;
				price +=		((BaseQuiver)item).Attributes.WeaponSpeed * 6;
				price +=		((BaseQuiver)item).Attributes.BonusStr * 10;
				price +=		((BaseQuiver)item).Attributes.BonusDex * 10;
				price +=		((BaseQuiver)item).Attributes.BonusInt * 10;
				price +=		((BaseQuiver)item).Attributes.EnhancePotions * 2;
				price +=		((BaseQuiver)item).Attributes.CastSpeed * 4;
				price +=		((BaseQuiver)item).Attributes.CastRecovery * 4;
				price +=		((BaseQuiver)item).Attributes.SpellDamage * 4;
			}

			return price;
		}

		public static int GetSellPrice( int val, bool guild )
		{
			ItemSalesInfo info = GetData( val );

			if ( info == null )
				return 0;

			if ( MyServerSettings.HigherPrice() > 0 )
				return (int)(info.iPrice + ( info.iPrice * MyServerSettings.HigherPrice() ));

			if ( info.iCategory == ItemSalesInfo.Category.Resource && MyServerSettings.ResourcePrice() > 0 )
				return (int)(info.iPrice + ( info.iPrice * MyServerSettings.ResourcePrice() ));

			if ( guild )
				return info.iPrice;

			return Utility.RandomMinMax( info.iPrice, (int)(info.iPrice*1.4) );
		}

		public static int GetBuysPrice( int val, bool guild, Item item, bool fluctuate, bool resale )
		{
			if ( val < 0 )
				return 0;

			ItemSalesInfo info = GetData( val );
			if ( info == null )
				return 0;

			int price = info.iPrice;

			if ( item != null )
				price = AddUpBenefits( item, price, false, resale );

			if ( info.iPrice >= 5000 && item != null && item.ArtifactLevel > (int)ArtifactLevel.None && resale )
				price = (int)(price * 3);
			else if ( resale && item != null && item is BaseClothing )
				price = (int)(price * 15);
			else if ( resale && item != null && item is Spellbook )
				price = (int)(price * 5);
			else if ( resale )
				price = (int)(price * 10);
			else
				price = (int)(price / 2);

			if ( !guild && fluctuate )
				price = Utility.RandomMinMax( (int)( price * 0.6 ), price );

			if ( price < 1 )
				price = 1;

			if ( MySettings.S_SellGoldCutRate > 0 )
			{
				price = (int)(price - ( price * MyServerSettings.SellGoldCutRate() ));
					if ( price < 1 )
						price = 1;
			}

			return price;
		}

		public static int GetQty( int val, bool guild )
		{
			int qty = 0;
			int rar = 0;
			ItemSalesInfo.Category tCategory = ItemSalesInfo.Category.None;

			ItemSalesInfo info = GetData( val );
			if ( info != null )
			{
				qty = info.iQty;
				rar = info.iRarity;
				tCategory = info.iCategory;
			}

			if ( rar == 200 )
				qty = 0;
			else if ( rar > 100 && !guild )
				qty = 0;
			else if ( guild )
				qty = qty * 10;

			if ( Utility.RandomMinMax( 1, 100 ) < rar && rar < 101 && rar > 0 )
				qty = 0;

			if ( qty < 1 )
				return 0;

			qty = Utility.RandomMinMax( 1, qty );

			if ( ( tCategory == ItemSalesInfo.Category.Resource || tCategory == ItemSalesInfo.Category.Reagent ) )
			{
				if ( qty > 0 )
				{
					qty = Utility.RandomMinMax( 10, 50 );

					if ( ( guild || MySettings.S_SoldResource ) && qty > 0 )
						qty = Utility.RandomMinMax( 100,850 );
				}

				if ( qty < 1 )
					qty = 0;
			}

			return qty;
		}

		public static bool WillDeal( int val, Mobile m, bool selling, bool blackMarket, ItemSalesInfo.World world, bool guild )
		{
			if ( MySettings.S_NoBuyResources && iCategory(val) == ItemSalesInfo.Category.Resource )
				return false;

			if ( blackMarket )
				return Utility.RandomBool();

			if ( MySettings.S_SellAll && selling )
				return true;

			if ( MySettings.S_BuyAll && !selling )
				return true;

			if ( guild )
				return true;

			// These areas only sell a couple of items so let them always sell
			if ( ( world == ItemSalesInfo.World.Ambrosia || world == ItemSalesInfo.World.Elf ) && WorldTest( val, m, world ) )
				return true;

			if ( Utility.RandomMinMax(1,10) > 4 )
				return true;

			return false;
		}

		public static void GetSellList( Mobile m, List<GenericBuyInfo> LIST, ItemSalesInfo.Category v_Category, ItemSalesInfo.Material v_Material, ItemSalesInfo.Market v_Market, ItemSalesInfo.World v_World, Type specificType )
		{
			ItemSalesInfo[] list = ItemSalesInfo.m_SellingInfo;

			bool v_Guild = false;
				if ( m is BaseGuildmaster )
					v_Guild = true;

			int entries = list.Length;
			int val = 0;
			int price = 0;
			int qty = 0;
			bool chemist = false;
			bool set = false;

			Item oItem = null;
			int oItemID = 0;
			int oHue = 0;
			string oName = null;

			string CurrentMonth = DateTime.Now.ToString("MM");

			Drinks( LIST, v_Market );

			while ( entries > 0 )
			{
				Type itemType = list[val].ItemsType;

				if ( itemType != null )
				{
					set = false;
					oItem = null;
					oItemID = 0;
					oHue = 0;
					oName = null;

					chemist = Chemist( val, v_Market, v_Category );

					if ( ( specificType != null && itemType == specificType ) || ( iSells(val) && v_Market == iMarket(val) && v_Category == iCategory(val) ) )
					{
						set = true;
					}
					else if
					(
						WillDeal( val, m, true, false, iWorld(val), v_Guild ) && itemType != null && specificType == null && 
						( chemist ||
						(
						( !chemist ) && 
						( v_Category == iCategory(val) || v_Category == ItemSalesInfo.Category.All ) && 
						( v_Material == iMaterial(val) || v_Material == ItemSalesInfo.Material.All ) && 
						( v_Market == iMarket(val) || v_Market == ItemSalesInfo.Market.All ) && 
						( v_World == iWorld(val) || WorldTest( val, m, v_World ) )
						)
						)
					)
					{
						set = true;
					}

					if ( CurrentMonth != "12" && iCategory(val) == ItemSalesInfo.Category.Christmas )
						set = false;

					if ( CurrentMonth != "10" && iCategory(val) == ItemSalesInfo.Category.Halloween )
						set = false;

					if ( v_Market == ItemSalesInfo.Market.Sage && v_Category == ItemSalesInfo.Category.Artifact && iMarket(val) != ItemSalesInfo.Market.Thief && iCategory(val) == ItemSalesInfo.Category.Artifact )
					{
						// This section is just for the sage to display artifacts that cannot be bought.
						oItem = (Item)Activator.CreateInstance( itemType );

						if ( oItem != null )
						{
							oItemID = oItem.ItemID;
							oHue = oItem.Hue;
							oName = oItem.Name;
							oItem.Delete();				

							if ( !LIST.Contains( new GenericBuyInfo( oName, itemType, 0, 1, oItemID, oHue ) ) )
								LIST.Add( new GenericBuyInfo( oName, itemType, 0, 1, oItemID, oHue ) );
						}
					}
					else if ( set )
					{
						qty = GetQty( val, v_Guild );

						// These areas only sell a couple of items so let them always sell at least 1
						if ( qty < 1 && ( iWorld(val) == ItemSalesInfo.World.Ambrosia || iWorld(val) == ItemSalesInfo.World.Elf ) )
							qty = 1;

						price = GetSellPrice( val, v_Guild );

						if ( !MySettings.S_LawnsAllowed && itemType == typeof( LawnTools ) )
							qty = 0;
						else if ( qty < 0 )
							qty = 0;
						else if ( !MySettings.S_ShantysAllowed && itemType == typeof( ShantyTools ) )
							qty = 0;
						else if ( !MySettings.S_Basements && itemType == typeof( BasementDoor ) )
							qty = 0;

						if ( qty > 0 )
						{
							oItem = (Item)Activator.CreateInstance( itemType );

							if ( oItem != null )
							{
								oItemID = oItem.ItemID;
								ResourceMods.DefaultItemHue( oItem );
								oHue = oItem.Hue;
									oHue = ClothHue( oHue, iMaterial(val), iMarket(val) );
								oName = oItem.Name;
								oItem.Delete();

								if ( !LIST.Contains( new GenericBuyInfo( oName, itemType, price, qty, oItemID, oHue ) ) )
									LIST.Add( new GenericBuyInfo( oName, itemType, price, qty, oItemID, oHue ) );
							}
						}
					}
				}
				entries--;
				val++;
			}

		}

		public static void GetBuysList( Mobile m, GenericSellInfo LIST, ItemSalesInfo.Category v_Category, ItemSalesInfo.Material v_Material, ItemSalesInfo.Market v_Market, ItemSalesInfo.World v_World, Type specificType, bool force = false )
		{
			ItemSalesInfo[] list = ItemSalesInfo.m_SellingInfo;

			bool v_Guild = false;
				if ( m is BaseGuildmaster )
					v_Guild = true;

			int entries = list.Length;
			int val = 0;
			int price = 0;
			bool chemist = false;
			bool set = false;

			if ( v_Market == ItemSalesInfo.Market.Cartographer && !(LIST.IsInList( typeof( PresetMapEntry ) ) ) )
				LIST.Add( typeof( PresetMapEntry ), 3 );

			while ( entries > 0 )
			{
				Type itemType = list[val].ItemsType;

				if ( itemType != null )
				{
                    set = false;
					chemist = Chemist( val, v_Market, v_Category );

					if ( force || ( ( specificType != null && itemType == specificType ) || ( iRarity(val) == 200 && v_Market == iMarket(val) ) ) || ( iBuys(val) && v_Market == iMarket(val) && v_Category == iCategory(val) ) )
					{
						set = true;
					}
					else if
					(
						WillDeal( val, m, false, false, iWorld(val), v_Guild ) && list[val].ItemsType != null && specificType == null && 
						( chemist ||
						(
						( !chemist ) && 
						( v_Category == iCategory(val) || v_Category == ItemSalesInfo.Category.All ) && 
						( v_Material == iMaterial(val) || v_Material == ItemSalesInfo.Material.All ) && 
						( v_Market == iMarket(val) || 
						v_Market == ItemSalesInfo.Market.All ||
                        // If vendor buys art, also accept stone items named "Statue"
                        (v_Market == ItemSalesInfo.Market.Art && iMarket(val) == ItemSalesInfo.Market.Stone && itemType.Name.IndexOf("Statue", StringComparison.OrdinalIgnoreCase) >= 0))
						)
						)
					)
					{
						set = true;
					}

					if ( !SetAllowedSell( iCategory(val), list[val].ItemsType ) )
						set = false;

					if ( set )
					{
						price = GetBuysPrice( val, v_Guild, null, !force, false );

						if ( LIST.IsInList( list[val].ItemsType ) )
							price = 0;

                        if ( price > 0 )
                            LIST.Add( list[val].ItemsType, price );
                    }
                }
                entries--;
				val++;
			}
		}

		public static int ItemTableRef( Item item )
		{
			ItemSalesInfo[] list = ItemSalesInfo.m_SellingInfo;
			int entries = list.Length;
			int val = 0;

			while ( entries > 0 )
			{
				Type itemType = list[val].ItemsType;

				if ( itemType == item.GetType() )
					return val;

				entries--;
				val++;
			}

			return 0;
		}

		public static void BlackMarketList( Mobile m, ItemSalesInfo.Category v_Category, ItemSalesInfo.Material v_Material, ItemSalesInfo.Market v_Market, ItemSalesInfo.World v_World )
		{
			BlackMarketList( m, v_Category, v_Material, v_Market, v_World, null );
		}

		public static void BlackMarketList( Mobile m, ItemSalesInfo.Category v_Category, ItemSalesInfo.Material v_Material, ItemSalesInfo.Market v_Market, ItemSalesInfo.World v_World, Type specificType )
		{
			ItemSalesInfo[] list = ItemSalesInfo.m_SellingInfo;

			int entries = list.Length;
			int val = 0;
			int price = 0;
			bool set = false;
			bool skip = false;

			while ( entries > 0 )
			{
				Type itemType = list[val].ItemsType;

				if ( itemType != null )
				{
					set = false;
					skip = false;

					if ( specificType != null && itemType == specificType && WillDeal( val, m, true, true, iWorld(val), false ) )
					{
						set = true;
					}
					else if
					(
						WillDeal( val, m, true, true, iWorld(val), false ) && list[val].ItemsType != null && specificType == null && 
						(
						( v_Category == iCategory(val) || v_Category == ItemSalesInfo.Category.All ) && 
						( v_Material == iMaterial(val) || v_Material == ItemSalesInfo.Material.All ) && 
						( v_Market == iMarket(val) || v_Market == ItemSalesInfo.Market.All ) && 
						( v_World == iWorld(val) || WorldTest( val, m, v_World ) )
						)
					)
						set = true;

					if ( set )
					{
						price = GetBuysPrice( val, false, null, true, false );

						if ( price > 0 )
						{
							Item product = null;

							if ( itemType == typeof( MagicalWand ) )
								product = new MagicalWand( Utility.RandomList(8,7,7,6,6,6,5,5,5,5,4,4,4,4,4,3,3,3,3,3,3,2,2,2,2,2,2,2,1,1,1,1,1,1,1,1) );
							else
								product = (Item)Activator.CreateInstance( list[val].ItemsType );

							if ( !(product is BaseInstrument) && v_Market == ItemSalesInfo.Market.Bard )
								skip = true;
							else if ( !(product is BaseClothing) && v_Material == ItemSalesInfo.Material.Cloth )
								skip = true;
							else if ( CraftResources.GetType( product.Resource ) == CraftResourceType.Leather && v_Material == ItemSalesInfo.Material.Cloth )
								skip = true;
							else if ( product is BaseRanged && v_Material == ItemSalesInfo.Material.Wood && v_Market != ItemSalesInfo.Market.Bow )
								skip = true;

							if ( skip )
							{
								product.Delete();
							}
							else
							{
								if ( CraftResources.GetType( product.Resource ) == CraftResourceType.Metal && Utility.Random(20) == 0 )
									product.Resource = CraftResource.AmethystBlock;
								else if ( CraftResources.GetType( product.Resource ) == CraftResourceType.Leather && Utility.Random(20) == 0 )
									product.Resource = CraftResource.DemonSkin;

								ResourceMods.SetRandomResource( false, false, product, product.Resource, true, m );

								if ( product is BaseTrinket && v_Market == ItemSalesInfo.Market.Jeweler )
									BaseTrinket.RandomGem( (BaseTrinket)product );

								if ( Item.IsStandardResource( product.Resource ) && !(product is MagicalWand) )
									product.Delete();
								else
									m.BankBox.DropItem( product );
							}
						}
					}
				}
				entries--;
				val++;
			}
		}

		public static int ClothHue ( int hue, ItemSalesInfo.Material v_Material, ItemSalesInfo.Market v_Market )
		{
			if ( v_Material == ItemSalesInfo.Material.Cloth )
			{
				if ( v_Market == ItemSalesInfo.Market.Sailor )
					return Utility.RandomDyedHue();
				if ( v_Market == ItemSalesInfo.Market.Tailor )
					return Utility.RandomDyedHue();
				if ( v_Market == ItemSalesInfo.Market.Wizard )
					return Utility.RandomDyedHue();
			}

			return hue;
		}

		public static bool SetAllowedSell( ItemSalesInfo.Category v_Category, Type itemType )
		{
			if ( v_Category == ItemSalesInfo.Category.MonsterRace && MySettings.S_MonsterCharacters < 1 )
				return false;

			if ( !MySettings.S_BuyCloth )
			{
				if ( itemType == typeof( SpoolOfThread ) )
					return false;
				if ( itemType == typeof( Flax ) )
					return false;
				if ( itemType == typeof( Cotton ) )
					return false;
				if ( itemType == typeof( Wool ) )
					return false;
				if ( itemType == typeof( Fabric ) )
					return false;
			}

			return true;
		}

		public static bool WorldTest( int val, Mobile m, ItemSalesInfo.World world )
		{
			Region reg = Region.Find( m.Location, m.Map );

			ItemSalesInfo.World area = ItemSalesInfo.World.None;
			ItemSalesInfo info = GetData( val );
			if ( info != null )
				area = info.iWorld;

			if ( world == ItemSalesInfo.World.Orient || area == ItemSalesInfo.World.Orient )
				return false;
			if ( area == ItemSalesInfo.World.None )
				return true;
			if ( reg.IsPartOf( "the Enchanted Pass" ) && area == ItemSalesInfo.World.Elf )
				return true;
			if ( Worlds.isHauntedRegion( m ) && area == ItemSalesInfo.World.Necro )
				return true;
			if ( ( Worlds.IsSeaDungeon( m.Location, m.Map ) || Worlds.IsWaterSea( m ) ) && area == ItemSalesInfo.World.Sea )
				return true;
			if ( m.Land == Land.Lodoria && area == ItemSalesInfo.World.Lodor )
				return true;
			if ( m.Land == Land.Sosaria && area == ItemSalesInfo.World.Sosaria )
				return true;
			if ( m.Land == Land.Underworld && area == ItemSalesInfo.World.Underworld )
				return true;
			if ( m.Land == Land.Serpent && area == ItemSalesInfo.World.Serpent )
				return true;
			if ( m.Land == Land.IslesDread && area == ItemSalesInfo.World.Dread )
				return true;
			if ( m.Land == Land.Savaged && area == ItemSalesInfo.World.Savage )
				return true;
			if ( m.Land == Land.Ambrosia && area == ItemSalesInfo.World.Ambrosia )
				return true;
			if ( m.Land == Land.UmberVeil && area == ItemSalesInfo.World.Umber )
				return true;

			return false;
		}

		public static void Drinks( List<GenericBuyInfo> LIST, ItemSalesInfo.Market market )
		{
			int d1 = Utility.Random(3);		int x1 = Utility.Random(5);
			int d2 = Utility.Random(3);		int x2 = Utility.Random(5);
			int d3 = Utility.Random(3);		int x3 = Utility.Random(5);
			int d4 = Utility.Random(3);		int x4 = Utility.Random(5);
			int d5 = Utility.Random(3);		int x5 = Utility.Random(5);
			int d6 = Utility.Random(3);		int x6 = Utility.Random(5);

			if ( market == ItemSalesInfo.Market.Tavern )
			{
				if ( d1 == 0 ){ LIST.Add( new BeverageBuyInfo( typeof( BeverageBottle ), BeverageType.Ale, 4+x1, Utility.Random( 1,15 ), 0x282A, 0x83b ) ); }
				if ( d2 == 0 ){ LIST.Add( new BeverageBuyInfo( typeof( BeverageBottle ), BeverageType.Cider, 4+x2, Utility.Random( 1,15 ), 0x282A, 0x981 ) ); }
				if ( d3 == 0 ){ LIST.Add( new BeverageBuyInfo( typeof( BeverageBottle ), BeverageType.Liquor, 4+x3, Utility.Random( 1,15 ), 0x282A, 0xB51 ) ); }
				if ( d4 == 0 ){ LIST.Add( new BeverageBuyInfo( typeof( BeverageBottle ), BeverageType.Wine, 4+x4, Utility.Random( 1,15 ), 0x282A, 0xB64 ) ); }
			}

			if ( market == ItemSalesInfo.Market.Farmer || market == ItemSalesInfo.Market.Cook || market == ItemSalesInfo.Market.Tavern )
				{ if ( d5 == 0 ){ LIST.Add( new BeverageBuyInfo( typeof( BeverageBottle ), BeverageType.Milk, 4+x5, Utility.Random( 1,15 ), 0x282A, 0x9A3 ) ); } }

			if ( market == ItemSalesInfo.Market.Tavern || market == ItemSalesInfo.Market.Provisions )
				{ if ( d6 == 0 ){ LIST.Add( new BeverageBuyInfo( typeof( BeverageBottle ), BeverageType.Water, 4+x6, Utility.Random( 1,15 ), 0x282A, 0xB40 ) ); } }

			// ----------------------------------------------------------------------------------------------------------------------------

			if ( market == ItemSalesInfo.Market.Tavern )
			{
				if ( d1 == 1 ){ LIST.Add( new BeverageBuyInfo( typeof( Jug ), BeverageType.Ale, 16+x1, Utility.Random( 1,15 ), 0x4CEF, 0x83b ) ); }
				if ( d2 == 1 ){ LIST.Add( new BeverageBuyInfo( typeof( Jug ), BeverageType.Cider, 16+x2, Utility.Random( 1,15 ), 0x4CEF, 0x981 ) ); }
				if ( d3 == 1 ){ LIST.Add( new BeverageBuyInfo( typeof( Jug ), BeverageType.Liquor, 16+x3, Utility.Random( 1,15 ), 0x4CEF, 0xB51 ) ); }
				if ( d4 == 1 ){ LIST.Add( new BeverageBuyInfo( typeof( Jug ), BeverageType.Wine, 16+x4, Utility.Random( 1,15 ), 0x4CEF, 0xB64 ) ); }
			}

			if ( market == ItemSalesInfo.Market.Farmer || market == ItemSalesInfo.Market.Cook || market == ItemSalesInfo.Market.Tavern )
				{ if ( d5 == 1 ){ LIST.Add( new BeverageBuyInfo( typeof( Jug ), BeverageType.Milk, 16+x5, Utility.Random( 1,15 ), 0x4CEF, 0x9A3 ) ); } }

			if ( market == ItemSalesInfo.Market.Tavern || market == ItemSalesInfo.Market.Provisions )
				{ if ( d6 == 1 ){ LIST.Add( new BeverageBuyInfo( typeof( Jug ), BeverageType.Water, 16+x6, Utility.Random( 1,15 ), 0x4CEF, 0xB40 ) ); } }

			// ----------------------------------------------------------------------------------------------------------------------------

			if ( market == ItemSalesInfo.Market.Tavern )
			{
				if ( d1 == 2 ){ LIST.Add( new BeverageBuyInfo( typeof( Pitcher ), BeverageType.Ale, 9+x1, Utility.Random( 1,15 ), 0x65BA, 0x83b ) ); }
				if ( d2 == 2 ){ LIST.Add( new BeverageBuyInfo( typeof( Pitcher ), BeverageType.Cider, 9+x2, Utility.Random( 1,15 ), 0x65BA, 0x981 ) ); }
				if ( d3 == 2 ){ LIST.Add( new BeverageBuyInfo( typeof( Pitcher ), BeverageType.Liquor, 9+x3, Utility.Random( 1,15 ), 0x65BA, 0xB51 ) ); }
				if ( d4 == 2 ){ LIST.Add( new BeverageBuyInfo( typeof( Pitcher ), BeverageType.Wine, 9+x4, Utility.Random( 1,15 ), 0x65BA, 0xB64 ) ); }
			}

			if ( market == ItemSalesInfo.Market.Farmer || market == ItemSalesInfo.Market.Cook || market == ItemSalesInfo.Market.Tavern )
				{ if ( d5 == 2 ){ LIST.Add( new BeverageBuyInfo( typeof( Pitcher ), BeverageType.Milk, 9+x5, Utility.Random( 1,15 ), 0x65BA, 0x9A3 ) ); } }

			if ( market == ItemSalesInfo.Market.Tavern || market == ItemSalesInfo.Market.Provisions )
				{ if ( d6 == 2 ){ LIST.Add( new BeverageBuyInfo( typeof( Pitcher ), BeverageType.Water, 9+x6, Utility.Random( 1,15 ), 0x65BA, 0xB40 ) ); } }
		}

		public static bool Chemist( int val, ItemSalesInfo.Market mkt, ItemSalesInfo.Category cat )
		{
			bool chemist = false;

			if ( cat == ItemSalesInfo.Category.Reagent )
			{
				ItemSalesInfo.Market category = ItemSalesInfo.Market.None;

				ItemSalesInfo info = GetData( val );
				if ( info != null )
					category = info.iMarket;

				if ( mkt == ItemSalesInfo.Market.Alchemy )
				{
					if ( category == ItemSalesInfo.Market.Reg_AH ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Reg_AHD ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Reg_AHDW ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Reg_AHW ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Reg_MAHD ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Reg_MAHDW ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Reg_NA ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Reg_NAHW ){ chemist = true; }
				}
				else if ( mkt == ItemSalesInfo.Market.Necro )
				{
					if ( category == ItemSalesInfo.Market.Reg_NA ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Reg_NAHW ){ chemist = true; }
				}
				else if ( mkt == ItemSalesInfo.Market.Druid )
				{
					if ( category == ItemSalesInfo.Market.Reg_AHD ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Reg_AHDW ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Reg_MAHD ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Reg_MAHDW ){ chemist = true; }
				}
				else if ( mkt == ItemSalesInfo.Market.Witch )
				{
					if ( category == ItemSalesInfo.Market.Reg_AHDW ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Reg_AHW ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Reg_MAHDW ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Reg_NAHW ){ chemist = true; }
				}
				else if ( mkt == ItemSalesInfo.Market.Mage )
				{
					if ( category == ItemSalesInfo.Market.Reg_MAHD ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Reg_MAHDW ){ chemist = true; }
				}
				else if ( mkt == ItemSalesInfo.Market.Herbalist )
				{
					if ( category == ItemSalesInfo.Market.Reg_AH ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Reg_AHD ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Reg_AHDW ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Reg_AHW ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Reg_MAHD ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Reg_MAHDW ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Reg_NAHW ){ chemist = true; }
				}
			}
			else if ( cat == ItemSalesInfo.Category.Resource )
			{
				ItemSalesInfo.Market category = ItemSalesInfo.Market.None;

				ItemSalesInfo info = GetData( val );
				if ( info != null )
					category = info.iMarket;

				if ( mkt == ItemSalesInfo.Market.Alchemy )
				{
					if ( category == ItemSalesInfo.Market.Res_AH ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Res_NAHW ){ chemist = true; }
				}
				else if ( mkt == ItemSalesInfo.Market.Necro )
				{
					if ( category == ItemSalesInfo.Market.Res_NAHW ){ chemist = true; }
				}
				else if ( mkt == ItemSalesInfo.Market.Druid )
				{
					if ( category == ItemSalesInfo.Market.Res_DW ){ chemist = true; }
				}
				else if ( mkt == ItemSalesInfo.Market.Witch )
				{
					if ( category == ItemSalesInfo.Market.Res_DW ){ chemist = true; }
				}
				else if ( mkt == ItemSalesInfo.Market.Mage )
				{
					if ( category == ItemSalesInfo.Market.Res_MAHD ){ chemist = true; }
				}
				else if ( mkt == ItemSalesInfo.Market.Herbalist )
				{
					if ( category == ItemSalesInfo.Market.Res_AH ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Res_MAHD ){ chemist = true; }
					if ( category == ItemSalesInfo.Market.Res_NAHW ){ chemist = true; }
				}
			}

			return chemist;
		}

		public static int iRarity( int val ){ ItemSalesInfo info = GetData( val ); return ( info == null ? 0 : info.iRarity ); }
		public static bool iSells( int val ){ ItemSalesInfo info = GetData( val ); return ( info == null ? false : info.iSells ); }
		public static bool iBuys( int val ){ ItemSalesInfo info = GetData( val ); return ( info == null ? false : info.iBuys ); }
		public static ItemSalesInfo.Category iCategory( int val ){ ItemSalesInfo info = GetData( val ); return ( info == null ? ItemSalesInfo.Category.None : info.iCategory ); }
		public static ItemSalesInfo.Material iMaterial( int val ){ ItemSalesInfo info = GetData( val ); return ( info == null ? ItemSalesInfo.Material.None : info.iMaterial ); }
		public static ItemSalesInfo.Market iMarket( int val ){ ItemSalesInfo info = GetData( val ); return ( info == null ? ItemSalesInfo.Market.None : info.iMarket ); }
		public static ItemSalesInfo.World iWorld( int val ){ ItemSalesInfo info = GetData( val ); return ( info == null ? ItemSalesInfo.World.None : info.iWorld ); }
	}

	public class ItemSalesInfo
	{
		private Type m_ItemsType;
		private int m_Price;
		private int m_Qty;
		private int m_Rarity;
		private bool m_Sells;
		private bool m_Buys;
		private World m_World;
		private Category m_Category;
		private Material m_Material;
		private Market m_Market;

		public Type ItemsType{ get{ return m_ItemsType; } }
		public int iPrice { get{ return m_Price; } }
		public int iQty { get{ return m_Qty; } }
		public int iRarity { get{ return m_Rarity; } }
		public bool iSells { get{ return m_Sells; } }
		public bool iBuys { get{ return m_Buys; } }
		public World iWorld { get{ return m_World; } }
		public Category iCategory { get{ return m_Category; } }
		public Material iMaterial { get{ return m_Material; } }
		public Market iMarket { get{ return m_Market; } }

		public ItemSalesInfo( Type v_ItemType, int v_Price, int v_Qty, int v_Rarity, bool v_Sells, bool v_Buys, World v_World, Category v_Category, Material v_Material, Market v_Market )
		{
			m_ItemsType = v_ItemType;
			m_Price = v_Price;
			m_Qty = v_Qty;
			m_Rarity = v_Rarity;
			m_Sells = v_Sells;
			m_Buys = v_Buys;
			m_World = v_World;
			m_Category = v_Category;
			m_Material = v_Material;
			m_Market = v_Market;
		}

		public enum World
		{
			None = 0,
			Ambrosia = 1,
			Dread = 2,
			Elf = 3,
			Lodor = 4,
			Necro = 5,
			Orient = 6,
			Savage = 7,
			Sea = 8,
			Serpent = 9,
			Sosaria = 10,
			Umber = 11,
			Underworld = 12
		}

		public enum Material
		{
			None = 0,
			Bone = 1,
			Cloth = 2,
			Leather = 3,
			Metal = 4,
			Scales = 5,
			Wood = 6,
			All = 7
		}

		public enum Category
		{
			None = 0,
			Armor = 1,
			Artifact = 2,
			Book = 3,
			Christmas = 4,
			Halloween = 5,
			MonsterRace = 6,
			Pack = 7,
			Potion = 8,
			Rare = 9,
			Reagent = 10,
			Resource = 11,
			Rune = 12,
			Scroll = 13,
			Shield = 14,
			Supply = 15,
			Tavern = 16,
			Wand = 17,
			Weapon = 18,
			All = 19
		}

		public enum Market
		{
			None = 0,
			Alchemy = 1,
			Animals = 2,
			Art = 3,
			Assassin = 4,
			Banker = 5,
			Barber = 6,
			Bard = 7,
			Bow = 8,
			Butcher = 9,
			Carpenter = 10,
			Cartographer = 11,
			Cattle = 12,
			Cook = 13,
			Death = 14,
			Druid = 15,
			Elemental = 16,
			Evil = 17,
			Farmer = 18,
			Fighter = 19,
			Fisherman = 20,
			Glass = 21,
			Healer = 22,
			Herbalist = 23,
			Home = 24,
			Inn = 25,
			Jester = 26,
			Jeweler = 27,
			Leather = 28,
			Lumber = 29,
			Mage = 30,
			Mill = 31,
			Miner = 32,
			Monk = 33,
			Necro = 34,
			Painter = 35,
			Paladin = 36,
			Provisions = 37,
			Ranger = 38,
			Reg_AH = 39,
			Reg_AHD = 40,
			Reg_AHDW = 41,
			Reg_AHW = 42,
			Reg_MAHD = 43,
			Reg_MAHDW = 44,
			Reg_NA = 45,
			Reg_NAHW = 46,
			Res_AH = 47,
			Res_DW = 48,
			Res_MAHD = 49,
			Res_NAHW = 50,
			Sage = 51,
			Sailor = 52,
			Scribe = 53,
			Shoes = 54,
			Smith = 55,
			Stable = 56,
			Stone = 57,
			Supplies = 58,
			Tailor = 59,
			Tanner = 60,
			Tavern = 61,
			Thief = 62,
			Tinker = 63,
			Undertaker = 64,
			Wax = 65,
			Witch = 66,
			Wizard = 67,
			All = 68
		}

        public static ItemSalesInfo[] m_SellingInfo = new ItemSalesInfo[0];

        public static void LoadFromDisk()
        {
            string path = Path.Combine(Core.BaseDirectory, "Data/ItemSales.txt");
            if (!File.Exists(path)) return;

            List<ItemSalesInfo> temp = new List<ItemSalesInfo>();
            Dictionary<string, Type> typeCache = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

            foreach (string line in File.ReadAllLines(path))
            {
                // Skip headers, comments, and empty lines
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith("-"))
                    continue;

                string[] s = line.Split('|');
                if (s.Length < 10) continue;

                string typeName = s[0].Trim();
                Type t;
                if (!typeCache.TryGetValue(typeName, out t))
                {
                    t = ScriptCompiler.FindTypeByName(typeName);
                    typeCache[typeName] = t;
                }

                if (t == null) continue;

                try
                {
                    // Split the market column by comma to handle multiple markets
                    string[] markets = s[9].Split(',');

                    foreach (string marketName in markets)
                    {
                        temp.Add(new ItemSalesInfo(
                            t,
                            int.Parse(s[1].Trim()),
                            int.Parse(s[2].Trim()),
                            int.Parse(s[3].Trim()),
                            bool.Parse(s[4].Trim()),
                            bool.Parse(s[5].Trim()),
                            ParseEnum<World>(s[6]),
                            ParseEnum<Category>(s[7]),
                            ParseEnum<Material>(s[8]),
                            ParseEnum<Market>(marketName)
                        ));
                    }
                }
                catch { Console.WriteLine("Parsing error on ItemSales.txt"); }
            }
            m_SellingInfo = temp.ToArray();
            Console.WriteLine("ItemSales: {0} entries loaded.", m_SellingInfo.Length);
        }

        // Helper to handle "Market.Alchemy" vs "Alchemy"
        private static T ParseEnum<T>(string value) where T : struct
        {
            value = value.Trim();
            if (value.Contains("."))
                value = value.Split('.')[1]; // Turns "Market.Alchemy" into "Alchemy"

            T result;
            Enum.TryParse(value, true, out result);
            return result;
        }

        static ItemSalesInfo() { LoadFromDisk(); }

    }
}

namespace Server.Commands
{
	public class TestStock
	{
		private static Mobile m_Mobile;

		public static void Initialize()
		{
			CommandSystem.Register( "TestStock", AccessLevel.Administrator, new CommandEventHandler( TestStock_OnCommand ) );
		}

		public static void UpdateFile(string filename, string header)
		{
			string tempfile = Path.GetTempFileName();
			StreamWriter writer = null;
			StreamReader reader = null;
			using (writer = new StreamWriter(tempfile))
			using (reader = new StreamReader(filename))
			{
				writer.WriteLine(header);
				while (!reader.EndOfStream)
				{
					writer.WriteLine(reader.ReadLine());
				}
			}

			if (writer != null)
				writer.Dispose();

			if (reader != null)
				reader.Dispose();

			File.Copy(tempfile, filename, true);
			File.Delete(tempfile);
		}

		[Usage( "TestStock" )]
		[Description( "Add all of the items available in the store listing." )]
		public static void TestStock_OnCommand( CommandEventArgs e )
		{
			m_Mobile = e.Mobile;

			ItemSalesInfo[] list = ItemSalesInfo.m_SellingInfo;

			int entries = list.Length;
			int val = 0;
			Item oItem = null;
			string sPath = "Data/stock.txt";

			/// CREATE THE FILE IF IT DOES NOT EXIST ///
			StreamWriter w = null; 
			try
			{
				using (w = File.AppendText( sPath ) ){}
			}
			catch(Exception)
			{
			}
			finally
			{
				if (w != null)
					w.Dispose();
			}

			while ( entries > 0 )
			{
				Type itemType = list[val].ItemsType;

				if ( itemType != null )
				{
					UpdateFile(sPath, "" + itemType + "");
					oItem = (Item)Activator.CreateInstance( itemType );
					int qty = ItemInformation.GetQty( val, true );
					int price = ItemInformation.GetSellPrice( val, true );
					UpdateFile(sPath, "" + oItem.Name + "");
					oItem.Delete();
				}
				entries--;
				val++;
			}
			m_Mobile.SendMessage( "Finished test. See stock.txt for details." );
		}
	}
}