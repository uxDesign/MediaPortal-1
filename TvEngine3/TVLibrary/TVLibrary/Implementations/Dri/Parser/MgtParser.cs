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

namespace TvLibrary.Implementations.Dri.Parser
{
  public class MgtParser
  {
    public void Decode(byte[] section)
    {
      if (section == null || section.Length < 19)
      {
        Log.Log.Error("MGT: invalid section length");
        return;
      }

      byte tableId = section[2];
      if (tableId != 0xc7)
      {
        return;
      }
      bool sectionSyntaxIndicator = ((section[3] & 0x80) != 0);
      bool privateIndicator = ((section[3] & 0x40) != 0);
      int sectionLength = ((section[3] & 0x0f) << 8) + section[4];
      if (section.Length != 2 + sectionLength + 3)
      {
        Log.Log.Error("MGT: invalid section length = {0}, byte count = {1}", sectionLength, section.Length);
        return;
      }
      int mapId = (section[5] << 8) + section[6];
      int versionNumber = ((section[7] >> 1) & 0x1f);
      bool currentNextIndicator = ((section[7] & 0x80) != 0);
      if (!currentNextIndicator)
      {
        // Not applicable yet. Technically this should never happen.
        return;
      }
      byte sectionNumber = section[8];
      byte lastSectionNumber = section[9];
      byte protocolVersion = section[10];
      int tablesDefined = (section[11] << 8) + section[12];
      Log.Log.Debug("MGT: map ID = 0x{0:x}, version number = {2}, section number = {3}, last section number {4}, protocol version = {5}, tables defined = 0x{6:x}",
        mapId, versionNumber, sectionNumber, lastSectionNumber, protocolVersion, tablesDefined);

      int pointer = 13;
      int endOfSection = section.Length - 4;
      for (int i = 0; i < tablesDefined; i++)
      {
        if (pointer + 11 + 2 > endOfSection)
        {
          Log.Log.Error("MGT: detected tables defined {0} is invalid in loop {1}, pointer = {2}, end of section = {3}", tablesDefined, i, pointer, endOfSection);
          return;
        }
        int tableType = (section[pointer] << 8) + section[pointer + 1];
        pointer += 2;
        int tableTypePid = ((section[pointer] & 0x1f) << 8) + section[pointer + 1];
        pointer += 2;
        int tableTypeVersionNumber = (section[pointer++] & 0x1f);
        int numberBytes = (section[pointer] << 24) + (section[pointer + 1] << 16) + (section[pointer + 2] << 8) + section[pointer + 3];
        pointer += 4;
        Log.Log.Debug("MGT: table type = 0x{0:x}, PID = 0x{1:x}, version number = {2}", tableType, tableTypePid, tableTypeVersionNumber);

        int tableTypeDescriptorsLength = ((section[pointer] & 0x0f) << 8) + section[pointer + 1];
        int endOfTableTypeDescriptors = pointer + tableTypeDescriptorsLength;
        if (endOfTableTypeDescriptors > endOfSection)
        {
          Log.Log.Error("MGT: invalid table type descriptors length {0}, pointer = {1}, end of section = {2}", tableTypeDescriptorsLength, pointer, endOfSection);
          return;
        }
        while (pointer + 1 < endOfTableTypeDescriptors)
        {
          byte tag = section[pointer++];
          byte length = section[pointer++];
          Log.Log.Debug("MGT: table type descriptor, tag = 0x{0:x}, length = {1}", tag, length);
          if (pointer + length > endOfTableTypeDescriptors)
          {
            Log.Log.Error("MGT: invalid table type descriptor length {0}, pointer = {1}, end of table type descriptors = {2}", length, pointer, endOfTableTypeDescriptors);
            return;
          }
          pointer += length;
        }
        if (pointer != endOfTableTypeDescriptors)
        {
          Log.Log.Error("MGT: corruption detected at end of table type descriptors, pointer = {0}, end of section = {1}, end of table type descriptors = {2}", pointer, endOfSection, endOfTableTypeDescriptors);
          return;
        }
      }

      int descriptorsLength = ((section[pointer] & 0x0f) << 8) + section[pointer + 1];
      if (pointer + descriptorsLength != endOfSection)
      {
        Log.Log.Error("MGT: invalid descriptors length {0}, pointer = {1}, end of section = {2}", descriptorsLength, pointer, endOfSection);
        return;
      }
      while (pointer + 1 < endOfSection)
      {
        byte tag = section[pointer++];
        byte length = section[pointer++];
        Log.Log.Debug("NTT: descriptor, tag = 0x{0:x}, length = {1}", tag, length);
        if (pointer + length > endOfSection)
        {
          Log.Log.Error("NTT: invalid descriptor length {0}, pointer = {1}, end of section = {2}", length, pointer, endOfSection);
          return;
        }
        pointer += length;
      }

      if (pointer != endOfSection)
      {
        Log.Log.Error("MGT: corruption detected at end of section, pointer = {0}, end of section = {1}", pointer, endOfSection);
        return;
      }
    }
  }
}
