using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.CharacterCache;
using NexusForever.WorldServer.Game.Contact.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NLog;

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class ContactsHandler
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [MessageHandler(GameMessageOpcode.ClientContactsStatusChange)]
        public static void HandleStatusChange(WorldSession session, ClientContactsStatusChange request)
        {
            // Respond with 0x03AA
            session.EnqueueMessageEncrypted(new ServerContactsSetPresence
            {
                AccountId = session.Account.Id,
                Presence = request.Presence
            });
        }

        [MessageHandler(GameMessageOpcode.ClientContactsRequestAdd)]
        public static void HandleContactRequestAdd(WorldSession session, ClientContactsRequestAdd request)
        {
            if(request.PlayerName == session.Player.Name)
            {
                session.Player.ContactManager.SendContactsResult(ContactResult.CannotInviteSelf);
                return;
            }

            ulong? characterId = CharacterManager.Instance.GetCharacterIdByName(request.PlayerName);
            if (characterId != null && characterId > 0)
            {
                // TODO: Handle Rival, Ignore, and Account Requests
                if (request.Type == ContactType.Account)
                    session.Player.ContactManager.SendContactsResult(ContactResult.UnableToProcess);
                else
                    switch (request.Type)
                    {
                        case ContactType.Friend:
                            session.Player.ContactManager.CreateFriendRequest((ulong)characterId, request.Message);
                            break;
                        case ContactType.Rival:
                            session.Player.ContactManager.CreateRival((ulong)characterId);
                            break;
                        case ContactType.Ignore:
                            session.Player.ContactManager.CreateIgnored((ulong)characterId);
                            break;
                        default:
                            session.Player.ContactManager.SendContactsResult(ContactResult.InvalidType);
                            break;
                    }

                return;
            }
            else
                session.Player.ContactManager.SendContactsResult(ContactResult.PlayerNotFound);
        }

        [MessageHandler(GameMessageOpcode.ClientContactsRequestResponse)]
        public static void HandleRequestResponse(WorldSession session, ClientContactsRequestResponse request)
        {
            switch (request.Response)
            {
                case ContactResponse.Mutual:
                    session.Player.ContactManager.AcceptFriendRequest(request.ContactId, true);
                    break;
                case ContactResponse.Accept:
                    session.Player.ContactManager.AcceptFriendRequest(request.ContactId);
                    break;
                case ContactResponse.Decline:
                    session.Player.ContactManager.DeclineFriendRequest(request.ContactId);
                    break;
                case ContactResponse.Ignore:
                    session.Player.ContactManager.DeclineFriendRequest(request.ContactId, true);
                    break;
            }
        }

        [MessageHandler(GameMessageOpcode.ClientContactsRequestDelete)]
        public static void HandleDeleteResponse(WorldSession session, ClientContactsRequestDelete request)
        {
            session.Player.ContactManager.DeleteContact(request.PlayerIdentity, request.Type);
        }

        [MessageHandler(GameMessageOpcode.ClientContactsSetNote)]
        public static void HandleModifyPrivateNote(WorldSession session, ClientContactsSetNote request)
        {
            session.Player.ContactManager.SetPrivateNote(request.PlayerIdentity, request.Note);
        }

        //[MessageHandler(GameMessageOpcode.Client03BF)]
        //public static void Handle03BF(WorldSession session, Client03BF request)
        //{
        //    // Correct response according to parses. No idea what it does, currently.
        //    session.EnqueueMessageEncrypted(new Server03C0
        //    {
        //        Unknown0 = 1,
        //        Unknown1 = new byte[] { 21, 70, 14, 0, 0, 0, 0, 26 },
        //        Unknown2 = new byte[] { 18, 5, 0, 0 }
        //    });
        //}
    }
}
