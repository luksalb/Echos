// Domain/Users/LoginAttempt.cs
public class LoginAttemptTracker
{
    private static readonly Dictionary<string, LoginAttemptInfo> _attempts = new();
    private static readonly TimeSpan _lockoutDuration = TimeSpan.FromMinutes(15);
    private const int _maxAttempts = 5;

    public class LoginAttemptInfo
    {
        public int FailedAttempts { get; set; }
        public DateTime? LockoutEnd { get; set; }
    }

    public bool IsLockedOut(string identifier)
    {
        if (_attempts.TryGetValue(identifier, out var info))
        {
            if (info.LockoutEnd.HasValue && info.LockoutEnd.Value > DateTime.UtcNow)
            {
                return true;
            }

            // Lockout expirou, resetar
            if (info.LockoutEnd.HasValue)
            {
                _attempts.Remove(identifier);
            }
        }

        return false;
    }

    public void RecordFailedAttempt(string identifier)
    {
        if (!_attempts.ContainsKey(identifier))
        {
            _attempts[identifier] = new LoginAttemptInfo();
        }

        _attempts[identifier].FailedAttempts++;

        if (_attempts[identifier].FailedAttempts >= _maxAttempts)
        {
            _attempts[identifier].LockoutEnd = DateTime.UtcNow.Add(_lockoutDuration);
        }
    }

    public void ResetAttempts(string identifier)
    {
        _attempts.Remove(identifier);
    }

    public (int current, int max, DateTime? lockoutEnd) GetAttemptInfo(string identifier)
    {
        if (_attempts.TryGetValue(identifier, out var info))
        {
            return (info.FailedAttempts, _maxAttempts, info.LockoutEnd);
        }

        return (0, _maxAttempts, null);
    }
}