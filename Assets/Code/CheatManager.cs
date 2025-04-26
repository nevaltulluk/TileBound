using UnityEngine;
using UnityEngine.UI;

public class CheatManager : MonoBehaviour
{
    private EventBus eventBus;
    
    [SerializeField] private Button springButton;
    [SerializeField] private Button summerButton;
    [SerializeField] private Button fallButton;
    [SerializeField] private Button winterButton;
    void Start()
    {
        eventBus = MainContainer.Instance.Resolve<EventBus>();
        springButton.onClick.AddListener(() => eventBus.Fire(new OnSpringButtonClickEvent()));
        summerButton.onClick.AddListener(() => eventBus.Fire(new OnSummerButtonClickEvent()));
        fallButton.onClick.AddListener(() => eventBus.Fire(new OnFallButtonClickEvent()));
        winterButton.onClick.AddListener(() => eventBus.Fire(new OnWinterButtonClickEvent()));
    }
}

public struct OnSpringButtonClickEvent {}
public struct OnSummerButtonClickEvent {}
public struct OnFallButtonClickEvent {}
public struct OnWinterButtonClickEvent {}
