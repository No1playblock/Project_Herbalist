using System;
using UnityEditor;
using Herbalist.StageOne;
using Herbalist.Presentation;
namespace Herbalist.Editor
{
    public static class StageOneChecks
    {
        private static void Check(bool value, string message) { if (!value) throw new Exception(message); }
        public static string Run()
        {
            var s=AssetDatabase.LoadAssetAtPath<StageOneSettings>("Assets/_Project/Data/Stages/Stage_01/SO_StageOneSettings.asset");
            Check(s.Valid(out var error),error);
            Check(StageBrewingRules.FireStrength(0,s)==StageFireStrength.Low &&
                  StageBrewingRules.FireStrength(1,s)==StageFireStrength.Low &&
                  StageBrewingRules.FireStrength(2,s)==StageFireStrength.Medium &&
                  StageBrewingRules.FireStrength(3,s)==StageFireStrength.Medium &&
                  StageBrewingRules.FireStrength(4,s)==StageFireStrength.High,"Fan rates must use the document's 0-1/2-3/4+ bands");
            var brewingRecipe=s.recipes[0];
            float temp=brewingRecipe.targetMin, gauge=0;
            StageBrewingRules.Advance(s,brewingRecipe,2,1f,ref temp,ref gauge);
            Check(temp==brewingRecipe.targetMin && gauge==8f,"Medium fire holds temperature and gains 8 gauge per second");
            StageBrewingRules.Advance(s,brewingRecipe,4,1f,ref temp,ref gauge);
            Check(temp==brewingRecipe.targetMin+6f && gauge==16f,"High fire heats by 6 per second");
            StageBrewingRules.Advance(s,brewingRecipe,0,1f,ref temp,ref gauge);
            Check(temp==brewingRecipe.targetMin && gauge==24f,"Low fire cools by 6 per second");
            temp=0; gauge=5;
            StageBrewingRules.Advance(s,brewingRecipe,0,1f,ref temp,ref gauge);
            Check(temp==0 && gauge==0,"Temperature and gauge clamp at zero, and out-of-range gauge loses 12 per second");
            temp=brewingRecipe.targetMin; gauge=99;
            StageBrewingRules.Advance(s,brewingRecipe,2,1f,ref temp,ref gauge);
            Check(gauge==100,"Gauge clamps at 100");
            int duyeong=Array.IndexOf(s.slotRoles,CharacterRole.Duyeong), sodam=1-duyeong;
            var solo=new StageProgress(s);
            int soloHerb=s.ItemId(s.recipes[0].ingredient);
            Check(!solo.Gather(duyeong,0,soloHerb,CharacterRole.Sodam),"Sodam state must not gather in solo test");
            Check(solo.Gather(duyeong,0,soloHerb,CharacterRole.Duyeong),"Duyeong state can gather in solo test");
            for(int first=0;first<s.recipes.Length;first++)
            {
                var p=new StageProgress(s); int second=1-first;
                int herb=s.ItemId(s.recipes[first].ingredient);
                Check(!p.Gather(sodam,first,herb),"Sodam must not gather");
                Check(p.Gather(duyeong,first,herb),"Duyeong gather");
                Check(!p.Gather(duyeong,second,s.ItemId(s.recipes[second].ingredient)),"Full hand gather rejected");
                Check(!p.Craft(duyeong),"Duyeong must not craft");
                Check(p.Give(duyeong,sodam),"Herb delivery");
                Check(!p.Gather(duyeong,first,herb),"Duplicate harvest rejected");
                Check(!p.Consume(sodam,_=>true),"Herb must not be drinkable");
                Check(p.Craft(sodam),"First craft");
                Check(!p.Craft(sodam),"Repeated craft rejected");
                Check(!p.Consume(sodam,_=>false) && p.Held[sodam]!=0,"Failed grant keeps potion");
                Check(p.Consume(sodam,_=>true),"First drink");
                Check(!p.GateOpen && !p.CheckClear(true,true),"One potion cannot clear");
                Check(p.Gather(duyeong,second,s.ItemId(s.recipes[second].ingredient)),"Second gather");
                Check(p.Give(duyeong,sodam) && p.Craft(sodam),"Second delivery and craft");
                bool called=false; Check(!p.Consume(sodam,_=>{called=true;return true;})&&!called,"Already unlocked cannot consume");
                Check(p.Give(sodam,duyeong),"Potion delivery");
                Check(p.Consume(duyeong,_=>true) && p.GateOpen,"Both potions open gate");
                Check(!p.CheckClear(true,false)&&!p.CheckClear(false,true),"Both must be inside simultaneously");
                Check(p.CheckClear(true,true)&&!p.CheckClear(true,true),"Clear exactly once");
                Check(p.Consumed[sodam]==s.ItemId(s.recipes[first].result),"Choice independent of character");
            }
            return "PASS: shared fan-rate/temperature/gauge rules, both potion assignments, role restrictions including solo state, capacity, duplicate collection/crafting, failed unlock, repeated consumption, transfer, gate prerequisites, simultaneous two-player entry and one-shot clear.";
        }
    }
}
