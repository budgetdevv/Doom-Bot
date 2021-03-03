using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace DiscordNetTemplate.PreconditionalAttributes
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
  public class RequireDoomOwnerAttribute: PreconditionAttribute
  {
    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext Context, CommandInfo Command, IServiceProvider Services)
    {
      return Task.FromResult(Context.User.Id == 773209210904903680 ? PreconditionResult.FromSuccess() : PreconditionResult.FromError(":negative_squared_cross_mark: | Only the `Bot Owner` may execute this command!"));
    }
  }
}
