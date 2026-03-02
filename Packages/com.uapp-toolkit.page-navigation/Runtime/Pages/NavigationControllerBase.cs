using System.Collections.Generic;
using UAppToolKit.PageNavigation.Loading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UAppToolKit.PageNavigation.Pages
{
    public abstract class NavigationControllerBase : MonoBehaviour
    {
        [Header("Page links")]
        public PageBaseLink StartPageLink;

        [Header("Page loading screen")]
        public LoadingScreen LoadingScreenPrefab;
        [HideInInspector] 
        public LoadingScreen LoadingScreen;

        public static int CurrentPageIndex;
        public static readonly List<IPageBaseLink> Pages = new List<IPageBaseLink>();
        public bool BackNavigationByEscape = true;

        public abstract IEnumerator<float> Initialize();
        public abstract PageBase ActivePage();
        public abstract void NavigateTo(IPageBaseLink nextPage, object newPageArgs);
        public abstract bool CanGoBack();
        public abstract void GoBack(object prevPageArgs);
        public abstract void GoToStartPage();
        public abstract void RestartLastPage(object restartPageArgs);

        public virtual void NavigateTo(IPageBaseLink nextPage)
        {
            NavigateTo(nextPage, null);
        }

        public virtual void GoBack()
        {
            GoBack(null);
        }

        public virtual void RestartLastPage()
        {
            RestartLastPage(null);
        }

        public virtual void RunStartPage()
        {
            var activePage = ActivePage();
            if (activePage == null) 
            {
                Debug.LogError("Active page is null.", this);
                return;
            }

            if (activePage.PageBaseLink != StartPageLink)
            {
                Debug.LogError("Active page is not start page.", this);
                return;
            }

            activePage.OnNavigatedToCompleted();
        }

        protected virtual void QuitApplication()
        {
            Debug.Log("quit");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }

        protected void InitStartPage(Navigator startPage)
        {
            startPage.NavigationController = this;
            startPage.OnNavigatedTo(null);
        }

        protected string GetSceneName(PageBaseLink pageBaseLink)
        {
            return pageBaseLink.SceneName;
        }
    }
}