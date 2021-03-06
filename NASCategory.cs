using System.Collections.Generic;
using KSP.UI.Screens;
using KSP.Localization;
using UnityEngine;

namespace AntiSubmarineWeapon
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class NASCategory : MonoBehaviour
    {
        private static readonly List<AvailablePart> availableParts = new List<AvailablePart>();

        void Awake()
        {
            GameEvents.onGUIEditorToolbarReady.Add(NASCategoryFunc);
        }

        void NASCategoryFunc()
        {
            const string customCategoryName = "NAS";
            availableParts.Clear();
            availableParts.AddRange(PartLoader.LoadedPartsList.NASParts());
            Texture2D iconTex = GameDatabase.Instance.GetTexture("NAS/Plugins/NASIcon", false);
            RUI.Icons.Selectable.Icon icon = new RUI.Icons.Selectable.Icon("NAS", iconTex, iconTex, false);
            PartCategorizer.Category filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == "Filter by Function");

            if (filter == null)
                Debug.Log("filter is null");
            else
                PartCategorizer.AddCustomSubcategoryFilter(filter, customCategoryName, Localizer.Format("#autoLOC_NAS_Category"), icon, p => availableParts.Contains(p));
        }
    }
}
