namespace OsuTopPlaysGUI;

public class PpInfo
{
    public int Times { get; set; }

    public double Pp { get; set; }

    public double Percentage { get; set; }
}

public class ModPpInfo : PpInfo
{
    public string Mod { get; set; }
}

public class MapperPpInfo : PpInfo
{
    public int Id { get; set; }

    public string Name { get; set; }
}
