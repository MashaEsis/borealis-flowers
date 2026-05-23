using borealis_flowers.api.Data;

namespace borealis_flowers.api.Features.Specialists;
public class SpecialistDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string ImgUrl { get; set; }
    public string Address { get; set; }
    public string Specialization { get; set; }

    public string City { get; set; }
    public string StyleDescription { get; set; } = "";
    public List<string> PortfolioPreview { get; set; } = [];
}

public class SpecialistUpdateVM
{
    public Guid Id { get; set; }
    public Guid SpecializationId { get; set; }
    public string FullName { get; set; }
    public string ImgUrl { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
