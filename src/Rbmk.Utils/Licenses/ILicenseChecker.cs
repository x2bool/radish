namespace Rbmk.Utils.Licenses
{
    public interface ILicenseChecker
    {
        bool CheckSignature(License license);

        string GenerateSignature(License license);
    }
}