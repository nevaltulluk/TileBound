using UnityEngine;
using UnityEngine.UI;

public class CheatManager : MonoBehaviour
{
    private EventBus _eventBus;
    
    [SerializeField] private Button springButton;
    [SerializeField] private Button summerButton;
    [SerializeField] private Button fallButton;
    [SerializeField] private Button winterButton;
    void Start()
    {
        _eventBus = MainContainer.instance.Resolve<EventBus>();
        springButton.onClick.AddListener(() => _eventBus.Fire(new OnSpringButtonClickEvent()));
        summerButton.onClick.AddListener(() => _eventBus.Fire(new OnSummerButtonClickEvent()));
        fallButton.onClick.AddListener(() => _eventBus.Fire(new OnFallButtonClickEvent()));
        winterButton.onClick.AddListener(() => _eventBus.Fire(new OnWinterButtonClickEvent()));
    }
}

public struct OnSpringButtonClickEvent {}
public struct OnSummerButtonClickEvent {}
public struct OnFallButtonClickEvent {}
public struct OnWinterButtonClickEvent {}
