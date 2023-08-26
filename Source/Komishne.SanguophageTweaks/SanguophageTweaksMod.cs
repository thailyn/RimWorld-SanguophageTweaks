using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Komishne.SanguophageTweaks
{
    public class SanguophageTweaksSettings : ModSettings
    {
        protected static bool enableSkipVictimBodySizeEffectOnBiterGainsDefault = true;
        public static bool enableSkipVictimBodySizeEffectOnBiterGains = enableSkipVictimBodySizeEffectOnBiterGainsDefault;

        public override void ExposeData()
        {
            Scribe_Values.Look(
                ref enableSkipVictimBodySizeEffectOnBiterGains,
                /*label=*/"enableSkipVictimBodySizeEffectOnBiterGains",
                /*defaultValue=*/enableSkipVictimBodySizeEffectOnBiterGainsDefault,
                /*forceSave=*/true);
            base.ExposeData();
        }
    }

    public class SanguophageTweaksMod : Mod
    {
        public SanguophageTweaksSettings Settings;

        public SanguophageTweaksMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<SanguophageTweaksSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            
            listingStandard.CheckboxLabeled(
                "KOM.SanguophageTweaks.Settings.EnableSkipVictimBodySizeEffectOnBiterGains.Label".Translate(),
                ref SanguophageTweaksSettings.enableSkipVictimBodySizeEffectOnBiterGains,
                "KOM.SanguophageTweaks.Settings.EnableSkipVictimBodySizeEffectOnBiterGains.Tooltip".Translate());
            
            //listingStandard.Label("exampleFloatExplanation");
            //Settings.exampleFloat = listingStandard.Slider(Settings.exampleFloat, 100f, 300f);
            
            listingStandard.End();

            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "KOM.SanguophageTweaks.Settings.Category".Translate();
        }
    }
}
