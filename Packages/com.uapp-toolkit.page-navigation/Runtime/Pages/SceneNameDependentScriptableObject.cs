using UnityEngine;

namespace UAppToolKit.PageNavigation.Pages
{
    public abstract class SceneNameDependentScriptableObject : ScriptableObject
    {
#if UNITY_EDITOR
        public abstract void AnySceneNameChanged();
#endif
    }
}