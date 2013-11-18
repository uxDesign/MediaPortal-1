// Copyright (C) 2005-2013 Team MediaPortal
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

#pragma once

#ifndef ITSRSTATUS
#define ITSRSTATUS

struct STATUSDATA
{
  int    videoBuffCount;
  int    audioBuffCount;
  float  videoDelta;
  float  audioDemuxDelta;
  float  audioDeltaRef;
  float  audioPinDelta;
  bool   isMPARcontrol;
  bool   isTimeShifting;
  unsigned int clockAdjustments;
};

// {007b4e6a-c1e2-4bfe-8109-cdf2bc3a28f1}
static const GUID IID_ITSRStatus = { 0x007b4e6a, 0xc1e2, 0x4bfe, { 0x81, 0x09, 0xcd, 0xf2, 0xbc, 0x3a, 0x28, 0xf1 } };
DEFINE_GUID(CLSID_ITSRStatus, 0x007b4e6a, 0xc1e2, 0x4bfe, 0x81, 0x09, 0xcd, 0xf2, 0xbc, 0x3a, 0x28, 0xf1);

MIDL_INTERFACE("007b4e6a-c1e2-4bfe-8109-cdf2bc3a28f1")
ITSRStatus: public IUnknown
{
public:
  virtual HRESULT STDMETHODCALLTYPE GetStatusData(STATUSDATA *statusData) = 0;
  virtual HRESULT STDMETHODCALLTYPE AdjustClock(DOUBLE adjustment) = 0;
  virtual HRESULT STDMETHODCALLTYPE SetBias(DOUBLE bias) = 0;
  virtual HRESULT STDMETHODCALLTYPE SetAudioDelay(DOUBLE delay) = 0; //delay is in milliseconds  
};

#endif // ITSRSTATUS
