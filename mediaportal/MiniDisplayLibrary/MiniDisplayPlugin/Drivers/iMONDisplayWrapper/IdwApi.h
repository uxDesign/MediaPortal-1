//------------------------------------------------------------------------------
#ifndef IDWAPI_H_INCLUDED
#define IDWAPI_H_INCLUDED
//------------------------------------------------------------------------------
#include <windows.h>
#include "Mutex.h"
#include "iMONDisplayDefines.h"
#include "iMONDisplayWrapper.h"
#include "IdwThread.h"
//------------------------------------------------------------------------------
class IdwApi
{
public:
  IdwApi(HINSTANCE hInstance);
  ~IdwApi();

public:
  DSPResult Init(IDW_INITRESULT* pInitResult);
  DSPResult Uninit();
  DSPResult IsInited();
  DSPResult IsPluginModeEnabled();
  DSPResult SetVfdText(LPCWSTR lpszLine1, LPCWSTR lpszLine2);
  DSPResult SetVfdEqData(PDSPEQDATA pEqData);
  DSPResult SetLcdText(LPCWSTR lpszText);
  DSPResult SetLcdEqData(PDSPEQDATA pEqDataL, PDSPEQDATA pEqDataR);
  DSPResult SetLcdAllIcons(BOOL bOn);
  DSPResult SetLcdOrangeIcon(BYTE btIconData1, BYTE btIconData2);
  DSPResult SetLcdMediaTypeIcon(BYTE btIconData);
  DSPResult SetLcdSpeakerIcon(BYTE btIconData1, BYTE btIconData2);
  DSPResult SetLcdVideoCodecIcon(BYTE btIconData);
  DSPResult SetLcdAudioCodecIcon(BYTE btIconData);
  DSPResult SetLcdAspectRatioIcon(BYTE btIconData);
  DSPResult SetLcdEtcIcon(BYTE btIconData);
  DSPResult SetLcdProgress(int nCurPos, int nTotal);

private:
  IdwApi(const IdwApi& other) {}
  IdwApi& operator=(const IdwApi& other) { return *this; }

private:
  HINSTANCE m_hInstance;
  Mutex m_mutex;
  int m_nInitCount;
  IdwThread* m_pIdwThread;
};
//------------------------------------------------------------------------------
#endif
//------------------------------------------------------------------------------
