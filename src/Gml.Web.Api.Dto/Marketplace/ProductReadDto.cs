using System;
using System.Collections.Generic;

namespace Gml.Web.Api.Dto.Marketplace;

public class ProductReadDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ProjectLink { get; set; }
    public string ImageUrl { get; set; }
    public bool IsFree { get; set; }
    public double Price { get; set; }
    public List<CategoryReadDto> Categories { get; set; }
}
