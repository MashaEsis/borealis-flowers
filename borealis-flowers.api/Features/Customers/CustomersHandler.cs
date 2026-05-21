using borealis_flowers.api.Data;
using borealis_flowers.api.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Security.Claims;
using borealis_flowers.api.Data.Models;

namespace borealis_flowers.api.Features.Customers;

public static class CustomersHandler
{
    public static Func<DataContext, Task<List<CustomerDto>>> GetCustomers()
    {
        return async (DataContext db) => await db.Customers
            .Select(s => new CustomerDto
            {
                Id = s.Id,
                Name = s.Name,
                Email = s.Email,
                Phone = s.Phone,
                IsAdmin = s.IsAdmin,
                IsMaster = s.IsMaster,
                FirstVisit = s.FirstVisit,
                LastVisit = s.LastVisit,

            }).ToListAsync();
    }

    public static Func<string, DataContext, Task<IResult>> GetCustomer()
    {
        return async (string id, DataContext db) =>
        {
            var result = await db.Customers
                .FirstOrDefaultAsync(x => x.ExternalUserId == id);

            return result != null
                ? Results.Ok(result)
                : Results.NotFound();
        };
    }

    public static Func<string, DataContext, Task<IResult>> GetAnonymousCustomer()
    {
        return async (string id, DataContext db) =>
        {
            var result = await db.Customers
                .FirstOrDefaultAsync(x => x.ExternalUserId == id && x.IsAnonymous);

            return result != null
                ? Results.Ok(result)
                : Results.NotFound();
        };
    }

    public static Func<string, DataContext, Task<IResult>> GetCustomerIdByExternalVisitorId()
    {
        return async (string id, DataContext db) =>
        {
            var customer = await db.Customers
                .FirstOrDefaultAsync(x => x.VisitorId == id && x.IsAnonymous);

            if (customer != null)
                return Results.Ok(customer.Id);

            // Fallback: look up by email if the customer was seeded without ExternalUserId
            // (id might also be an email in some flows, but typically we search all customers
            //  that have no ExternalUserId and match later during CreateCustomer)
            return Results.NotFound();
        };
    }

    public static Func<string, DataContext, Task<IResult>> GetCustomerIdByExternalUserId()
    {
        return async (string id, DataContext db) =>
        {
            var customer = await db.Customers
                .FirstOrDefaultAsync(x => x.ExternalUserId == id);

            if (customer != null)
                return Results.Ok(customer.Id);

            // Fallback: look up by email if the customer was seeded without ExternalUserId
            // (id might also be an email in some flows, but typically we search all customers
            //  that have no ExternalUserId and match later during CreateCustomer)
            return Results.NotFound();
        };
    }

    public static Func<string, DataContext, Task<IResult>> UpdateLastVisitInfo()
    {
        return async (string id, DataContext db) =>
        {
            try
            {
                var updatedCount = await db.Customers
                    .Where(x => x.ExternalUserId == id)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(c => c.LastVisit, DateTime.UtcNow)
                    );

                return Results.Ok(new { success = true, message = "Last visit updated successfully" });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Error updating last visit",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        };
    }
    public static Func<AnonymousCustomer?, DataContext, ClaimsPrincipal, Task<IResult>> CreateAnonymousCustomer()
    {
        return async (AnonymousCustomer? anonymousCustomer, DataContext db, ClaimsPrincipal user) =>
        {
            try
            {
                if (await HandleExistsConflict(anonymousCustomer, db) is IResult conflict and not null)
                    return conflict;

                var customer = new Customer
                {
                    Phone = anonymousCustomer.Phone,
                    Name = anonymousCustomer.FullName,
                    ExternalUserId = anonymousCustomer.ExternalUserId,
                    IsAnonymous = anonymousCustomer.IsAnonymous
                };

                await db.Customers.AddAsync(customer);
                await db.SaveChangesAsync();

                return Results.Ok(customer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(MethodBase.GetCurrentMethod()?.ReflectedType?.FullName);
                Console.WriteLine(ex.Message);
                return Results.Problem($"Try to create anonymous customer with ExternalUserId: {anonymousCustomer?.ExternalUserId} was failed. Details: {ex.Message}");
            }
        };
    }

    private static async Task<IResult?> HandleExistsConflict(AnonymousCustomer? anonymousCustomer, DataContext db)
    {
        if (anonymousCustomer is null)
        {
            return Results.BadRequest();
        }

        var existingCustomer = await db.Customers
            .FirstOrDefaultAsync(x => x.ExternalUserId == anonymousCustomer.ExternalUserId);

        if (existingCustomer != null)
        {
            return Results.Conflict(new
            {
                message = "Customer already exists",
                customer = existingCustomer
            });
        }

        return null;
    }

    public static Func<TokenUser, DataContext, ClaimsPrincipal, Task<IResult>> CreateCustomer()
    {
        return async (TokenUser tokenUser, DataContext db, ClaimsPrincipal user) =>
        {
            try
            {
                if (tokenUser is null)
                    return Results.BadRequest();

                if (string.IsNullOrWhiteSpace(tokenUser.Id))
                    return Results.BadRequest("Firebase UID is required");

                var existingCustomer = await db.Customers
                    .FirstOrDefaultAsync(x => x.ExternalUserId == tokenUser.Id);

                if (existingCustomer != null)
                {
                    return Results.Conflict(new
                    {
                        message = "Customer already exists",
                        customer = existingCustomer
                    });
                }

                // Check if a customer with same email exists but without ExternalUserId (e.g. seeded)
                var customerByEmail = await db.Customers
                    .FirstOrDefaultAsync(x => x.Email == tokenUser.Email && x.ExternalUserId == null);

                if (customerByEmail != null)
                {
                    customerByEmail.ExternalUserId = tokenUser.Id;
                    if (!string.IsNullOrWhiteSpace(tokenUser.FullName))
                        customerByEmail.Name = tokenUser.FullName;
                    await db.SaveChangesAsync();

                    return Results.Ok(customerByEmail);
                }

                var customer = new Customer()
                {
                    Phone = tokenUser.Phone,
                    Email = tokenUser.Email,
                    Name = !string.IsNullOrWhiteSpace(tokenUser.FullName)
                        ? tokenUser.FullName
                        : tokenUser.Email,
                    ExternalUserId = tokenUser.Id,
                    IsAdmin = false,
                    IsMaster = false,
                    FirstVisit = DateTime.UtcNow,
                    LastVisit = DateTime.UtcNow,
                    IsAnonymous = tokenUser.IsAnonymous,
                };

                await db.Customers.AddAsync(customer);
                await db.SaveChangesAsync();

                return Results.Ok(customer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(MethodBase.GetCurrentMethod()?.ReflectedType?.FullName);
                Console.WriteLine(ex.Message);
                return Results.Problem();
            }
        };
    }

    public static Func<LinkAnonymousCustomerRequest, DataContext, ClaimsPrincipal, Task<IResult>> LinkAnonymousCustomer()
    {
        return async (LinkAnonymousCustomerRequest request, DataContext db, ClaimsPrincipal user) =>
        {
            if (request is null)
                return Results.BadRequest();

            if (string.IsNullOrWhiteSpace(request.FirebaseUserId))
                return Results.BadRequest("Firebase UID is required");

            var normalizedEmail = string.IsNullOrWhiteSpace(request.Email)
                ? null
                : request.Email.Trim();

            var normalizedAnonymousExternalId = string.IsNullOrWhiteSpace(request.AnonymousExternalUserId)
                ? null
                : request.AnonymousExternalUserId.Trim();

            await using var tx = await db.Database.BeginTransactionAsync();

            var targetCustomer = await db.Customers
                .FirstOrDefaultAsync(x => x.ExternalUserId == request.FirebaseUserId);

            if (targetCustomer is null && !string.IsNullOrWhiteSpace(normalizedEmail))
            {
                targetCustomer = await db.Customers
                    .OrderBy(x => x.IsAnonymous)
                    .FirstOrDefaultAsync(x => x.Email == normalizedEmail);
            }

            if (targetCustomer is null)
            {
                targetCustomer = new Customer
                {
                    Id = Guid.NewGuid(),
                    Name = !string.IsNullOrWhiteSpace(request.FullName)
                        ? request.FullName
                        : normalizedEmail ?? "Anonymous",
                    Email = normalizedEmail,
                    Phone = request.Phone,
                    ExternalUserId = request.FirebaseUserId,
                    IsAnonymous = false,
                    IsAdmin = false,
                    IsMaster = false,
                    FirstVisit = DateTime.UtcNow,
                    LastVisit = DateTime.UtcNow
                };

                await db.Customers.AddAsync(targetCustomer);
                await db.SaveChangesAsync();
            }

            var historyExternalIdsToMigrate = new HashSet<string>(StringComparer.Ordinal);

            if (!string.IsNullOrWhiteSpace(targetCustomer.ExternalUserId) &&
                !string.Equals(targetCustomer.ExternalUserId, request.FirebaseUserId, StringComparison.Ordinal))
            {
                historyExternalIdsToMigrate.Add(targetCustomer.ExternalUserId);
            }

            var anonymousCustomersToMerge = await db.Customers
                .Where(x =>
                    x.Id != targetCustomer.Id &&
                    x.IsAnonymous &&
                    (
                        (!string.IsNullOrWhiteSpace(normalizedAnonymousExternalId) &&
                         x.ExternalUserId == normalizedAnonymousExternalId) ||
                        (!string.IsNullOrWhiteSpace(normalizedEmail) && x.Email == normalizedEmail)
                    ))
                .ToListAsync();

            var sourceCustomerIds = anonymousCustomersToMerge
                .Select(x => x.Id)
                .Distinct()
                .ToList();

            foreach (var sourceCustomer in anonymousCustomersToMerge)
            {
                if (!string.IsNullOrWhiteSpace(sourceCustomer.ExternalUserId) &&
                    !string.Equals(sourceCustomer.ExternalUserId, request.FirebaseUserId, StringComparison.Ordinal))
                {
                    historyExternalIdsToMigrate.Add(sourceCustomer.ExternalUserId);
                }
            }

            if (!string.IsNullOrWhiteSpace(normalizedAnonymousExternalId) &&
                !string.Equals(normalizedAnonymousExternalId, request.FirebaseUserId, StringComparison.Ordinal))
            {
                historyExternalIdsToMigrate.Add(normalizedAnonymousExternalId);
            }

            if (sourceCustomerIds.Count > 0)
            {
                await db.Timeslots
                    .Where(x => x.CustomerId.HasValue && sourceCustomerIds.Contains(x.CustomerId.Value))
                    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.CustomerId, _ => targetCustomer.Id));
            }

            var historyExternalIds = historyExternalIdsToMigrate.ToList();
            if (historyExternalIds.Count > 0)
            {
                await db.HistoryTimeslots
                    .Where(x => historyExternalIds.Contains(x.ExternalUserId))
                    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ExternalUserId, _ => request.FirebaseUserId));
            }

            targetCustomer.ExternalUserId = request.FirebaseUserId;
            targetCustomer.IsAnonymous = false;
            targetCustomer.LastVisit = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(request.FullName))
                targetCustomer.Name = request.FullName;
            if (!string.IsNullOrWhiteSpace(normalizedEmail))
                targetCustomer.Email = normalizedEmail;
            if (!string.IsNullOrWhiteSpace(request.Phone))
                targetCustomer.Phone = request.Phone;

            if (sourceCustomerIds.Count > 0)
            {
                await db.Customers
                    .Where(x => sourceCustomerIds.Contains(x.Id))
                    .ExecuteDeleteAsync();
            }

            try
            {
                await db.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                Console.WriteLine($"Error committing transaction in LinkAnonymousCustomer: {ex.Message}");
                return Results.Problem(ex.Message);
            }

            return Results.Ok(targetCustomer);
        };
    }

    public static Func<Guid, Customer, DataContext, Task<IResult>> UpdateCustomer()
    {
        return async (Guid id, Customer updatedCustomer, DataContext db) =>
        {
            var customer = await db.Customers.FindAsync(id);
            if (customer == null)
                return Results.NotFound($"Customer with ID {id} not found");

            customer.Name = updatedCustomer.Name;
            //TODO: investigate is it necessary to update Email or not customer.Email = updatedCustomer.Email;

            customer.Phone = updatedCustomer.Phone;
            customer.Birthday = updatedCustomer.Birthday;
            customer.LastVisit = DateTime.UtcNow;

            //TODO: IsAdmin and IsMaster should be updated separately in other endpoint with proper authorization, but for now we can update them here as well
            await db.SaveChangesAsync();
            return Results.Ok();
        };
    }
}
