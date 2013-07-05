//------------------------------------------------------------------------------
#include <windows.h>
#include "iMONDisplayWrapper.h"
#include "IdwApi.h"
//------------------------------------------------------------------------------
IdwApi* pIdwApi = NULL;
//------------------------------------------------------------------------------
BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
  switch (fdwReason)
  {
  case DLL_PROCESS_ATTACH:
    pIdwApi = new IdwApi(hinstDLL);
    break;
  case DLL_PROCESS_DETACH:
    delete pIdwApi;
    break;
  }
  return TRUE;
}
//------------------------------------------------------------------------------
DSPResult IDW_Init(IDW_INITRESULT* pInitResult)
{
  return pIdwApi->Init(pInitResult);
}
//------------------------------------------------------------------------------
DSPResult IDW_Uninit()
{
  return pIdwApi->Uninit();
}
//------------------------------------------------------------------------------
DSPResult IDW_IsInited()
{
  return pIdwApi->IsInited();
}
//------------------------------------------------------------------------------
DSPResult IDW_IsPluginModeEnabled()
{
  return pIdwApi->IsPluginModeEnabled();
}
//------------------------------------------------------------------------------
DSPResult IDW_SetVfdText(LPCWSTR lpszLine1, LPCWSTR lpszLine2)
{
  return pIdwApi->SetVfdText(lpszLine1, lpszLine2);
}
//------------------------------------------------------------------------------
DSPResult IDW_SetVfdEqData(PDSPEQDATA pEqData)
{
  return pIdwApi->SetVfdEqData(pEqData);
}
//------------------------------------------------------------------------------
DSPResult IDW_SetLcdText(LPCWSTR lpszLine1)
{
  return pIdwApi->SetLcdText(lpszLine1);
}
//------------------------------------------------------------------------------
DSPResult IDW_SetLcdEqData(PDSPEQDATA pEqDataL, PDSPEQDATA pEqDataR)
{
  return pIdwApi->SetLcdEqData(pEqDataL, pEqDataR);
}
//------------------------------------------------------------------------------
DSPResult IDW_SetLcdAllIcons(BOOL bOn)
{
  return pIdwApi->SetLcdAllIcons(bOn);
}
//------------------------------------------------------------------------------
DSPResult IDW_SetLcdOrangeIcon(BYTE btIconData1, BYTE btIconData2)
{
  return pIdwApi->SetLcdOrangeIcon(btIconData1, btIconData2);
}
//------------------------------------------------------------------------------
DSPResult IDW_SetLcdMediaTypeIcon(BYTE btIconData)
{
	return pIdwApi->SetLcdMediaTypeIcon(btIconData);
}
//------------------------------------------------------------------------------
DSPResult IDW_SetLcdSpeakerIcon(BYTE btIconData1, BYTE btIconData2)
{
  return pIdwApi->SetLcdSpeakerIcon(btIconData1, btIconData2);
}
//------------------------------------------------------------------------------
DSPResult IDW_SetLcdVideoCodecIcon(BYTE btIconData)
{
	return pIdwApi->SetLcdVideoCodecIcon(btIconData);
}
//------------------------------------------------------------------------------
DSPResult IDW_SetLcdAudioCodecIcon(BYTE btIconData)
{
	return pIdwApi->SetLcdAudioCodecIcon(btIconData);
}
//------------------------------------------------------------------------------
DSPResult IDW_SetLcdAspectRatioIcon(BYTE btIconData)
{
	return pIdwApi->SetLcdAspectRatioIcon(btIconData);
}
//------------------------------------------------------------------------------
DSPResult IDW_SetLcdEtcIcon(BYTE btIconData)
{
	return pIdwApi->SetLcdEtcIcon(btIconData);
}
//------------------------------------------------------------------------------
DSPResult IDW_SetLcdProgress(int nCurPos, int nTotal)
{
	return pIdwApi->SetLcdProgress(nCurPos, nTotal);
}
//------------------------------------------------------------------------------