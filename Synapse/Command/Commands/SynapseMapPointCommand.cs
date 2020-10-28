﻿namespace Synapse.Command.Commands
{
    [CommandInformations(
        Name = "MapPoint",
        Aliases = new [] { "GetMapPoint","MP" },
        Description = "A Command to get your Current Location as MapPoint",
        Permission = "synapse.command.mappoint",
        Platforms = new [] { Platform.RemoteAdmin },
        Usage = "Just use the Command and you get the Point"
        )]
    public class SynapseMapPointCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();
            if (!context.Player.HasPermission("synapse.command.mappoint"))
            {
                result.Message = "You dont have permissions to execute this command";
                result.State = CommandResultState.NoPermission;
                return result;
            }
            var point = context.Player.MapPoint;
            result.Message = "Your Current Position:" +
                $"  room: {point.Room.RoomName}" +
                $"  x: {point.RelativePosition.x}" +
                $"  y: {point.RelativePosition.y}" +
                $"  z: {point.RelativePosition.z}";
            result.State = CommandResultState.Ok;
            return result;
        }
    }
}
