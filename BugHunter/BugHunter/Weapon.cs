namespace BugHunter
{
    public class Weapon
    {
        // Delays für Waffen
        public int CppDelayMs { get; set; }
        public int CDelayMs { get; set; }
        public int JavaDelayMs { get; set; }
        public int CsharpDelayMs { get; set; }
        public int MaschinenspracheDelayMs { get; set; }

        // Schaden für waffen
        public int CppDamage { get; set; }
        public int CDamage { get; set; }
        public int JavaDamage { get; set; }
        public int CsharpDamage { get; set; }
        public int MaschinenspracheDamage { get; set; }

        // Max Munition für Waffe
        public int CppAmmoAmount { get; set; }
        public int CAmmoAmount { get; set; }
        public int JavaAmmoAmount { get; set; }
        public int CsharpAmmoAmount { get; set; }
        public int MaschinenspracheAmmoAmount { get; set; }


        public const int GeneralMaxAmmo = 200;

        public enum WeaponTypes : byte { cpp, java, c, csharp, maschinensprache }

        public Weapon()
        {
            // Delays für Waffen
            CppDelayMs = 275;
            CDelayMs = 350;
            JavaDelayMs = 200;
            CsharpDelayMs = 175;
            MaschinenspracheDelayMs = 1000;

            // Schaden für waffen
            CppDamage = 20;
            CDamage = 25;
            JavaDamage = 15;
            CsharpDamage = 15;
            MaschinenspracheDamage = 100;

            // Max Munition für Waffe
            CppAmmoAmount = 40;
            CAmmoAmount = 35;
            JavaAmmoAmount = 60;
            CsharpAmmoAmount = 70;
            MaschinenspracheAmmoAmount = 30;
        }
            
        public int GetDelayAktWeapon(WeaponTypes aktWeapon)
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

            return 0;
        }

        public int GetDamageAktWeapon(WeaponTypes aktWeapon)
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

            return 0;
        }
        public int GetMaxAmmoAmountAktWeapon(WeaponTypes aktWeapon)
        {
            switch (aktWeapon)
            {
                case WeaponTypes.cpp:
                    return CppAmmoAmount;
                case WeaponTypes.c:
                    return CAmmoAmount;
                case WeaponTypes.java:
                    return JavaAmmoAmount;
                case WeaponTypes.maschinensprache:
                    return MaschinenspracheAmmoAmount;
                case WeaponTypes.csharp:
                    return CsharpAmmoAmount;
            }

            return 0;
        }
    }
}
