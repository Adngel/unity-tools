using System.Collections.Generic;
public static class DI_VoiceFolderOrder
{
    public static readonly Dictionary<string, int> Chronology = new Dictionary<string, int>
    {
        // --- MAIN QUESTS (10-99) ---
        { "MQ01_Prologue", 10 },
        { "BA_MQ02_Mansions", 11 },
        { "MQ03", 12 },
        { "MQ04BelAir", 13 },
        { "MQ04Halperin", 14 },
        { "MQ05BelAir", 15 },
        { "MQ05BevHills", 16 },
        { "MQ05FilmSet", 17 },
        { "MQ06_AmmoRun", 18 },
        { "MQ08BelAir", 19 },
        { "VB_MQ_WelcomeToVenice", 20 },
        { "VB_MQ_LegDay", 21 },
        { "MQ08bGauntlet", 22 },
        { "VB_MQ_BeachOffensive", 23 },
        { "MQ08bOceanAvenue", 24 },
        { "MQ08Sewers", 25 },
        { "MQ09OceanAvenue", 26 },
        { "MQ10", 27 },
        { "MQ15Hollywood", 28 },
        { "MQ15Metro", 29 },
        { "MQ16", 30 },
        { "MQ18", 31 },

        // --- HUBS (100-199) ---
        { "EmmaHub_AllNPCs_", 100 },
        { "RoxanneHub_AllNPCs_", 101 },
        { "FilmSetHub_SarahSebastian_", 102 },
        { "PattonHub_Patton_", 103 },
        { "BlueCrabHub", 104 },
        { "VB_Ambient_BlueCrab", 105 },
        { "VeniceTowerHub_Melissa_", 106 },
        { "SerlingHub", 107 },
        { "SM_HubHotel_HubState", 108 },
        { "PierLifeguardHub_AmandaCarmenEzekielRita_", 109 },
        { "TishaHub_AvaDannyJohnTisha_", 110 },
        { "EschatonHub_AllNPCs_", 111 },

        // --- SIDE QUESTS (200-299) ---
        { "BA_SQ002_VintageTastes", 200 },
        { "BA_SQ015_GoodBadZombie", 201 },
        { "SQ016", 202 },
        { "VB_SQ_CremainsOfTheDay", 203 },
        { "PIER_SQ_MessageInABottle", 204 },
        { "VB_SQ_IPredictARiot", 205 },
        { "VB_SQ_MailStrom", 206 },
        { "VB_SQ_OrganDonor", 207 },

        // --- MISSING PEOPLE (300-399) ---
        { "MP201", 300 },
        { "MP301", 301 },
        { "MP601", 302 },

        // --- TREASURE HUNTS (400-499) ---
        { "BA_TH103_Postman", 400 },
        { "TH303", 401 }
    };

    public static int GetOrder(string rawName)
    {
        if (Chronology.TryGetValue(rawName, out int order)) 
            return order;

        return 999;
    }
}
