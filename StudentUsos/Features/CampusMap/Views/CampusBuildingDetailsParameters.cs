namespace StudentUsos.Features.CampusMap.Views;

public class CampusBuildingDetailsParameters
{
    public required string ShortName { get; set; }
    public required string LongName { get; set; }

    public Action GoToBuildingMap { get; set; }
    public bool HasFloorMaps { get; set; }
}

