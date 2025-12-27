using Echos.Api.Domain.Users;

namespace Echos.Api.Domain.Echos
{
    public class Echo
    {
        public long Id { get; private set; }
        public long UserId { get; private set; }
        public string Content { get; private set; }
        public DateTime CreationTime { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletionTime { get; private set; }
        public long? DeleterUserId { get; private set; }
        public User User { get; private set; } = null!;
        
        private Echo() { } // EF

        public Echo(long userId, string content)
        {
            UserId = userId;
            Content = content;
            CreationTime = DateTime.UtcNow;
            IsDeleted = false;
        }

        public void Delete(long DeleterUserId) {
            IsDeleted = true;
            DeletionTime = DateTime.UtcNow;
            this.DeleterUserId = DeleterUserId;
        }
    }
}
