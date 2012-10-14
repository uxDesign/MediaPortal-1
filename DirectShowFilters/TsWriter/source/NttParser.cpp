/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#include "NttParser.h"

extern void LogDebug(const char *fmt, ...);
extern bool DisableCRCCheck();

CNttParser::CNttParser(void)
{
  //SetPid(0);
  if (DisableCRCCheck())
  {
    EnableCrcCheck(false);
  }
  Reset();
  m_pCallBack = NULL;
}

CNttParser::~CNttParser(void)
{
}

void CNttParser::Reset()
{
  LogDebug("NttParser: reset");
  CSectionDecoder::Reset();
  LogDebug("NttParser: reset done");
}

void CNttParser::SetCallBack(INttCallBack* callBack)
{
  m_pCallBack = callBack;
}

void CNttParser::OnNewSection(CSection& sections)
{
  if (sections.table_id != 0xc3)
  {
    LogDebug("NttParser: other table ID 0x%x", sections.table_id);
    return;
  }
  if (m_pCallBack == NULL)
  {
    return;
  }
  byte* section = sections.Data;

  try
  {
    int section_length = ((section[1] & 0x0f) << 8) + section[2];
    if (section_length > 1021 || section_length < 9)
    {
      LogDebug("NttParser: invalid section length = %d", section_length);
      return;
    }
    int protocol_version = (section[3] & 0x1f);
    unsigned int iso_639_language_code = (section[4] << 16) + (section[5] << 8) + section[6];
    int transmission_medium = (section[7] >> 4);
    int table_subtype = (section[7] & 0xf);

    LogDebug("NttParser: section length = %d, protocol version = %d, ISO 639 language = %s, transmission medium = %d, table subtype = %d", section_length, protocol_version, &iso_639_language_code, transmission_medium, table_subtype);
    if (table_subtype != 6)
    {
      LogDebug("NttParser: unsupported table subtype %d", table_subtype);
      return;
    }

    //-------------------------------------------------------------------------
    // Source name subtable...
    int number_of_sns_records = section[8];
    LogDebug("NttParser: number of SNS records = %d", number_of_sns_records);
    int pointer = 9;
    int endOfSection = section_length - 1;
    for (int i = 0; i < number_of_sns_records; i++)
    {
      if (pointer + 4 > endOfSection)
      {
        LogDebug("NttParser: detected number of SNS records %d is invalid in loop %d, pointer = %d, end of section = %d, section length = %d", number_of_sns_records, i, pointer, endOfSection, section_length);
        return;
      }

      int application_type = (section[pointer++] >> 7);
      int source_id = (section[pointer] << 8) + section[pointer + 1];   // source_id when application_type is 0, otherwise application_id
      pointer += 2;
      int name_length = section[pointer++];
      LogDebug("NttParser: source ID = 0x%x, application type = %d, name length = %d", source_id, application_type, name_length);

      if (pointer + name_length + 1 > endOfSection)
      {
        LogDebug("NttParser: invalid name length = %d, pointer = %d, end of section = %d, section length = %d", name_length, pointer, endOfSection, section_length);
        return;
      }

      char* name = NULL;
      DecodeMultilingualText(&section[pointer], name_length, &name);
      if (name != NULL)
      {
        LogDebug("NttParser: name = %s", name);
        if (m_pCallBack != NULL)
        {
          m_pCallBack->OnNttReceived(source_id, application_type, name, iso_639_language_code);
        }
        else
        {
          delete[] name;
          name = NULL;
        }
      }
      pointer += name_length;

      int sns_descriptors_count = section[pointer++];
      LogDebug("NttParser: SNS decriptor count = %d", sns_descriptors_count);
      for (int j = 0; j < sns_descriptors_count; j++)
      {
        if (pointer + 1 > endOfSection)
        {
          LogDebug("NttParser: detected SNS descriptor count %d is invalid in loop %d, pointer = %d, end of section = %d", sns_descriptors_count, j, pointer, endOfSection);
          return;
        }

        int tag = section[pointer++];
        int length = section[pointer++];
        LogDebug("NttParser: SNS descriptor, tag = 0x%x, length = %d, pointer = %d, end of descriptor = %d, end of section = %d, section length = %d", tag, length, pointer, pointer + length, endOfSection, section_length);
        if (pointer + length > endOfSection)
        {
          LogDebug("NttParser: invalid SNS descriptor length = %d, pointer = %d, end of descriptor = %d, end of section = %d, section length = %d", length, pointer, pointer + length, endOfSection, section_length);
          return;
        }

        pointer += length;
      }
    }
    //-------------------------------------------------------------------------

    while (pointer + 1 < endOfSection)
    {
      int tag = section[pointer++];
      int length = section[pointer++];
      LogDebug("NttParser: descriptor, tag = 0x%x, length = %d, pointer = %d, end of descriptor = %d, end of section = %d, section length = %d", tag, length, pointer, pointer + length, endOfSection, section_length);
      if (pointer + length > endOfSection)
      {
        LogDebug("NttParser: invalid descriptor length = %d, pointer = %d, end of descriptor = %d, end of section = %d, section length = %d", length, pointer, pointer + length, endOfSection, section_length);
        return;
      }

      pointer += length;
    }

    if (pointer != endOfSection)
    {
      LogDebug("NttParser: section parsing error");
    }
  }
  catch (...)
  {
    LogDebug("NttParser: unhandled exception in OnNewSection()");
  }
}

void CNttParser::DecodeMultilingualText(byte* b, int length, char** string)
{
  if (length < 2)
  {
    LogDebug("NttParser: invalid multilingual text length = %d", length);
    return;
  }
  *string = new char[length + 1];
  if (*string == NULL)
  {
    LogDebug("NttParser: failed to allocate memory in DecodeMultilingualText()");
    return;
  }
  int stringOffset = 0;

  int pointer = 0;
  while (pointer + 1 < length)
  {
    int mode = b[pointer++];
    int segment_length = b[pointer++];
    if (pointer + segment_length > length)
    {
      LogDebug("NttParser: invalid multilingual text segment length = %d, pointer = %d, string offset = %d, mode = 0x%x, text length = %d", segment_length, pointer, stringOffset, mode, length);
      delete[] *string;
      *string = NULL;
      return;
    }

    if (mode == 0)
    {
      // We only support ASCII encoding at this time.
      memcpy(*string, &b[pointer], segment_length);
      stringOffset += segment_length;
      pointer += segment_length;
      (*string)[stringOffset] = 0;  // NULL terminate
    }
    else
    {
      LogDebug("NttParser: unsupported segment mode in DecodeMultilingualText(), mode = 0x%x", mode);
      for (int i = 0; i < segment_length; i++)
      {
        LogDebug("  %d: 0x%x", b[pointer++]);
      }
    }
  }
}