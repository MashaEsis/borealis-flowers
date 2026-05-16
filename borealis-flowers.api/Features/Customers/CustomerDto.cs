namespace borealis_flowers.api.Features.Customers;

public class CustomerDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsMaster { get; set; }
    public DateTime FirstVisit { get; set; }
    public DateTime LastVisit { get; set; }
    public DateTime? Birthday { get; set; }
}

public class AnonymousCustomer
{
    public string FullName { get; set; }
    public string Phone { get; set; }
    public bool IsAnonymous { get; set; } = true;
    public string? VisitorId { get; set; } // TODO: use fingerprintjs to generate visitorId for anonymous users and save it in localStorage
    public string? ExternalUserId { get; set; }

}

public class LinkAnonymousCustomerRequest
{
    public string FirebaseUserId { get; set; }
    public string? AnonymousExternalUserId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
