namespace ShiftOne.Shared.Constants
{
    public enum Roles
    {
        SuperAdmin = 0,
        CompanyAdmin = 1,
        HR = 2,
        Employee = 3,

        // Kept only for compatibility with older database rows/tokens during early setup.
        Admin = 100,
        Customer = 101,
        Vendor = 102
    }
}
