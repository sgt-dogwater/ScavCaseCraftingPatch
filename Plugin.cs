using SPT.Reflection.Patching;
using BepInEx;
using BepInEx.Logging;

using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using EFT.Hideout;
using EFT.InventoryLogic;
using Comfort.Common;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ScavCaseCraftingPatch
{
  [BepInPlugin("sgt_dogwater.ScavCaseCraftingPatch", "SgtDogwater-ScavCaseCraftingPatch", "0.0.0")]
  [BepInDependency("com.SPT.core", "3.10.5")]
  public class ScavCaseCraftingPatchPlugin : BaseUnityPlugin
  {
    private void Awake()
    {
      new ScavCaseViewPatch().Enable();
      new Class1785Patch().Enable();
    }
  }

  public class Class1785Patch : ModulePatch
  {
    private static FieldInfo _productionsField;

    protected override MethodBase GetTargetMethod()
    {
      return AccessTools.Method(typeof(ScavCaseView.Class1785), nameof(ScavCaseView.Class1785.method_1));
    }

    public static void Fixed_method_1(ScavCaseView.Class1785 __instance, ItemRequirement requirement, HideoutProductionRequirementView view)
    {
      view.Show(__instance.itemUiContext, __instance.inventoryController, requirement, __instance.scavCaseView_0.Scheme, __instance.allItems, __instance.scavCaseView_0.Producer.ProducingItems.Any<KeyValuePair<string, GClass2156>>(kv => kv.Key == __instance.scavCaseView_0.Scheme._id));
      __instance.scavCaseView_0.Boolean_0 = __instance.scavCaseView_0.Boolean_0 & view.IsFulfilled;
    }

    [PatchPrefix]
    public static bool PatchPrefix(ScavCaseView.Class1785 __instance, ItemRequirement requirement, HideoutProductionRequirementView view)
    {
      Fixed_method_1(__instance, requirement, view);
      return false;
    }
  }

  public class ScavCaseViewPatch : ModulePatch
  {
    private static FieldInfo _productionsField;

    protected override MethodBase GetTargetMethod()
    {
      _productionsField = AccessTools.Field(typeof(ScavCaseView), "gclass3468_0");
      return AccessTools.Method(typeof(ScavCaseView), nameof(ScavCaseView.UpdateView));
    }

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(ProduceViewBase<GClass2154, GClass2159>), nameof(ProduceViewBase<GClass2154, GClass2159>.UpdateView))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    static string BaseUpdateViewDummy(ScavCaseView instance) => null;

    public static void Fixed_UpdateView(ScavCaseView __instance)
    {
      if (__instance == null)
        return;

      BaseUpdateViewDummy(__instance);

      __instance.method_2();

      GClass3468<ItemRequirement, HideoutProductionRequirementView> requirementsToView = (GClass3468<ItemRequirement, HideoutProductionRequirementView>)_productionsField.GetValue(__instance);
      if (requirementsToView == null)
      {
        return;
      }

      List<Item> list = Singleton<HideoutClass>.Instance.GetAvailableItemsByFilter<Item>(null, null).ToList<Item>();
      bool flag = true;
      foreach (KeyValuePair<ItemRequirement, HideoutProductionRequirementView> keyValuePair in requirementsToView)
      {
        ItemRequirement itemRequirement;
        HideoutProductionRequirementView hideoutProductionRequirementView;
        keyValuePair.Deconstruct(out itemRequirement, out hideoutProductionRequirementView);
        ItemRequirement itemRequirement2 = itemRequirement;
        HideoutProductionRequirementView hideoutProductionRequirementView2 = hideoutProductionRequirementView;
        hideoutProductionRequirementView2.Show(__instance.ItemUiContext, __instance.InventoryController, itemRequirement2, __instance.Scheme, list, __instance.Producer.ProducingItems.Any<KeyValuePair<string, GClass2156>>(kv => kv.Key == __instance.Scheme._id));
        flag &= hideoutProductionRequirementView2.IsFulfilled;
      }
      __instance.Boolean_0 = flag;
    }

    [PatchPrefix]
    public static bool PatchPrefix(ScavCaseView __instance)
    {
      Fixed_UpdateView(__instance);
      return false;
    }
  }
}