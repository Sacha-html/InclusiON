namespace InclusiON.ApplicationBusiness.Interfaces.Infrastructure
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string providedPassword);    
    }
}
