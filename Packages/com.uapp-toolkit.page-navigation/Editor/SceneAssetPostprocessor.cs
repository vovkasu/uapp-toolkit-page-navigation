using System.Collections.Generic;
using System.Linq;
using UAppToolKit.PageNavigation.Pages;
using UnityEditor;

namespace UAppToolKit.PageNavigation.Editor
{
    public class SceneAssetPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths,
            bool didDomainReload)
        {
            var assetsList = new List<string>(importedAssets);
            assetsList.AddRange(deletedAssets);
            assetsList.AddRange(movedAssets);
            var scenes = assetsList.Where(_ => System.IO.Path.GetExtension(_).ToLower() == ".unity").ToList();
            if (scenes.Count > 0)
            {
                var objects = AssetDatabase.FindAssets($"t:{nameof(SceneNameDependentScriptableObject)}")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<SceneNameDependentScriptableObject>)
                    .ToList();
                foreach (var obj in objects)
                {
                    obj.AnySceneNameChanged();
                }
            }
        }
    }
}