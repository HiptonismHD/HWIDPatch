using System;
using System.Reflection;
using System.Text;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(HWIDPatch.HwidPatchMod), "HWIDPatch", "1.0.2", "knah")]
[assembly: MelonGame]

namespace HWIDPatch
{
    public class HwidPatchMod : MelonMod
    {
        private static string generatedHwid;

        public override void OnApplicationStart()
        {
            try
            {
                var category = MelonPreferences.CreateCategory("HWIDPatch", "HWID Patch");
                var hwidEntry = category.CreateEntry("HWID", "", is_hidden: true);

                generatedHwid = hwidEntry.Value;

                if (string.IsNullOrEmpty(generatedHwid) || generatedHwid.Length != SystemInfo.deviceUniqueIdentifier.Length)
                {
                    var random = new System.Random(Environment.TickCount);
                    var bytes = new byte[SystemInfo.deviceUniqueIdentifier.Length / 2];
                    random.NextBytes(bytes);

                    var sb = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                        sb.Append(bytes[i].ToString("x2"));

                    generatedHwid = sb.ToString();
                    hwidEntry.Value = generatedHwid;
                    category.SaveToFile(false);
                    MelonLogger.Msg("Generated and saved new HWID.");
                }

                var harmony = HarmonyInstance.Create("hwidpatch.mod");
                var method = typeof(SystemInfo).GetProperty("deviceUniqueIdentifier").GetGetMethod();
                var prefix = typeof(HwidPatchMod).GetMethod("DeviceIdPrefix", BindingFlags.Static | BindingFlags.NonPublic);
                harmony.Patch(method, new HarmonyMethod(prefix));

                MelonLogger.Msg("HWID patch applied.");
                MelonLogger.Msg("Fake HWID: " + SystemInfo.deviceUniqueIdentifier);
            }
            catch (Exception ex)
            {
                MelonLogger.Error("Failed to patch HWID: " + ex);
            }
        }

        private static bool DeviceIdPrefix(ref string __result)
        {
            __result = generatedHwid;
            return false; // skip original
        }
    }
}
