Changelog
=========

[1.0.0] - 2026.02.03
--------------------
* **Added**
    * NavigationControllerBase – abstract controller that orchestrates page links, loading screen and basic navigation operations.
    * NavigationController – a concrete NavigationControllerBase that implements scene‑based page transitions, splash/start‑page handling, back‑navigation and loading‑screen logic for the application.
    * PageBase – abstract page class inheriting Navigator that also holds a reference to its PageBaseLink (with editor helper code).
    * IPageBaseLink – marker interface for objects that link to navigable pages.
    * PageBaseLink – a ScriptableObject linking a page to a scene name/path and implementing IPageBaseLink.
    * Navigator – component attached to pages that tracks active state, manages pop‑ups and responds to navigation events.
    * PopUpBase – a base MonoBehaviour for popup windows that handles show/close logic, context linking and destruction.
    * LoadingScreen – handles showing/hiding a loading overlay with fade‑in/out animations on background and foreground elements.
    * PopUpAnimationController – drives scale/alpha animations for popups and provides hooks for when open animations finish.
    * PopUpAnimation (serializable) – stores a RectTransform, curve and delay for a single popup element animation.
    * PopUpAnimationSettings (serializable) – groups lists of scale and alpha animation settings for a popup.
    * ScaleAnimationSettings (serializable) – configuration for a transform’s scale animation including curves and timing.
    * AlphaAnimationSettings (serializable) – configuration for fading a Graphic element with curves and timing.
    * SceneNameDependentScriptableObject – base ScriptableObject declaring an editor callback when a scene name changes.
* **Changed** -
* **Fixed** -