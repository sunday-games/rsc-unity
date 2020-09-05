using UnityEngine.UI;
using IngameAdvisor;

public class AskButton : Core
{
    public Text mainText;
    Advisor.Reply reply;

    public AskButton Copy(Advisor.Reply reply)
    {
        var instance = Instantiate(this);
        instance.transform.SetParent(this.transform.parent, false);
        instance.Setup(reply);
        return instance;
    }

    public void Setup(Advisor.Reply reply)
    {
        this.reply = reply;

        mainText.text = reply.name;

        gameObject.SetActive(true);
    }

    public void OnClick()
    {
        ui.advisor.OnAskButtonClick(reply);
    }
}