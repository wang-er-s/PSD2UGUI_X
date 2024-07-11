using System.Xml;
using HarmonyLib;

[HarmonyPatch(typeof(XmlElement), nameof(XmlElement.InnerText), MethodType.Getter)]
internal class MagicPatch
{
    private static void Prefix()
    {
    }

    private static void Postfix(ref string __result)
    {
        if (__result == "20220516")
            __result = "20250516";
    }
}