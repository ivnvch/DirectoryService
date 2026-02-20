using System.Collections;

namespace DirectoryService.Shared.Errors;

public class Errors :  IEnumerable<Error>
{
    private readonly List<Error> _errors;

    public Errors(IEnumerable<Error> errors)
    {
        _errors = [..errors];
    }
    public IEnumerator<Error> GetEnumerator()
    {
        return _errors.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static implicit operator Errors(Error[] errors)
        => new(errors);

    public static implicit operator Errors(Error error)
        => new([error]);
}