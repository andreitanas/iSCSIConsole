/* Copyright (C) 2012-2016 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace ISCSI.Client
{
    public class ConnectionParameters
    {
        /// <summary>
        /// The default MaxRecvDataSegmentLength is used during Login
        /// </summary>
        public const int DefaultMaxRecvDataSegmentLength = 8192;
        public static int DeclaredMaxRecvDataSegmentLength = 262144;

        public ushort CID; // connection ID, generated by the initiator
        
        /// <summary>
        /// per direction parameter that the target or initator declares.
        /// maximum data segment length that the target (or initator) can receive in a single iSCSI PDU.
        /// </summary>
        public int InitiatorMaxRecvDataSegmentLength = DeclaredMaxRecvDataSegmentLength;
        public int TargetMaxRecvDataSegmentLength = DefaultMaxRecvDataSegmentLength;

        public bool StatusNumberingStarted;
        public uint ExpStatSN;
    }
}