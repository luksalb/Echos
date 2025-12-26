namespace XClone.Api.Domain.Users;

public class User
{
    public Guid Id { get; private set; }

    public string UserName { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }

    public string PasswordHash { get; private set; }

    public DateTime CreationTime { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletionTime { get; private set; }
    public Guid? DeleterUserId { get; private set; }

    private User() { } // EF

    public User(string userName, string name, string email, string passwordHash)
    {
        Id = Guid.NewGuid();
        UserName = userName;
        Name = name;
        Email = email;
        PasswordHash = passwordHash;

        CreationTime = DateTime.UtcNow;
        IsDeleted = false;
    }

    public void Delete(Guid deleterUserId)
    {
        IsDeleted = true;
        DeletionTime = DateTime.UtcNow;
        DeleterUserId = deleterUserId;
    }
}
