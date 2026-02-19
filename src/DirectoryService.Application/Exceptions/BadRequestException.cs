namespace DirectoryService.Application.Exceptions;

public class BadRequestException : Exception
{
    protected BadRequestException(string error) : base(error)
    {
        
    }

    protected BadRequestException(IEnumerable<string>  errors) : base(string.Join(", ", errors))
    {
        
    }
}