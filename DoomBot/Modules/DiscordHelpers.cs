using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace DoomBot.Modules
{
    public static class DiscordHelpers
    {
        public static bool TryParseMsgLink(string MsgLink, out ulong ChannelID, out ulong MsgID)
        {
            var Span = MsgLink.AsSpan();

            var SearchSpan = Span;
            
            var Index = SearchSpan.LastIndexOf('/');

            if (Index == -1)
            {
                goto Fail;
            }

            var FirstSlashFromRight = Index;

            SearchSpan = SearchSpan.Slice(0, Index);
            
            Index = SearchSpan.LastIndexOf('/');

            if (Index == -1)
            {
                goto Fail;
            }

            var SecondSlashFromRightOffsetByOne = unchecked(Index + 1);

            var ChannelIDSpan = Span.Slice(SecondSlashFromRightOffsetByOne, FirstSlashFromRight - SecondSlashFromRightOffsetByOne);

            var MsgIDSpan = Span.Slice(unchecked(FirstSlashFromRight + 1));

            if (!ulong.TryParse(ChannelIDSpan, out ChannelID))
            {
                Unsafe.SkipInit(out MsgID);

                return false;
            }

            return ulong.TryParse(MsgIDSpan, out MsgID);

            Fail:
            {
                Unsafe.SkipInit(out ChannelID);
                
                Unsafe.SkipInit(out MsgID);

                return false;
            }
        }

        public static async Task<IMessage> TryParseMessageLinkAsync(SocketCommandContext Context, string MsgLink)
        {
            if (!TryParseMsgLink(MsgLink, out var ChannelID, out var MsgID))
            {
                return null;
            }

            var TC = Context.Guild.GetTextChannel(ChannelID);

            if (TC == null)
            {
                return null;
            }
            
            var Msg = await TC.GetMessageAsync(MsgID);

            return Msg;
        }

        public static void RemoveExtensionFromString(ref ReadOnlySpan<char> String)
        {
            var Index = String.LastIndexOf('.');

            if (Index == -1)
            {
                return;
            }

            String = String.Slice(0, Index);
        }
        
        public static void RemoveExtensionFromString(ref string String)
        {
            var Span = String.AsSpan();
            
            RemoveExtensionFromString(ref Span);

            String = Span.ToString();
        }
    }
}