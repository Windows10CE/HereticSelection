using System;
using System.Linq;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using MonoMod.RuntimeDetour;
using HarmonyLib;

namespace HereticSelection
{
    [BepInPlugin(ModGUID, ModName, ModVer)]
    public class HereticSelectionPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "com.Windows10CE.HereticSelection";
        public const string ModName = "HereticSelection";
        public const string ModVer = "1.0.0";

        new internal static ManualLogSource Logger;

        public void Awake()
        {
            HereticSelectionPlugin.Logger = base.Logger;

            new Hook(AccessTools.Method(typeof(ContentManager), nameof(ContentManager.SetContentPacks)), AccessTools.Method(typeof(HereticSelectionPlugin), nameof(FixHereticDef)));
            new Hook(AccessTools.Method(typeof(CharacterMaster), nameof(CharacterMaster.SpawnBody)), AccessTools.Method(typeof(HereticSelectionPlugin), nameof(CharacterMasterBegin)));
        }

        internal static void FixHereticDef(Action<List<ContentPack>> orig, List<ContentPack> packs)
        {
            orig(packs);
            SurvivorDef bird = ContentManager.survivorDefs.First(x => x.displayNameToken == "HERETIC_BODY_NAME");
            bird.hidden = false;
            bird.displayPrefab = Resources.Load<GameObject>("Prefabs/CharacterBodies/HereticBody");
        }

        internal static CharacterBody CharacterMasterBegin(Func<CharacterMaster, Vector3, Quaternion, CharacterBody> orig, CharacterMaster self, Vector3 pos, Quaternion rot)
        {
            var body = orig(self, pos, rot);
            if (!NetworkServer.active || !self || !self.inventory || !Run.instance || (Run.instance.stageClearCount != 0 && !RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.randomSurvivorOnRespawnArtifactDef)))
                return body;
            if (body.baseNameToken != "HERETIC_BODY_NAME")
                return body;

            ItemDef[] requiredItems = new ItemDef[] 
            { 
                RoR2Content.Items.LunarPrimaryReplacement,
                RoR2Content.Items.LunarSecondaryReplacement,
                RoR2Content.Items.LunarSpecialReplacement,
                RoR2Content.Items.LunarUtilityReplacement 
            };

            foreach (var item in requiredItems)
                if (self.inventory.GetItemCount(item) < 1)
                    self.inventory.GiveItem(item);

            return body;
        }
    }
}
