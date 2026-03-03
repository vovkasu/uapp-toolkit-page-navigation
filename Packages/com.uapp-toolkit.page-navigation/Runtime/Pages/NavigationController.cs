using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UAppToolKit.PageNavigation.Loading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace UAppToolKit.PageNavigation.Pages
{
    public class NavigationController : NavigationControllerBase
    {
        public bool ShowSplashScreen = true;
        protected PageBase CurrentPage;
        protected bool IsNavigationRan;
        protected DateTime _startPageNavigationTime;
        public TimeSpan MinTimePageLoading = TimeSpan.FromSeconds(1);

        public event Action OnStartPageLoaded;

        private Scene _applicationScene;

        private bool isSplashScreenReady;
        private bool isStartPageReady;

        private event Action _onLoadedAction;

        public override IEnumerator<float> Initialize(Scene applicationScene)
        {
            _applicationScene = applicationScene;
            return Initialize();
        }

        public override IEnumerator<float> Initialize()
        {
            var startPageSceneName = GetSceneName(StartPageLink);

            var scenesForUnload = new List<Scene>();
            Scene? startPageScene = null;
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene == _applicationScene)
                {
                    continue;
                }

                if (scene.name == startPageSceneName)
                {
                    startPageScene = scene;
                    continue;
                }
                scenesForUnload.Add(scene);
            }

            float progressSize = scenesForUnload.Count + 1f;
            float progress = 0f;

            foreach (var scene in scenesForUnload)
            {
                var unloadSceneAsync = SceneManager.UnloadSceneAsync(scene.name);
                while (!unloadSceneAsync.isDone)
                {
                    yield return (progress + unloadSceneAsync.progress) / progressSize;
                }

                progress += 1f;
            }

            if (startPageScene.HasValue)
            {
                SceneManager.SetActiveScene(startPageScene.Value);
                CurrentPage = FindPageBase(startPageScene.Value, StartPageLink);
                InitStartPage(ActivePage());
                OnStartPageActive(OnStartPageLoaded);
            }
            else
            {
                var loadSceneAsync = LoadSceneAsync(StartPageLink, _ =>
                {
                    OnPageLoaded(_, null, InitStartPage, true);
                    OnStartPageActive(OnStartPageLoaded);
                });

                while (loadSceneAsync.MoveNext())
                {
                    yield return (progress + loadSceneAsync.Current) / progressSize;
                }
            }
            Pages.Add(StartPageLink);
            IsNavigationRan = false;

            InstantiateLoadingScreen(LoadingScreenPrefab);
            yield return 1f;
        }

        public override PageBase ActivePage()
        {
            return CurrentPage;
        }

        public override void NavigateTo(IPageBaseLink nextPage, object newPageArgs)
        {
            if (IsNavigationRan) return;
            CurrentPageIndex++;

            GoToPage((PageBaseLink) nextPage, newPageArgs);

            Pages.Add(nextPage);
        }

        public override bool CanGoBack()
        {
            return CurrentPage.GetPopUpsCount() > 0 || CurrentPageIndex > 0;
        }

        public override void GoBack(object prevPageArgs)
        {
            if (IsNavigationRan) return;
            if (!CanGoBack())
            {
                QuitApplication();
                return;
            }

            if (CurrentPage.GetPopUpsCount() > 0)
            {
                var popUp = CurrentPage.GetLastPopUp();
                CurrentPage.ClosePopUp(popUp);
                return;
            }

            var prevPageLink = Pages[CurrentPageIndex - 1];

            GoToPage((PageBaseLink)prevPageLink, prevPageArgs);

            Pages.Remove(Pages.Last());
            CurrentPageIndex--;
        }

        public override void RestartLastPage(object restartPageArgs)
        {
            if (IsNavigationRan) return;
            GoToPage((PageBaseLink) Pages.Last(), restartPageArgs);
        }

        public override void GoToStartPage()
        {
            if (IsNavigationRan) return;
            var firstPageLink = Pages.First();
            Pages.Clear();
            CurrentPageIndex = 0;

            GoToPage((PageBaseLink) firstPageLink, null);

            Pages.Add(firstPageLink);
        }

        protected virtual void OnPageLoaded(PageBase page, object newPageArgs, Action<PageBase> initPage = null, bool isStartPage = false)
        {
            CurrentPage = page;
            if (initPage == null)
            {
                page.NavigationController = this;
                page.OnNavigatedTo(newPageArgs);
            }
            else
            {
                initPage(CurrentPage);
            }
            StartCoroutine(DelayStartPage(page, DateTime.Now - _startPageNavigationTime, isStartPage));
        }

        private void OnStartPageActive(Action onPageLoad)
        {
            if (ActivePage().IsActive)
            {
                StartPageLocker(false, onPageLoad);
            }
            else
            {
                ActivePage().OnPageLoaded += () => { StartPageLocker(false, onPageLoad); };
            }
        }

        private IEnumerator DelayStartPage(PageBase page, TimeSpan pageLoadTime, bool isStartPage)
        {
            var secs = Mathf.Max((float) (MinTimePageLoading.TotalSeconds - pageLoadTime.TotalSeconds), 0f);
            yield return new WaitForSeconds(secs);

            HideLoadingScreen(() =>
            {
                if (!isStartPage)
                {
                    page.OnNavigatedToCompleted();
                }
                IsNavigationRan = false;
            });
        }

        protected static PageBase FindPageBase(Scene sceneByName, PageBaseLink pageBaseLink)
        {
            var rootGameObjects = sceneByName.GetRootGameObjects();
            return rootGameObjects
                .Select(_ => _.GetComponent<PageBase>())
                .FirstOrDefault(_ => _ != null && _.PageBaseLink == pageBaseLink);
        }

        protected virtual void GoToPage(PageBaseLink nextPage, object newPageArgs)
        {
            IsNavigationRan = true;
            _startPageNavigationTime = DateTime.Now;
            var prevPage = CurrentPage;

            prevPage.OnNavigatedFrom();
            var prevPageLink = prevPage.PageBaseLink;
            ShowLoadingScreen(() =>
            {
                UnloadSceneAsync(prevPageLink, () =>
                {
                    prevPage.OnNavigatedFromCompleted();
                    LoadScene(nextPage, _ => OnPageLoaded(_, newPageArgs));
                });
            });
        }

        private void UnloadSceneAsync(PageBaseLink pageBaseLink, Action onComplete)
        {
            var unloadSceneAsync = SceneManager.UnloadSceneAsync(GetSceneName(pageBaseLink));
            void Unloaded(AsyncOperation asyncOperation)
            {
                unloadSceneAsync.completed -= Unloaded;
                if (onComplete != null) onComplete();
            }

            unloadSceneAsync.completed += Unloaded;
        }

        protected virtual void ShowLoadingScreen(Action onComplete)
        {
            LoadingScreen.FadeIn(onComplete);
        }

        private void HideLoadingScreen(Action onComplete)
        {
            if (LoadingScreen != null)
            {
                LoadingScreen.FadeOut(onComplete);
            }
        }

        public virtual LoadingScreen InstantiateLoadingScreen(LoadingScreen prefab)
        {
            if (prefab != null)
            {
                if (LoadingScreen != null)
                {
                    DestroyImmediate(LoadingScreen.gameObject, true);
                }
                LoadingScreen = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
                LoadingScreen.name = "Loading Screen";
                LoadingScreen.Init();
            }
            return LoadingScreen;
        }

        protected virtual void LoadScene(PageBaseLink nextPage, Action<PageBase> onFinishAction)
        {
            StartCoroutine(LoadSceneAsync(nextPage, onFinishAction));
        }

        private IEnumerator<float> LoadSceneAsync(PageBaseLink nextPage, Action<PageBase> onFinishAction)
        {
            var sceneName = GetSceneName(nextPage);
            var result = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            result.allowSceneActivation = true;

            while (!result.isDone)
            {
                yield return result.progress;
            }
            var sceneByName = SceneManager.GetSceneByName(sceneName);
            SceneManager.SetActiveScene(sceneByName);
            var pageBase = FindPageBase(sceneByName, nextPage);
            onFinishAction(pageBase);
            yield return 1f;
        }

        public void StartPageLocker(bool isTimer, Action onLoadedAction)
        {
            if (onLoadedAction != null)
            {
                _onLoadedAction += onLoadedAction;
            }
            if (isTimer)
            {
                isSplashScreenReady = true;
            }
            else
            {
                isStartPageReady = true;
            }
            if ((isSplashScreenReady || !ShowSplashScreen) && isStartPageReady)
            {
                if (_onLoadedAction != null) _onLoadedAction();
            }
        }
    }
}