namespace BTL_WEB.Services;

public interface IPasswordHasher
{
    bool Verify(string inputPassword, string storedPassword);
}
