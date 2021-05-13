using System.Linq;
using BepInEx;
using BepInEx.Logging;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using HarmonyLib;
using RoR2.ContentManagement;

namespace HereticSelection
{
    [BepInPlugin(ModGUID, ModName, ModVer)]
    [BepInDependency(ModCommon.ModCommonPlugin.ModGUID, BepInDependency.DependencyFlags.HardDependency)]
    [ModCommon.NetworkModlistInclude]
    [HarmonyPatch]
    public class HereticSelectionPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "com.Windows10CE.HereticSelection";
        public const string ModName = "HereticSelection";
        public const string ModVer = "2.0.1";

        new internal static ManualLogSource Logger;

        public void Awake()
        {
            HereticSelectionPlugin.Logger = base.Logger;

            Harmony.CreateAndPatchAll(typeof(HereticSelectionPlugin).Assembly, ModGUID);
            ContentManager.onContentPacksAssigned += FixHereticDef;
        }

        internal static void FixHereticDef(HG.ReadOnlyArray<ReadOnlyContentPack> packs)
        {
            SurvivorDef bird = ContentManager.survivorDefs.First(x => x.displayNameToken == "HERETIC_BODY_NAME");
            bird.hidden = false;
            bird.displayPrefab = Resources.Load<GameObject>("Prefabs/CharacterBodies/HereticBody").transform.Find("ModelBase/mdlHeretic").gameObject;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterMaster), nameof(CharacterMaster.SpawnBody))]
        internal static void CharacterMasterBegin(CharacterMaster __instance, CharacterBody __result)
        {
            if (!NetworkServer.active || !__instance || !__instance.inventory || !Run.instance || (Run.instance.stageClearCount != 0 && !RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.randomSurvivorOnRespawnArtifactDef)))
                return;
            if (__result.baseNameToken != "HERETIC_BODY_NAME")
                return;

            ItemDef[] requiredItems = new ItemDef[] 
            { 
                RoR2Content.Items.LunarPrimaryReplacement,
                RoR2Content.Items.LunarSecondaryReplacement,
                RoR2Content.Items.LunarSpecialReplacement,
                RoR2Content.Items.LunarUtilityReplacement 
            };

            foreach (var item in requiredItems)
                if (__instance.inventory.GetItemCount(item) < 1)
                    __instance.inventory.GiveItem(item);
        }
    }
}
