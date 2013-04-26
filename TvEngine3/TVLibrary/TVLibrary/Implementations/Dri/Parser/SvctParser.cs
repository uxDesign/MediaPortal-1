#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;

namespace TvLibrary.Implementations.Dri.Parser
{
  public class SvctParser
  {
    private enum TableSubtype
    {
      VirtualChannelMap = 0,
      DefinedChannelMap,
      InverseChannelMap
    }

    private enum TransportType
    {
      Mpeg2,
      NonMpeg2
    }

    private enum ChannelType
    {
      Normal,
      Hidden
    }

    private enum VideoStandard
    {
      Ntsc = 0,
      Pal625,
      Pal525,
      Secam,
      Mac
    }

    public void Decode(byte[] section)
    {
      if (section == null || section.Length < 13)
      {
        return;
      }

      byte tableId = section[2];
      if (tableId != 0xc4)
      {
        return;
      }
      int sectionLength = ((section[3] & 0x0f) << 8) + section[4];
      if (section.Length != 2 + sectionLength + 3)
      {
        Log.Log.Error("S-VCT: invalid section length = {0}, byte count = {1}", sectionLength, section.Length);
        return;
      }
      byte protocolVersion = (byte)(section[5] & 0x1f);
      byte transmissionMedium = (byte)(section[6] >> 4);
      TableSubtype tableSubtype = (TableSubtype)(section[6] & 0x0f);
      int vctId = (section[7] << 8) + section[8];
      Log.Log.Debug("S-VCT: protocol version = {0}, transmission medium = {1}, table subtype = {2}, VCT ID = 0x{3}",
        protocolVersion, transmissionMedium, tableSubtype, vctId);

      int pointer = 9;
      int endOfSection = section.Length - 4;
      try
      {
        switch (tableSubtype)
        {
          case TableSubtype.DefinedChannelMap:
            DecodeDefinedChannelMap(section, endOfSection, ref pointer);
            break;
          case TableSubtype.VirtualChannelMap:
            DecodeVirtualChannelMap(section, endOfSection, ref pointer);
            break;
          case TableSubtype.InverseChannelMap:
            DecodeInverseChannelMap(section, endOfSection, ref pointer);
            break;
          default:
            Log.Log.Error("S-VCT: unsupported subtable type {0}", tableSubtype);
            return;
        }
      }
      catch (Exception ex)
      {
        Log.Log.Error(ex.Message);
        return;
      }

      while (pointer + 1 < endOfSection)
      {
        byte tag = section[pointer++];
        byte length = section[pointer++];
        Log.Log.Debug("S-VCT: descriptor, tag = 0x{0:x}, length = {1}", tag, length);
        if (pointer + length > endOfSection)
        {
          Log.Log.Error("S-VCT: invalid descriptor length, pointer = {0}, end of section = {1}, descriptor length = {2}", pointer, endOfSection, length);
          return;
        }

        if (tag == 0x93)  // revision detection descriptor
        {
          ParserCommon.DecodeRevisionDetectionDescriptor(section, pointer, length);
        }

        pointer += length;
      }

      if (pointer != endOfSection)
      {
        Log.Log.Error("S-VCT: corruption detected at end of section, pointer = {0}, end of section = {1}", pointer, endOfSection);
        return;
      }
    }

    private void DecodeDefinedChannelMap(byte[] section, int endOfSection, ref int pointer)
    {
      if (pointer + 3 > endOfSection)
      {
        throw new Exception(string.Format("S-VCT: corruption detected at defined channel map, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }

      int firstVirtualChannel = ((section[pointer] & 0x0f) << 8) + section[pointer + 1];
      pointer += 2;
      int dcmDataLength = (section[pointer++] & 0x7f);
      if (pointer + dcmDataLength > endOfSection)
      {
        throw new Exception(string.Format("S-VCT: invalid defined channel map data length {0}, pointer = {1}, end of section = {2}", dcmDataLength, pointer, endOfSection));
      }
      Log.Log.Debug("S-VCT: defined channel map, first virtual channel = {0}, DCM data length = {1}", firstVirtualChannel, dcmDataLength);
      for (int i = 0; i < dcmDataLength; i++)
      {
        bool rangeDefined = ((section[pointer] & 0x80) != 0);
        int channelsCount = (section[pointer++] & 0x7f);
        Log.Log.Debug("S-VCT: range defined = {0}, channels count = {1}", rangeDefined, channelsCount);
      }
    }

    private void DecodeVirtualChannelMap(byte[] section, int endOfSection, ref int pointer)
    {
      if (pointer + 7 > endOfSection)
      {
        throw new Exception(string.Format("S-VCT: corruption detected at virtual channel map, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }

      bool descriptorsIncluded = ((section[pointer++] & 0x20) != 0);
      bool splice = ((section[pointer++] & 0x80) != 0);
      uint activationTime = 0;
      for (byte b = 0; b < 3; b++)
      {
        activationTime = activationTime << 8;
        activationTime = section[pointer++];
      }
      byte numberOfVcRecords = section[pointer++];
      Log.Log.Debug("S-VCT: virtual channel map, descriptors included = {0}, splice = {1}, activation time = {2}, number of VC records = {3}",
        descriptorsIncluded, splice, activationTime, numberOfVcRecords);

      for (byte i = 0; i < numberOfVcRecords; i++)
      {
        if (pointer + 9 > endOfSection)
        {
          throw new Exception(string.Format("S-VCT: detected number of virtual channel records {0} is invalid, pointer = {1}, end of section = {2}, loop = {3}", numberOfVcRecords, pointer, endOfSection, i));
        }

        int virtualChannelNumber = ((section[pointer] & 0x0f) << 8) + section[pointer + 1];
        pointer += 2;
        bool applicationVirtualChannel = ((section[pointer] & 0x80) != 0);
        int pathSelect = ((section[pointer] & 0x20) >> 5);
        TransportType transportType = (TransportType)((section[pointer] & 0x10) >> 4);
        ChannelType channelType = (ChannelType)(section[pointer++] & 0x0f);
        int sourceId = (section[pointer] << 8) + section[pointer + 1];
        pointer += 2;
        byte cdsReference = section[pointer++];
        int programNumber = 0;
        byte mmsReference = 0;
        bool scrambled = false;
        VideoStandard videoStandard = VideoStandard.Ntsc;
        if (transportType == TransportType.Mpeg2)
        {
          programNumber = (section[pointer] << 8) + section[pointer + 1];
          pointer += 2;
          mmsReference = section[pointer++];
        }
        else
        {
          scrambled = ((section[pointer] & 0x80) != 0);
          videoStandard = (VideoStandard)(section[pointer++] & 0x0f);
          pointer += 2;
        }
        Log.Log.Debug("S-VCT: virtual channel number = {0}, application virtual channel = {1}, path select = {2}, transport type = {3}, channel type = {4}, source ID = 0x{5:x}, CDS reference = {6}, program number = 0x{7:x}, MMS reference = {8}, scrambled = {9}, video standard = {10}",
          virtualChannelNumber, applicationVirtualChannel, pathSelect, transportType, channelType, sourceId, cdsReference, programNumber, mmsReference, scrambled, videoStandard);

        if (descriptorsIncluded)
        {
          if (pointer >= endOfSection)
          {
            throw new Exception(string.Format("S-VCT: invalid section length at virtual channel map loop {0}, pointer = {0}, end of section = {1}, loop = {2}", pointer, endOfSection, i));
          }
          byte descriptorCount = section[pointer++];
          for (byte d = 0; d < descriptorCount; d++)
          {
            if (pointer + 2 > endOfSection)
            {
              throw new Exception(string.Format("S-VCT: detected virtual channel map descriptor count {0} is invalid, pointer = {1}, end of section = {2}, loop = {3}, inner loop = {4}", descriptorCount, pointer, endOfSection, i, d));
            }
            byte tag = section[pointer++];
            byte length = section[pointer++];
            Log.Log.Debug("S-VCT: virtual channel map descriptor, tag = 0x{0:x}, length = {1}", tag, length);
            if (pointer + length > endOfSection)
            {
              throw new Exception(string.Format("S-VCT: invalid virtual channel map descriptor length {0}, pointer = {1}, end of section = {2}, loop = {3}, inner loop = {4}", length, pointer, endOfSection, i, d));
            }
            pointer += length;
          }
        }
      }
    }

    private void DecodeInverseChannelMap(byte[] section, int endOfSection, ref int pointer)
    {
      if (pointer + 3 > endOfSection)
      {
        throw new Exception(string.Format("S-VCT: corruption detected at inverse channel map, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }

      int firstMapIndex = ((section[pointer] & 0x0f) << 8) + section[pointer + 1];
      pointer += 2;
      int recordCount = (section[pointer++] & 0x7f);
      if (pointer + (recordCount * 4) > endOfSection)
      {
        throw new Exception(string.Format("S-VCT: invalid inverse channel map record count {0}, pointer = {1}, end of section = {2}", recordCount, pointer, endOfSection));
      }
      Log.Log.Debug("S-VCT: inverse channel map, first map index = {0}, record count = {1}", firstMapIndex, recordCount);
      for (int i = 0; i < recordCount; i++)
      {
        int sourceId = (section[pointer] << 8) + section[pointer + 1];
        pointer += 2;
        int virtualChannelNumber = ((section[pointer] & 0x0f) << 8) + section[pointer + 1];
        pointer += 2;
        Log.Log.Debug("S-VCT: source ID = 0x{0:x}, virtual channel number = {2}", sourceId, virtualChannelNumber);
      }
    }
  }
}
