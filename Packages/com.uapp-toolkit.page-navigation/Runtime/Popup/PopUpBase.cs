using System;
using UAppToolKit.PageNavigation.Pages;
using UnityEngine;
using UnityEngine.UI;

namespace UAppToolKit.PageNavigation.Popup
{
    public class PopUpBase : PopUpAnimationController
    {
        public Button CloseButton;
        public event Action OnClosed;
        
        private event Action _onPopupDestryed;
        private Navigator _context;

        protected virtual void Awake()
        {
            if (CloseButton != null) CloseButton.onClick.AddListener(Close);
            Show();
        }

        protected virtual void Start()
        {
            
        }

        protected virtual void Update()
        {
        }

        protected virtual void OnDestroy()
        {
            if (_onPopupDestryed != null) _onPopupDestryed();
        }

        public virtual void Close()
        {
            _context.ClosePopUp(this);
        }

        public void SetContext(Navigator context)
        {
            _context = context;
        }

        public bool CloseAndDestroy(Navigator context)
        {
            if (_context != context)
            {
                Debug.LogError("Close popup by parent page.");
                return false;
            }

            Closed();
            DestroyView();
            return true;
        }

        private void DestroyView()
        {
            Destroy(gameObject);
        }

        protected virtual void Closed()
        {
            Action handler = OnClosed;
            if (handler != null) handler();
        }
    }
}