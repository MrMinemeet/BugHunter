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
        public const int JavaDelayMs = 175;
        public const int MaschinenspracheDelayMs = 1000;

        // Schaden für waffen
        public const int CppDamage = 20;
        public const int CDamage = 25;
        public const int JavaDamage = 15;
        public const int MaschinenspracheDamage = 100;


        public enum WeaponTypes : byte { cpp, java, c, maschinensprache }

        public static int getDelayforAktWeapon(WeaponTypes aktWeapon)
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
            }

            return -1;
        }
        public static int getDamageforAWeapon(WeaponTypes aktWeapon)
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
            }

            return -1;
        }
    }
}
