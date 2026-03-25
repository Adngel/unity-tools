using System;
using UnityEngine;

public class FileData_Sequence : IComparable<FileData_Sequence>
{
    // Ejemplo:
    // <Sequence ObjectId="aDve7xkbAz" BuildType="Main" Type="Cinematic" SequenceOrder="014" Name="Patton" Filename="" PhysicalFilename="patton"/>

    public string ObjectId;
    public string BuildType;
    public string Type;
    public string SequenceOrder;
    public string Name;
    public string PhysicalFileName;

    public int CompareTo(FileData_Sequence other)
    {
        if (other == null) return 1;

        // NIVEL 1: Tipo (Cinematic > Mission > Journal)
        int typeComp = GetTypePriority(this.Type).CompareTo(GetTypePriority(other.Type));
        if (typeComp != 0) return typeComp;

        // NIVEL 2: Build (Main > EXP1 > EXP2)
        int buildComp = GetBuildPriority(this.BuildType).CompareTo(GetBuildPriority(other.BuildType));
        if (buildComp != 0) return buildComp;

        // NIVEL 3: Orden numérico
        return string.Compare(this.SequenceOrder, other.SequenceOrder);
    }

    private int GetTypePriority(string type)
    {
        switch (type)
        {
            case "Cinematic":
                return 1;
            case "Mission":
                return 2;
            case "Journal":
                return 3;
            default:
                return 4;
        }
    }

    private int GetBuildPriority(string build)
    {
        switch (build)
        {
            case "Main":
                return 1;
            case "EXP1":
                return 2;
            case "EXP2":
                return 3;
            default:
                return 4;
        }
    }
}

