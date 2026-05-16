using borealis_flowers.api.Data;
using borealis_flowers.api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Features.Requests;

public static class RequestsHandler
{
    public static Func<CreateRequestDto, DataContext, Task<IResult>> CreateRequest()
    {
        return async (CreateRequestDto dto, DataContext db) =>
        {
            var customer = await db.Customers.FindAsync(dto.CustomerId);
            if (customer == null)
                return Results.NotFound($"User with id: {dto.CustomerId} not found");

            var specialist = new Specialist
            {
                FullName = customer.Name,
                ImgUrl = "https://loremflickr.com/200/200?random=1",
                City = dto.City,
                Address = dto.Address,
                SpecializationId = dto.SpecializationId
            };
            await db.Specialists.AddAsync(specialist);

            var request = new Request
            {
                CustomerId = dto.CustomerId,
                SpecialistId = specialist.Id,
                State = State.Pending,
                CreatedAt = DateTime.UtcNow,
                Description = dto.Description
            };
            await db.Requests.AddAsync(request);
            await db.SaveChangesAsync();

            return Results.Ok(request.Id);
        };
    }

    public static Func<DataContext, Task<IResult>> GetAllRequests()
    {
        return async (DataContext db) =>
        {
            var requests = await db.Requests
                .Include(r => r.Customer)
                .Include(r => r.Specialist)
                    .ThenInclude(s => s!.Specialization)
                .Select(r => new RequestDetailDto
                {
                    Id = r.Id,
                    CustomerId = r.CustomerId,
                    CustomerName = r.Customer != null ? r.Customer.Name : null,
                    SpecialistId = r.SpecialistId,
                    State = r.State,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    Description = r.Description,
                    Resolution = r.Resolution,
                    City = r.Specialist != null ? r.Specialist.City : null,
                    Address = r.Specialist != null ? r.Specialist.Address : null,
                    SpecializationId = r.Specialist != null ? r.Specialist.SpecializationId : null,
                    SpecializationName = r.Specialist != null && r.Specialist.Specialization != null 
                        ? r.Specialist.Specialization.Name : null
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Results.Ok(requests);
        };
    }

    public static Func<string, DataContext, Task<IResult>> GetRequestsByState()
    {
        return async (string state, DataContext db) =>
        {
            if (!Enum.TryParse<State>(state, true, out var stateEnum))
            {
                return Results.BadRequest($"Invalid state value: {state}. Valid values are: {string.Join(", ", Enum.GetNames<State>())}");
            }

            var requests = await db.Requests
                .Include(r => r.Customer)
                .Include(r => r.Specialist)
                    .ThenInclude(s => s!.Specialization)
                .Where(r => r.State == stateEnum)
                .Select(r => new RequestDetailDto
                {
                    Id = r.Id,
                    CustomerId = r.CustomerId,
                    CustomerName = r.Customer != null ? r.Customer.Name : null,
                    SpecialistId = r.SpecialistId,
                    State = r.State,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    Description = r.Description,
                    Resolution = r.Resolution,
                    City = r.Specialist != null ? r.Specialist.City : null,
                    Address = r.Specialist != null ? r.Specialist.Address : null,
                    SpecializationId = r.Specialist != null ? r.Specialist.SpecializationId : null,
                    SpecializationName = r.Specialist != null && r.Specialist.Specialization != null 
                        ? r.Specialist.Specialization.Name : null
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Results.Ok(requests);
        };
    }

    public static Func<Guid, UpdateRequestDto, DataContext, Task<IResult>> UpdateRequest()
    {
        return async (Guid id, UpdateRequestDto dto, DataContext db) =>
        {
            var request = await db.Requests.FindAsync(id);
            if (request is null)
            {
                return Results.NotFound($"Request with id {id} was not found");
            }

            var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                request.State = dto.State;
                request.UpdatedAt = DateTime.UtcNow;
                request.Resolution = dto.Resolution;

                await db.SaveChangesAsync();

                // IN case of Approved change IsMaster flag in Customer table
                // Specialist IsActive = true should be set in Specialists table
                if (dto.State == State.Approved)
                {
                    // Set Customer's IsMaster flag cause request was approved
                    var relatedCustomer = await db.Customers.FindAsync(request.CustomerId);
                    if (relatedCustomer != null)
                    {
                        relatedCustomer.IsMaster = true;
                        db.Customers.Update(relatedCustomer);
                    }

                    // Set Specialist IsActive cause fo approval stated of request
                    var relatedSpecialist = await db.Specialists.FindAsync(request.SpecialistId);
                    if (relatedSpecialist != null)
                    {
                        relatedSpecialist.IsActive = true;
                        db.Specialists.Update(relatedSpecialist);
                    }

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Results.Problem($"Error updating request: {ex.Message}" );
            }

            return Results.Ok(new RequestDto
            {
                Id = request.Id,
                CustomerId = request.CustomerId,
                SpecialistId = request.SpecialistId,
                State = request.State,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt,
                Description = request.Description,
                Resolution = request.Resolution
            });
        };
    }
}
