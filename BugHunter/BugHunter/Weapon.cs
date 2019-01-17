using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugHunter
{
    public class Weapon
    {
        // Delays für Waffen
        public int CppDelayMs = 275;
        public int CDelayMs = 350;
        public int JavaDelayMs = 200;
        public int CsharpDelayMs = 175;
        public int MaschinenspracheDelayMs = 1000;

        // Schaden für waffen
        public int CppDamage = 20;
        public int CDamage = 25;
        public int JavaDamage = 15;
        public int CsharpDamage = 15;
        public int MaschinenspracheDamage = 100;

        // Max Munition für Waffe
        public int CppAmmoAmout = 40;
        public int CAmmoAmount = 35;
        public int JavaAmmoAmount = 60;
        public int CsharpAmmoAmount = 70;
        public int MaschinenspracheAmmoAmount = 30;

        public enum WeaponTypes : byte { cpp, java, c, csharp, maschinensprache }

        public int getDelayAktWeapon(WeaponTypes aktWeapon)
        {
            switch (aktWeapon)
            {
                case WeaponTypes.cpp:
                    return CppDelayMs;
                case WeaponTypes.c:
                    return CDelayMs;
                case WeaponTypes.java:
                    return JavaDelayMs;
                case WeaponTypes.maschinensprache:
                    return MaschinenspracheDelayMs;
                case WeaponTypes.csharp:
                    return CsharpDelayMs;
            }

            return -1;
        }

        public int getDamageAktWeapon(WeaponTypes aktWeapon)
        {
            switch (aktWeapon)
            {
                case WeaponTypes.cpp:
                    return CppDamage;
                case WeaponTypes.c:
                    return CDamage;
                case WeaponTypes.java:
                    return JavaDamage;
                case WeaponTypes.maschinensprache:
                    return MaschinenspracheDamage;
                case WeaponTypes.csharp:
                    return CsharpDamage;
            }

            return -1;
        }
        public int getMaxAmmoAmountAktWeapon(WeaponTypes aktWeapon)
        {
            switch (aktWeapon)
            {
                case WeaponTypes.cpp:
                    return CppAmmoAmout;
                case WeaponTypes.c:
                    return CAmmoAmount;
                case WeaponTypes.java:
                    return JavaAmmoAmount;
                case WeaponTypes.maschinensprache:
                    return MaschinenspracheAmmoAmount;
                case WeaponTypes.csharp:
                    return CsharpAmmoAmount;
            }

            return -1;
        }

        public int getMaxAmmoAmountSpecificWeapon(WeaponTypes weaponType)
        {
            switch (weaponType)
            {
                case WeaponTypes.cpp:
                    return CppAmmoAmout;
                case WeaponTypes.c:
                    return CAmmoAmount;
                case WeaponTypes.java:
                    return JavaAmmoAmount;
                case WeaponTypes.maschinensprache:
                    return MaschinenspracheAmmoAmount;
                case WeaponTypes.csharp:
                    return CsharpAmmoAmount;
            }

            return -1;
        }
    }
}
