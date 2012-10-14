/* 
 *  Copyright (C) 2006-2008 Team MediaPortal
 *  http://www.team-mediaportal.com
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
#pragma once
#include <Windows.h>
#include <map>
#include "..\..\shared\ChannelInfo.h"
#include "..\..\shared\SectionDecoder.h"

using namespace std;

class ISvctCallBack
{
  public:
    virtual void OnSvctReceived(const CChannelInfo& vctInfo) = 0;
};

class CSvctParser : public CSectionDecoder
{
  public:
    CSvctParser(void);
    virtual ~CSvctParser(void);

    void Reset();
    void SetCallBack(ISvctCallBack* callBack);
    void OnNewSection(CSection& sections);

  private:
    void DecodeVirtualChannelMap(byte* b, int sectionLength, int* mapLength);
    void DecodeDefinedChannelMap(byte* b, int sectionLength, map<int, bool>* dcm, int* mapLength);
    void DecodeInverseChannelMap(byte* b, int sectionLength, map<int, int>* icm, int* mapLength);
    void DecodeVirtualChannel(byte* b, CChannelInfo** info);

    ISvctCallBack* m_pCallBack;
    map<int, bool> m_mDefinedChannelMap;
    map<int, int> m_mInverseChannelMap;   // source/application ID -> virtual channel number
};