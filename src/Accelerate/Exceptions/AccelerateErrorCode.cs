namespace Accelerate.Exceptions;

public enum AccelerateErrorCode
{
    None,

    Undefined,

    GitCommandExecutionFailed,

    ParsingMarkdownDocumentFailed,

    MarkdownDocumentHeadingMissing,

    AuthenticationCredentialsMissing,

    CreatingPullRequestFailed
}