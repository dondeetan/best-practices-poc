/*
 * Bridge
 * Description: Separates an abstraction from its implementation so both can vary independently.
 * Usage frequency: Occasional; useful when a feature has multiple dimensions of variation,
 * such as message formats and delivery channels.
 */

internal sealed class BridgePattern
{
    public string Description => "Send a release digest through a replaceable message sender.";

    public string UsageFrequency => "Occasional";

    public void Run()
    {
        ConsoleSection.Print("Bridge");

        var digest = new ReleaseDigest(new EmailMessageSender());
        Console.WriteLine(digest.Send("Blue/green swap completed."));
    }
}

internal interface IMessageSender
{
    string SendMessage(string body);
}

internal sealed class EmailMessageSender : IMessageSender
{
    public string SendMessage(string body) => $"Email sent: {body}";
}

internal abstract class MessageDigest(IMessageSender sender)
{
    public string Send(string content) => sender.SendMessage(Format(content));

    protected abstract string Format(string content);
}

internal sealed class ReleaseDigest(IMessageSender sender) : MessageDigest(sender)
{
    protected override string Format(string content) => $"[Release Digest] {content}";
}
