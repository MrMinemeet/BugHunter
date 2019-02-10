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
        public int CppAmmoAmout { get; set; }
        public int CAmmoAmount { get; set; }
        public int JavaAmmoAmount { get; set; }
        public int CsharpAmmoAmount { get; set; }
        public int MaschinenspracheAmmoAmount { get; set; }

        public Weapon()
        {
            // Delays
            CppDelayMs = 275;
            CDelayMs = 350;
            JavaDelayMs = 200;
            CsharpDelayMs = 175;
            MaschinenspracheDelayMs = 1000;

            // Schaden
            CppDamage = 20;
            CDamage = 25;
            JavaDamage = 15;
            CsharpDamage = 15;
            MaschinenspracheDamage = 100;

            // Maximale Munition
            CppAmmoAmout = 40;
            CAmmoAmount = 35;
            JavaAmmoAmount = 60;
            CsharpAmmoAmount = 70;
            MaschinenspracheAmmoAmount = 30;
        }


        public const int GeneralMaxAmmo = 200;

        public enum WeaponTypes : byte { cpp, java, c, csharp, maschinensprache }

        public int getDamageAktWeapon(WeaponTypes AktWeapon)
        {
            switch (AktWeapon)
            {
                case WeaponTypes.cpp:
                    return CppDamage;
                case WeaponTypes.c:
                    return CDamage;
                case WeaponTypes.java:
                    return JavaDamage;
                case WeaponTypes.csharp:
                    return CsharpDamage;
                case WeaponTypes.maschinensprache:
                    return MaschinenspracheDamage;
                default:
                    return 0;
            }
        }

        public int getDelayAktWeapon(WeaponTypes AktWeapon)
        {
            switch (AktWeapon)
            {
                case WeaponTypes.cpp:
                    return CppDelayMs;
                case WeaponTypes.c:
                    return CDelayMs;
                case WeaponTypes.java:
                    return JavaDelayMs;
                case WeaponTypes.csharp:
                    return CsharpDelayMs;
                case WeaponTypes.maschinensprache:
                    return MaschinenspracheDelayMs;
                default:
                    return 0;
            }
        }
        public int getMaxAmmoAmountAktWeapon(WeaponTypes AktWeapon)
        {
            switch (AktWeapon)
            {
                case WeaponTypes.cpp:
                    return CppAmmoAmout;
                case WeaponTypes.c:
                    return CAmmoAmount;
                case WeaponTypes.java:
                    return JavaAmmoAmount;
                case WeaponTypes.csharp:
                    return CsharpAmmoAmount;
                case WeaponTypes.maschinensprache:
                    return MaschinenspracheAmmoAmount;
                default:
                    return 0;
            }
        }
    }
}
