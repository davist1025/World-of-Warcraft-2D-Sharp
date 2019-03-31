﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Network.Packet.OpCodes
{
    /// <summary>
    /// Opcodes used by the server.
    /// </summary>
    public enum ServerOpcodes
    {
        SMSG_LOGON                      =           0x01,
        SMSG_LOGON_SUCCESS              =           0x00,
        SMSG_LOGON_UNK                  =           0x10,
        SMSG_LOGON_FAILED               =           0x11,
        SMSG_LOGON_SERVER_ERROR         =           0x12,
        SMSG_LOGON_ALREADY_LOGGED_IN    =           0x13,
        
        SMSG_CHARACTER_LIST              =          0x02,

        SMSG_CHARACTER_CREATE           =           0x03,
        SMSG_CHARACTER_SUCCESS          =           0x00,
        SMSG_CHARACTER_EXISTS           =           0x01,
        SMSG_CHARACTER_SERVER_ERROR     =           0x02,

        SMSG_CHARACTER_DELETE           =           0x04,

        SMSG_REALMLIST                  =           0x10,
    }
}