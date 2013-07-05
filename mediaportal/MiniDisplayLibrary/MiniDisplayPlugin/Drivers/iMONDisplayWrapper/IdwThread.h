//------------------------------------------------------------------------------
#ifndef IDWTHREAD_H_INCLUDED
#define IDWTHREAD_H_INCLUDED
//------------------------------------------------------------------------------
#include <windows.h>
#include "Thread.h"
#include "Event.h"
#include "Mutex.h"
#include "iMONDisplayDefines.h"
#include "iMONDisplayWrapper.h"
//------------------------------------------------------------------------------
class IdwThread : public Thread
{
public:
  IdwThread(HINSTANCE hInstance);
  virtual ~IdwThread();

private:
  IdwThread(const IdwThread& other) {}
  IdwThread& operator=(const IdwThread& other) { return *this; }

public:
  virtual void Interrupt();
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

protected:
  virtual void Run();

private:
  bool RegisterClass();
  bool CreateMessageWindow();
  void AllowImonMessages();
  bool WaitForWindow();
  static LRESULT CALLBACK WndProc(
    HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
  static void MapChars(
    LPWSTR lpszTarget, LPCWSTR lpszSource, int nMaxLength);
  static wchar_t MapChar(wchar_t ch);

private:
  HINSTANCE m_hInstance;
  HWND m_hWnd;
  Event m_eventWindowCreationDone;
};
//------------------------------------------------------------------------------
#endif
//------------------------------------------------------------------------------
