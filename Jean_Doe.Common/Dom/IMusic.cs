using Jean_Doe.Common;
using System.Collections.Generic;
public interface IMusic:IHasId
{
    string Logo { get; set; }
    string Id { get; set; }
    string Name { get; set; }
    dynamic  JsonObject { get; set; }
    EnumMusicType Type { get; }
    void CreateFromJson(dynamic obj);
}
