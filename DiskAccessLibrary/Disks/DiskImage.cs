/* Copyright (C) 2014 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utilities;

namespace DiskAccessLibrary
{
    public abstract partial class DiskImage : Disk
    {
        // There is no way to specify sector size for IMG/VHD/VMDK.
        public const int DEFAULT_BYTES_PER_SECTOR = 512;

        private string m_path;

        public DiskImage(string diskImagePath)
        {
            m_path = diskImagePath;
        }

        public void CheckBoundaries(long sectorIndex, int sectorCount)
        {
            if (sectorIndex < 0 || sectorIndex + (sectorCount - 1) >= this.TotalSectors)
            {
                throw new ArgumentOutOfRangeException("Attempted to access data outside of disk");
            }
        }

        public abstract void Extend(long additionalNumberOfBytes);

        public abstract bool ExclusiveLock();

        public abstract bool ReleaseLock();

        public override int BytesPerSector
        {
            get
            {
                return DEFAULT_BYTES_PER_SECTOR;
            }
        }

        public string Path
        {
            get
            {
                return m_path;
            }
        }

        public static DiskImage GetDiskImage(string path)
        {
            if (path.EndsWith(".vhd", StringComparison.OrdinalIgnoreCase))
            {
                return new VirtualHardDisk(path);
            }
            else if (path.EndsWith(".vmdk", StringComparison.OrdinalIgnoreCase))
            {
                return new VirtualMachineDisk(path);
            }
            else
            {
                return new RawDiskImage(path);
            }
        }
    }
}
