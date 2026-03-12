namespace TheGatekeeper.Contracts;

public class MatchDetailsDtoV1
{
    public MatchMetadataDtoV1 Metadata { get; set; }
    public MatchInfoDtoV1 Info { get; set; }
}

public class MatchMetadataDtoV1
{
    public string MatchId { get; set; }
    public string[] Participants { get; set; }
}

public class MatchInfoDtoV1
{
    public long GameCreation { get; set; }
    public int GameDuration { get; set; }
    public string GameMode { get; set; }
    public string GameType { get; set; }
    public int MapId { get; set; }
    public int QueueId { get; set; }
    public List<MatchParticipantDtoV1> Participants { get; set; }
}

public class MatchParticipantDtoV1
{
    public string Puuid { get; set; }
    public int ChampionId { get; set; }
    public string ChampionName { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public bool Win { get; set; }
    public int GoldEarned { get; set; }
    public int TotalDamageDealtToChampions { get; set; }
    public int TotalDamageTaken { get; set; }
    public int ChampLevel { get; set; }
    public int Item0 { get; set; }
    public int Item1 { get; set; }
    public int Item2 { get; set; }
    public int Item3 { get; set; }
    public int Item4 { get; set; }
    public int Item5 { get; set; }
    public int Item6 { get; set; }
}
