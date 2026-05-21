namespace borealis_flowers.api.Features.Customers;

public static class CustomersModule
{
    public static void CustomersEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/customers", CustomersHandler.GetCustomers()).WithTags("Customers");
        endpoints.MapGet("/customers/{id}", CustomersHandler.GetCustomer()).WithTags("Customers");
        endpoints.MapGet("/customersExternal/{id}", CustomersHandler.GetCustomerIdByExternalUserId()).WithTags("Customers");
        endpoints.MapGet("/customersVisitors/{id}", CustomersHandler.GetCustomerIdByExternalVisitorId()).WithTags("Customers");
        endpoints.MapPost("/customers", CustomersHandler.CreateCustomer()).WithTags("Customers");
        endpoints.MapPost("/customers/link-anonymous", CustomersHandler.LinkAnonymousCustomer()).WithTags("Customers");
        endpoints.MapPost("/customers/sync-authenticated", CustomersHandler.LinkAnonymousCustomer()).WithTags("Customers");

        endpoints.MapGet("/customers/anonymous/{id}", CustomersHandler.GetAnonymousCustomer()).WithTags("Customers");
        endpoints.MapPost("/customers/anonymous", CustomersHandler.CreateAnonymousCustomer()).WithTags("Customers");

        endpoints.MapPost("/customers/update-visit/{id}", CustomersHandler.UpdateLastVisitInfo()).WithTags("Customers");
        endpoints.MapPut("/customers/{id}", CustomersHandler.UpdateCustomer()).WithTags("Customers");
    }
}
