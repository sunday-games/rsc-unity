using UnityEngine;

public class PopupRateApp : Popup
{
    public override void Init()
    {
        Analytic.EventProperties("Other", "RateApp", "Show");
        PlayerPrefs.SetInt("RateApp", user.gameSessions);

        previous = ui.prepare;
    }

    public void Rate()
    {
        Analytic.EventProperties("Other", "RateApp", "Rate");

        PlayerPrefs.SetInt("RateApp", int.MaxValue); // Флаг, чтобы это окно больше не вылезало

        ui.options.OpenRateApp();
        ui.PopupClose();
    }

    public void Email()
    {
        Analytic.EventProperties("Other", "RateApp", "Email");

        PlayerPrefs.SetInt("RateApp", int.MaxValue); // Флаг, чтобы это окно больше не вылезало

        ui.options.SendEmail();
        ui.PopupClose();
    }
}
