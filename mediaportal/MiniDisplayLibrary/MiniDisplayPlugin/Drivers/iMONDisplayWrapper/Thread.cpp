//------------------------------------------------------------------------------
#include "Thread.h"
//------------------------------------------------------------------------------
Thread::Thread() : m_hThread(NULL), m_dwThreadId(0)
{
}
//------------------------------------------------------------------------------
Thread::~Thread()
{
  if (m_hThread != NULL)
  {
    TerminateThread(m_hThread, -1);
    m_hThread = NULL;
  }
}
//------------------------------------------------------------------------------
void Thread::Start()
{
  if (m_hThread != NULL)
    return;
  m_hThread = CreateThread(
    NULL,
    0,
    Thread::ThreadProc,
    this,
    0,
    &m_dwThreadId);
}
//------------------------------------------------------------------------------
void Thread::Join() const
{
  WaitForSingleObject(m_hThread, INFINITE);
}
//------------------------------------------------------------------------------
DWORD WINAPI Thread::ThreadProc(LPVOID lpParam)
{
  Thread* pThread = (Thread*)lpParam;
  pThread->Run();
  return 0;
}
//------------------------------------------------------------------------------
