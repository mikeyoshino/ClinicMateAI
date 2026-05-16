namespace ClinicMateAI.Domain.Errors;

public sealed class BusinessException : Exception
{
    public BusinessErrorCode Code { get; }

    public BusinessException(BusinessErrorCode code)
        : base(code.ToString())
    {
        Code = code;
    }

    public BusinessException(BusinessErrorCode code, string detail)
        : base($"{code}: {detail}")
    {
        Code = code;
    }
}
