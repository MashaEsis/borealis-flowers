using borealis_flowers.api.Data.Models;

namespace borealis_flowers.api.Features.Requests;

public class RequestDto
{
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? SpecialistId { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? Description { get; set; }
    public string? Resolution { get; set; }
}

public class RequestDetailDto
{
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? SpecialistId { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? Description { get; set; }
    public string? Resolution { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public Guid? SpecializationId { get; set; }
    public string? SpecializationName { get; set; }
}

public class CreateRequestDto
{
    public Guid CustomerId { get; set; }
    public string City { get; set; }
    public string Address { get; set; }
    public Guid SpecializationId { get; set; }
    public string? Description { get; set; }
}

public class UpdateRequestDto
{
    public Guid Id { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? Resolution { get; set; }
}
