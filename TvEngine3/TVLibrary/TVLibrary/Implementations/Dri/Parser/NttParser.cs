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
using TvLibrary.Implementations.DVB;

namespace TvLibrary.Implementations.Dri.Parser
{
  public class NttParser
  {
    private enum TableSubtype
    {
      // ATSC A-56 seems to be out of date
      TransponderName = 1,
      SatelliteText,
      RatingsText,
      RatingSystem,
      CurrencySystem,
      SourceName,
      MapName
    }

    public void Decode(byte[] section)
    {
      if (section == null || section.Length < 14)
      {
        Log.Log.Error("NTT: invalid section length");
        return;
      }

      byte tableId = section[2];
      if (tableId != 0xc3)
      {
        return;
      }
      int sectionLength = ((section[3] & 0x3f) << 8) + section[4];  // difference between A-56 and SCTE 65
      if (section.Length != 2 + sectionLength + 3)
      {
        Log.Log.Error("NTT: invalid section length = {0}, byte count = {1}", sectionLength, section.Length);
        return;
      }
      int protocolVersion = (section[5] & 0x1f);
      string isoLangCode = System.Text.Encoding.ASCII.GetString(section, 6, 3);
      AtscTransmissionMedium transmissionMedium = (AtscTransmissionMedium)(section[9] >> 4);
      TableSubtype tableType = (TableSubtype)(section[9] & 0x0f);
      Log.Log.Debug("NTT: protocol version = {0}, ISO language code = {1}, transmission medium = {2}, table type = {3}",
        protocolVersion, isoLangCode, transmissionMedium, tableType);

      int pointer = 10;
      int endOfSection = section.Length - 4;

      try
      {
        if (tableType == TableSubtype.TransponderName)
        {
          DecodeTransponderName(section, endOfSection, ref pointer);
        }
        else if (tableType == TableSubtype.SatelliteText)
        {
          DecodeSatelliteText(section, endOfSection, ref pointer);
        }
        else if (tableType == TableSubtype.RatingsText)
        {
          DecodeRatingsText(section, endOfSection, ref pointer);
        }
        else if (tableType == TableSubtype.RatingSystem)
        {
          DecodeRatingSystem(section, endOfSection, ref pointer);
        }
        else if (tableType == TableSubtype.CurrencySystem)
        {
          DecodeCurrencySystem(section, endOfSection, ref pointer);
        }
        else if (tableType == TableSubtype.SourceName)
        {
          DecodeSourceName(section, endOfSection, ref pointer);
        }
        else if (tableType == TableSubtype.MapName)
        {
          DecodeMapName(section, endOfSection, ref pointer);
        }
        else
        {
          Log.Log.Error("NTT: unsupported subtable type {0}", tableType);
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
        Log.Log.Debug("NTT: descriptor, tag = 0x{0:x}, length = {1}", tag, length);
        if (pointer + length > endOfSection)
        {
          Log.Log.Error("NTT: invalid descriptor length, pointer = {0}, end of section = {1}, descriptor length = {2}", pointer, endOfSection, length);
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
        Log.Log.Error("NTT: corruption detected at end of section, pointer = {0}, end of section = {1}", pointer, endOfSection);
        return;
      }
    }

    private void DecodeTransponderName(byte[] section, int endOfSection, ref int pointer)
    {
      if (pointer + 3 > endOfSection)
      {
        throw new Exception(string.Format("NTT: corruption detected at transponder name, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }

      byte satelliteId = section[pointer++];
      byte firstIndex = section[pointer++];
      byte numberOfTntRecords = section[pointer++];
      Log.Log.Debug("NTT: transponder name, satellite ID = 0x{0:x}, first index = {1}, number of TNT records = {2}", satelliteId, firstIndex, numberOfTntRecords);

      for (byte i = 0; i < numberOfTntRecords; i++)
      {
        if (pointer + 3 > endOfSection)
        {
          throw new Exception(string.Format("NTT: detected transponder name subtable number of TNT records {0} is invalid in loop {1}, pointer = {2}, end of section = {3}", numberOfTntRecords, i, pointer, endOfSection));
        }
        int transponderNumber = (section[pointer++] & 0x3f);
        int transponderNameLength = (section[pointer++] & 0x1f);
        if (pointer + transponderNameLength > endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid transponder name subtable transponder name length {0}, pointer = {1}, end of section = {2}", transponderNameLength, pointer, endOfSection));
        }
        string transponderName = DecodeMultilingualText(section, pointer + transponderNameLength, ref pointer);
        Log.Log.Debug("NTT: transponder name, number = {0}, name = {1}", transponderNumber, transponderName);

        // subtable descriptors
        if (pointer >= endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid section length at transponder name subtable descriptor count, pointer = {0}, end of section = {1}", pointer, endOfSection));
        }
        byte descriptorCount = section[pointer++];
        for (byte d = 0; d < descriptorCount; d++)
        {
          if (pointer + 2 > endOfSection)
          {
            throw new Exception(string.Format("NTT: detected transponder name subtable descriptor count {0} is invalid in loop {1}, pointer = {2}, end of section = {3}", descriptorCount, d, pointer, endOfSection));
          }
          byte tag = section[pointer++];
          byte length = section[pointer++];
          Log.Log.Debug("NTT: transponder name subtable descriptor, tag = 0x{0:x}, length = {1}", tag, length);
          if (pointer + length > endOfSection)
          {
            throw new Exception(string.Format("NTT: invalid transponder name subtable descriptor length {0}, pointer = {1}, end of section = {2}", length, pointer, endOfSection));
          }
          pointer += length;
        }
      }
    }

    private void DecodeSatelliteText(byte[] section, int endOfSection, ref int pointer)
    {
      if (pointer + 2 > endOfSection)
      {
        throw new Exception(string.Format("NTT: corruption detected at satellite text, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }

      byte firstIndex = section[pointer++];
      byte numberOfSttRecords = section[pointer++];
      Log.Log.Debug("NTT: satellite text, first index = {0}, number of STT records = {1}", firstIndex, numberOfSttRecords);

      for (byte i = 0; i < numberOfSttRecords; i++)
      {
        if (pointer + 4 > endOfSection)
        {
          throw new Exception(string.Format("NTT: detected satellite text subtable number of STT records {0} is invalid in loop {1}, pointer = {2}, end of section = {3}", numberOfSttRecords, i, pointer, endOfSection));
        }
        byte satelliteId = section[pointer++];
        int satelliteReferenceNameLength = (section[pointer++] & 0x0f);
        if (pointer + satelliteReferenceNameLength > endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid satellite text subtable satellite reference name length {0}, pointer = {1}, end of section = {2}", satelliteReferenceNameLength, pointer, endOfSection));
        }
        string satelliteReferenceName = DecodeMultilingualText(section, pointer + satelliteReferenceNameLength, ref pointer);

        if (pointer >= endOfSection)
        {
          throw new Exception(string.Format("NTT: corruption detected at satellite text subtable full satellite name, pointer = {0}, end of section = {1}", pointer, endOfSection));
        }
        int fullSatelliteNameLength = (section[pointer++] & 0x1f);
        if (pointer + fullSatelliteNameLength > endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid satellite text subtable full satellite name length {0}, pointer = {1}, end of section = {2}", fullSatelliteNameLength, pointer, endOfSection));
        }
        string fullSatelliteName = DecodeMultilingualText(section, pointer + fullSatelliteNameLength, ref pointer);

        Log.Log.Debug("NTT: satellite text, satellite ID = 0x{0:x}, reference name = {1}, full name = {2}", satelliteId, satelliteReferenceName, fullSatelliteName);

        // subtable descriptors
        if (pointer >= endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid section length at satellite text subtable descriptor count, pointer = {0}, end of section = {1}", pointer, endOfSection));
        }
        byte descriptorCount = section[pointer++];
        for (byte d = 0; d < descriptorCount; d++)
        {
          if (pointer + 2 > endOfSection)
          {
            throw new Exception(string.Format("NTT: detected satellite text subtable descriptor count {0} is invalid in loop {1}, pointer = {2}, end of section = {3}", descriptorCount, d, pointer, endOfSection));
          }
          byte tag = section[pointer++];
          byte length = section[pointer++];
          Log.Log.Debug("NTT: satellite text subtable descriptor, tag = 0x{0:x}, length = {1}", tag, length);
          if (pointer + length > endOfSection)
          {
            throw new Exception(string.Format("NTT: invalid satellite text subtable descriptor length {0}, pointer = {1}, end of section = {2}", length, pointer, endOfSection));
          }
          pointer += length;
        }
      }
    }

    private void DecodeRatingsText(byte[] section, int endOfSection, ref int pointer)
    {
      if (pointer >= endOfSection)
      {
        throw new Exception(string.Format("NTT: corruption detected at ratings text, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }

      byte ratingRegion = section[pointer++];
      for (byte i = 0; i < 6; i++)
      {
        if (pointer >= endOfSection)
        {
          throw new Exception(string.Format("NTT: corruption detected at ratings text subtable levels defined loop {0}, pointer = {1}, end of section = {2}", i, pointer, endOfSection));
        }
        byte levelsDefined = section[pointer++];
        if (levelsDefined > 0)
        {
          if (pointer >= endOfSection)
          {
            throw new Exception(string.Format("NTT: corruption detected at ratings text subtable dimension name length, pointer = {0}, end of section = {1}", pointer, endOfSection));
          }
          byte dimensionNameLength = section[pointer++];
          if (pointer + dimensionNameLength > endOfSection)
          {
            throw new Exception(string.Format("NTT: invalid ratings text subtable dimension name length {0}, pointer = {1}, end of section = {2}", dimensionNameLength, pointer, endOfSection));
          }
          string dimensionName = DecodeMultilingualText(section, pointer + dimensionNameLength, ref pointer);
          Log.Log.Debug("NTT: ratings text, dimension name = {0}, levels defined = {1}", dimensionName, levelsDefined);
          for (byte l = 0; l < levelsDefined; l++)
          {
            byte ratingNameLength = section[pointer++];
            if (pointer + ratingNameLength > endOfSection)
            {
              throw new Exception(string.Format("NTT: invalid ratings text subtable rating name length {0}, pointer = {1}, end of section = {2}", ratingNameLength, pointer, endOfSection));
            }
            string ratingName = DecodeMultilingualText(section, pointer + ratingNameLength, ref pointer);
            Log.Log.Debug("  rating name = {0}", ratingName);
          }
        }
      }
    }

    private void DecodeRatingSystem(byte[] section, int endOfSection, ref int pointer)
    {
      if (pointer >= endOfSection)
      {
        throw new Exception(string.Format("NTT: corruption detected at rating system, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }

      byte regionsDefined = section[pointer++];
      for (byte i = 0; i < regionsDefined; i++)
      {
        if (pointer + 3 > endOfSection)
        {
          throw new Exception(string.Format("NTT: detected rating system subtable regions defined {0} is invalid in loop {1}, pointer = {2}, end of section = {3}", regionsDefined, i, pointer, endOfSection));
        }
        byte dataLength = section[pointer++];
        int endOfData = pointer + dataLength;
        if (endOfData > endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid rating system subtable data length {0}, pointer = {1}, end of section = {2}", dataLength, pointer, endOfSection));
        }
        byte ratingRegion = section[pointer++];
        byte stringLength = section[pointer++];
        if (pointer + stringLength > endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid rating system subtable string length {0}, pointer = {1}, end of section = {2}", stringLength, pointer, endOfSection));
        }
        string ratingSystem = DecodeMultilingualText(section, pointer + stringLength, ref pointer);
        Log.Log.Debug("NTT: rating system, region = {0}, system = {1}", ratingRegion, ratingSystem);

        // subtable descriptors
        while (pointer + 1 < endOfData)
        {
          byte tag = section[pointer++];
          byte length = section[pointer++];
          Log.Log.Debug("NTT: rating system subtable descriptor, tag = 0x{0:x}, length = {1}", tag, length);
          if (pointer + length > endOfSection)
          {
            throw new Exception(string.Format("NTT: invalid rating system subtable descriptor length {0}, pointer = {1}, end of section = {2}", length, pointer, endOfSection));
          }
          pointer += length;
        }
        if (pointer != endOfData)
        {
          throw new Exception(string.Format("NTT: corruption detected at end of rating system data, pointer = {0}, end of section = {1}, end of data = {2}", pointer, endOfSection, endOfData));
        }
      }
    }

    private void DecodeCurrencySystem(byte[] section, int endOfSection, ref int pointer)
    {
      if (pointer >= endOfSection)
      {
        throw new Exception(string.Format("NTT: corruption detected at currency system, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }

      byte regionsDefined = section[pointer++];
      for (byte i = 0; i < regionsDefined; i++)
      {
        if (pointer + 3 > endOfSection)
        {
          throw new Exception(string.Format("NTT: detected currency system subtable regions defined {0} is invalid in loop {1}, pointer = {2}, end of section = {3}", regionsDefined, i, pointer, endOfSection));
        }
        byte dataLength = section[pointer++];
        int endOfData = pointer + dataLength;
        if (endOfData > endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid currency system subtable data length {0}, pointer = {1}, end of section = {2}", dataLength, pointer, endOfSection));
        }
        byte currencyRegion = section[pointer++];
        byte stringLength = section[pointer++];
        if (pointer + stringLength > endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid currency system subtable string length {0}, pointer = {1}, end of section = {2}", stringLength, pointer, endOfSection));
        }
        string currencySystem = DecodeMultilingualText(section, pointer + stringLength, ref pointer);
        Log.Log.Debug("NTT: currency system, region = {0}, system = {1}", currencyRegion, currencySystem);

        // subtable descriptors
        while (pointer + 1 < endOfData)
        {
          byte tag = section[pointer++];
          byte length = section[pointer++];
          Log.Log.Debug("NTT: currency system subtable descriptor, tag = 0x{0:x}, length = {1}", tag, length);
          if (pointer + length > endOfSection)
          {
            throw new Exception(string.Format("NTT: invalid currency system subtable descriptor length {0}, pointer = {1}, end of section = {2}", length, pointer, endOfSection));
          }
          pointer += length;
        }
        if (pointer != endOfData)
        {
          throw new Exception(string.Format("NTT: corruption detected at end of currency system data, pointer = {0}, end of section = {1}, end of data = {2}", pointer, endOfSection, endOfData));
        }
      }
    }

    private void DecodeSourceName(byte[] section, int endOfSection, ref int pointer)
    {
      if (pointer >= endOfSection)
      {
        throw new Exception(string.Format("NTT: corruption detected at source name, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }

      byte numberOfSntRecords = section[pointer++];
      for (byte i = 0; i < numberOfSntRecords; i++)
      {
        if (pointer + 5 > endOfSection)
        {
          throw new Exception(string.Format("NTT: detected source name subtable number of SNT records {0} is invalid in loop {1}, pointer = {2}, end of section = {3}", numberOfSntRecords, i, pointer, endOfSection));
        }
        bool isApplication = ((section[pointer++] & 0x80) != 0);
        int sourceId = (section[pointer] << 8) + section[pointer + 1];
        pointer += 2;
        byte nameLength = section[pointer++];
        if (pointer + nameLength > endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid source name subtable string length {0}, pointer = {1}, end of section = {2}", nameLength, pointer, endOfSection));
        }
        string sourceName = DecodeMultilingualText(section, pointer + nameLength, ref pointer);
        Log.Log.Debug("NTT: source name, source ID = 0x{0:x}, name = {1}, is application = {2}", sourceId, sourceName, isApplication);

        // subtable descriptors
        if (pointer >= endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid section length at source name subtable descriptor count, pointer = {0}, end of section = {1}", pointer, endOfSection));
        }
        byte descriptorCount = section[pointer++];
        for (byte d = 0; d < descriptorCount; d++)
        {
          if (pointer + 2 > endOfSection)
          {
            throw new Exception(string.Format("NTT: detected source name subtable descriptor count {0} is invalid in loop {1}, pointer = {2}, end of section = {3}", descriptorCount, d, pointer, endOfSection));
          }
          byte tag = section[pointer++];
          byte length = section[pointer++];
          Log.Log.Debug("NTT: source name subtable descriptor, tag = 0x{0:x}, length = {1}", tag, length);
          if (pointer + length > endOfSection)
          {
            throw new Exception(string.Format("NTT: invalid source name subtable descriptor length {0}, pointer = {1}, end of section = {2}", length, pointer, endOfSection));
          }
          pointer += length;
        }
      }
    }

    private void DecodeMapName(byte[] section, int endOfSection, ref int pointer)
    {
      if (pointer >= endOfSection)
      {
        throw new Exception(string.Format("NTT: corruption detected at map name, pointer = {0}, end of section = {1}", pointer, endOfSection));
      }

      byte numberOfMntRecords = section[pointer++];
      for (byte i = 0; i < numberOfMntRecords; i++)
      {
        if (pointer + 4 > endOfSection)
        {
          throw new Exception(string.Format("NTT: detected map name subtable number of MNT records {0} is invalid in loop {1}, pointer = {2}, end of section = {3}", numberOfMntRecords, i, pointer, endOfSection));
        }
        int vctId = (section[pointer] << 8) + section[pointer + 1];
        pointer += 2;
        byte mapNameLength = section[pointer++];
        if (pointer + mapNameLength > endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid map name subtable map name length {0}, pointer = {1}, end of section = {2}", mapNameLength, pointer, endOfSection));
        }
        string mapName = DecodeMultilingualText(section, pointer + mapNameLength, ref pointer);
        Log.Log.Debug("NTT: map name, VCT ID = 0x{0:x}, name = {1}", vctId, mapName);

        // subtable descriptors
        if (pointer >= endOfSection)
        {
          throw new Exception(string.Format("NTT: invalid section length at map name subtable descriptor count, pointer = {0}, end of section = {1}", pointer, endOfSection));
        }
        byte descriptorCount = section[pointer++];
        for (byte d = 0; d < descriptorCount; d++)
        {
          if (pointer + 2 > endOfSection)
          {
            throw new Exception(string.Format("NTT: detected map name subtable descriptor count {0} is invalid in loop {1}, pointer = {2}, end of section = {3}", descriptorCount, d, pointer, endOfSection));
          }
          byte tag = section[pointer++];
          byte length = section[pointer++];
          Log.Log.Debug("NTT: map name subtable descriptor, tag = 0x{0:x}, length = {1}", tag, length);
          if (pointer + length > endOfSection)
          {
            throw new Exception(string.Format("NTT: invalid map name subtable descriptor length {0}, pointer = {1}, end of section = {2}", length, pointer, endOfSection));
          }
          pointer += length;
        }
      }
    }

    public string DecodeMultilingualText(byte[] section, int endOfString, ref int pointer)
    {
      string text = string.Empty;
      while (pointer + 1 < endOfString)
      {
        byte mode = section[pointer++];
        byte segmentLength = section[pointer++];
        if (pointer + segmentLength > endOfString)
        {
          throw new Exception(string.Format("NTT: invalid multilingual text segment length {0}, pointer = {1}, end of string = {2}", segmentLength, pointer, endOfString));
        }

        if (mode == 0)
        {
          // We only support ASCII encoding at this time.
          text += System.Text.Encoding.ASCII.GetString(section, pointer, segmentLength);
        }
        else
        {
          Log.Log.Debug("NTT: unsupported multilingual text segment mode 0x{0:x}", mode);
          DVB_MMI.DumpBinary(section, pointer, segmentLength);
        }
        pointer += segmentLength;
      }
      if (pointer != endOfString)
      {
        throw new Exception(string.Format("NTT: corruption detected at end of multilingual string, pointer = {0}, end of string = {2}", pointer, endOfString));
      }
      return text;
    }
  }
}
