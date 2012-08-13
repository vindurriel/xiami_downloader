using Jean_Doe.Common;
public interface IMusic
{
    string Logo { get; set; }
    string Id { get; set; }
    string Name { get; set; }
	string Type { get; }
    void CreateFromJson(dynamic obj);
}
