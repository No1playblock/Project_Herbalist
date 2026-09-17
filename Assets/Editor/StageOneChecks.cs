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
            var s=AssetDatabase.LoadAssetAtPath<StageOneSettings>("Assets/StageOne/StageOneSettings.asset");
            Check(s.Valid(out var error),error);
            int duyeong=Array.IndexOf(s.slotRoles,CharacterRole.Duyeong), sodam=1-duyeong;
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
            return "PASS: both potion assignments, role restrictions, capacity, duplicate collection/crafting, failed unlock, repeated consumption, transfer, gate prerequisites, simultaneous two-player entry and one-shot clear.";
        }
    }
}
