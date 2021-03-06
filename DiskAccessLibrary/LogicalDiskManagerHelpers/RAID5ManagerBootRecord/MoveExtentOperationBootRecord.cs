/* Copyright (C) 2014 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.Text;
using Utilities;

namespace DiskAccessLibrary
{
    public class MoveExtentOperationBootRecord : RAID5ManagerBootRecord
    {
        public Guid VolumeGuid; // offset 16
        public ulong NumberOfCommittedSectors; // for an array, this would be the total of all sectors that can be now read from the new array
        public ulong ExtentID;
        public ulong OldStartSector;
        public ulong NewStartSector;
        public ulong BootRecordBackupSector;
        public ulong BackupBufferStartSector;
        public uint BackupBufferSizeLBA;
        public bool RestoreFromBuffer;
        public bool RestoreRAID5;

        public MoveExtentOperationBootRecord()
        {
            Operation = RAID5ManagerOperation.MoveExtent;
        }

        public MoveExtentOperationBootRecord(byte[] buffer) : base(buffer)
        { 

        }

        protected override void ReadOperationParameters(byte[] buffer, int offset)
        {
 	        VolumeGuid = BigEndianConverter.ToGuid(buffer, offset + 0);
            NumberOfCommittedSectors = BigEndianConverter.ToUInt64(buffer, offset + 16);
            ExtentID = BigEndianConverter.ToUInt64(buffer, offset + 24);
            OldStartSector = BigEndianConverter.ToUInt64(buffer, offset + 32);
            NewStartSector = BigEndianConverter.ToUInt64(buffer, offset + 40);
            BootRecordBackupSector = BigEndianConverter.ToUInt64(buffer, offset + 48);
            BackupBufferStartSector = BigEndianConverter.ToUInt64(buffer, offset + 56);
            BackupBufferSizeLBA = BigEndianConverter.ToUInt32(buffer, offset + 64);
            RestoreFromBuffer = ByteReader.ReadByte(buffer, offset + 68) == 1;
            RestoreRAID5 = ByteReader.ReadByte(buffer, offset + 69) == 1;
        }

        protected override void WriteOperationParameters(byte[] buffer, int offset)
        {
            BigEndianWriter.WriteGuidBytes(buffer, offset + 0, VolumeGuid);
            BigEndianWriter.WriteUInt64(buffer, offset + 16, NumberOfCommittedSectors);

            BigEndianWriter.WriteUInt64(buffer, offset + 24, ExtentID);
            BigEndianWriter.WriteUInt64(buffer, offset + 32, OldStartSector);
            BigEndianWriter.WriteUInt64(buffer, offset + 40, NewStartSector);
            BigEndianWriter.WriteUInt64(buffer, offset + 48, BootRecordBackupSector);
            BigEndianWriter.WriteUInt64(buffer, offset + 56, BackupBufferStartSector);
            BigEndianWriter.WriteUInt64(buffer, offset + 64, BackupBufferSizeLBA);
            ByteWriter.WriteByte(buffer, offset + 68, Convert.ToByte(RestoreFromBuffer));
            ByteWriter.WriteByte(buffer, offset + 69, Convert.ToByte(RestoreRAID5));
        }
    }
}
