﻿using BepInEx;
using BepInEx.Logging;
using Logger = BepInEx.Logger;
using Harmony;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using Shared;
using UnityEngine;

namespace Sideloader
{
    public static class Hooks
    {
        public static void InstallHooks()
        {
            var harmony = HarmonyInstance.Create("com.bepis.bepinex.sideloader");
            harmony.PatchAll(typeof(Hooks));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AssetBundleCheck), nameof(AssetBundleCheck.IsFile))]
        public static void IsFileHook(string assetBundleName, ref bool __result)
        {
            if (!__result)
            {
                if (BundleManager.Bundles.ContainsKey(assetBundleName))
                    __result = true;
                if (Sideloader.IsPngFolderOnly(assetBundleName))
                    __result = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AssetBundleData))]
        [HarmonyPatch(nameof(AssetBundleData.isFile), PropertyMethod.Getter)]
        public static void IsFileHook2(ref bool __result, AssetBundleData __instance)
        {
            if (!__result)
            {
                if (BundleManager.Bundles.ContainsKey(__instance.bundle))
                    __result = true;
                if (Sideloader.IsPngFolderOnly(__instance.bundle))
                    __result = true;
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Studio.Info), "LoadExcelData")]
        public static void LoadExcelDataPostfix(string _bundlePath, string _fileName, ref ExcelData __result)
        {
            var studioList = ResourceRedirector.ListLoader.ExternalStudioDataList.Where(x => x.AssetBundleName == _bundlePath && x.FileNameWithoutExtension == _fileName).ToList();

            if (studioList.Count() > 0)
            {
                bool didHeader = false;
                if (__result == null) //Create a new ExcelData
                    __result = (ExcelData)ScriptableObject.CreateInstance(typeof(ExcelData));
                else //Adding to an existing ExcelData
                    didHeader = true;

                foreach (var studioListData in studioList)
                {
                    if (!didHeader) //Write the header. I think it's pointless and will be skipped when the ExcelData is read, but it's expected to be there.
                    {
                        var param = new ExcelData.Param();
                        param.list = studioListData.Header;
                        __result.list.Add(param);
                        didHeader = true;
                    }
                    foreach (var entry in studioListData.Entries)
                    {
                        var param = new ExcelData.Param();
                        param.list = entry;
                        __result.list.Add(param);
                    }
                }
            }
        }
    }
}
