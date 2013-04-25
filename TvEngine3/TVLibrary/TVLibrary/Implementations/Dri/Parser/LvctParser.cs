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
  public class LvctParser
  {
    private enum ModulationMode : byte
    {
      Analog = 0x01,
      ScteMode1 = 0x02, // 64 QAM
      ScteMode2 = 0x03, // 256 QAM
      Atsc8Vsb = 0x04,
      Atsc16Vsb = 0x05,
      PrivateDescriptor = 0x80
    }

    private enum EtmLocation : byte
    {
      None,
      PhysicalChannelThis,
      PhysicalChannelTsid
    }

    private enum AtscServiceType : byte
    {
      /// <summary>
      /// analog television (A/65)
      /// </summary>
      AnalogTelevision = 0x01,
      /// <summary>
      /// ATSC digital television service (A/53 part 3)
      /// </summary>
      DigitalTelevision = 0x02,
      /// <summary>
      /// ATSC audio service (A/53 part 3)
      /// </summary>
      Audio = 0x03,
      /// <summary>
      /// ATSC data only service (A/90)
      /// </summary>
      DataOnly = 0x04,
      /// <summary>
      /// ATSC software download service (A/97)
      /// </summary>
      SoftwareDownload = 0x05,
      /// <summary>
      /// unassociated/small screen service (A/53 part 3)
      /// </summary>
      SmallScreen = 0x06,
      /// <summary>
      /// parameterised service (A/71)
      /// </summary>
      Parameterised = 0x07,
      /// <summary>
      /// Non Real Time service (A/103)
      /// </summary>
      Nrt = 0x08,
      /// <summary>
      /// extended parametised service (A/71)
      /// </summary>
      ExtendedParameterised = 0x09
    }

    public void Decode(byte[] section)
    {
      if (section == null || section.Length < 18)
      {
        Log.Log.Error("L-VCT: invalid section length");
        return;
      }

      byte tableId = section[2];
      // 0xc8 = ATSC terrestrial, A-65
      // 0xc9 = SCTE cable, SCTE-65
      // The cable and terrestrial L-VCT formats are almost identical. The few
      // differences are noted below.
      if (tableId != 0xc8 && tableId != 0xc9)
      {
        return;
      }
      bool sectionSyntaxIndicator = ((section[3] & 0x80) != 0);
      bool privateIndicator = ((section[3] & 0x40) != 0);
      int sectionLength = ((section[3] & 0x0f) << 8) + section[4];
      if (section.Length != 2 + sectionLength + 3)
      {
        Log.Log.Error("L-VCT: invalid section length = {0}, byte count = {1}", sectionLength, section.Length);
        return;
      }
      int mapId = (section[5] << 8) + section[6];
      int versionNumber = ((section[7] >> 1) & 0x1f);
      bool currentNextIndicator = ((section[7] & 0x80) != 0);
      if (!currentNextIndicator)
      {
        // Not applicable yet.
        return;
      }
      byte sectionNumber = section[8];
      byte lastSectionNumber = section[9];
      byte protocolVersion = section[10];
      int numChannelsInSection = section[11];
      Log.Log.Debug("L-VCT: map ID = 0x{0:x}, version number = {2}, section number = {3}, last section number {4}, protocol version = {5}, number of channels in section = 0x{6:x}",
        mapId, versionNumber, sectionNumber, lastSectionNumber, protocolVersion, numChannelsInSection);

      int pointer = 12;
      int endOfSection = section.Length - 4;
      for (int i = 0; i < numChannelsInSection; i++)
      {
        if (pointer + 32 + 2 > endOfSection)  // + 2 for the fixed bytes after the loop
        {
          Log.Log.Error("L-VCT: detected number of channels in section {0} is invalid in loop {1}, pointer = {2}, end of section = {3}", numChannelsInSection, i, pointer, endOfSection);
          return;
        }

        string shortName = System.Text.Encoding.Unicode.GetString(section, pointer, 14);
        pointer += 14;
        int majorChannelNumber = ((section[pointer] & 0x0f) << 6) + (section[pointer + 1] >> 2);
        pointer++;
        int minorChannelNumber = ((section[pointer] & 0x03) << 8) + section[pointer + 1];
        pointer++;
        // SCTE cable supports both one-part and two-part channel numbers where
        // the major and minor channel number range is 0..999.
        // ATSC supports only two-part channel numbers where the major range is
        // 1..99 and the minor range is 0..999.
        // When the minor channel number is 0 it indicates an analog channel.
        int onePartVirtualChannelNumber = 0;
        if ((majorChannelNumber & 0x03f0) == 0x03f0)
        {
          onePartVirtualChannelNumber = ((majorChannelNumber & 0x0f) << 10) + minorChannelNumber;
        }
        ModulationMode modulationMode = (ModulationMode)section[pointer++];
        int carrierFrequency = 0;
        for (byte b = 0; b < 3; b++)
        {
          carrierFrequency = carrierFrequency << 8;
          carrierFrequency = section[pointer++];
        }
        int channelTsid = (section[pointer] << 8) + section[pointer + 1];
        pointer += 2;
        int programNumber = (section[pointer] << 8) + section[pointer + 1];
        pointer += 2;
        EtmLocation etmLocation = (EtmLocation)(section[pointer] >> 6);   // ATSC only, SCTE reserved
        bool accessControlled = ((section[pointer] & 0x20) != 0);
        bool hidden = ((section[pointer] & 0x10) != 0);
        int pathSelect = ((section[pointer] & 0x08) >> 3);                // SCTE only, ATSC reserved
        bool outOfBand = ((section[pointer] & 0x04) != 0);                // SCTE only, ATSC reserved
        bool hideGuide = ((section[pointer++] & 0x02) != 0);
        AtscServiceType serviceType = (AtscServiceType)(section[pointer++] & 0x3f);
        int sourceId = (section[pointer] << 8) + section[pointer + 1];
        pointer += 2;
        int descriptorsLength = ((section[pointer] & 0x03) << 8) + section[pointer + 1];
        pointer += 2;
        int endOfDescriptors = pointer + descriptorsLength;
        if (endOfDescriptors > endOfSection)
        {
          Log.Log.Error("L-VCT: invalid descriptors length {0}, pointer = {1}, end of section = {2}", descriptorsLength, pointer, endOfSection);
          return;
        }
        while (pointer + 1 < endOfDescriptors)
        {
          byte tag = section[pointer++];
          byte length = section[pointer++];
          Log.Log.Debug("L-VCT: descriptor, tag = 0x{0:x}, length = {1}", tag, length);
          if (pointer + length > endOfDescriptors)
          {
            Log.Log.Error("L-VCT: invalid descriptor length {0}, pointer = {1}, end of descriptors = {2}", length, pointer, endOfDescriptors);
            return;
          }
          pointer += length;
        }
        if (pointer != endOfDescriptors)
        {
          Log.Log.Error("L-VCT: corruption detected at end of descriptors loop {0}, pointer = {1}, end of section = {2}, end of descriptors = {3}", i, pointer, endOfSection, endOfDescriptors);
          return;
        }
      }

      int additionalDescriptorsLength = ((section[pointer] & 0x03) << 8) + section[pointer + 1];
      pointer += 2;
      if (pointer + additionalDescriptorsLength != endOfSection)
      {
        Log.Log.Error("L-VCT: invalid additional descriptors length {0}, pointer = {1}, end of section = {2}", additionalDescriptorsLength, pointer, endOfSection);
        return;
      }
      while (pointer + 1 < endOfSection)
      {
        byte tag = section[pointer++];
        byte length = section[pointer++];
        Log.Log.Debug("L-VCT: additional descriptor, tag = 0x{0:x}, length = {1}", tag, length);
        if (pointer + length > endOfSection)
        {
          Log.Log.Error("L-VCT: invalid additional descriptor length {0}, pointer = {1}, end of section = {2}", length, pointer, endOfSection);
          return;
        }
        pointer += length;
      }

      if (pointer != endOfSection)
      {
        Log.Log.Error("L-VCT: corruption detected at end of section, pointer = {0}, end of section = {1}", pointer, endOfSection);
        return;
      }
    }
  }
}
