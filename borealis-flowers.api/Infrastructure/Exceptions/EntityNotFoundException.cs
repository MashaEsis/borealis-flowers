namespace borealis_flowers.api.Infrastructure.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entityType, Guid id) :
        base($"Entity of type: {entityType} with ID: {id} was not found.")
    {
    }

    public EntityNotFoundException(string entityType, string name) :
        base($"{entityType} with name: {name} was not found.")
    {

    }
}
