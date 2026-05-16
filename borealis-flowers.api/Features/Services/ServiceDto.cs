namespace borealis_flowers.api.Features.Services;

public class ServiceDto
{
    public Guid Id { get; set; }
    public Guid SpecializationId { get; set; }
    public string SpecializationName { get; set; }
    public string Name { get; set; }
    public double Price { get; set; }
    public int? EstimatedTime { get; set; }
}
