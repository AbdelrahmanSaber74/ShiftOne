namespace ShiftOne.Shared.Constants
{
    public enum FilePathType
    {
        UserProfiles = 0
    }

    public static class FilePathConstants
    {
        public static readonly Dictionary<FilePathType, string> PathMappings = new()
        {
            { FilePathType.UserProfiles, $"uploads/UserProfiles/" }
        };
    }    
}



