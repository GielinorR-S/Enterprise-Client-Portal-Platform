using System.Security.Cryptography;
using System.Text;

namespace HelixPortal.Api.Auth;

/// <summary>
/// Password hasher using HMACSHA256 with random salt.
/// Stores salt + hashed password as a single base64 string.
/// </summary>
public class PasswordHasher
{
    private const int SaltSize = 32; // 256 bits
    private const int HashSize = 32; // 256 bits

    /// <summary>
    /// Hashes a password using HMACSHA256 with a random salt.
    /// Returns base64 encoded string containing salt + hash.
    /// </summary>
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        // Generate random salt
        byte[] salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Compute HMACSHA256 hash using salt as key
        byte[] hash;
        using (var hmac = new HMACSHA256(salt))
        {
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        // Combine salt + hash into single array
        byte[] saltAndHash = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, saltAndHash, 0, SaltSize);
        Array.Copy(hash, 0, saltAndHash, SaltSize, HashSize);

        // Return as base64 string
        return Convert.ToBase64String(saltAndHash);
    }

    /// <summary>
    /// Verifies a password against a stored hash.
    /// Recomputes the hash and compares with stored value.
    /// </summary>
    public bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;
        
        if (string.IsNullOrWhiteSpace(storedHash))
            return false;

        try
        {
            // Decode base64 string
            byte[] saltAndHash = Convert.FromBase64String(storedHash);
            
            if (saltAndHash.Length != SaltSize + HashSize)
                return false;

            // Extract salt and stored hash
            byte[] salt = new byte[SaltSize];
            byte[] storedHashBytes = new byte[HashSize];
            Array.Copy(saltAndHash, 0, salt, 0, SaltSize);
            Array.Copy(saltAndHash, SaltSize, storedHashBytes, 0, HashSize);

            // Recompute hash using stored salt
            byte[] computedHash;
            using (var hmac = new HMACSHA256(salt))
            {
                computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }

            // Compare hashes (constant-time comparison to prevent timing attacks)
            return ConstantTimeEquals(computedHash, storedHashBytes);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Constant-time comparison to prevent timing attacks.
    /// </summary>
    private static bool ConstantTimeEquals(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;

        int result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }
}
