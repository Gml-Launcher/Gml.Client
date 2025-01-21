using System;
using System.Collections.Generic;

namespace Gml.Web.Api.Dto.Mods;

public class ModVersionDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string VersionName { get; set; }
    public string VersionNumber { get; set; }
    public DateTimeOffset DatePublished { get; set; }
    public int Downloads { get; set; }
    public List<ModVersionDtoDependency> Dependencies { get; set; } = [];
    public List<string> Files { get; set; } = [];
}
public class ModVersionDtoDependency
{
    public string VersionId { get; set; }
    public string ProjectId { get; set; }
    public string FileName { get; set; }
    public string DependencyType { get; set; }
}


