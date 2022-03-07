// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class Id
{
    public string server { get; set; }
    public string user { get; set; }
    public string _serialized { get; set; }
    public bool fromMe { get; set; }
    public Remote remote { get; set; }
    public string id { get; set; }
}

public class Remote
{
    public string server { get; set; }
    public string user { get; set; }
    public string _serialized { get; set; }
}

public class LastReceivedKey
{
    public bool fromMe { get; set; }
    public Remote remote { get; set; }
    public string id { get; set; }
    public string _serialized { get; set; }
}

public class TcToken
{
}

public class From
{
    public string server { get; set; }
    public string user { get; set; }
    public string _serialized { get; set; }
}

public class To
{
    public string server { get; set; }
    public string user { get; set; }
    public string _serialized { get; set; }
}

public class ScansSidecar
{
}

public class Msg
{
    public Id id { get; set; }
    public int rowId { get; set; }
    public string body { get; set; }
    public string type { get; set; }
    public int t { get; set; }
    public From from { get; set; }
    public To to { get; set; }
    public string self { get; set; }
    public int ack { get; set; }
    public bool invis { get; set; }
    public bool star { get; set; }
    public bool isFromTemplate { get; set; }
    public List<object> mentionedJidList { get; set; }
    public bool isVcardOverMmsDocument { get; set; }
    public bool isForwarded { get; set; }
    public List<object> labels { get; set; }
    public bool hasReaction { get; set; }
    public bool productHeaderImageRejected { get; set; }
    public int lastPlaybackProgress { get; set; }
    public bool isDynamicReplyButtonsMsg { get; set; }
    public bool isMdHistoryMsg { get; set; }
    public bool requiresDirectConnection { get; set; }
    public bool pttForwardedFeaturesEnabled { get; set; }
    public bool? broadcast { get; set; }
    public bool? ephemeralOutOfSync { get; set; }
    public List<object> interactiveAnnotations { get; set; }
    public string deprecatedMms3Url { get; set; }
    public string directPath { get; set; }
    public string mimetype { get; set; }
    public string filehash { get; set; }
    public string encFilehash { get; set; }
    public int? size { get; set; }
    public string mediaKey { get; set; }
    public int? mediaKeyTimestamp { get; set; }
    public bool? isViewOnce { get; set; }
    public int? width { get; set; }
    public int? height { get; set; }
    public string staticUrl { get; set; }
    public List<int> scanLengths { get; set; }
    public ScansSidecar scansSidecar { get; set; }
}

public class Chat
{
    public Id id { get; set; }
    public List<object> labels { get; set; }
    public LastReceivedKey lastReceivedKey { get; set; }
    public int t { get; set; }
    public int unreadCount { get; set; }
    public int muteExpiration { get; set; }
    public bool notSpam { get; set; }
    public int ephemeralDuration { get; set; }
    public string disappearingModeInitiator { get; set; }
    public int unreadMentionCount { get; set; }
    public bool hasUnreadMention { get; set; }
    public bool archiveAtMentionViewedInDrawer { get; set; }
    public bool hasChatBeenOpened { get; set; }
    public TcToken tcToken { get; set; }
    public int? tcTokenTimestamp { get; set; }
    public int endOfHistoryTransferType { get; set; }
    public bool pendingInitialLoading { get; set; }
    public List<Msg> msgs { get; set; }
}

public class ChatStore
{
    public List<Chat> chat { get; set; }
}