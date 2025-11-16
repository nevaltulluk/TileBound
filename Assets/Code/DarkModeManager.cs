using System.Collections.Generic;
using Code;
using UnityEngine;

public class DarkModeManager : MonoBehaviour, IService
{
    private List<DarkModeImage> darkModeImages = new List<DarkModeImage>();
    private List<SeaMaterialChanger> seaMaterialChangers = new List<SeaMaterialChanger>();
    private EventBus _eventBus;

    private void Awake()
    {
        MainContainer.instance.Register(this);
        _eventBus = MainContainer.instance.Resolve<EventBus>();
        _eventBus.Subscribe<Events.ToggleDarkMode>(OnToggleDarkMode);
    }

    private void Start()
    {
        // Find all objects with DarkModeImage and SeaMaterialChanger scripts in scene
        RefreshLists();
    }

    private void RefreshLists()
    {
        // Refresh both lists to catch all objects (active or inactive)
        darkModeImages.Clear();
        darkModeImages.AddRange(FindObjectsOfType<DarkModeImage>(true));
        
        seaMaterialChangers.Clear();
        seaMaterialChangers.AddRange(FindObjectsOfType<SeaMaterialChanger>(true));
        
        Debug.Log($"DarkModeManager found {darkModeImages.Count} DarkModeImage components and {seaMaterialChangers.Count} SeaMaterialChanger components in scene.");
    }

    private void Update()
    {
        // Keyboard shortcuts: 'd' for dark mode, 's' for standard/light mode
        if (Input.GetKeyDown(KeyCode.D))
        {
            _eventBus.Fire(new Events.ToggleDarkMode(true));
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            _eventBus.Fire(new Events.ToggleDarkMode(false));
        }
    }

    private void OnToggleDarkMode(Events.ToggleDarkMode evt)
    {
        // Refresh lists to ensure we have all current objects (active or inactive)
        RefreshLists();

        // Update all images (active or inactive)
        foreach (var darkModeImage in darkModeImages)
        {
            if (darkModeImage != null)
            {
                darkModeImage.SetDarkMode(evt.IsDark);
            }
        }

        // Update all sea materials (active or inactive)
        foreach (var seaMaterialChanger in seaMaterialChangers)
        {
            if (seaMaterialChanger != null)
            {
                seaMaterialChanger.SetDarkMode(evt.IsDark);
            }
        }
        
        Debug.Log($"Dark mode toggled: {evt.IsDark} - Updated {darkModeImages.Count} images and {seaMaterialChangers.Count} sea materials.");
    }
}

