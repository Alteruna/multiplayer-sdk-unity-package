using TMPro;

public class DirectConnectMenu : Alteruna.Multiplayer.CommunicationBridge
{
    public TMP_InputField inputField;

    new void OnEnable()
    {
        base.OnEnable();
        inputField.text = "";
    }

    public void Connect()
    {
        string s = inputField.text.Trim();
        if (s == "") s = "localhost";
        Multiplayer.DirectConnect(s);
    }
}
