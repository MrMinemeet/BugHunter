using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugHunter
{
    class Weapon
    {
        // Delays für Waffen
        public const int CppDelayMs = 275;
        public const int CDelayMs = 350;
        public const int JavaDelayMs = 200;
        public const int CsharpDelayMs = 175;
        public const int MaschinenspracheDelayMs = 1000;

        // Schaden für waffen
        public const int CppDamage = 20;
        public const int CDamage = 25;
        public const int JavaDamage = 15;
        public const int CsharpDamage = 15;
        public const int MaschinenspracheDamage = 100;

        // Max Munition für Waffe
        public const int CppAmmoAmout = 30;
        public const int CAmmoAmount = 28;
        public const int JavaAmmoAmount = 50;
        public const int CsharpAmmoAmount = 60;
        public const int MaschinenspracheAmmoAmount = 20;

        public enum WeaponTypes : byte { cpp, java, c, csharp, maschinensprache }

        public static int getDelayAktWeapon(WeaponTypes aktWeapon)
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
        public static int getDamageAktWeapon(WeaponTypes aktWeapon)
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
        public static int getMaxAmmoAmountAktWeapon(WeaponTypes aktWeapon)
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
        public static int getMaxAmmoAmountSpecificWeapon(WeaponTypes weaponType)
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
