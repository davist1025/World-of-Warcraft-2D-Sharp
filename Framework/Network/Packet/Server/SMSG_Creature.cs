﻿using Framework.Entity;
using Framework.Network.Packet.OpCodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Network.Packet.Server
{
    /// <summary>
    /// Describes the state of a creature.
    /// </summary>
    public class SMSG_Creature : IPacket
    {
        public enum CreatureState : byte
        {
            Add = 0x0,
            Move = 0x01,
            MoveStop = 0x02,
        }

        public WorldCreature Creature;
        public CreatureState State;

        public SMSG_Creature() : base((byte)ServerOpcodes.Opcodes.SMSG_CREATURE) { }

        public override byte[] Serialize()
        {
            var formatter = new BinaryFormatter();
            using (var memStr = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memStr))
                {
                    writer.Write(_opcode);
                    writer.Write((byte)State);
                    formatter.Serialize(memStr, Creature);
                }
                return memStr.ToArray();
            }
        }

        public override IPacket Deserialize(byte[] data)
        {
            var obj = new SMSG_Creature();
            var formatter = new BinaryFormatter();
            using (var memStr = new MemoryStream(data))
            {
                using (var reader = new BinaryReader(memStr))
                {
                    reader.ReadByte();
                    obj.State = (CreatureState)reader.ReadByte();
                    obj.Creature = (WorldCreature)formatter.Deserialize(memStr);
                }
            }
            return obj;
        }
    }
}