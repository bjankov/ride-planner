namespace RydePlannr.Domain.Exceptions;

public class DuplicateFieldException : Exception
{
    public string FieldName { get; }

    public DuplicateFieldException(string fieldName, string message) : base(message)
    {
        FieldName = fieldName;
    }
}