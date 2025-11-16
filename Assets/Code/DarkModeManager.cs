using System.Collections.Generic;
using Code;
using UnityEngine;

public class DarkModeManager : MonoBehaviour, IService
{
    private List<DarkModeImage> darkModeImages = new List<DarkModeImage>();
    private EventBus _eventBus;

    private void Awake()
    {
        MainContainer.instance.Register(this);
        _eventBus = MainContainer.instance.Resolve<EventBus>();
        _eventBus.Subscribe<Events.ToggleDarkMode>(OnToggleDarkMode);
    }

    private void Start()
    {
        // Find all objects with DarkModeImage script in scene
        darkModeImages.Clear();
        darkModeImages.AddRange(FindObjectsOfType<DarkModeImage>(true));
        Debug.Log($"DarkModeManager found {darkModeImages.Count} DarkModeImage components in scene.");
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
        foreach (var darkModeImage in darkModeImages)
        {
            if (darkModeImage != null)
            {
                darkModeImage.SetDarkMode(evt.IsDark);
            }
        }
        Debug.Log($"Dark mode toggled: {evt.IsDark}");
    }
}

