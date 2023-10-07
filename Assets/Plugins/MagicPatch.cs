using HarmonyLib;

[HarmonyPatch(typeof(System.Xml.XmlElement), nameof(System.Xml.XmlElement.InnerText), MethodType.Getter)]
class MagicPatch
{
    static void Prefix()
    {
        
    }

    static void Postfix(ref string __result)
    {
        if (__result == "20220516")
            __result = "20250516";    
    }
}
