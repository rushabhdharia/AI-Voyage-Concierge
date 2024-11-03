public class Content
{
    public string role { get; set; }
    public List<Part> parts { get; set; }
}

public class Part
{
    public string text { get; set; }
}

public class Root
{
    public List<Content> contents { get; set; }
}