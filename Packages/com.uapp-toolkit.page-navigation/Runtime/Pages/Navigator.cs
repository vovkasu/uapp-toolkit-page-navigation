using System;
using System.Collections.Generic;
using System.Linq;
using UAppToolKit.PageNavigation.Popup;
using UnityEngine;

namespace UAppToolKit.PageNavigation.Pages
{
    public class Navigator : MonoBehaviour
    {
        protected readonly List<PopUpBase> PopUps = new List<PopUpBase>();
        [HideInInspector]
        public bool IsActive;
        public event Action OnPageLoaded;

        public NavigationControllerBase NavigationController { get; set; }
        
        public virtual void OnNavigatedTo(object arg)
        {
            IsActive = true;
            if (OnPageLoaded != null) OnPageLoaded();
        }

        public virtual void OnNavigatedToCompleted()
        {
        }

        public virtual void OnNavigatedFrom()
        {
            IsActive = false;
        }

        public virtual void OnNavigatedFromCompleted()
        {
        }

        public int GetPopUpsCount()
        {
            return PopUps.Count;
        }

        public PopUpBase GetLastPopUp()
        {
            return PopUps.Last();
        }

        public void AddPopUp(PopUpBase popUp)
        {
            popUp.SetContext(this);
            PopUps.Add(popUp);
        }

        public void ClosePopUp(PopUpBase popUp)
        {
            if (popUp.CloseAndDestroy(this))
            {
                PopUps.Remove(popUp);
            }
        }

        protected virtual void Start()
        {
        }

        protected virtual void Awake()
        {
        }

        protected virtual void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                if (NavigationController.BackNavigationByEscape)
                {
                    GoBackLoadingType();
                }
            }
        }

        protected virtual void GoBackLoadingType()
        {
            NavigationController.GoBack();
        }

        protected virtual void OnDestroy()
        {
        }
    }
}