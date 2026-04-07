namespace StrategyPattern.Api.Domain.Users;

public abstract class Entity
{
    public readonly Guid Id;

    protected Entity()
    {
        
    }
    
    protected Entity(Guid id)
    {
        Id = id;
    }
}