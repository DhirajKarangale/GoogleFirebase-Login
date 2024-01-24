using UnityEngine;

public class Lobby : MonoBehaviour
{
    public void ButtonGame(int game)
    {
        AddressablesManager.instance.ButtonGame(game);
    }
}