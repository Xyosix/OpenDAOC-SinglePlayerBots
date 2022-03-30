using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DOL.Database;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;

namespace DOL.GS;

public class ConquestManager
{
    private List<DBKeep> DBKeeps;
    private List<AbstractGameKeep> _albionKeeps;
    private List<AbstractGameKeep> _hiberniaKeeps;
    private List<AbstractGameKeep> _midgardKeeps;
    private int[] albionKeepIDs = new[] {50, 51, 52, 53, 54, 55, 56};
    private int[] midgardKeepIDs = new[] {75, 76, 77, 78, 79, 80, 81};
    private int[] hiberniaKeepIDs = new[] {100, 101, 102, 103, 104, 105, 106};
    
    private Dictionary<ConquestObjective, int> _albionObjectives;
    private Dictionary<ConquestObjective, int> _hiberniaObjectives;
    private Dictionary<ConquestObjective, int> _midgardObjectives;

    public ConquestObjective ActiveAlbionObjective;
    public ConquestObjective ActiveHiberniaObjective;
    public ConquestObjective ActiveMidgardObjective;

    public long LastTaskRolloverTick;
    
    public int SumOfContributions { get { return AlbionContribution + HiberniaContribution + MidgardContribution; }}
    int HiberniaContribution = 0;
    int AlbionContribution = 0;
    int MidgardContribution = 0;
    
    public List<ConquestObjective> GetActiveObjectives
    {
        get
        {
            var list = new List<ConquestObjective>();
            list.Add(ActiveAlbionObjective);
            list.Add(ActiveHiberniaObjective);
            list.Add(ActiveMidgardObjective);
            return list;
        }
        set { }
    }

    public ConquestManager()
    {
        ResetKeeps();
        ResetObjectives();
        RotateKeeps();
    }

    private void ResetKeeps()
    {
        if (_albionKeeps == null) _albionKeeps = new List<AbstractGameKeep>();
        if (_hiberniaKeeps == null) _hiberniaKeeps = new List<AbstractGameKeep>();
        if (_midgardKeeps == null) _midgardKeeps = new List<AbstractGameKeep>();
        _albionKeeps.Clear();
        _hiberniaKeeps.Clear();
        _midgardKeeps.Clear();
        foreach (var keep in GameServer.KeepManager.GetAllKeeps())
        {
            if(albionKeepIDs.Contains(keep.KeepID))
                _albionKeeps.Add(keep);
            if(hiberniaKeepIDs.Contains(keep.KeepID))
                _hiberniaKeeps.Add(keep);
            if(midgardKeepIDs.Contains(keep.KeepID))
                _midgardKeeps.Add(keep);
        }
    }

    private void ResetObjectives()
    {
        if (_albionObjectives == null) _albionObjectives = new Dictionary<ConquestObjective, int>();
        if (_hiberniaObjectives == null) _hiberniaObjectives = new Dictionary<ConquestObjective, int>();
        if (_midgardObjectives == null) _midgardObjectives = new Dictionary<ConquestObjective, int>();
        
        _albionObjectives.Clear();
        _hiberniaObjectives.Clear();
        _midgardObjectives.Clear();

        foreach (var keep in _albionKeeps)
        {
            _albionObjectives.Add(new ConquestObjective(keep), GetConquestValue(keep));
        }
        
        foreach (var keep in _hiberniaKeeps)
        {
            _hiberniaObjectives.Add(new ConquestObjective(keep), GetConquestValue(keep));
        }
        
        foreach (var keep in _midgardKeeps)
        {
            _midgardObjectives.Add(new ConquestObjective(keep), GetConquestValue(keep));
        }
    }

    private int GetConquestValue(AbstractGameKeep keep)
    {
        switch (keep.KeepID)
        {
            case 50: //benowyc
            case 75: //bledmeer
            case 100: //crauchon
                return 1;
            //alb
            case 52: //erasleigh
            case 53: //boldiam
            case 54: //sursbrooke
            //mid
            case 76: //nottmoor
            case 77: //hlidskialf
            case 78: //blendrake
            //hib    
            case 101: //crimthain
            case 102: //bold
            case 104: //da behn
                return 2;
            //alb
            case 51: //berkstead
            case 55: //hurbury
            case 56: //renaris
            //mid                
            case 79: //glenlock
            case 80: //fensalir
            case 81: //arvakr
            //hib
            case 103: //na nGed
            case 105: //scathaig
            case 106: //ailline
                return 3;
        }

        return 1;
    }

    public void ConquestCapture(AbstractGameKeep CapturedKeep)
    {
        //notify everyone an objective was captured
        foreach (var client in WorldMgr.GetAllPlayingClients())
        {
            if(client.Player.CurrentZone.IsRvR)
                client.Player.Out.SendMessage($"{GetStringFromRealm(CapturedKeep.Realm)} has captured a conquest objective!", eChatType.CT_ScreenCenterSmaller_And_CT_System, eChatLoc.CL_SystemWindow);
        }

        foreach (var activeObjective in GetActiveObjectives)
        {
            AddSubtotalToOverallFrom(activeObjective);
        }
        
        
    }
    
    private void AwardContributorsForRealm(eRealm realmToAward)
    {
        foreach (var conquestObjective in GetActiveObjectives)
        {
            foreach (var contributingPlayer in conquestObjective.GetContributingPlayers())
            {
                //if player is of the correct realm, award them their realm's portion of the overall reward
                if (contributingPlayer.Realm == realmToAward)
                {
                    if(realmToAward == eRealm.Hibernia)
                        contributingPlayer.GainRealmPoints((SumOfContributions * (HiberniaContribution/SumOfContributions)), false, true);
                    if(realmToAward == eRealm.Albion)
                        contributingPlayer.GainRealmPoints((SumOfContributions * (AlbionContribution/SumOfContributions)), false, true);
                    if(realmToAward == eRealm.Midgard)
                        contributingPlayer.GainRealmPoints((SumOfContributions * (MidgardContribution/SumOfContributions)), false, true);
                }
            }
        }
    }

    private string GetStringFromRealm(eRealm realm)
    {
        switch (realm)
        {
            case eRealm.Albion:
                return "Albion";
            case eRealm.Midgard:
                return "Midgard";
            case eRealm.Hibernia:
                return "Hibernia";
            default:
                return "Undefined Realm";
        }
    }

    public void RotateKeeps()
    {
        SetKeepForRealm(eRealm.Albion);
        SetKeepForRealm(eRealm.Hibernia);
        SetKeepForRealm(eRealm.Midgard);
        LastTaskRolloverTick = GameLoop.GameLoopTime;
    }

    public void AddSubtotalToOverallFrom(ConquestObjective objective)
    {
        HiberniaContribution += objective.HiberniaContribution;
        AlbionContribution += objective.AlbionContribution;
        MidgardContribution += objective.MidgardContribution;
    }

    private void SetKeepForRealm(eRealm realm)
    {
        Dictionary<ConquestObjective, int> keepDict = new Dictionary<ConquestObjective, int>();
        switch (realm)
        {
            case eRealm.Albion:
                keepDict = _albionObjectives;
                break;
            case eRealm.Hibernia:
                keepDict = _hiberniaObjectives;
                break;
            case eRealm.Midgard:
                keepDict = _midgardObjectives;
                break;
        }

        int objectiveWeight = 1;

        foreach (var objective in keepDict)
        {
            if (objective.Key.Keep.OriginalRealm != objective.Key.Keep.Realm && objective.Value > objectiveWeight)
            {
                objectiveWeight = objective.Value;
            }
                
        }

        switch (realm)
        {
            case eRealm.Albion:
                List<ConquestObjective> albKeepsSort = new List<ConquestObjective>(keepDict.Keys.Where(x => keepDict[x] == objectiveWeight)); //get a list of all keeps with the current weight
                ActiveAlbionObjective = albKeepsSort[Util.Random(albKeepsSort.Count()-1)]; //pick one at random
                Console.WriteLine($"New albion objective: {ActiveAlbionObjective.Keep.Name} weight {keepDict[ActiveAlbionObjective]}");
                break;
            case eRealm.Hibernia:
                List<ConquestObjective> hibKeepsSort = new List<ConquestObjective>(keepDict.Keys.Where(x => keepDict[x] == objectiveWeight)); //get a list of all keeps with the current weight
                ActiveHiberniaObjective = hibKeepsSort[Util.Random(hibKeepsSort.Count()-1)]; //pick one at random
                Console.WriteLine($"New hibernia objective: {ActiveHiberniaObjective.Keep.Name} weight {keepDict[ActiveHiberniaObjective]}");
                break;
            case eRealm.Midgard:
                List<ConquestObjective> midKeepsSort = new List<ConquestObjective>(keepDict.Keys.Where(x => keepDict[x] == objectiveWeight)); //get a list of all keeps with the current weight
                ActiveMidgardObjective = midKeepsSort[Util.Random(midKeepsSort.Count()-1)]; //pick one at random
                Console.WriteLine($"New midgard objective: {ActiveMidgardObjective.Keep.Name} weight {keepDict[ActiveMidgardObjective]}");
                break;
        }
    }


}