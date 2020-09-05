using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ExceptionView : Core
{
    public static List<ExceptionView> exceptionViews = new List<ExceptionView>();

    public static void Show(string error)
    {
        if (exceptionViews.Count > 5) return;

        var exceptionView = Instantiate(factory.exceptionView) as ExceptionView;
        exceptionView.transform.SetParent(ui.canvas[2].transform, false);
        exceptionView.errorText.text = error;
        exceptionViews.Add(exceptionView);
    }

    public Text errorText;

    public void Send()
    {
        string subject = "Error Report " + Random.Range(100000, 999999);

        string body = string.Format("Hello\n\nMy device: {0}, {1}, Screen {2}, RAM {3} MB, CPU {4}, GPU {5} {6} MB {7}\n\nMy profile: {8} (Facebook {9}, Game Center {10}, Google Games {11})\n\nGame version: {12}\n\n{13}",
            SystemInfo.deviceModel, SystemInfo.operatingSystem, SG_Utils.resolution, SystemInfo.systemMemorySize, SystemInfo.processorType, SystemInfo.graphicsDeviceName,
            SystemInfo.graphicsMemorySize, SystemInfo.graphicsDeviceVersion, user.isId ? user.id : "-",
            string.IsNullOrEmpty(user.facebookId) ? "-" : user.facebookId,
            string.IsNullOrEmpty(user.gameCenterId) ? "-" : user.gameCenterId,
            string.IsNullOrEmpty(user.googleGamesId) ? "-" : user.googleGamesId,
            build.version, errorText.text);

        SG_Utils.Email(server.links.supportEmail, subject, body);

        Hide();
    }

    public void Hide()
    {
        exceptionViews.Remove(this);
        Destroy(gameObject);
    }
}
