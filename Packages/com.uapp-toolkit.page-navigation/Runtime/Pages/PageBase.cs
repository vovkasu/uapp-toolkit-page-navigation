#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace UAppToolKit.PageNavigation.Pages
{
    public abstract class PageBase : Navigator
    {
        [HideInInspector]
        public PageBaseLink PageBaseLink;

#if UNITY_EDITOR
        [MenuItem("CONTEXT/PageBase/Create page link")]
        public static void CreatePageLink(MenuCommand command)
        {
            var pageBase = (PageBase)command.context;
            if (pageBase.PageBaseLink != null)
            {
                EditorGUIUtility.PingObject(pageBase.PageBaseLink);
                return;
            }

            var scene = pageBase.gameObject.scene;
            var scenePath = scene.path;
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

            var sceneName = sceneAsset.name;
            string pageBaseName = pageBase.name;

            var directoryName = System.IO.Path.GetDirectoryName(scenePath);
            var pageLinkPath = System.IO.Path.Combine(directoryName, $"{pageBaseName}_pageLink.asset");

            var pageBaseLink = ScriptableObject.CreateInstance<PageBaseLink>();

            pageBaseLink.SceneAsset = sceneAsset;
            pageBaseLink.SceneName  = sceneName;
            pageBaseLink.ScenePath = scenePath;
            pageBaseLink.PageName = pageBaseName;
            pageBase.PageBaseLink = pageBaseLink;

            AssetDatabase.CreateAsset(pageBaseLink, pageLinkPath);
            EditorUtility.SetDirty(pageBase);
        }
#endif
    }
}