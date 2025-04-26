public class MainContainer : Container
{
    public static MainContainer instance;
    
    private void Awake()
    {
        base.Awake();
        instance = this;
        Register(new EventBus());
    }
}
