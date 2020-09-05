using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PopupPromoteCats : Popup
{
    [Space(10)]
    public GameObject add;
    public RectTransform addWindow;
    [Space(10)]
    public RectTransform window;
    public Transform catsParent;
    public CatPromote catExpViewPrefab;
    public GameObject nextButton;
    [Space(10)]
    public GameObject boost;

    List<CatPromote> catExpViews = new List<CatPromote>();
    List<CatSlot> catSlots = new List<CatSlot>();

    public override void Init()
    {
        add.SetActive(true);

        boost.SetActive(false);

        nextButton.SetActive(false);
        nextButton.transform.localScale = Vector3.zero;

        foreach (CatSlot slot in ui.game.catSlots)
            if (slot.catItem != null && slot.catItem.expGame > 0) catSlots.Add(slot);

        window.localScale = new Vector3(1f, 0.1f, 1f);
        window.sizeDelta = new Vector2(window.sizeDelta.x, 70f + 165f * catSlots.Count);
        iTween.ScaleTo(window.gameObject, iTween.Hash("y", 1, "easeType", "easeOutBack", "time", 0.5f));

        addWindow.localScale = new Vector3(1f, 0.1f, 1f);
        addWindow.sizeDelta = new Vector2(addWindow.sizeDelta.x, 70f + 165f * catSlots.Count);
        iTween.ScaleTo(addWindow.gameObject, iTween.Hash("y", 1, "easeType", "easeOutBack", "time", 0.5f));

        if (!user.IsTutorialShown(Tutorial.Part.CatExperience))
            ui.tutorial.Show(Tutorial.Part.CatExperience, new Transform[] { addWindow, window });

        StartCoroutine(Promote());
    }

    IEnumerator Promote()
    {
        yield return new WaitForSeconds(0.4f);

        if (gameplay.boosts.experience.ON) boost.SetActive(true);

        foreach (CatSlot slot in catSlots)
            if (slot.catItem != null)
            {
                CatPromote catPromote = Instantiate(catExpViewPrefab) as CatPromote;
                catPromote.transform.SetParent(catsParent.transform, false);
                catPromote.Init(slot.catItem);
                catExpViews.Add(catPromote);
            }
        user.CollectionSave(true);

        nextButton.SetActive(true);
        iTween.ScaleTo(nextButton, iTween.Hash("x", 1f, "y", 1f, "easeType", "easeOutBack", "time", 0.3f));

        sound.Play(sound.getExperience);
        bool isAllDone = false;
        while (!isAllDone)
        {
            yield return new WaitForEndOfFrame();

            isAllDone = true;
            foreach (CatPromote catExpView in catExpViews)
                if (!catExpView.isDone) isAllDone = false;
        }
        sound.Stop(sound.getExperience);
    }

    public override void OnEscapeKey() { Next(); }

    public void Next()
    {
        if (gameplay.score > gameplay.oldPermanentRecord) ui.PopupShow(ui.highscore);
        else ui.PopupShow(ui.result);
    }

    public override void Reset()
    {
        add.SetActive(false);

        sound.Stop(sound.getExperience);
        StopAllCoroutines();

        foreach (CatSlot slot in catSlots) slot.Clear();
        catSlots.Clear();

        foreach (CatPromote catExpView in catExpViews)
        {
            catExpView.catSlot.Clear();
            Destroy(catExpView.gameObject);
        }
        catExpViews.Clear();
    }
}
