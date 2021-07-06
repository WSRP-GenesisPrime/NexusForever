using NexusForever.Shared.Network;

namespace NexusForever.WorldServer.Game.Entity.Network.Command
{
    [EntityCommand(EntityCommand.SetRotation)]
    public class SetRotationCommand : IEntityCommandModel
    {
        public Position Rotation { get; set; }
        public bool Blend { get; set; }

        public void Read(GamePacketReader reader)
        {
            Rotation = new Position();
            Rotation.Read(reader);
            Blend = reader.ReadBit();
        }

        public void Write(GamePacketWriter writer)
        {
            Rotation.Write(writer);
            writer.Write(Blend);
        }
    }
}
