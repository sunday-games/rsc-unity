using System;
using System.Collections.Generic;

public class Promocode : SG.Promocode
{
    public Promocode(Dictionary<string, object> data) : base(data) { }

    public override void Activate()
    {
        base.Activate();

        if (action.ContainsKey("coins"))
            Core.user.UpdateCoins(Convert.ToInt32(action["coins"]), true);

        if (action.ContainsKey("spins"))
            Core.user.UpdateSpins(Convert.ToInt32(action["spins"]), true);

        if (action.ContainsKey("levelUp") && Core.user.level < Missions.MAX_LEVEL)
            Core.user.LevelUp();

        if (action.ContainsKey("catboxPremium") && Core.user.isCanGetPremiumBox)
            Core.user.GetPremiumCatbox();

        if (action.ContainsKey("catboxSimple") && Core.user.isCanGetSimpleBox)
            Core.user.GetSimpleCatbox();

        if (action.ContainsKey("hats"))
            Core.user.newYearHats += Convert.ToInt32(action["hats"]);

        if (action.ContainsKey("hearts"))
            Core.user.stValentinHearts += Convert.ToInt32(action["hearts"]);

        if (action.ContainsKey("bats"))
            Core.user.halloweenBats += Convert.ToInt32(action["bats"]);

        if (action.ContainsKey("useSausagesHistory"))
        {
            Core.user.useSausagesHistory += Convert.ToInt32(action["useSausagesHistory"]);
            Core.achievements.OnUseSausages();
        }

        if (action.ContainsKey("honored"))
        {
            Core.user.honored = 1;
            Core.achievements.OnUpdateHonored();
        }

        foreach (var cat in Core.gameplay.superCats)
            if ((action.ContainsKey(cat.name) && !Core.user.isOwned(cat)))
                Core.user.GetCat(cat, Convert.ToInt32(action[cat.name]));

        Analytic.Event("Promocode", code);
    }
}