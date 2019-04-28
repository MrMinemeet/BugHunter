using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;

// This File contains all SoundEffects

namespace BugHunter
{
    public class SoundFX
    {
        public SoundEffect ScoreSound;

        public SoundEffectInstance HintergrundMusikEffect;
        public SoundEffect HintergrundMusik;

        public SoundEffect[] Schuesse = new SoundEffect[6];

        // <== Spieler ==>
        public List<SoundEffect> MaleDeathSound = new List<SoundEffect>();
        public List<SoundEffect> MaleDamageSound = new List<SoundEffect>();


        // <== GEGNER ==>
        public List<SoundEffect> EnemieDeathSound = new List<SoundEffect>();
    }
}