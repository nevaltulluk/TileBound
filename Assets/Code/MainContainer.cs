public class MainContainer : Container
{
    public static MainContainer Instance;
    
    private void Awake()
    {
        base.Awake();
        Instance = this;
        Register(new EventBus());
    }
}
