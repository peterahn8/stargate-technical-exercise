namespace StargateAPI.Business.Logging
{
    /// <summary>
    /// Marks a request the process log skips. Used for reading the log itself, so refreshing the
    /// activity feed doesn't fill the feed with its own reads (T41).
    /// </summary>
    public interface IUnloggedRequest
    {
    }
}
