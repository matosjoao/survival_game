
public interface IInteractable 
{
    string GetInteractPrompt();
    void OnInteract(PlayerController playerController);
    void OnDesinteract(PlayerController playerController);
}
